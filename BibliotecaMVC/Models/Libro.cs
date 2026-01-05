using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaMVC.Models
{
    public class Libro
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un autor")]
        public int? AutorID { get; set; }

        [ForeignKey("AutorID")]
        public Autor? Autor { get; set; }
    }
}


