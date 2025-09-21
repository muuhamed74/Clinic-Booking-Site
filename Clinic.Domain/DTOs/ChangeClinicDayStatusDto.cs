using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Domain.DTOs
{
    public class ChangeClinicDayStatusDto
    {
        public DateTime Date { get; set; }
        public bool IsClosed { get; set; }
    }
}
