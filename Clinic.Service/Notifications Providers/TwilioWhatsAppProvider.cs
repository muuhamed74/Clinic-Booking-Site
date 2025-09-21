using System;
using System.Collections.Generic;
using System.Linq;
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
    public class TwilioWhatsAppProvider : IWhatsAppProvider
    {
        private readonly TwilioSettings _settings;

        public TwilioWhatsAppProvider(IOptions<TwilioSettings> settings)
        {
            _settings = settings.Value;
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
        }

        public async Task SendAsync(string toPhoneNumber, string message)
        {
            await MessageResource.CreateAsync(
                from: new PhoneNumber(_settings.WhatsAppFrom),
                to: new PhoneNumber($"whatsapp:{toPhoneNumber}"),
                body: message
            );
        }
    }
}
