using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.Entities
{
    public class NewApiRequest
    {
        public string recipients { get; set; }
        public string message { get; set; }
        public string contentType { get; set; } = "string";
        public string sessionId { get; set; }
        public bool no_duplication { get; set; } = false;
    }
}
