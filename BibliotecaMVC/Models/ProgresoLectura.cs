using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Almacena el estado de lectura de un usuario para un libro específico.
    /// Esto permite la funcionalidad de "Continuidad de Lectura", recordando la última página.
    /// </summary>
    public class ProgresoLectura
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser? Usuario { get; set; }

        [Required]
        public int LibroId { get; set; }

        [ForeignKey("LibroId")]
        public virtual Libro? Libro { get; set; }

        /// <summary>
        /// La última página visualizada por el usuario. 
        /// Para PDFs es el número de página, para otros formatos puede ser un porcentaje.
        /// </summary>
        [Required]
        public int PaginaActual { get; set; } = 1;

        /// <summary>
        /// Marca de tiempo de la última vez que se guardó el progreso.
        /// </summary>
        public DateTime UltimoAcceso { get; set; } = DateTime.Now;

        /// <summary>
        /// Metadata adicional (ej: zoom, preferencias de visualización).
        /// </summary>
        public string? Metadata { get; set; }
    }
}
