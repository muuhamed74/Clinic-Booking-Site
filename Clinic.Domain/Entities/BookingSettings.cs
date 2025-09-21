using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.Entities
{
    public class BookingSettings 
    {
        public TimeSpan? ClinicStartTime { get; set; }
        public TimeSpan? ClinicEndTime { get; set; }
        public int MinutesPerCase { get; set; }
    }
}
