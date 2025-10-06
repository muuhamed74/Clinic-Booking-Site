using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Microsoft.Extensions.Options;
using Service_Abstraction;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Clinic.Service.Notifications_Providers
{
    public class NewSmsProvider : IMessageProvider
    {
        private readonly string _apiUrl;
        private readonly string _token;
        private readonly string _fromNumber;

        public NewSmsProvider(IOptions<SmsSettings> settings)
        {
            _apiUrl = settings.Value.ApiUrl;
            _token = settings.Value.ApiToken;
        }

        public async Task SendAsync(string toPhoneNumber, string message , int templateId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var payload = new
            {
                name = "AmiramohsenClinic",
                phoneNumber = toPhoneNumber,
                template_id = templateId
            };

            var response = await client.PostAsJsonAsync($"{_apiUrl}/send", payload);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send SMS: {content}");
            }
        }
    }
}
