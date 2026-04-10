namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Contrato para servicios de mensajería SMS/WhatsApp.
    /// Define la capacidad de enviar notificaciones cortas a dispositivos móviles.
    /// </summary>
    public interface ISmsSender
    {
        /// <summary>
        /// Envía un mensaje de texto de forma asíncrona.
        /// </summary>
        /// <param name="number">Número telefónico receptor.</param>
        /// <param name="message">Contenido del mensaje.</param>
        Task SendSmsAsync(string number, string message);
    }
}
