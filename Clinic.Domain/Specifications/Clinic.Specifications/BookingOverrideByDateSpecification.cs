using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class BookingOverrideByDateSpecification  : BaseSpecification<BookingOverride>
    {
    public BookingOverrideByDateSpecification(DateTime utcDate)
        : base(x => x.Date.HasValue &&
               x.Date.Value >= DateTime.SpecifyKind(utcDate.Date, DateTimeKind.Utc) &&
               x.Date.Value < DateTime.SpecifyKind(utcDate.Date.AddDays(1), DateTimeKind.Utc))
        {
       }
    
    }
}
