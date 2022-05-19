using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Tarefar.DB.Models;
using Tarefar.API.Controllers;
using Microsoft.Extensions.Configuration;
using Tarefar.Services.Models.Authentication;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Tarefar.Infra.Models;

namespace Tarefar.Tests.Controllers
{
    public class AuthenticationControllerTest
    {
        private readonly IConfiguration _configurationSubstitute;
        public AuthenticationControllerTest()
        {
            _configurationSubstitute = Substitute.For<IConfiguration>();
            _configurationSubstitute["JWT:ValidAudience"] = "http://localhost:4200";
            _configurationSubstitute["JWT:ValidIssuer"] = "http://localhost:5000";
            _configurationSubstitute["JWT:Secret"] = "JWTAuthenticationHIGHsecuredPasswordTESTTESTTEST";
        }

        #region Login
        [Theory, MemberData(nameof(LoginTests))]
        public async void LoginTest(LoginTestInput test)
        {
            var userStoreSubstitute = Substitute.For<IUserStore<ApplicationUser>>();
            var userManagerSubstitute = Substitute.For<UserManager<ApplicationUser>>(userStoreSubstitute, null, null, null, null, null, null, null, null);
            var roleStoreSubstitute = Substitute.For<IRoleStore<IdentityRole<long>>>();
            var roleManagerSubstitute = Substitute.For<RoleManager<IdentityRole<long>>>(roleStoreSubstitute, null, null, null, null);

            userManagerSubstitute
                .FindByNameAsync(Arg.Is(test.Request?.UserName))
                .Returns(Task.FromResult(test.MockFindByNameAsyncResponse));
            userManagerSubstitute
                .CheckPasswordAsync(Arg.Is(test.MockFindByNameAsyncResponse), Arg.Is(test.Request?.Password))
                .Returns(Task.FromResult(test.MockCheckPasswordAsyncResponse));
            userManagerSubstitute
                .GetRolesAsync(Arg.Is(test.MockFindByNameAsyncResponse))
                .Returns(Task.FromResult(test.MockGetRolesAsyncResponse));
            
            var controller = new AuthenticateController(userManagerSubstitute, roleManagerSubstitute, _configurationSubstitute);
            IActionResult response = await controller.Login(test.Request);

            Assert.Equal(test.ExpectedResponseType, response.GetType());
        }

        public static readonly TheoryData<LoginTestInput> LoginTests = new()
        {
            new LoginTestInput()
            {
                TestName = "Fail, invalid UserName",
                ExpectSuccess = false,
                ExpectedResponseType = typeof(UnauthorizedResult),
                Request = new LoginRequestModel()
                {
                    UserName = "isabella"
                },
                MockFindByNameAsyncResponse = null
            },
            new LoginTestInput()
            {
                TestName = "Fail, invalid Password",
                ExpectSuccess = false,
                ExpectedResponseType = typeof(UnauthorizedResult),
                Request = new LoginRequestModel()
                {
                    UserName = "isabella"
                },
                MockFindByNameAsyncResponse = new ApplicationUser()
                {
                    Name = "Isabella",
                    UserName = "isabella"
                },
                MockCheckPasswordAsyncResponse = false
            },
            new LoginTestInput()
            {
                TestName = "Success, default role",
                ExpectSuccess = true,
                ExpectedResponseType = typeof(OkObjectResult),
                Request = new LoginRequestModel()
                {
                    UserName = "isabella"
                },
                MockFindByNameAsyncResponse = new ApplicationUser()
                {
                    Name = "Isabella",
                    UserName = "isabella"
                },
                MockCheckPasswordAsyncResponse = true,
                MockGetRolesAsyncResponse = new List<string>()
                {
                    "Default"
                }
            }
        };
        public class LoginTestInput
        {
            public string TestName { get; set; }
            public bool ExpectSuccess { get; set; }
            public Type ExpectedResponseType { get; set; }
            public LoginRequestModel Request { get; set; }
            public ApplicationUser MockFindByNameAsyncResponse { get; set; }
            public bool MockCheckPasswordAsyncResponse { get; set; }
            public IList<string> MockGetRolesAsyncResponse { get; set; }
        }
        #endregion Login

