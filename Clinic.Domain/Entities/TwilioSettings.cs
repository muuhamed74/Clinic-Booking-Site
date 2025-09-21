using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.Entities
{
    public class TwilioSettings
    {
        public string? AccountSid { get; set; }
        public string? AuthToken { get; set; }
        public string? WhatsAppFrom { get; set; }
        public string? JoinCode { get; set; }
    }
}
