using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class AppointmentArchiveByPhoneSpecification : BaseSpecification<AppointmentArchive>
    {
        public AppointmentArchiveByPhoneSpecification(string phone) 
            : base(p => p.Phone == phone)
        {
        }
    }
}
