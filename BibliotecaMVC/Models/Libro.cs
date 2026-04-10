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

        [StringLength(20)]
        public string? ISBN { get; set; }

        [Display(Name = "URL de la Portada")]
        public string? ImagenUrl { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();

        public bool TieneStock => Stock > 0;
    }
}
