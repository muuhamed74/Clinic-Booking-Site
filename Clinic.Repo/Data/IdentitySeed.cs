using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Clinic.Repo.Data
{
    public class IdentitySeed
    {
        public static async Task SeedUserAsync(UserManager<Appuser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (!userManager.Users.Any())
            {
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                var newuser = new Appuser()
                {
                    UserName = "Amira_Mohsen_admin",
                    Email = "amira_admin@example.com"
                };
                var result = await userManager.CreateAsync(newuser, "Amira1234@admin");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newuser, "Admin");
                }
            }
        }
    }
}

