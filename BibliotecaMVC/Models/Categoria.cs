using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Clasificación temática para organizar los libros (ej: Ficción, Ciencia).
    /// </summary>
    public class Categoria
    {
        public int Id { get; set; }

        /// <summary>
        /// Etiqueta descriptiva de la categoría.
        /// </summary>
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Colección de libros que pertenecen a esta categoría (Relación Muchos a Muchos).
        /// </summary>
        public ICollection<Libro> Libros { get; set; } = new List<Libro>();
    }
}
