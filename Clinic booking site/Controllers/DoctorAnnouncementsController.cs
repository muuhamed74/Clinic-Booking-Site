using Clinic.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service_Abstraction;

namespace Clinic_booking_site.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorAnnouncementsController : ControllerBase
    {
        private readonly IDoctorAnnouncementService _service;

        public DoctorAnnouncementsController(IDoctorAnnouncementService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateDoctorAnnouncementDto dto)
        {
            var result = await _service.AddAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
    }
}
