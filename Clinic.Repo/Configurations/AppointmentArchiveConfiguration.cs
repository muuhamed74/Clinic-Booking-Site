using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Repo.Configurations
{ 
    public class AppointmentArchiveConfiguration : IEntityTypeConfiguration<AppointmentArchive>
    {
        public void Configure(EntityTypeBuilder<AppointmentArchive> builder)
        {
            builder.ToTable("AppointmentArchives");

            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.Appointment)
                   .WithOne(appt => appt.Archive)
                   .HasForeignKey<AppointmentArchive>(a => a.AppointmentId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
