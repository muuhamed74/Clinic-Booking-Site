using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;


namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class AppointmentsForReminderSpecification : BaseSpecification<Appointment>
    {
        public AppointmentsForReminderSpecification(DateTime fromUtc, DateTime toUtc)
        : base(a => a.EstimatedTime.HasValue &&
                    a.EstimatedTime.Value >= DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc) &&
                    a.EstimatedTime.Value <= DateTime.SpecifyKind(toUtc, DateTimeKind.Utc) &&
                    (a.Status == AppointmentStatus.Waiting || a.Status == AppointmentStatus.Rescheduled))
        {
        }
    }
}
