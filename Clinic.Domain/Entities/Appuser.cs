using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Clinic.Domain.Entities
{
    public class Appuser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
