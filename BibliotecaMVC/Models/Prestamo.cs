using System;
using BibliotecaMVC.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaMVC.Models
{
    public class Prestamo
    {
        public int Id { get; set; }

        [Required]
        public int LibroId { get; set; }

        public Libro? Libro { get; set; }

        [Required]
        [Display(Name = "Nombre del solicitante")]
        [StringLength(150)]
        public string NombreSolicitante { get; set; } = string.Empty;

        [Display(Name = "Fecha de préstamo")]
        public DateTime FechaPrestamo { get; set; } = DateTime.Now;

        [Display(Name = "Fecha límite devolución")]
        public DateTime? FechaDevolucion { get; set; }

        public DateTime? FechaDevolucionReal { get; set; }

        public bool Devuelto { get; set; }

        public int DiasRetraso { get; set; }

        public decimal? Multa { get; set; }

        public string? UsuarioId { get; set; }

        public IdentityUser? Usuario { get; set; }

        public bool TieneMulta => Multa.HasValue && Multa > 0;

        public bool EstaActivo => !Devuelto;
    }
}
