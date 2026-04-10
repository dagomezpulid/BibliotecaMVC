using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Representa a un creador literario dentro del sistema.
    /// </summary>
    public class Autor
    {
        public int Id { get; set; }

        /// <summary>
        /// Nombre completo o pseudónimo legal del autor.
        /// </summary>
        [Required(ErrorMessage = "El nombre del autor es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Relación de navegación: Libros que han sido vinculados a este autor (Relación 1:N).
        /// </summary>
        public ICollection<Libro> Libros { get; set; } = new List<Libro>();
    }
}

