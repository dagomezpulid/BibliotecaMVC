using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Representa una calificación y comentario realizado por un lector sobre un libro.
    /// Permite cuantificar la satisfacción del usuario mediante un sistema de 1 a 5 estrellas.
    /// </summary>
    public class Resena
    {
        public int Id { get; set; }

        /// <summary>
        /// ID del libro calificado.
        /// </summary>
        [Required]
        public int LibroId { get; set; }
        public Libro? Libro { get; set; }

        /// <summary>
        /// ID del usuario que emite la opinión.
        /// </summary>
        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        /// <summary>
        /// Puntuación numérica obligatoria entre 1 y 5.
        /// </summary>
        [Required]
        [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5 estrellas.")]
        public int Puntuacion { get; set; }

        /// <summary>
        /// Opinión textual sobre la obra.
        /// </summary>
        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 1000 caracteres.")]
        [DataType(DataType.MultilineText)]
        public string Comentario { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora en que se publicó la reseña.
        /// </summary>
        public DateTime FechaPublicacion { get; set; } = DateTime.Now;
    }
}
