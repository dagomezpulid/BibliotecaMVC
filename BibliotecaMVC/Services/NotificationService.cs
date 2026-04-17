using BibliotecaMVC.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using BibliotecaMVC.Hubs;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Implementación del servicio de notificaciones.
    /// Gestiona alertas internas, despacho de SMS y comunicaciones en tiempo real vía SignalR.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly BibliotecaContext _context;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        /// <summary>
        /// Inicializa el servicio con sus dependencias.
        /// </summary>
        public NotificationService(
            BibliotecaContext context, 
            ISmsSender smsSender, 
            ILogger<NotificationService> logger,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _smsSender = smsSender;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <inheritdoc />
        public async Task CreateNotificationAsync(string userId, string titulo, string contenido, string tipo = "info")
        {
            try
            {
                var notif = new Notificacion
                {
                    UsuarioId = userId,
                    Titulo = titulo,
                    Contenido = contenido,
                    Tipo = tipo,
                    FechaCreacion = DateTime.Now,
                    Leida = false
                };
                _context.Notificaciones.Add(notif);
                await _context.SaveChangesAsync();
                
                // 🚀 Enviar notificación en tiempo real vía SignalR
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new {
                    id = notif.Id,
                    titulo = notif.Titulo,
                    contenido = notif.Contenido,
                    tipo = notif.Tipo,
                    fechaCreacion = notif.FechaCreacion,
                    leida = notif.Leida
                });

                _logger.LogInformation("Notificación interna creada y enviada vía SignalR para usuario {UserId}: {Titulo}", userId, titulo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación interna para usuario {UserId}", userId);
            }
        }

        /// <inheritdoc />
        public async Task SendSmsAsync(ApplicationUser usuario, string tituloLibro, string mensaje)
        {
            if (usuario != null && !string.IsNullOrEmpty(usuario.PhoneNumber))
            {
                try
                {
                    string smsBody = $"BibliotecaMVC: {mensaje} (Libro: '{tituloLibro}').";
                    await _smsSender.SendSmsAsync(usuario.PhoneNumber, smsBody);
                    _logger.LogInformation("SMS enviado a {PhoneNumber} para el libro {TituloLibro}", usuario.PhoneNumber, tituloLibro);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar SMS a {PhoneNumber}", usuario.PhoneNumber);
                }
            }
            else
            {
                _logger.LogWarning("No se envió SMS: El usuario no tiene número de teléfono o es nulo.");
            }
        }
    }
}
