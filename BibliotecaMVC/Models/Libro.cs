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
        [RegularExpression(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$", ErrorMessage = "El formato del ISBN no es válido. Use 10 o 13 dígitos.")]
        // El Regex anterior permite guiones y valida que existan exactamente 10 o 13 dígitos numéricos
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
        /// Lista de reseñas y calificaciones otorgadas por los usuarios.
        /// </summary>
        public ICollection<Resena> Resenas { get; set; } = new List<Resena>();

        /// <summary>
        /// Calcula el promedio de estrellas redondeado a un decimal.
        /// </summary>
        public double RatingPromedio => Resenas.Any() ? Math.Round(Resenas.Average(r => r.Puntuacion), 1) : 0;

        /// <summary>
        /// Propiedad calculada que determina si existen unidades disponibles para prestar.
        /// </summary>
        public bool TieneStock => Stock > 0;

        /// <summary>
        /// Propiedad volatil para indicar si el usuario actual tiene este libro en favoritos.
        /// No se persiste en DB, se llena en tiempo de ejecución.
        /// </summary>
        /// <summary>
        /// Propiedad volatil para indicar si el usuario actual tiene este libro en favoritos.
        /// No se persiste en DB, se llena en tiempo de ejecución.
        /// </summary>
        [NotMapped]
        public bool EsFavorito { get; set; } = false;

        /// <summary>
        /// Ruta física o URL local donde se encuentra el archivo principal del libro digital (PDF, EPUB, DOCX).
        /// </summary>
        [Display(Name = "Archivo del Libro")]
        public string? ArchivoRuta { get; set; }

        /// <summary>
        /// Token de concurrencia para evitar colisiones en el stock (Optimistic Concurrency).
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
