using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities.Enums;

namespace Clinic.Domain.Entities
{
    public class AppointmentArchive : BaseEntity
    {
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public DateTime? Date { get; set; }
        public int QueueNumber { get; set; }
        public DateTime? EstimatedTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Waiting;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public AppointmentType AppointmentType { get; set; }

        public string? PatientName { get; set; }
        public string? Phone { get; set; }
    }
}
