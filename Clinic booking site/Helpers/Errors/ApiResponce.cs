namespace Clinic_booking_site.Helpers.Errors
{
    public class ApiResponce
    {
        public ApiResponce(int statuscode, string? statusmessage = null)
        {
            StatusCode = statuscode;
            StatusMessage = statusmessage ?? GetDefaultMessageForStatusCode();
        }

        private string? GetDefaultMessageForStatusCode()
        {
            return StatusCode switch
            {
                500 => "Internal Server Error",
                400 => "Bad Request",
                404 => "Request Not Found",
                401 => "You Are Not Authorized",
                _ => null
            };
        }

        public int StatusCode { get; set; }
        public string? StatusMessage { get; set; }
    }
}

