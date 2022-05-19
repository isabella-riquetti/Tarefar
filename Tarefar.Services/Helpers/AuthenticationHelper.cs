using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tarefar.Services.Helpers
{
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Return JWT token for user claims
        /// </summary>
        /// <param name="authClaims">User claim</param>
        /// <returns>JWT Token valid for 7 days</returns>
        public static JwtSecurityToken GetToken(List<Claim> authClaims, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.Date.AddDays(7),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
