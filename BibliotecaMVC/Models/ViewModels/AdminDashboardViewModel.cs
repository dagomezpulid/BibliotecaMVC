using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Models.ViewModels
{
    /// <summary>
    /// Estructura de datos consolidada para alimentar las métricas y el listado 
    /// del Panel de Control Administrativo.
    /// </summary>
    public class AdminDashboardViewModel
    {
        /// <summary>Número total de usuarios registrados en el sistema.</summary>
        public int TotalUsuarios { get; set; }
        /// <summary>Número total de libros catalogados.</summary>
        public int TotalLibros { get; set; }
        /// <summary>Número total de autores registrados.</summary>
        public int TotalAutores { get; set; }
        /// <summary>Cantidad de préstamos activos en este momento.</summary>
        public int PrestamosActivos { get; set; }
        /// <summary>Suma de los montos de todas las multas sin pagar a nivel global.</summary>
        public decimal TotalMultasPendientes { get; set; }

        /// <summary>
        /// Lista de usuarios con información de perfil y roles para administración directa.
        /// </summary>
        public List<UserViewModel> Usuarios { get; set; } = new List<UserViewModel>();

        // --- DATOS PARA GRÁFICOS (CHART.JS) ---

        /// <summary>
        /// Etiquetas (Nombres) de los usuarios con mayores deudas históricas.
        /// </summary>
        public List<string> LabelsMorosos { get; set; } = new List<string>();

        /// <summary>
        /// Montos acumulados de deuda para el Top 5 de morosos.
        /// </summary>
        public List<decimal> ValoresMorosos { get; set; } = new List<decimal>();

        /// <summary>
        /// Sumatoria de días de retraso acumulados para el Top 5 de morosos.
        /// </summary>
        public List<int> ValoresDiasMora { get; set; } = new List<int>();

        /// <summary>
        /// Títulos de los libros más solicitados en el sistema.
        /// </summary>
        public List<string> LabelsLibrosPopulares { get; set; } = new List<string>();

        /// <summary>
        /// Conteo de préstamos realizados para los libros más populares.
        /// </summary>
        public List<int> ValoresLibrosPopulares { get; set; } = new List<int>();

        /// <summary>
        /// Nombres de los meses analizados para la tendencia de actividad.
        /// </summary>
        public List<string> LabelsTendencia { get; set; } = new List<string>();

        /// <summary>
        /// Cantidad de préstamos registrados por cada mes analizado.
        /// </summary>
        public List<int> ValoresTendencia { get; set; } = new List<int>();
    }
}
