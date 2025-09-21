using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Service_Abstraction
{
    public interface INotificationService
    {
        Task SendStatusChangedAsync(Appointment appointment);
        Task SendReminderAsync(Appointment appointment);
    }
}