        #region Register
        [Theory, MemberData(nameof(RegisterTests))]
        public async void RegisterTest(RegisterPlusTestInput test)
        {
            var userStoreSubstitute = Substitute.For<IUserStore<ApplicationUser>>();
            var userManagerSubstitute = Substitute.For<UserManager<ApplicationUser>>(userStoreSubstitute, null, null, null, null, null, null, null, null);
            var roleStoreSubstitute = Substitute.For<IRoleStore<IdentityRole<long>>>();
            var roleManagerSubstitute = Substitute.For<RoleManager<IdentityRole<long>>>(roleStoreSubstitute, null, null, null, null);

            userManagerSubstitute
                .FindByNameAsync(Arg.Is(test.Request?.UserName))
                .Returns(Task.FromResult(test.MockFindByNameAsyncResponse));
            userManagerSubstitute
                .CreateAsync(Arg.Any<ApplicationUser>(), Arg.Is(test.Request.Password))
                .Returns(Task.FromResult(test.MockCreateAsyncResponse));

            roleManagerSubstitute
                .RoleExistsAsync(Arg.Is(UserRoles.Default))
                .Returns(
                    x => Task.FromResult(test.MockRoleExistsAsyncResponses[0]),
                    x => Task.FromResult(test.MockRoleExistsAsyncResponses[1]));

            var controller = new AuthenticateController(userManagerSubstitute, roleManagerSubstitute, _configurationSubstitute);
            IActionResult response = await controller.Register(test.Request);

            await roleManagerSubstitute
                .Received(test.ExpectedRoleCreationCalls)
                .CreateAsync(Arg.Any<IdentityRole<long>>());
            await userManagerSubstitute
                .Received(test.ExpectedRoleAdditionToUserCalls)
                .AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Is(UserRoles.Default));
            Assert.Equal(test.ExpectedResponseType, response.GetType());
            Assert.Equal((response as dynamic).Value, test.ExpectedResponseValue);
        }

        public static readonly TheoryData<RegisterPlusTestInput> RegisterTests = new()
        {
            new RegisterPlusTestInput()
            {
                TestName = "Fail, user already exists",
                ExpectSuccess = false,
                ExpectedResponseType = typeof(ObjectResult),
                ExpectedResponseValue = "User already exists!",
                Request = new RegisterRequestModel()
                {
                    UserName = "isabella"
                },
                MockFindByNameAsyncResponse = new ApplicationUser()
                {
                    Name = "Isabella",
                    UserName = "isabella"
                }
            },
            new RegisterPlusTestInput()
            {
                TestName = "Fail at the creation",
                ExpectSuccess = false,
                ExpectedResponseType = typeof(ObjectResult),
                ExpectedResponseValue = "User creation failed! Please check user details and try again.",
                Request = new RegisterRequestModel()
                {
                    UserName = "isabella"
                },
                MockFindByNameAsyncResponse = null,
                MockCreateAsyncResponse = IdentityResult.Failed()
            },
            new RegisterPlusTestInput()
            {
                TestName = "Success creation, with role creation",
                ExpectSuccess = true,
                ExpectedResponseType = typeof(OkObjectResult),
                ExpectedResponseValue = "User created successfully!",
                ExpectedRoleCreationCalls = 1,
                ExpectedRoleAdditionToUserCalls = 1,
                Request = new RegisterRequestModel()
                {
                    UserName = "isabella"
                },
                MockFindByNameAsyncResponse = null,
                MockCreateAsyncResponse = IdentityResult.Success,
                MockRoleExistsAsyncResponses = new List<bool>() { false, true }
            },
            new RegisterPlusTestInput()
            {
                TestName = "Success creation, with pre-existing role",
                ExpectSuccess = true,
                ExpectedResponseType = typeof(OkObjectResult),
                ExpectedResponseValue = "User created successfully!",
                ExpectedRoleCreationCalls = 0,
                ExpectedRoleAdditionToUserCalls = 1,
                Request = new RegisterRequestModel()
                {
                    UserName = "isabella"
                },
                MockFindByNameAsyncResponse = null,
                MockCreateAsyncResponse = IdentityResult.Success,
                MockRoleExistsAsyncResponses = new List<bool>() { true, true }
            }
        };
        public class RegisterPlusTestInput
        {
            public string TestName { get; set; }
            public bool ExpectSuccess { get; set; }
            public Type ExpectedResponseType { get; set; }
            public object ExpectedResponseValue { get; set; }
            public int ExpectedRoleCreationCalls { get; set; }
            public int ExpectedRoleAdditionToUserCalls { get; set; }
            public RegisterRequestModel Request { get; set; }
            public ApplicationUser MockFindByNameAsyncResponse { get; set; }
            public IdentityResult MockCreateAsyncResponse { get; set; }
            public IList<bool> MockRoleExistsAsyncResponses { get; set; }
        }
        #endregion Register

