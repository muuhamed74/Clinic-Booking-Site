using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Repo.Data
{
    public class ClinicContext : IdentityDbContext<Appuser>
    {
        public ClinicContext(DbContextOptions<ClinicContext> options) : base(options)
        {
        }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BookingOverride> BookingOverride { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
