using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Clinic.Domain.Entities;
using Microsoft.Extensions.Options;
using Service_Abstraction;

namespace Clinic.Service.Notifications_Providers
{
    public class MetaWhatsAppProvider : IWhatsAppProvider
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly WhatsAppSettings _settings;

        public MetaWhatsAppProvider(IHttpClientFactory clientFactory, IOptions<WhatsAppSettings> whatsAppConfig)
        {
            _clientFactory = clientFactory;
            _settings = whatsAppConfig.Value;
        }

        public async Task SendAsync(string toPhoneNumber, string message)
        {
            var client = _clientFactory.CreateClient();
            var url = $"{_settings.BaseUrl}/{_settings.PhoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "text",
                text = new { body = message }
            };

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.AccessToken);

            await client.PostAsJsonAsync(url, payload);
        }
    }
}
