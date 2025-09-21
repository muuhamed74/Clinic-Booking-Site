using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.DTOs
{
    public class RescheduleAppointmentRequestDto
    {
        public int AppointmentId { get; set; }
        public DateTime NewTime { get; set; }
    }
}
