using System;
using BibliotecaMVC.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Gestiona el ciclo de vida de la renta de un libro por un usuario.
    /// Controla fechas de entrega, moras y estados de devolución.
    /// </summary>
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

        /// <summary>
        /// Estado actual: "Activo", "Devuelto", "Perdido".
        /// </summary>
        public string Estado { get; set; } = "Activo";

        /// <summary>
        /// Entidad de multa asociada si la devolución fue tardía (Relación 1:1).
        /// </summary>
        public Multa? Multa { get; set; }

        /// <summary>
        /// Determina si ya se envió el mensaje de alerta vía SMS/Email para evitar spam.
        /// </summary>
        public bool AlertaMoraEnviada { get; set; } = false;

        /// <summary>
        /// Lógica de negocio: Verdadero si no se ha devuelto y ya pasó la fecha programada.
        /// </summary>
        public bool EstaVencido => FechaDevolucionReal == null && DateTime.Now > FechaDevolucionProgramada;
        
        public int DiasMora 
        {
            get 
            {
                var fin = FechaDevolucionReal ?? DateTime.Now;
                return fin > FechaDevolucionProgramada ? (fin - FechaDevolucionProgramada).Days : 0;
            }
        }
    }
}
