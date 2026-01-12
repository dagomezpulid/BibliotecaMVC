using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC
{
    public class Prestamo
    {
        public int PrestamoID { get; set; }

        [Display(Name = "Nombre del Solicitante")]
        public string NombreSolicitante { get; set; }

        [Display(Name = "Fecha de Préstamo")]
        public DateTime FechaPrestamo { get; set; }

        [Display(Name = "Fecha de Devolución")]
        public DateTime? FechaDevolucion { get; set; }

        public int LibroID { get; set; }
        public Libro Libro { get; set; }
    }
}

