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
                        await SendInternalAsync(
                            appointment,
                            templateId: 101,
                            $"أهلاً {appointment.PatientName}\n" +
                            $"تم حجز موعدك يوم {dateEgypt:yyyy/MM/dd} الساعة {estimatedTimeEgypt:HH:mm tt} في عيادة دكتورة أميرة محسن.\n" +
                            $"رقم دورك: {appointment.QueueNumber}\n" +
                            $"برجاء التواجد في العيادة قبل الدور بحالتين ولمتابعة دوركم أولاً بأول يمكنكم الدخول علي الرابط التالي:\n" +
                            $"https://amiramohsenclinic.com/info",
                    NotificationType.BookingConfirmation
                            );
                    break;


                case AppointmentStatus.Completed:
                    break;

                case AppointmentStatus.Cancelled:
                    if (_notificationSettings.SendCancellation)
                        await SendInternalAsync(
                            appointment,
                            templateId: 102,
                            $"تنويه: تم إلغاء حجزكم في عيادة د. أميرة محسن.\n" +
                            $"الاسم: {appointment.PatientName}\n" +
                            $"قد يكون سبب الإلغاء: التأخر عن الموعد، أو ظرف طارئ لدى الدكتورة.\n" +
                            $"يمكنكم إعادة الحجز أو المتابعة عبر الرابط: https://amiramohsenclinic.com/booking",
                            NotificationType.Cancellation
                            );
                    break;

                case AppointmentStatus.Rescheduled: 
                    await SendInternalAsync(
                        appointment,
                        templateId: 103,
                        $"تنويه: تم تأجيل موعد حجزكم في عيادة د. أميرة محسن.\n" +
                        $"الاسم: {appointment.PatientName}\n" +
                        $"الموعد الجديد: يوم {dateEgypt:yyyy/MM/dd} الساعة {estimatedTimeEgypt:HH:mm tt}.\n" +
                        $"الدور الجديد: {appointment.QueueNumber}\n" +
                        $"يمكنكم المتابعة عبر الرابط: https://amiramohsenclinic.com/info",
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
                templateId: 104,
                $"تنبيه: برجاء سرعة التواجد بعيادة د. أميرة محسن لتفادي إلغاء حجزك.\n" +
                $"{appointment.PatientName}" +
                $"موعدك المتوقع: {estimatedTimeEgypt: HH: mm tt}",
            NotificationType.Reminder
                );
        }



        private async Task SendInternalAsync(Appointment appointment, int templateId, string message , NotificationType type)
        {
            var notification = await SaveNotificationAsync(appointment.Id, message , type);
            await _messageProvider.SendAsync(appointment.Phone, message, templateId);


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