        #region AddRole
        [Theory, MemberData(nameof(AddRoleTests))]
        public async void AddRoleTest(AddRoleTestInput test)
        {
            var userStoreSubstitute = Substitute.For<IUserStore<ApplicationUser>>();
            var userManagerSubstitute = Substitute.For<UserManager<ApplicationUser>>(userStoreSubstitute, null, null, null, null, null, null, null, null);
            var roleStoreSubstitute = Substitute.For<IRoleStore<IdentityRole<long>>>();
            var roleManagerSubstitute = Substitute.For<RoleManager<IdentityRole<long>>>(roleStoreSubstitute, null, null, null, null);

            userManagerSubstitute
                .FindByNameAsync(Arg.Is(test.UserName))
                .Returns(Task.FromResult(test.MockFindByNameAsyncResponse));
            userManagerSubstitute
                .IsInRoleAsync(Arg.Any<ApplicationUser>(), test.Role)
                .Returns(Task.FromResult(test.MockIsInRoleAsyncResponse));

            roleManagerSubstitute
                .RoleExistsAsync(Arg.Is(UserRoles.Plus))
                .Returns(
                    x => Task.FromResult(test.MockRoleExistsAsyncResponses[0]),
                    x => Task.FromResult(test.MockRoleExistsAsyncResponses[1]));

            var controller = new AuthenticateController(userManagerSubstitute, roleManagerSubstitute, _configurationSubstitute);
            IActionResult response = await controller.AddRole(test.UserName, test.Role);

            await roleManagerSubstitute
                .Received(test.ExpectedRoleCreationCalls)
                .CreateAsync(Arg.Any<IdentityRole<long>>());
            await userManagerSubstitute
                .Received(test.ExpectedRoleAdditionToUserCalls)
                .AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Is(UserRoles.Plus));
            Assert.Equal(test.ExpectedResponseType, response.GetType());
            Assert.Equal((response as dynamic).Value, test.ExpectedResponseValue);
        }

        public static readonly TheoryData<AddRoleTestInput> AddRoleTests = new()
        {
            new AddRoleTestInput()
            {
                TestName = "Fail, user does not exist",
                ExpectSuccess = false,
                ExpectedResponseType = typeof(ObjectResult),
                ExpectedResponseValue = "User does not exists!",
                UserName = "isabella",
                Role = UserRoles.Plus,
                MockFindByNameAsyncResponse = null
            },
            new AddRoleTestInput()
            {
                TestName = "Success, user already has role",
                ExpectSuccess = true,
                ExpectedResponseType = typeof(OkObjectResult),
                ExpectedResponseValue = "User 'isabella' already is a Plus user!",
                ExpectedRoleCreationCalls = 0,
                ExpectedRoleAdditionToUserCalls = 0,
                UserName = "isabella",
                Role = UserRoles.Plus,
                MockFindByNameAsyncResponse = new ApplicationUser(),
                MockRoleExistsAsyncResponses = new List<bool>() { true, true },
                MockIsInRoleAsyncResponse = true
            },
            new AddRoleTestInput()
            {
                TestName = "Success addition, with role creation",
                ExpectSuccess = true,
                ExpectedResponseType = typeof(OkObjectResult),
                ExpectedResponseValue = "User 'isabella' now is a Plus user!",
                ExpectedRoleCreationCalls = 1,
                ExpectedRoleAdditionToUserCalls = 1,
                UserName = "isabella",
                Role = UserRoles.Plus,
                MockFindByNameAsyncResponse = new ApplicationUser(),
                MockRoleExistsAsyncResponses = new List<bool>() { false, true },
                MockIsInRoleAsyncResponse = false
            },
            new AddRoleTestInput()
            {
                TestName = "Success addition, with pre-existing role",
                ExpectSuccess = true,
                ExpectedResponseType = typeof(OkObjectResult),
                ExpectedResponseValue = "User 'isabella' now is a Plus user!",
                ExpectedRoleCreationCalls = 0,
                ExpectedRoleAdditionToUserCalls = 1,
                UserName = "isabella",
                Role = UserRoles.Plus,
                MockFindByNameAsyncResponse = new ApplicationUser(),
                MockRoleExistsAsyncResponses = new List<bool>() { true, true },
                MockIsInRoleAsyncResponse = false
            }
        };
        public class AddRoleTestInput
        {
            public string TestName { get; set; }
            public bool ExpectSuccess { get; set; }
            public Type ExpectedResponseType { get; set; }
            public object ExpectedResponseValue { get; set; }
            public int ExpectedRoleCreationCalls { get; set; }
            public int ExpectedRoleAdditionToUserCalls { get; set; }
            public string UserName { get; set; }
            public string Role { get; set; }
            public ApplicationUser MockFindByNameAsyncResponse { get; set; }
            public IList<bool> MockRoleExistsAsyncResponses { get; set; }
            public bool MockIsInRoleAsyncResponse { get; set; }
        }
        #endregion AddRole

        #region RemoveRole
        [Theory, MemberData(nameof(RemoveRoleTests))]
        public async void RemoveRoleTest(RemoveRoleTestInput test)
        {
            var userStoreSubstitute = Substitute.For<IUserStore<ApplicationUser>>();
            var userManagerSubstitute = Substitute.For<UserManager<ApplicationUser>>(userStoreSubstitute, null, null, null, null, null, null, null, null);
            var roleStoreSubstitute = Substitute.For<IRoleStore<IdentityRole<long>>>();
            var roleManagerSubstitute = Substitute.For<RoleManager<IdentityRole<long>>>(roleStoreSubstitute, null, null, null, null);

            userManagerSubstitute
                .FindByNameAsync(Arg.Is(test.UserName))
                .Returns(Task.FromResult(test.MockFindByNameAsyncResponse));
            userManagerSubstitute
                .IsInRoleAsync(Arg.Any<ApplicationUser>(), test.Role)
                .Returns(Task.FromResult(test.MockIsInRoleAsyncResponse));

            roleManagerSubstitute
                .RoleExistsAsync(Arg.Is(UserRoles.Plus))
                .Returns(Task.FromResult(test.MockRoleExistsAsyncResponses));

            var controller = new AuthenticateController(userManagerSubstitute, roleManagerSubstitute, _configurationSubstitute);
            IActionResult response = await controller.RemoveRole(test.UserName, test.Role);

            await userManagerSubstitute
                .Received(test.ExpectedRemoveFromRoleAsyncCalls)
                .RemoveFromRoleAsync(Arg.Any<ApplicationUser>(), Arg.Is(UserRoles.Plus));
            Assert.Equal(test.ExpectedResponseType, response.GetType());
            Assert.Equal((response as dynamic).Value, test.ExpectedResponseValue);
        }

        public static readonly TheoryData<RemoveRoleTestInput> RemoveRoleTests = new()
        {
            new RemoveRoleTestInput()
            {
                TestName = "Fail, user does not exist",
                ExpectSuccess = false,
                ExpectedResponseType = typeof(ObjectResult),
                ExpectedResponseValue = "User does not exists!",
                UserName = "isabella",
                Role = UserRoles.Plus,
                MockFindByNameAsyncResponse = null
            },
            new RemoveRoleTestInput()
            {
                TestName = "Fail, role does not exist",
                ExpectSuccess = false,
                ExpectedResponseType = typeof(ObjectResult),
                ExpectedResponseValue = "Role does not exist!",
                ExpectedRemoveFromRoleAsyncCalls = 0,
                UserName = "isabella",
                Role = UserRoles.Plus,
                MockFindByNameAsyncResponse = new ApplicationUser(),
                MockRoleExistsAsyncResponses = false
            },
            new RemoveRoleTestInput()
            {
                TestName = "Success, user already not in role",
                ExpectSuccess = true,
                ExpectedResponseType = typeof(OkObjectResult),
                ExpectedResponseValue = "User 'isabella' already isn't a Plus user!",
                ExpectedRemoveFromRoleAsyncCalls = 0,
                UserName = "isabella",
                Role = UserRoles.Plus,
                MockFindByNameAsyncResponse = new ApplicationUser(),
                MockRoleExistsAsyncResponses = true,
                MockIsInRoleAsyncResponse = false
            },
            new RemoveRoleTestInput()
            {
                TestName = "Success, remove role",
                ExpectSuccess = true,
                ExpectedResponseType = typeof(OkObjectResult),
                ExpectedResponseValue = $"User 'isabella' isn't a Plus user anymore!",
                ExpectedRemoveFromRoleAsyncCalls = 1,
                UserName = "isabella",
                Role = UserRoles.Plus,
                MockFindByNameAsyncResponse = new ApplicationUser(),
                MockRoleExistsAsyncResponses = true,
                MockIsInRoleAsyncResponse = true
            }
        };
        public class RemoveRoleTestInput
        {
            public string TestName { get; set; }
            public bool ExpectSuccess { get; set; }
            public Type ExpectedResponseType { get; set; }
            public object ExpectedResponseValue { get; set; }
            public int ExpectedRemoveFromRoleAsyncCalls { get; set; }
            public string UserName { get; set; }
            public string Role { get; set; }
            public ApplicationUser MockFindByNameAsyncResponse { get; set; }
            public bool MockRoleExistsAsyncResponses { get; set; }
            public bool MockIsInRoleAsyncResponse { get; set; }
        }
        #endregion AddRole
    }
}
