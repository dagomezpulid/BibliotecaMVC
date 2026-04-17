using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Servicio centralizado para la gestión de notificaciones internas y externas (SMS).
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Crea una notificación persistente en la base de datos.
        /// </summary>
        Task CreateNotificationAsync(string userId, string titulo, string contenido, string tipo = "info");

        /// <summary>
        /// Envía un SMS al usuario si este tiene un número de teléfono registrado.
        /// </summary>
        Task SendSmsAsync(ApplicationUser usuario, string tituloLibro, string mensaje);
    }
}
