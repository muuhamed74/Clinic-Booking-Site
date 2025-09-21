using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.Entities
{
    public class WhatsAppSettings
    {
        public string BaseUrl { get; set; } = null!;
        public string PhoneNumberId { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string FromPhoneNumber { get; set; } = null!;
    }
}
