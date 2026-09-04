using Clinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Abstraction
{
    public interface Iwpsenderservice
    {
        Task SendStatusAsync(Appointment appointment);
        Task SendReminderAsync(Appointment appointment);
    }
}
