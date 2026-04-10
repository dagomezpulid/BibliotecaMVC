using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Models.ViewModels
{
    /// <summary>
    /// Estructura de datos consolidada para alimentar las métricas y el listado 
    /// del Panel de Control Administrativo.
    /// </summary>
    public class AdminDashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalLibros { get; set; }
        public int TotalAutores { get; set; }
        public int PrestamosActivos { get; set; }
        public decimal TotalMultasPendientes { get; set; }

        /// <summary>
        /// Lista de usuarios con información de perfil y roles para administración directa.
        /// </summary>
        public List<UserViewModel> Usuarios { get; set; } = new List<UserViewModel>();

        // --- DATOS PARA GRÁFICOS (CHART.JS) ---
        
        // Usuarios con más mora (Top 5)
        public List<string> LabelsMorosos { get; set; } = new List<string>();
        public List<decimal> ValoresMorosos { get; set; } = new List<decimal>();

        // Libros más prestados (Top 5)
        public List<string> LabelsLibrosPopulares { get; set; } = new List<string>();
        public List<int> ValoresLibrosPopulares { get; set; } = new List<int>();

        // Tendencia de préstamos (Meses)
        public List<string> LabelsTendencia { get; set; } = new List<string>();
        public List<int> ValoresTendencia { get; set; } = new List<int>();
    }
}
