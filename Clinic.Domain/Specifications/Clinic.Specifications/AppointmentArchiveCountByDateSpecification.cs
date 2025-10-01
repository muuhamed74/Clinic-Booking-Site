using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class AppointmentArchiveCountByDateSpecification : BaseSpecification<AppointmentArchive>
    {
        public AppointmentArchiveCountByDateSpecification(DateTime date)
       : base(a => a.Date.HasValue && a.Date.Value.Date == DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified))
        {
        }
    }
}
