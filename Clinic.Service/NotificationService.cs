using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;
using Clinic.Domain.Repositories;
using Clinic.Domain.Specifications.Clinic.Specifications;
using Microsoft.Extensions.Options;
using Service_Abstraction;

namespace Clinic.Service
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageProvider _messageProvider;
        private readonly NotificationSettings _notificationSettings;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IOptions<NotificationSettings> notificationConfig,
             IMessageProvider whatsAppProvider)
        {
            _unitOfWork = unitOfWork;
            _messageProvider = whatsAppProvider;
            _notificationSettings = notificationConfig.Value;
        }


        public async Task SendStatusChangedAsync(Appointment appointment)
        {
            TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            var estimatedTimeEgypt = TimeZoneInfo.ConvertTimeFromUtc(appointment.EstimatedTime.Value, egyptZone);
            var dateEgypt = TimeZoneInfo.ConvertTimeFromUtc(appointment.Date.Value, egyptZone);

            switch (appointment.Status)
            {
                case AppointmentStatus.Waiting:
                    if (_notificationSettings.SendBookingConfirmation)
                        await SendInternalAsync( appointment, templateId: 8612 ,NotificationType.BookingConfirmation);
                    break;


                case AppointmentStatus.Completed:
                    break;

                case AppointmentStatus.Cancelled:
                    if (_notificationSettings.SendCancellation)
                        await SendInternalAsync(appointment,templateId: 8613, NotificationType.Cancellation);
                    break;

                case AppointmentStatus.Rescheduled: 
                    await SendInternalAsync(appointment,templateId: 8614, NotificationType.Rescheduling);
                    break;

                default:

                    break;
            }
        }



        public async Task SendReminderAsync(Appointment appointment)
        {
            TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            var estimatedTimeEgypt = TimeZoneInfo.ConvertTimeFromUtc(appointment.EstimatedTime.Value, egyptZone);

            if (!_notificationSettings.SendReminder)
                return;

            await SendInternalAsync(appointment,templateId: 8615, NotificationType.Reminder);
        }



        private async Task SendInternalAsync(Appointment appointment, int templateId, NotificationType type)
        {
            TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            //var estimatedTimeEgypt = TimeZoneInfo.ConvertTimeFromUtc(appointment.EstimatedTime.Value, egyptZone);
            //var dateEgypt = TimeZoneInfo.ConvertTimeFromUtc(appointment.Date.Value, egyptZone);

            var utcTime = DateTime.SpecifyKind(appointment.EstimatedTime.Value, DateTimeKind.Utc);

            var egyptFullTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, egyptZone);

            var arCulture = new CultureInfo("ar-EG");

            string formattedDate = egyptFullTime.ToString("yyyy/MM/dd", arCulture);
            string formattedHour = egyptFullTime.ToString("hh:mm tt", arCulture);


            List<string> variables = templateId switch
            {
                //710 => new()
                8612 => new()
            {
            appointment.PatientName,
            formattedDate,
            formattedHour,
            appointment.QueueNumber.ToString()
            },
                // 711 => new()
                8613 => new()
            {
                appointment.PatientName
            },
                // 712 => new()
                8614 => new()
            {
            appointment.PatientName,
            formattedDate,
            formattedHour,
            appointment.QueueNumber.ToString()
            },
                // 713 => new()
                8615 => new()
            {
            appointment.PatientName,
            formattedHour
            },
                _ => new()
            };

            var notification = await SaveNotificationAsync(appointment.Id, $"Template {templateId}", type);

            await _messageProvider.SendAsync(appointment.Phone, templateId, variables);

            notification.IsSent = true;
            notification.SentAt = DateTime.UtcNow;
            await _unitOfWork.CompleteAsync();
        }

        private async Task<Notification> SaveNotificationAsync(int appointmentId, string message , NotificationType type)
        {
            var notification = new Notification
            {
                AppointmentId = appointmentId,
                Channel = "SMS",
                Message = message,
                IsSent = false,
                Type = type
            };

            await _unitOfWork.Reposit<Notification>().AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            return notification;
        }

    }   
}
