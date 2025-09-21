
using Clinic.Domain.Entities;
using Clinic.Repo.Data;
using Clinic_booking_site.Extinsions;
using Clinic_booking_site.Helpers.Errors;
using Clinic_booking_site.Helpers.Hangfire;
using Clinic_booking_site.Helpers.Mapping;
using Clinic_booking_site.MiddleWares;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_booking_site
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            await builder.Services.AddClinicServices(builder.Configuration);
            builder.Services.AddAutoMapper(cfg => {cfg.AddProfile<MappingProfile>(); });
            builder.Services.AddHangfireServer();
 
         


  

            //for validation error
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(e => e.Value.Errors)
                        .Select(e => e.ErrorMessage).ToArray();

                    var errorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(errorResponse);
                };
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
            });





            var app = builder.Build();




            app.UseHangfireDashboard();

            var egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

            // Schedule Recurring Jobs
            RecurringJob.AddOrUpdate<ReminderJob>(
            recurringJobId: "SendAppointmentReminders",      
            methodCall: job => job.SendRemindersAsync(),     
            cronExpression: Cron.Minutely,                   
            options: new RecurringJobOptions
            {
               TimeZone = egyptZone,                       
            },
                queue: "default"

            );

           

            //seeding admin and roles
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<Appuser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                await IdentitySeed.SeedUserAsync(userManager, roleManager);
            }


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");


            app.UseRouting();
            app.UseHttpsRedirection();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
