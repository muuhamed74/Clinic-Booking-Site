using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clinic.Domain.DTOs;
using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;
using Clinic.Domain.Repositories;
using Clinic.Domain.Specifications.Clinic.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Service_Abstraction;

namespace Clinic.Service
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly BookingSettings _settings; 

        public BookingService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IOptions<BookingSettings> settings,
             IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _mapper = mapper;
            _settings = settings.Value;
        }

        public async Task<AppointmentDto> BookAppointmentAsync(AppointmentRequestDto request)
        {
            //   غيرها لـ "Africa/Cairo" اما تيجي تشغل الدوكر
            TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            const int maxRetries = 3;
            int attempt = 0;
            int queueNumber = 1;


            DateTime bookingDateEgypt, startTimeEgypt, estimatedTimeEgypt , bookingDateUtc ,
                clinicOpenUtc, clinicCloseUtc , startTimeUtc , estimatedTimeUtc; 

            while (true)
            {

                // Add IsolationLevel to prevent the duplicate booking issue
                // just in case two requests come at the same time

                attempt++;
                await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    #region Check Booking Date Validations
                    DateTime nowUtc = DateTime.UtcNow;
                    DateTime nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, egyptZone);
                    var todayEgypt = nowEgypt.Date;
                    var tomorrowEgypt = todayEgypt.AddDays(1);

                    bookingDateEgypt = DateTime.SpecifyKind(request.BookingDate.Date, DateTimeKind.Unspecified);
                    bookingDateUtc = TimeZoneInfo.ConvertTimeToUtc(bookingDateEgypt, egyptZone);
                    if (bookingDateUtc.Date < bookingDateEgypt.Date)
                        bookingDateUtc = bookingDateUtc.AddDays(1);

                    Console.WriteLine("---- Booking Debug ----");
                    Console.WriteLine($"Now (UTC): {nowUtc}, Kind: {nowUtc.Kind}");
                    Console.WriteLine($"Now (Egypt): {nowEgypt}, Kind: {nowEgypt.Kind}");
                    Console.WriteLine($"Request.BookingDate (raw): {request.BookingDate}, Kind: {request.BookingDate.Kind}");
                    Console.WriteLine($"BookingDateEgypt: {bookingDateEgypt}, Kind: {bookingDateEgypt.Kind}");
                    Console.WriteLine($"BookingDateUtc: {bookingDateUtc}, Kind: {bookingDateUtc.Kind}");


                    if (bookingDateEgypt.Date < todayEgypt)
                        throw new ArgumentException("لا يمكن الحجز في تاريخ قديم.");

                    if (bookingDateEgypt.Date > tomorrowEgypt)
                        throw new ArgumentException("يمكنك الحجز لليوم الحالي أو الغد فقط.");
                    #endregion


                    #region Check Patient Existence
                    var patientSpec = new PatientByPhoneSpecification(request.Phone);
                    var patient = await _unitOfWork.Reposit<Patient>().GetEntityWithSpec(patientSpec);
                    if (patient != null)
                    {
                        var existingAppointmentSpec = new AppointmentByDateAndPhoneSpecification(request.Phone, bookingDateUtc);
                        var existingAppointment = await _unitOfWork.Reposit<Appointment>().GetEntityWithSpec(existingAppointmentSpec);

                        if (existingAppointment != null)
                            throw new ArgumentException("لا يمكنك الحجز أكثر من مرة في نفس اليوم.");
                    }
                    else
                    {
                        patient = _mapper.Map<Patient>(request);
                        patient.CreatedAt = DateTime.UtcNow;
                        await _unitOfWork.Reposit<Patient>().AddAsync(patient);
                        await _unitOfWork.CompleteAsync();
                    }


                    #endregion



                    #region Clinc Start and End Time
                    TimeSpan clinicStart, clinicEnd;
                    int minutesPerCase = _settings.MinutesPerCase;

                    var overrideSetting = await _unitOfWork.Reposit<BookingOverride>()
                        .GetEntityWithSpec(new BookingOverrideByDateSpecification(bookingDateUtc));

                    // For Closed Days
                    if (overrideSetting != null && overrideSetting.IsClosed)
                    {
                        throw new ArgumentException("لا يمكن الحجز في هذا اليوم لأنه مغلق من قبل الدكتور.");
                    }

                    // check if doctor update the time or not 
                    if (overrideSetting != null)
                    {
                        clinicStart = overrideSetting.ClinicStartTime.Value;
                        clinicEnd = overrideSetting.ClinicEndTime.Value;
                    }
                    else
                    {
                        clinicStart = _settings.ClinicStartTime.Value;
                        clinicEnd = _settings.ClinicEndTime.Value;
                    }


                    // clinic working hours in egypt time zone
                    var clinicOpenEgypt = bookingDateEgypt.Date.Add(clinicStart);
                    clinicOpenUtc = TimeZoneInfo.ConvertTimeToUtc(clinicOpenEgypt, egyptZone);
                    var clinicCloseEgypt = bookingDateEgypt.Add(clinicEnd);
                    clinicCloseUtc = TimeZoneInfo.ConvertTimeToUtc(clinicCloseEgypt, egyptZone);
                    startTimeEgypt = clinicOpenEgypt; 
                    startTimeUtc = TimeZoneInfo.ConvertTimeToUtc(startTimeEgypt, egyptZone);

                    if (bookingDateEgypt.Date == nowEgypt.Date)
                    {
                        startTimeEgypt = nowEgypt > clinicOpenEgypt ? nowEgypt : clinicOpenEgypt;
                    }
                    else
                    {
                        startTimeEgypt = clinicOpenEgypt;
                    }
                    startTimeUtc = TimeZoneInfo.ConvertTimeToUtc(startTimeEgypt, egyptZone);
                    #endregion


                    #region Detrmine Estimated Time and Queue Number
                    var spec = new AppointmentByDateSpecification(bookingDateUtc);
                    var todaysAppointments = await _unitOfWork.Reposit<Appointment>().ListAsync(spec);

                    if (todaysAppointments.Any())
                    {
                        var lastAppointment = todaysAppointments.OrderByDescending(a => a.EstimatedTime).FirstOrDefault();


                        if (lastAppointment != null)
                        {
                            var lastEstimatedUtc = lastAppointment.EstimatedTime.Value;
                            var lastEstimatedEgypt = TimeZoneInfo.ConvertTimeFromUtc(lastEstimatedUtc, egyptZone);
                            var nextAvailableEgypt = lastEstimatedEgypt.AddMinutes(_settings.MinutesPerCase);

                            estimatedTimeEgypt = new[]
                            {
                             startTimeEgypt,
                             nextAvailableEgypt
                            }.Max();

                            queueNumber = todaysAppointments.Max(a => a.QueueNumber) + 1;
                        }
                        else
                        {
                            estimatedTimeEgypt = startTimeEgypt;
                            queueNumber = 1;
                        }
                    }
                    else
                    {
                        estimatedTimeEgypt = startTimeEgypt;
                        queueNumber = 1;
                    }

                    estimatedTimeUtc = TimeZoneInfo.ConvertTimeToUtc(estimatedTimeEgypt, egyptZone);
                    if (estimatedTimeUtc >= clinicCloseUtc)
                        throw new ArgumentException("لا يوجد مواعيد متاحة في هذا اليوم.");
                    #endregion


                    #region Make The Appointment and sending Notification
                    var appointment = _mapper.Map<Appointment>(request);    
                    appointment.PatientId = patient.Id;
                    appointment.PatientName = request.Name;   
                    appointment.Phone = patient.Phone;
                    appointment.QueueNumber = queueNumber;
                    appointment.Status = AppointmentStatus.Waiting;
                    appointment.EstimatedTime = estimatedTimeUtc;  
                    appointment.Date = bookingDateUtc;



                    await _unitOfWork.Reposit<Appointment>().AddAsync(appointment);
                    await _unitOfWork.CompleteAsync();
                    await transaction.CommitAsync();

                    var appointmentWithPatient = await _unitOfWork.Reposit<Appointment>()
                        .GetEntityWithSpec(new AppointmentByIdWithPatientSpecification(appointment.Id));

                    var dto = _mapper.Map<AppointmentDto>(appointment);

                    dto.EstimatedTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.EstimatedTime.Value, egyptZone);  
                    dto.Date = TimeZoneInfo.ConvertTimeFromUtc(appointment.Date.Value, egyptZone);
                    await _notificationService.SendStatusChangedAsync(appointment);
                    return dto;
                }
                catch (DbUpdateException ex) when (attempt < maxRetries)
                {
                    await transaction.RollbackAsync();
                    await Task.Delay(50); 
                    continue;
                }
                catch
                {

                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        #endregion


        public async Task<AppointmentDto?> GetAppointmentByPhoneAsync(string phoneNumber, DateTime date)
        {
            TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            DateTime dateEgypt = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);
            DateTime dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateEgypt, egyptZone);
            if (dateUtc.Date < dateEgypt.Date)
                dateUtc = dateUtc.AddDays(1);

            var spec = new AppointmentByDateAndPhoneSpecification(phoneNumber , dateUtc);
            var appointment = await _unitOfWork.Reposit<Appointment>().GetEntityWithSpec(spec);
            var dto = _mapper.Map<AppointmentDto?>(appointment);
            return dto ?? throw new KeyNotFoundException("لا يوجد حجز بهذا الرقم في هذا اليوم.");
        }
    }
}
