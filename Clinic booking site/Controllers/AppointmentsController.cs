using Clinic.Domain.DTOs;
using Clinic_booking_site.Helpers.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service_Abstraction;

namespace Clinic_booking_site.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AppointmentsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }


        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] AppointmentRequestDto request)
        {
            var appointment = await _bookingService.BookAppointmentAsync(request);
            return Ok(appointment);
        }

        [HttpGet("track")]
        public async Task<IActionResult> Track([FromQuery] string phoneNumber, [FromQuery] DateTime date)
        {
            var appointment = await _bookingService.GetAppointmentByPhoneAsync(phoneNumber, date);
            if (appointment == null)
                return NotFound(new ApiResponce( 400, "لا يوجد حجز بهذا الرقم في هذا اليوم."));

            return Ok(appointment);
        }
    }
}
