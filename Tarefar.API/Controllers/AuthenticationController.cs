using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Tarefar.DB.Models;
using Tarefar.Infra.Models;
using Tarefar.Services.Helpers;
using Tarefar.Services.Models.Authentication;

namespace Tarefar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<long>> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<long>> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <param name="model">UserName and Password of the user that is trying to loggin</param>
        /// <returns>Response type with message or token</returns>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = AuthenticationHelper.GetToken(authClaims, _configuration);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        /// <summary>
        /// Register a new user in the system
        /// </summary>
        /// <param name="model">UserName, Name, E-mail and Password of the user that should be registered</param>
        /// <returns>Response type with message</returns>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, "User already exists!");

            ApplicationUser user = new()
            {
                Email = model.Email,
                Name = model.Name,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result?.Succeeded != true)
                return StatusCode(StatusCodes.Status500InternalServerError, "User creation failed! Please check user details and try again.");

            if (!await _roleManager.RoleExistsAsync(UserRoles.Default))
                await _roleManager.CreateAsync(new IdentityRole<long>(UserRoles.Default));

            if (await _roleManager.RoleExistsAsync(UserRoles.Default))
                await _userManager.AddToRoleAsync(user, UserRoles.Default);

            return Ok("User created successfully!");
        }

        /// <summary>
        /// Add a role to an existing user
        /// </summary>
        /// <param name="userName">UserName of the user</param>
        /// <param name="role">Role that should be added to the user</param>
        /// <returns>Response type with message</returns>
        [HttpPost]
        [Route("add-role")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> AddRole(string userName, string role = UserRoles.Plus)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "User does not exists!");

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole<long>(role));
            else if (await _userManager.IsInRoleAsync(user, role))
                return Ok($"User '{userName}' already is a {role} user!");

            if (await _roleManager.RoleExistsAsync(role))
                await _userManager.AddToRoleAsync(user, role);

            return Ok($"User '{userName}' now is a {role} user!");
        }

        /// <summary>
        /// Remove a role from an existing user
        /// </summary>
        /// <param name="userName">UserName of the user</param>
        /// <param name="role">Role that should be removed</param>
        /// <returns>Response type with message</returns>
        [HttpPost]
        [Route("remove-role")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> RemoveRole(string userName, string role = UserRoles.Plus)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "User does not exists!");

            if (!await _roleManager.RoleExistsAsync(role))
                return StatusCode(StatusCodes.Status500InternalServerError, $"Role does not exist!");

            if (!await _userManager.IsInRoleAsync(user, role))
                return Ok($"User '{userName}' already isn't a {role} user!");

            await _userManager.RemoveFromRoleAsync(user, role);

            return Ok($"User '{userName}' isn't a {role} user anymore!");
        }
    }
}
