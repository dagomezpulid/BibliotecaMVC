using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    public class Prestamo
    {
        public int PrestamoID { get; set; }

        [Required]
        [Display(Name = "Nombre del solicitante")]
        public string NombreSolicitante { get; set; }

        [Display(Name = "Fecha de prestamo")]
        public DateTime FechaPrestamo { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de devolución")]
        public DateTime? FechaDevolucion { get; set; }

        public DateTime? FechaDevolucionReal { get; set; }

        public bool Devuelto { get; set; } = false;

        public int LibroID { get; set; }
        public Libro Libro { get; set; }
    }
}

