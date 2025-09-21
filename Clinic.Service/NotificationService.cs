using System;
using System.Collections.Generic;
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
        private readonly IWhatsAppProvider _whatsAppProvider;
        private readonly NotificationSettings _notificationSettings;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IOptions<NotificationSettings> notificationConfig,
             IWhatsAppProvider whatsAppProvider)
        {
            _unitOfWork = unitOfWork;
            _whatsAppProvider = whatsAppProvider;
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
                        await SendInternalAsync(
                            appointment,
                            $"تم الحجز بنجاح 🎉\n" +
                            $"الاسم: {appointment.Patient.Name}\n" +
                            $"الدور: {appointment.QueueNumber}\n" +
                            $"الوقت المتوقع: {estimatedTimeEgypt:HH:mm tt}\n",
                            NotificationType.BookingConfirmation
                            );
                    break;


                case AppointmentStatus.Completed:
                    break;

                case AppointmentStatus.Cancelled:
                    if (_notificationSettings.SendCancellation)
                        await SendInternalAsync(
                            appointment,
                            $"تنويه ❌\n" +
                            $"عذراً، تم إلغاء حجزك بتاريخ {dateEgypt:d}.\n" +
                            $"الاسم: {appointment.Patient.Name}\n" +
                            $"رقم الدور: {appointment.QueueNumber}.",
                            NotificationType.Cancellation
                            );
                    break;

                case AppointmentStatus.Rescheduled: 
                    await SendInternalAsync(
                        appointment,
                        $"تنويه ⚠️\n" +
                        $"تم تغيير موعدك.\n" +
                        $"الاسم: {appointment.Patient.Name}\n" +
                        $"الدور الجديد: {appointment.QueueNumber}\n" +
                        $"الوقت المتوقع الجديد: {estimatedTimeEgypt:HH:mm tt}.",
                        NotificationType.Rescheduling
                    );
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

            await SendInternalAsync(
                appointment,
                $"تذكير ⏰\n" +
                $"الاسم: {appointment.Patient.Name}\n" +
                $"موعدك سيبدأ قريبا , تقريباً في تمام {estimatedTimeEgypt:HH:mm tt}.",
                NotificationType.Reminder
                );
        }



        private async Task SendInternalAsync(Appointment appointment, string message , NotificationType type)
        {
            var notification = await SaveNotificationAsync(appointment.Id, message , type);
            await _whatsAppProvider.SendAsync(appointment.Patient.Phone, notification.Message);

            notification.IsSent = true;
            notification.SentAt = DateTime.UtcNow;
            await _unitOfWork.CompleteAsync();
        }

        private async Task<Notification> SaveNotificationAsync(int appointmentId, string message , NotificationType type)
        {
            var notification = new Notification
            {
                AppointmentId = appointmentId,
                Channel = "WhatsApp",
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
