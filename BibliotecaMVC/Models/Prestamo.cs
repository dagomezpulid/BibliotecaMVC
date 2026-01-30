using System;
using BibliotecaMVC.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace BibliotecaMVC.Models
{
    public class Prestamo
    {
        public int PrestamoID { get; set; }

        public int LibroID { get; set; }

        public Libro Libro { get; set; }

        [Required]
        [Display(Name = "Nombre del solicitante")]
        public string NombreSolicitante { get; set; }

        [Display(Name = "Fecha de prestamo")]
        public DateTime FechaPrestamo { get; set; }

        [Display(Name = "Fecha de devolución")]
        public DateTime? FechaDevolucion { get; set; }

        public DateTime? FechaDevolucionReal { get; set; }

        public bool Devuelto { get; set; }

        public int DiasRetraso { get; set; }

        public decimal? Multa { get; set; }

        public string? UsuarioId { get; set; }

        public IdentityUser? Usuario { get; set; }
    }
}
