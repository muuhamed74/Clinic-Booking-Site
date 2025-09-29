using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.DTOs;

namespace Service_Abstraction
{
    public interface IAdminService
    {
        Task<List<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto> CancelAppointmentAsync(int appointmentId);
        Task<AppointmentDto> CompleteAppointmentAsync(int appointmentId);
        Task<AppointmentDto> RescheduleAppointmentAsync(RescheduleAppointmentRequestDto requestDto);
        Task<AppointmentDto> RescheduleAppointmentToAnotherDayAsync(RescheduleAppointmentRequestDto requestDto);
        Task<BookingOverrideDto> UpsertBookingOverrideAsync(BookingOverrideDto request);
        Task CloseClinicDayAsync(ChangeClinicDayStatusDto request);
        Task ToggleClinicDayStatusAsync(ChangeClinicDayStatusDto request); 
    }
}
