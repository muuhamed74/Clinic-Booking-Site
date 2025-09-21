using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;
using Clinic.Domain.Repositories;
using Clinic.Domain.Specifications.Clinic.Specifications;
using Clinic.Repo.Repositories;
using Clinic.Service;
using Microsoft.AspNetCore.Routing;
using Service_Abstraction;

namespace Clinic_booking_site.Helpers.Hangfire
{
    public class ReminderJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReminderJob> _logger;

        public ReminderJob(IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<ReminderJob> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task SendRemindersAsync()
        {
            try
            {
                TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                DateTime nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptZone); 
                DateTime reminderToEgypt = nowEgypt.AddMinutes(30);
                DateTime fromEgypt = nowEgypt.AddMinutes(0); 
                DateTime toEgypt = nowEgypt.AddMinutes(35);

                DateTime fromUtc = TimeZoneInfo.ConvertTimeToUtc(fromEgypt, egyptZone);
                DateTime toUtc = TimeZoneInfo.ConvertTimeToUtc(toEgypt, egyptZone);

                var spec = new AppointmentsForReminderSpecification(fromUtc, toUtc);
                var appointments = await _unitOfWork.Reposit<Appointment>().ListAsync(spec);

                _logger.LogInformation($"Found {appointments.Count} appointments for reminder between {nowEgypt:HH:mm} and {reminderToEgypt:HH:mm} (Egypt time)");
                _logger.LogInformation($"Querying appointments between {fromUtc:HH:mm:ss} and {toUtc:HH:mm:ss} (UTC)");

                foreach (var appointment in appointments)
                {

                    var existingReminderSpec = new NotificationsByAppointmentIdAndTypeSpecification(appointment.Id, NotificationType.Reminder);
                    var existingReminder = await _unitOfWork.Reposit<Notification>().GetEntityWithSpec(existingReminderSpec);

                    if (existingReminder != null && existingReminder.IsSent)
                        continue; 

                    await _notificationService.SendReminderAsync(appointment);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReminderJob: Error during sending reminders.");
            }
        }
    }
}

