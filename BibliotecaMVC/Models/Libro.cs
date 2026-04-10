using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Representa un título o libro dentro del catálogo de la biblioteca.
    /// Contiene metadatos técnicos y visuales, además de la relación con autores y categorías.
    /// </summary>
    public class Libro
    {
        public int Id { get; set; }

        /// <summary>
        /// Título oficial de la obra.
        /// </summary>
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del autor (Relación 1:N).
        /// </summary>
        [Required(ErrorMessage = "El autor es obligatorio.")]
        public int? AutorId { get; set; }

        public Autor? Autor { get; set; }

        /// <summary>
        /// Cantidad física o digital disponible de copias para préstamo.
        /// </summary>
        [Required(ErrorMessage = "La cantidad (stock) es obligatoria.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int? Stock { get; set; }

        /// <summary>
        /// Identificador universal de edición. Ayuda a evitar duplicados y automatizar metadata.
        /// </summary>
        [StringLength(20)]
        public string? ISBN { get; set; }

        /// <summary>
        /// Enlace externo a la imagen de portada (Google Books, Amazon, etc).
        /// </summary>
        [Display(Name = "URL de la Portada")]
        public string? ImagenUrl { get; set; }

        /// <summary>
        /// Resumen o sinopsis del contenido del libro.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Lista de categorías asociadas (Relación Muchos a Muchos).
        /// </summary>
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();

        /// <summary>
        /// Propiedad calculada que determina si existen unidades disponibles para prestar.
        /// </summary>
        public bool TieneStock => Stock > 0;
    }
}
