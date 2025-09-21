using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities.Enums;

namespace Clinic.Domain.DTOs
{
    public class UpdateAppointmentStatusDto
    {
        public int AppointmentId { get; set; }
        public AppointmentStatus Status { get; set; } // "Waiting" / "Cancelled" / "Completed" / "InProgress"v
    }
}
