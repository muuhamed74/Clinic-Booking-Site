using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class OldAppointmentsSpecification : BaseSpecification<Appointment>
    {
        public OldAppointmentsSpecification(DateTime today)
        : base(a => a.Date < today)
        {
            AddInclude(a => a.Patient);
        }
    }
}
