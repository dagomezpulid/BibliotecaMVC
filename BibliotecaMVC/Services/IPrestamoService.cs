using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de préstamos.
    /// Define las operaciones de negocio para alquilar, devolver y rastrear préstamos.
    /// </summary>
    public interface IPrestamoService
    {
        /// <summary>
        /// Obtiene la lista de préstamos activos (no devueltos) de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        Task<List<Prestamo>> GetActiveLoansAsync(string userId);

        /// <summary>
        /// Obtiene el histórico completo de préstamos de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        Task<List<Prestamo>> GetLoanHistoryAsync(string userId);

        /// <summary>
        /// Obtiene todos los préstamos registrados en el sistema (Administrativo).
        /// </summary>
        Task<List<Prestamo>> GetAllLoansAsync();
        
        /// <summary>
        /// Procesa la solicitud de un nuevo préstamo validando reglas de negocio.
        /// </summary>
        /// <param name="userId">ID del usuario que solicita.</param>
        /// <param name="libroId">ID del libro solicitado.</param>
        /// <param name="diasPrestamo">Número de días solicitados.</param>
        /// <returns>Tupla con resultado de éxito y mensaje descriptivo.</returns>
        Task<(bool Success, string Message)> ProcessLoanAsync(string userId, int libroId, int diasPrestamo);

        /// <summary>
        /// Procesa la devolución de un préstamo, calculando posibles multas.
        /// </summary>
        /// <param name="userId">ID del usuario que devuelve.</param>
        /// <param name="prestamoId">ID del préstamo a devolver.</param>
        /// <returns>Tupla con resultado de éxito y mensaje descriptivo.</returns>
        Task<(bool Success, string Message)> ProcessReturnAsync(string userId, int prestamoId);
        
        /// <summary>
        /// Recupera el progreso de lectura de un libro para un usuario específico.
        /// </summary>
        Task<ProgresoLectura?> GetReadingProgressAsync(string userId, int libroId);

        /// <summary>
        /// Guarda o actualiza la página actual de lectura de un libro.
        /// </summary>
        Task SaveReadingProgressAsync(string userId, int libroId, int pagina);
    }
}
