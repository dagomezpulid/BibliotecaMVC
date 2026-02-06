using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    public class Autor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del autor es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Libro> Libros { get; set; } = new List<Libro>();
    }
}

