using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Service_Abstraction
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(Appuser user, UserManager<Appuser> userManager);
    }
}
