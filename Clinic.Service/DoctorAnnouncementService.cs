using AutoMapper;
using Clinic.Domain.DTOs;
using Clinic.Domain.Entities;
using Clinic.Domain.Repositories;
using Service_Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Service
{
    public class DoctorAnnouncementService : IDoctorAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DoctorAnnouncementService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DoctorAnnouncementDto> AddAsync(CreateDoctorAnnouncementDto dto)
        {
            var entity = new DoctorAnnouncement
            {
                Message = dto.Message
            };

            await _unitOfWork.Reposit<DoctorAnnouncement>().AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DoctorAnnouncementDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Reposit<DoctorAnnouncement>()
                .GetByIdAsync(id);

            if (entity == null)
                throw new ArgumentException("Message not found");

            _unitOfWork.Reposit<DoctorAnnouncement>().Delete(entity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<DoctorAnnouncementDto>> GetAllAsync()
        {
            var list = await _unitOfWork.Reposit<DoctorAnnouncement>()
                .GetAllAsync();

            return _mapper.Map<List<DoctorAnnouncementDto>>(list);
        }
    }
}
