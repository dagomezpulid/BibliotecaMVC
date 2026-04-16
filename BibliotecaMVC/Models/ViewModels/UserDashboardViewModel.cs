using System.Collections.Generic;

namespace BibliotecaMVC.Models.ViewModels
{
    /// <summary>
    /// Modelo de vista para el Dashboard personal del lector.
    /// Consolida métricas de lectura, estado de cuenta y recomendaciones.
    /// </summary>
    public class UserDashboardViewModel
    {
        /// <summary>Nombre del usuario para el saludo personalizado.</summary>
        public string NombreUsuario { get; set; } = string.Empty;

        // Métricas rápidas
        public int PrestamosActivos { get; set; }
        public int LibrosLeidosTotal { get; set; }
        public decimal DeudaTotal { get; set; }
        public int FavoritosCount { get; set; }

        /// <summary>Lista de libros que el usuario está leyendo actualmente con su último progreso.</summary>
        public List<PrestamoProgresoDTO> LecturaActual { get; set; } = new();

        /// <summary>Datos para la gráfica de géneros preferidos.</summary>
        public List<string> LabelsCategorias { get; set; } = new();
        public List<int> ValoresCategorias { get; set; } = new();

        /// <summary>Libros sugeridos según los géneros más leídos del usuario.</summary>
        public List<Libro> Recomendaciones { get; set; } = new();
    }

    /// <summary>
    /// Objeto de transferencia de datos para vincular un préstamo con su progreso de lectura.
    /// </summary>
    public class PrestamoProgresoDTO
    {
        public int PrestamoId { get; set; }
        public string TituloLibro { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string Portada { get; set; } = string.Empty;
        public int PaginaActual { get; set; }
        public int PaginasTotales { get; set; } // Estimado o real
        public double Porcentaje => PaginasTotales > 0 ? (double)PaginaActual / PaginasTotales * 100 : 0;
    }
}
