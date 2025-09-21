using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;

namespace Clinic.Domain.Specifications.Clinic.Specifications
{
    public class PatientByPhoneSpecification : BaseSpecification<Patient>
    {
        public PatientByPhoneSpecification(string phone) 
            : base(p => p.Phone == phone)
        {
        }
    }
}
