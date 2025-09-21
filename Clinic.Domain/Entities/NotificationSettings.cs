﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.Entities
{
    public class NotificationSettings
    {
        public bool SendBookingConfirmation { get; set; }
        public bool SendReminder { get; set; }
        public bool SendCancellation { get; set; }
    }
}
