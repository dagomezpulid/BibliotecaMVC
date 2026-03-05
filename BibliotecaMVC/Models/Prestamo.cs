using System;
using BibliotecaMVC.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaMVC.Models
{
    public class Prestamo
    {
        public int Id { get; set; }

        public int LibroId { get; set; }
        public Libro? Libro { get; set; }

        public string? UsuarioId { get; set; }
        public ApplicationUser? Usuario { get; set; }

        public DateTime FechaPrestamo { get; set; } = DateTime.Now;

        public DateTime FechaDevolucionProgramada { get; set; }

        public DateTime? FechaDevolucionReal { get; set; }

        public string Estado { get; set; } = "Activo";

        public Multa? Multa { get; set; }
    }
}
