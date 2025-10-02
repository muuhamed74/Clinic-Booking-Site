using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class AppointmentArchiveByAppointmentIdSpecification : BaseSpecification<AppointmentArchive>
    {
        public AppointmentArchiveByAppointmentIdSpecification(int appointmentId)
       : base(x => x.AppointmentId == appointmentId)
        {
        }
    }
}
