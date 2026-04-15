namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Estructura para la captura y visualización de diagnósticos de error.
    /// Utilizada por el middleware de manejo de excepciones.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// ID único de la petición que falló, útil para rastrear logs en el servidor.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// True si el RequestId tiene valor y por lo tanto es útil mostrarlo en la interfaz de error.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
