using Clinic.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Abstraction
{
    public interface IDoctorAnnouncementService
    {
        Task<DoctorAnnouncementDto> AddAsync(CreateDoctorAnnouncementDto dto);
        Task DeleteAsync(int id);
        Task<List<DoctorAnnouncementDto>> GetAllAsync();
    }
}
