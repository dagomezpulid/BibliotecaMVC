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

        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        [Required]
        public int LibroId { get; set; }
        public Libro? Libro { get; set; }

        public DateTime FechaAgregado { get; set; } = DateTime.Now;
    }
}
