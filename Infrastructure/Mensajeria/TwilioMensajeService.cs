using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Infrastructure.Mensajeria
{
    public class TwilioMensajeService : IMensajeService
    {
        private readonly IConfiguration _configuration;

        public TwilioMensajeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> EnviarWhatsAppAsync(string telefono, string mensaje, CancellationToken ct = default)
        {
            var authToken = _configuration["Twilio:AuthToken"];
            var accountSid = _configuration["Twilio:AccountSid"];
            var fromNumber = _configuration["Twilio:FromNumber"];

            if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(fromNumber))
                throw new InvalidOperationException("La configuración de Twilio no está completa.");

            TwilioClient.Init(accountSid, authToken);

            var from = fromNumber.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase)
                ? fromNumber
                : $"whatsapp:{fromNumber}";

            var to = telefono.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase)
                ? telefono
                : $"whatsapp:{telefono}";

            var message = await MessageResource.CreateAsync(
                from: new Twilio.Types.PhoneNumber(from),
                body: mensaje,
                to: new Twilio.Types.PhoneNumber(to));

            return message.ErrorCode == null;
        }
    }
}