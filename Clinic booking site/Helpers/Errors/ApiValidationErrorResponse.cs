namespace Clinic_booking_site.Helpers.Errors
{
    public class ApiValidationErrorResponse : ApiResponce
    {
        public IEnumerable<string> Errors { get; set; }

        public ApiValidationErrorResponse() : base(400)
        {
        }
    }
}
