namespace Clinic_booking_site.Helpers.Errors
{
    public class ApiException : ApiResponce
    {
        public ApiException(int statuscode, string? statusmessage = null, String? Details = null) : base(statuscode, statusmessage)
        {
        }

        public string? Details { get; set; }
    }
}

