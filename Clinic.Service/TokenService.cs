using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service_Abstraction;

namespace Clinic.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<Appuser> _userManager;


        public TokenService(IConfiguration config, UserManager<Appuser> userManager)
        {
            _config = config;
            _userManager = userManager;


        }



        public async Task<string> CreateTokenAsync(Appuser user, UserManager<Appuser> userManager)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? "noemail@example.com"),
                new Claim(ClaimTypes.Name, user.UserName ?? "NoName"),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: authClaims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JWT:TokenExpiryInMinuts"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
