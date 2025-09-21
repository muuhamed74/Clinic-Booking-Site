using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string Channel { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
