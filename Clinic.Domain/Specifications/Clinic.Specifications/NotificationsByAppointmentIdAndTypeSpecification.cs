using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class NotificationsByAppointmentIdAndTypeSpecification : BaseSpecification<Notification>
    {
        public NotificationsByAppointmentIdAndTypeSpecification(int appointmentId, NotificationType type)
        : base(n => n.AppointmentId == appointmentId && n.Type == type && n.IsSent)
        {
            AddInclude(n => n.Appointment);
        }
    }
}
