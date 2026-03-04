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

        [Display(Name = "Fecha de préstamo")]
        public DateTime FechaPrestamo { get; set; } = DateTime.Now;

        [Display(Name = "Fecha límite devolución")]
        public DateTime? FechaDevolucion { get; set; }

        public DateTime? FechaDevolucionReal { get; set; }

        public bool Devuelto { get; set; }

        public int DiasRetraso { get; set; }

        public decimal? Multa { get; set; }

        public string? UsuarioId { get; set; }

        public ApplicationUser? Usuario { get; set; }

        public bool TieneMulta => Multa.HasValue && Multa > 0;

        public bool EstaActivo => !Devuelto;

        public Multa? Multas { get; set; }

        public DateTime FechaDevolucionProgramada { get; set; }
        public string Estado { get; set; } = "Activo";
    }
}
