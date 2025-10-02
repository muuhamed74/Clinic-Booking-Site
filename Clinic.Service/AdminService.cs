using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clinic.Domain.DTOs;
using Clinic.Domain.Entities;
using Clinic.Domain.Entities.Enums;
using Clinic.Domain.Repositories;
using Clinic.Domain.Specifications.Clinic.Specifications;
using Microsoft.Extensions.Options;
using Service_Abstraction;

namespace Clinic.Service
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IOptions<BookingSettings> _settings;

        public AdminService(IUnitOfWork unitOfWork,
                            IMapper mapper,
                            INotificationService notificationService,
                            IOptions<BookingSettings> settings)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _settings = settings;
        }


        public async Task<(int Count, List<AppointmentDto> Appointments)> GetAllAppointmentsWithCountAsync()
        {

            var specAppointments = new AppointmentsWithPatientsSpecification();
            var appointments = await _unitOfWork.Reposit<Appointment>().ListAsync(specAppointments);
            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(appointments);

            DateTime todayUtc = DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Utc);

            var specArchive = new AppointmentArchiveCountByDateSpecification(todayUtc);
            var archiveAppointments = await _unitOfWork.Reposit<AppointmentArchive>().ListAsync(specArchive);
            int count = archiveAppointments.Count;

            return (count, appointmentDtos);
        }



        public async Task<(int Count, List<AppointmentDto> Appointments)> GetArchivedAppointmentsByDateAsync(DateTime? date = null)
        {
            DateTime filterDate = date ?? DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Utc);

            var specArchive = new AppointmentArchiveCountByDateSpecification(filterDate);
            var archiveAppointments = await _unitOfWork.Reposit<AppointmentArchive>().ListAsync(specArchive);

            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(archiveAppointments);

            return (archiveAppointments.Count, appointmentDtos);
        }



        public async Task<int> DeleteArchivedAppointmentsByDateAsync(DateTime? date = null)
        {
            DateTime filterDate = date ?? DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Utc);

            var specArchive = new AppointmentArchiveCountByDateSpecification(filterDate);
            var archiveAppointments = await _unitOfWork.Reposit<AppointmentArchive>().ListAsync(specArchive);

            if (!archiveAppointments.Any())
                return 0;

            foreach (var appointment in archiveAppointments)
            {
                _unitOfWork.Reposit<AppointmentArchive>().Delete(appointment);
            }

            await _unitOfWork.CompleteAsync();

            return archiveAppointments.Count;
        }



        public async Task<AppointmentDto> CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Reposit<Appointment>()
                .GetEntityWithSpec(new AppointmentByIdWithPatientSpecification(appointmentId));
            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found");

            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var oldQueue = appointment.QueueNumber;
                var oldEstimated = appointment.EstimatedTime;


                var egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                var appointmentDate = TimeZoneInfo.ConvertTimeFromUtc(appointment.EstimatedTime.Value, egyptZone).Date;

                var allAppointmentsToday = await _unitOfWork.Reposit<Appointment>()
                   .GetAllWithSpecAsync(new AppointmentsWithPatientsSpecification());

                var remainingAppointments = allAppointmentsToday
                        .Where(a =>
                        {
                            var apptDate = TimeZoneInfo.ConvertTimeFromUtc(a.EstimatedTime.Value, egyptZone).Date;
                            return apptDate == appointmentDate &&
                                   a.Id != appointment.Id &&
                                   (a.Status == AppointmentStatus.Waiting ||
                                    a.Status == AppointmentStatus.Rescheduled);
                        })
                         .OrderBy(a => a.QueueNumber)
                         .ToList();


                var affectedAppointments = remainingAppointments
                    .Where(a => a.QueueNumber > oldQueue)
                    .OrderBy(a => a.QueueNumber)
                    .ToList();

               
                appointment.Status = AppointmentStatus.Cancelled;

                await _notificationService.SendStatusChangedAsync(appointment);

                var archiveAppointment = await _unitOfWork.Reposit<AppointmentArchive>()
                   .GetEntityWithSpec(new AppointmentArchiveByAppointmentIdSpecification(appointment.Id));

                _unitOfWork.Reposit<AppointmentArchive>().Delete(archiveAppointment);

                _unitOfWork.Reposit<Appointment>().Delete(appointment);


                await _unitOfWork.CompleteAsync();


                int newQueue = oldQueue;
                DateTime currentTime = oldEstimated.Value;

                foreach (var appt in affectedAppointments)
                {
                    var archiveAppt = await _unitOfWork.Reposit<AppointmentArchive>()
                    .GetEntityWithSpec(new AppointmentArchiveByAppointmentIdSpecification(appt.Id));


                    archiveAppt.QueueNumber = appt.QueueNumber;
                    archiveAppt.EstimatedTime = appt.EstimatedTime;
                    archiveAppt.Status = appt.Status;
                    archiveAppt.Date = appt.Date;
                    _unitOfWork.Reposit<AppointmentArchive>().Update(archiveAppt);


                    appt.QueueNumber = newQueue++;
                    appt.EstimatedTime = currentTime;
                    appt.Status = AppointmentStatus.Rescheduled;
                    currentTime = currentTime.AddMinutes(_settings.Value.MinutesPerCase);
                    _unitOfWork.Reposit<Appointment>().Update(appt);

                }

                await _unitOfWork.CompleteAsync();

                foreach (var appt in affectedAppointments)
                {
                    await _notificationService.SendStatusChangedAsync(appt);
                }

                await transaction.CommitAsync();
                return _mapper.Map<AppointmentDto>(appointment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task<AppointmentDto> CompleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Reposit<Appointment>()
                    .GetEntityWithSpec(new AppointmentByIdWithPatientSpecification(appointmentId));

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found");

            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                var nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptZone);

                var appointmentDate = appointment.EstimatedTime.Value.Date;
                if (nowEgypt.Date != appointmentDate)
                    throw new InvalidOperationException("لا يمكن إنهاء الموعد في يوم مختلف عن موعده.");

                var oldQueue = appointment.QueueNumber;

                appointment.Status = AppointmentStatus.Completed;

                //_unitOfWork.Reposit<Appointment>().Update(appointment);
                _unitOfWork.Reposit<Appointment>().Delete(appointment);
                await _unitOfWork.CompleteAsync();

                var allAppointmentsToday = await _unitOfWork.Reposit<Appointment>()
                    .GetAllWithSpecAsync(new AppointmentsWithPatientsSpecification());

                var affectedAppointments = allAppointmentsToday
                    .Where(a =>
                    {
                        var apptDate = a.EstimatedTime.Value.Date; 
                        return apptDate == appointmentDate &&
                               a.QueueNumber > oldQueue &&
                               (a.Status == AppointmentStatus.Waiting || a.Status == AppointmentStatus.Rescheduled);
                    })
                    .OrderBy(a => a.QueueNumber)
                    .ToList();

                foreach (var appt in affectedAppointments)
                {
                    appt.QueueNumber -= 1; 
                    _unitOfWork.Reposit<Appointment>().Update(appt);
                }


                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                return _mapper.Map<AppointmentDto>(appointment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task<AppointmentDto> RescheduleAppointmentAsync(RescheduleAppointmentRequestDto requestDto)
        {
            var appointment = await _unitOfWork.Reposit<Appointment>()
                .GetEntityWithSpec(new AppointmentByIdWithPatientSpecification(requestDto.AppointmentId));

            if (appointment == null)
                throw new ArgumentException("Appointment not found");

            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                var newEgyptTime = DateTime.SpecifyKind(requestDto.NewTime, DateTimeKind.Unspecified);
                var newUtcTime = TimeZoneInfo.ConvertTimeToUtc(newEgyptTime, egyptZone);
                var appointmentDate = TimeZoneInfo.ConvertTimeFromUtc(appointment.EstimatedTime.Value, egyptZone).Date;


                if (newEgyptTime.Date != appointmentDate)
                    throw new ArgumentException("لا يمكن تغيير المعاد ليوم مختلف.");


                var clinicOpenTime = new TimeSpan(11, 30, 0); 
                var clinicCloseTime = new TimeSpan(22, 0, 0);  
                var newTimeOfDay = newEgyptTime.TimeOfDay;

                if (newTimeOfDay < clinicOpenTime || newTimeOfDay > clinicCloseTime)
                    throw new InvalidOperationException($"أوقات العيادة من {clinicOpenTime:hh\\:mm} صباحاً إلى {clinicCloseTime:hh\\:mm} مساءً.");


                appointment.EstimatedTime = newUtcTime;
                appointment.Status = AppointmentStatus.Rescheduled;
                _unitOfWork.Reposit<Appointment>().Update(appointment);
                await _unitOfWork.CompleteAsync();

                var allAppointmentsToday = await _unitOfWork.Reposit<Appointment>()
                    .GetAllWithSpecAsync(new AppointmentsWithPatientsSpecification());


                //error here 
                //الترتيب غلط هنا محتاج اخليه يرتب للي بعده بس مش ال في ال  que كله
                // a.QueueNumber > appointment.QueueNumber هتتاكد انها تجيب المواعيد الي بعده
                var sameDayAppointments = allAppointmentsToday
                          .Where(a =>
                              TimeZoneInfo.ConvertTimeFromUtc(a.EstimatedTime.Value, egyptZone).Date == appointmentDate &&
                              a.Id != appointment.Id &&
                              (a.Status == AppointmentStatus.Waiting || a.Status == AppointmentStatus.Rescheduled) &&
                              a.QueueNumber > appointment.QueueNumber)
                          .OrderBy(a => a.QueueNumber)
                          .ToList();

                var currentTime = newEgyptTime.AddMinutes(_settings.Value.MinutesPerCase);

                foreach (var appt in sameDayAppointments)
                {                  
                    appt.EstimatedTime = TimeZoneInfo.ConvertTimeToUtc(currentTime, egyptZone);
                    appt.Status = AppointmentStatus.Rescheduled;
                    currentTime = currentTime.AddMinutes(_settings.Value.MinutesPerCase);
                    _unitOfWork.Reposit<Appointment>().Update(appt);
                }
                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();
               
                foreach (var appt in sameDayAppointments)
                {
                    await _notificationService.SendStatusChangedAsync(appt);
                }

                return _mapper.Map<AppointmentDto>(appointment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task<AppointmentDto> RescheduleAppointmentToAnotherDayAsync(RescheduleAppointmentRequestDto requestDto)
        {
            var appointment = await _unitOfWork.Reposit<Appointment>()
                .GetEntityWithSpec(new AppointmentByIdWithPatientSpecification(requestDto.AppointmentId));
            if (appointment == null)
                throw new ArgumentException("Appointment not found");

            var archiveAppointment = await _unitOfWork.Reposit<AppointmentArchive>()
                .GetEntityWithSpec(new AppointmentArchiveByAppointmentIdSpecification(appointment.Id));

            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                var newDate = requestDto.NewTime.Date;
                var oldDate = appointment.EstimatedTime.Value.Date;

                if (newDate == oldDate)
                    throw new ArgumentException("لا يمكن تغيير المعاد لنفس اليوم.");

                var clinicOpenTime = new TimeSpan(11, 30, 0); 
                var clinicCloseTime = new TimeSpan(22, 0, 0); 
                var minutesPerCase = _settings.Value.MinutesPerCase;

                var allAppointments = await _unitOfWork.Reposit<Appointment>()
                    .GetAllWithSpecAsync(new AppointmentsWithPatientsSpecification());

                var sameDayAppointments = allAppointments
                    .Where(a =>
                        a.EstimatedTime.HasValue &&
                        a.EstimatedTime.Value.Date == newDate &&
                        (a.Status == AppointmentStatus.Waiting || a.Status == AppointmentStatus.Rescheduled))
                    .OrderBy(a => a.EstimatedTime)
                    .ToList();

                DateTime newUtcTime;

                if (sameDayAppointments.Any())
                {
                    var lastAppt = sameDayAppointments.Last();
                    newUtcTime = lastAppt.EstimatedTime.Value.AddMinutes(minutesPerCase);
                }
                else
                {
                    newUtcTime = TimeZoneInfo.ConvertTimeToUtc(newDate.Add(clinicOpenTime), egyptZone);
                }

                if (TimeZoneInfo.ConvertTimeFromUtc(newUtcTime, TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).TimeOfDay < clinicOpenTime ||
                    TimeZoneInfo.ConvertTimeFromUtc(newUtcTime, TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo")).TimeOfDay > clinicCloseTime)
                {
                    throw new ArgumentException($"أوقات العيادة من {clinicOpenTime:hh\\:mm} صباحاً إلى {clinicCloseTime:hh\\:mm} مساءً.");
                }

                var followingAppointments = allAppointments
                 .Where(a =>
                 a.EstimatedTime.HasValue &&
                 a.EstimatedTime.Value.Date == oldDate &&
                 a.QueueNumber > appointment.QueueNumber &&
                 (a.Status == AppointmentStatus.Waiting || a.Status == AppointmentStatus.Rescheduled))
                 .OrderBy(a => a.QueueNumber)
                 .ToList();

                archiveAppointment.EstimatedTime = newUtcTime;
                archiveAppointment.Status = AppointmentStatus.Rescheduled;
                archiveAppointment.Date = DateTime.SpecifyKind(newDate, DateTimeKind.Utc);
                _unitOfWork.Reposit<AppointmentArchive>().Update(archiveAppointment);

                appointment.EstimatedTime = newUtcTime;
                appointment.Status = AppointmentStatus.Rescheduled;
                appointment.Date = DateTime.SpecifyKind(newDate, DateTimeKind.Utc);
                _unitOfWork.Reposit<Appointment>().Update(appointment);



                var currentTime = TimeZoneInfo.ConvertTimeFromUtc(newUtcTime, egyptZone).AddMinutes(minutesPerCase);

                foreach (var appt in followingAppointments)
                {
                    appt.EstimatedTime = TimeZoneInfo.ConvertTimeToUtc(currentTime, egyptZone);
                    appt.Status = AppointmentStatus.Rescheduled;
                    appt.Date = DateTime.SpecifyKind(newDate, DateTimeKind.Utc);
                    currentTime = currentTime.AddMinutes(minutesPerCase);

                    var archiveAppt = await _unitOfWork.Reposit<AppointmentArchive>()
                       .GetEntityWithSpec(new AppointmentArchiveByAppointmentIdSpecification(appointment.Id));
                    archiveAppt.EstimatedTime = appt.EstimatedTime;
                    archiveAppt.Status = appt.Status;
                    archiveAppt.Date = appt.Date;

                    _unitOfWork.Reposit<AppointmentArchive>().Update(archiveAppt);
                    _unitOfWork.Reposit<Appointment>().Update(appt);
                    currentTime = currentTime.AddMinutes(minutesPerCase);

                }

                var updatedSameDayAppointments = allAppointments
                 .Where(a =>
                 a.EstimatedTime.HasValue &&
                 a.EstimatedTime.Value.Date == newDate &&
                 (a.Status == AppointmentStatus.Waiting || a.Status == AppointmentStatus.Rescheduled))
                 .OrderBy(a => a.EstimatedTime)
                 .ToList();

                int queue = 1;
                foreach (var appt in updatedSameDayAppointments)
                {
                var archiveAppt = await _unitOfWork.Reposit<AppointmentArchive>()
                 .GetEntityWithSpec(new AppointmentArchiveByAppointmentIdSpecification(appointment.Id));

                    archiveAppt.QueueNumber = appt.QueueNumber;
                    _unitOfWork.Reposit<AppointmentArchive>().Update(archiveAppt);


                    appt.QueueNumber = queue++;
                    _unitOfWork.Reposit<Appointment>().Update(appt);

                    
                }

                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                await _notificationService.SendStatusChangedAsync(appointment);
                foreach (var appt in followingAppointments)
                    await _notificationService.SendStatusChangedAsync(appt);

                return _mapper.Map<AppointmentDto>(appointment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task CloseClinicDayAsync(ChangeClinicDayStatusDto request)
        {
            TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            var bookingOverrideRepo = _unitOfWork.Reposit<BookingOverride>();
            var appointmentRepo = _unitOfWork.Reposit<Appointment>();

            DateTime dateEgypt = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Unspecified);
            DateTime dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateEgypt, egyptZone);
            if (dateUtc.Date < dateEgypt.Date)
                dateUtc = dateUtc.AddDays(1);

            var overrideEntity = await bookingOverrideRepo
                .GetEntityWithSpec(new BookingOverrideByDateSpecification(dateUtc));

            if (overrideEntity == null)
            {
                overrideEntity = new BookingOverride
                {
                    Date = dateUtc,
                    ClinicStartTime = _settings.Value.ClinicStartTime,
                    ClinicEndTime = _settings.Value.ClinicEndTime,
                    IsClosed = request.IsClosed
                };
                await bookingOverrideRepo.AddAsync(overrideEntity);
            }
            else
            {
                overrideEntity.IsClosed = request.IsClosed;
                bookingOverrideRepo.Update(overrideEntity);
            }

            if (request.IsClosed)
            {
                var todaysAppointments = await appointmentRepo
                    .ListAsync(new AppointmentByDateSpecification(dateUtc));

                foreach (var appointment in todaysAppointments)
                {
                    appointment.Status = AppointmentStatus.Cancelled;

                    await _notificationService.SendStatusChangedAsync(appointment);
                    appointmentRepo.Delete(appointment);
                }
            }

            await _unitOfWork.CompleteAsync();
        }


        public async Task ToggleClinicDayStatusAsync(ChangeClinicDayStatusDto request)
        {
            TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            var bookingOverrideRepo = _unitOfWork.Reposit<BookingOverride>();
            DateTime dateEgypt = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Unspecified);
            DateTime dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateEgypt, egyptZone);
            if (dateUtc.Date < dateEgypt.Date)
                dateUtc = dateUtc.AddDays(1);


            var overrideEntity = await bookingOverrideRepo
                .GetEntityWithSpec(new BookingOverrideByDateSpecification(dateUtc));
            if (overrideEntity == null)
            {
                overrideEntity = new BookingOverride
                {
                    Date = dateUtc,
                    ClinicStartTime = _settings.Value.ClinicStartTime,
                    ClinicEndTime = _settings.Value.ClinicEndTime,
                    IsClosed = request.IsClosed
                };
                await bookingOverrideRepo.AddAsync(overrideEntity);
            }
            else
            {
                overrideEntity.IsClosed = request.IsClosed;
                bookingOverrideRepo.Update(overrideEntity);
            }
            await _unitOfWork.CompleteAsync();
        }

        //Not Used Yet
        public async Task<BookingOverrideDto> UpsertBookingOverrideAsync(BookingOverrideDto request)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                DateTime dateUtc = DateTime.SpecifyKind(request.Date.Value, DateTimeKind.Utc);
                DateTime dateEgypt = TimeZoneInfo.ConvertTimeFromUtc(dateUtc, egyptZone).Date;

                var existingOverride = await _unitOfWork.Reposit<BookingOverride>()
                        .GetEntityWithSpec(new BookingOverrideByDateSpecification(dateUtc));

                BookingOverride entity;

                if (existingOverride != null)
                {
                    existingOverride.ClinicStartTime = request.ClinicStartTime;
                    existingOverride.ClinicEndTime = request.ClinicEndTime;
                    existingOverride.Date = dateUtc;
                    _unitOfWork.Reposit<BookingOverride>().Update(existingOverride);
                    entity = existingOverride;
                }
                else
                {
                    var newOverride = new BookingOverride
                    {
                        Date = dateUtc,
                        ClinicStartTime = request.ClinicStartTime,
                        ClinicEndTime = request.ClinicEndTime
                    };

                    await _unitOfWork.Reposit<BookingOverride>().AddAsync(newOverride);
                    entity = newOverride;
                }

                await _unitOfWork.CompleteAsync();


                var todaysAppointments = await _unitOfWork.Reposit<Appointment>()
                    .ListAsync(new AppointmentByDateSpecification(dateUtc));

                if (todaysAppointments.Any())
                {
                    var clinicOpenEgypt = dateEgypt.Add(request.ClinicStartTime.Value);
                    var clinicOpenUtc = TimeZoneInfo.ConvertTimeToUtc(clinicOpenEgypt, egyptZone);
                    var clinicCloseEgypt = dateEgypt.Add(request.ClinicEndTime.Value);
                    var clinicCloseUtc = TimeZoneInfo.ConvertTimeToUtc(clinicCloseEgypt, egyptZone);
                    var minutesPerCase = _settings.Value.MinutesPerCase;
                    var currentTimeUtc = DateTime.UtcNow;

                    var isCurrentDay = dateUtc.Date == DateTime.UtcNow.Date;


                    var validAppointments = todaysAppointments
                    .Where(a => a.EstimatedTime.HasValue && a.EstimatedTime.Value < clinicCloseUtc)
                    .OrderBy(a => a.EstimatedTime.Value)
                    .ToList();

                    if (validAppointments.Any())
                    {
                        var currentTimeUtcAdjusted = clinicOpenUtc;
                        int appointmentIndex = 0;

                        foreach (var appt in validAppointments.ToList())
                        {
                            if (isCurrentDay && (currentTimeUtcAdjusted > clinicCloseUtc
                                || currentTimeUtcAdjusted > currentTimeUtc))
                            {
                                if (appt.Status != AppointmentStatus.Cancelled)
                                {
                                    appt.Status = AppointmentStatus.Cancelled;
                                    await _notificationService.SendStatusChangedAsync(appt);
                                    _unitOfWork.Reposit<Appointment>().Delete(appt);
                                }
                            }
                            else
                            {
                                if (appt.EstimatedTime.Value < clinicOpenUtc)
                                {
                                    appt.EstimatedTime = currentTimeUtcAdjusted;
                                    if (appt.Status != AppointmentStatus.Rescheduled)
                                    {
                                        appt.Status = AppointmentStatus.Rescheduled;
                                        await _notificationService.SendStatusChangedAsync(appt);
                                    }
                                    _unitOfWork.Reposit<Appointment>().Update(appt);
                                }
                                currentTimeUtcAdjusted = currentTimeUtcAdjusted.AddMinutes(minutesPerCase);
                                appointmentIndex++;
                            }
                        }

                        var appointmentsToCancel = todaysAppointments
                            .Where(a => a.EstimatedTime.HasValue && (a.EstimatedTime < clinicOpenUtc
                            || (isCurrentDay && a.EstimatedTime >= clinicCloseUtc)
                            || a.EstimatedTime > currentTimeUtc))
                            .Where(a => a.Status != AppointmentStatus.Cancelled);
                        foreach (var appt in appointmentsToCancel.ToList())
                        {
                            if (appt.Status != AppointmentStatus.Cancelled)
                            {
                                appt.Status = AppointmentStatus.Cancelled;
                                await _notificationService.SendStatusChangedAsync(appt);
                                _unitOfWork.Reposit<Appointment>().Delete(appt);
                            }
                        }
                    }
                    else if (isCurrentDay && currentTimeUtc > clinicCloseUtc)
                    {
                        foreach (var appt in todaysAppointments.ToList())
                        {
                            if (appt.Status != AppointmentStatus.Cancelled)
                            {
                                appt.Status = AppointmentStatus.Cancelled;
                                await _notificationService.SendStatusChangedAsync(appt);
                                _unitOfWork.Reposit<Appointment>().Delete(appt);
                            }
                        }
                    }


                    await _unitOfWork.CompleteAsync();
                }
                await transaction.CommitAsync();
                var dto = _mapper.Map<BookingOverrideDto>(entity);
                dto.Date = TimeZoneInfo.ConvertTimeFromUtc(entity.Date.Value, egyptZone);
                return dto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Transaction failed: {ex.Message}");
                throw;
            }
        }




    }
}
