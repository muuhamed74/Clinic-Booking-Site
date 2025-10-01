using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class AppointmentByIdWithPatientSpecification : BaseSpecification<Appointment>
    {
        public AppointmentByIdWithPatientSpecification(int appointmentId)
        : base(a => a.Id == appointmentId)
        {
        }
    }
}
