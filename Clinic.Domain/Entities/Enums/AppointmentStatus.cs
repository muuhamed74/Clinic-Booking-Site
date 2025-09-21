using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.Entities.Enums
{
    public enum AppointmentStatus
    {
        [EnumMember(Value = "Waiting")]
        Waiting,

        [EnumMember(Value = "Completed")]
        Completed,

        [EnumMember(Value = "Cancelled")]
        Cancelled,

        [EnumMember(Value = "Rescheduled")]
        Rescheduled

    }
}
