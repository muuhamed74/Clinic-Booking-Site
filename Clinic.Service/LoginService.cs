using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.DTOs;
using Clinic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Service_Abstraction;

namespace Clinic.Service
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<Appuser> _userManager;
        private readonly ITokenService _tokenService;

        public LoginService(UserManager<Appuser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.Username);
            if (user == null) return null;

            var isValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
            if (!isValid) return null;

            var token = await _tokenService.CreateTokenAsync(user, _userManager);

            return new LoginResponseDto { Token = token };
        }
    }
}
