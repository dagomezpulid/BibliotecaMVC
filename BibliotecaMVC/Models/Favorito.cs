using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Representa la relación de interés de un usuario por un libro específico.
    /// Utilizado para la funcionalidad de Wishlist / Favoritos.
    /// </summary>
    public class Favorito
    {
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario que agregó el libro a favoritos.
        /// </summary>
        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        /// <summary>
        /// ID del libro marcado como favorito.
        /// </summary>
        [Required]
        public int LibroId { get; set; }
        public Libro? Libro { get; set; }

        /// <summary>
        /// Fecha y hora en que el usuario agregó el libro a su lista de favoritos.
        /// </summary>
        public DateTime FechaAgregado { get; set; } = DateTime.Now;
    }
}
