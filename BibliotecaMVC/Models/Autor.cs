using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    public class Autor
    {
        [Key]
        public int AutorID { get; set; }

        [Required]
        public string Nombre { get; set; }

        public ICollection<Libro>? Libros { get; set; }
    }

}

