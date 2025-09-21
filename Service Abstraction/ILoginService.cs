using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.DTOs;

namespace Service_Abstraction
{
    public interface ILoginService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
    }
}
