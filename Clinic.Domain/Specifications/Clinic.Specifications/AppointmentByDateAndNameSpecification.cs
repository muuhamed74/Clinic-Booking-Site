using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class AppointmentByDateAndNameSpecification : BaseSpecification<Appointment>
    {
        public AppointmentByDateAndNameSpecification(string fullName, DateTime utcDate)
        : base(a => a.Date.HasValue &&
                    a.Date.Value >= DateTime.SpecifyKind(utcDate.Date, DateTimeKind.Utc) &&
                    a.Date.Value < DateTime.SpecifyKind(utcDate.Date.AddDays(1), DateTimeKind.Utc) &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.PatientName == fullName)
        {
        }
    }
}
