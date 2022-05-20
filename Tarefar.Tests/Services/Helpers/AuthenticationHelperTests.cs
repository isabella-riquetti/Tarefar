using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tarefar.DB.Models;
using Tarefar.Services.Helpers;
using Xunit;

namespace Tarefar.Tests.Services.Helpers
{
    public class AuthenticationHelperTests
    {
        #region GetToken
        [Theory, MemberData(nameof(GetTokenTests))]
        public void GetTokenTest(GetTokenTestInput test)
        {
            var configurationSubstitute = Substitute.For<IConfiguration>();
            configurationSubstitute["JWT:ValidIssuer"] = "http://localhost:5000";
            configurationSubstitute["JWT:ValidAudience"] = "http://localhost:4200";
            configurationSubstitute["JWT:Secret"] = "JWTAuthenticationHIGHsecuredPasswordTESTTESTTEST";

            JwtSecurityToken response = AuthenticationHelper.GetToken(test.Claims, configurationSubstitute);

            response.Should().BeEquivalentTo(test.GetExpectedResponse());
        }

        public static readonly TheoryData<GetTokenTestInput> GetTokenTests = new()
        {
            new GetTokenTestInput()
            {
                TestName = "Get token",
                ExpectSuccess = true,
                Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "isabella"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, UserRoles.Default)
                }
            }
        };
        public class GetTokenTestInput
        {
            public string TestName { get; set; }
            public bool ExpectSuccess { get; set; }
            public List<Claim> Claims { get; set; }

            public JwtSecurityToken GetExpectedResponse()
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JWTAuthenticationHIGHsecuredPasswordTESTTESTTEST"));
                return new JwtSecurityToken(
                    issuer: "http://localhost:5000",
                    audience: "http://localhost:4200",
                    expires: DateTime.Now.Date.AddDays(7),
                    claims: Claims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
            }
        }
        #endregion GetToken
    }
}
