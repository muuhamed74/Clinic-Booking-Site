using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.DTOs
{
    public class DoctorAnnouncementDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
