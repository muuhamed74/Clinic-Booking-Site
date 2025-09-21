using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class RemainingAppointmentsSpecification : BaseSpecification<Appointment>
    {
        public RemainingAppointmentsSpecification(DateTime date)
        : base(a => a.Date.Value.Date == date.Date 
               &&(a.Status == AppointmentStatus.Waiting 
               || a.Status == AppointmentStatus.Rescheduled))
        {
            AddInclude(a => a.Patient);
            ApplyOrderBy(a => a.QueueNumber);
        }
    }
}
