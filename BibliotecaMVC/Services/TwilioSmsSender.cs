using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BibliotecaMVC.Services
{
    public class TwilioSmsSender : ISmsSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TwilioSmsSender> _logger;

        public TwilioSmsSender(IConfiguration config, ILogger<TwilioSmsSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendSmsAsync(string number, string message)
        {
            if (string.IsNullOrWhiteSpace(number)) 
            {
                _logger.LogWarning("Abortando SMS: El usuario no cuenta con un número de teléfono telefónico registrado.");
                return;
            }

            var accountSid = _config["TwilioSettings:AccountSid"];
            var authToken = _config["TwilioSettings:AuthToken"];
            var twilioPhoneNumber = _config["TwilioSettings:FromPhoneNumber"];

            // Modo Mock/Desarrollo Activo para probar hasta que el usuario reemplace sus variables.
            if (string.IsNullOrEmpty(accountSid) || accountSid.Contains("TU_SID_TWILIO"))
            {
                _logger.LogWarning($"\n========== TICKET SMS PREVENTIVO A {number} ==========\n{message}\n=========================================\n");
                return;
            }

            try
            {
                // Validación E.164: Inyectar +57 (Colombia)
                string safeNumber = number.Trim();
                if (!safeNumber.StartsWith("+"))
                {
                    safeNumber = $"+57{safeNumber}";
                }

                // 🔥 CONVERSIÓN OFICIAL DEL DRIVER A WHATSAPP
                string wTarget = $"whatsapp:{safeNumber}";
                string wSender = $"whatsapp:{twilioPhoneNumber}";

                TwilioClient.Init(accountSid, authToken);

                var messageOptions = new CreateMessageOptions(new PhoneNumber(wTarget))
                {
                    From = new PhoneNumber(wSender),
                    Body = message
                };

                var resource = await MessageResource.CreateAsync(messageOptions);
                _logger.LogInformation($"[WHATSAPP TWILIO] Mensaje despachado exitosamente hacia {wTarget} => Código ID: {resource.Sid}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fallo crítico en el despachador de Twilio hacia: {number}");
            }
        }
    }
}
