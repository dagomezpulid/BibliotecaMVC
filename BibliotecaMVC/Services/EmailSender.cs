using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Proveedor de servicios de correo electrónico (SMTP).
    /// Utilizado por ASP.NET Core Identity para confirmación de cuentas y recuperación de claves.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa el servicio de email inyectando la configuración de la aplicación.
        /// </summary>
        /// <param name="configuration">Acceso a appsettings y User Secrets (EmailSettings section).</param>
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Envía un correo electrónico usando el servidor SMTP configurado en appsettings/User Secrets.
        /// </summary>
        /// <param name="email">Dirección de destino.</param>
        /// <param name="subject">Asunto del correo.</param>
        /// <param name="htmlMessage">Cuerpo del correo en formato HTML.</param>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Extraer las configuraciones secretas
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"]!);
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];

            // Crear el artefacto cliente SMTP de .NET
            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            // Construir el mensaje
            var mailMessage = new MailMessage
            {
                From = new MailAddress(username!, "BibliotecaMVC Oficial"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            // Enviar el correo asincrónicamente
            await client.SendMailAsync(mailMessage);
        }
    }
}
