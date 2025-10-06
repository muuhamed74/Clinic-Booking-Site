using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.Entities
{
    public class SmsSettings
    {
        public string? ApiUrl { get; set; }
        public string? ApiToken { get; set; }
    }
}
