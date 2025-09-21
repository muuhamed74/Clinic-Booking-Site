using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.DTOs
{
    public class BookingOverrideDto
    {
        public DateTime? Date { get; set; }
        public TimeSpan? ClinicStartTime { get; set; }
        public TimeSpan? ClinicEndTime { get; set; }
    }
}
