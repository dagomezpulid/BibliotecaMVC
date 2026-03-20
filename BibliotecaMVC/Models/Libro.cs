using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    public class Libro
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El autor es obligatorio.")]
        public int? AutorId { get; set; }

        public Autor? Autor { get; set; }

        [Required(ErrorMessage = "La cantidad (stock) es obligatoria.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int? Stock { get; set; }

        public bool TieneStock => Stock > 0;
    }
}
