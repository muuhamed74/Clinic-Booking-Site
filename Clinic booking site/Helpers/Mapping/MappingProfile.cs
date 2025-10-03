using AutoMapper;
using Clinic.Domain.DTOs;
using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;

namespace Clinic_booking_site.Helpers.Mapping
{
    public class MappingProfile : Profile
    {
        private static readonly TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");


        public MappingProfile()
        {

            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.PatientName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.AppointmentType, opt => opt.MapFrom(src => src.AppointmentType.ToString()))
                .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EstimatedTime.HasValue
                           ? TimeZoneInfo.ConvertTimeFromUtc(src.EstimatedTime.Value, egyptZone)
                           : (DateTime?)null));

            CreateMap<AppointmentArchive, AppointmentDto>()
              .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.PatientName))
              .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
              .ForMember(dest => dest.AppointmentType, opt => opt.MapFrom(src => src.AppointmentType.ToString()))
              .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EstimatedTime.HasValue
                           ? TimeZoneInfo.ConvertTimeFromUtc(src.EstimatedTime.Value, egyptZone)
                           : (DateTime?)null));



            CreateMap<AppointmentRequestDto, Appointment>()
                 .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.BookingDate));

            CreateMap<Notification, NotificationDto>().ReverseMap();


            CreateMap<AppointmentDto, Appointment>()
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<AppointmentStatus>(src.Status)))
                 .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => TimeZoneInfo.ConvertTimeToUtc(src.EstimatedTime, egyptZone)))
                 .ForMember(dest => dest.Date, opt => opt.MapFrom(src => TimeZoneInfo.ConvertTimeToUtc(src.Date, egyptZone)));

            CreateMap<BookingOverride, BookingOverrideDto>().ReverseMap()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => TimeZoneInfo.ConvertTimeFromUtc(src.Date.Value, egyptZone).ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.ClinicStartTime, opt => opt.MapFrom(src => src.ClinicStartTime))
                .ForMember(dest => dest.ClinicEndTime, opt => opt.MapFrom(src => src.ClinicEndTime));

           

        }
    }
}
