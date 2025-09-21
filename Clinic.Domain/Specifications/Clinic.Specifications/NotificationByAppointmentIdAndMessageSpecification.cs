using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class NotificationByAppointmentIdAndMessageSpecification : BaseSpecification<Notification>
    {
        public NotificationByAppointmentIdAndMessageSpecification(int appointmentId, string message)
      : base(n => n.AppointmentId == appointmentId && n.Message == message)
        {
        }
    }
}
