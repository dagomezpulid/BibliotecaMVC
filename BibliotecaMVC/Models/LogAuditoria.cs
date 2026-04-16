using System;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Entidad de auditoría para el seguimiento de acciones críticas en el sistema.
    /// Registra el quién, qué y cuándo de las interacciones con activos digitales.
    /// </summary>
    public class LogAuditoria
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Identificador del usuario que realizó la acción.</summary>
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>Navegación al usuario (Virtual para EF).</summary>
        public virtual ApplicationUser? Usuario { get; set; }

        /// <summary>Descripción de la acción realizada (ej: 'Descarga PDF', 'Lectura Online').</summary>
        [Required]
        [StringLength(100)]
        public string Accion { get; set; } = string.Empty;

        /// <summary>Identificador del recurso afectado (ej: ID del Libro).</summary>
        public string? RecursoId { get; set; }

        /// <summary>Detalles adicionales en formato texto o JSON.</summary>
        public string? Detalles { get; set; }

        /// <summary>Marca de tiempo exacta de la transacción.</summary>
        public DateTime Fecha { get; set; } = DateTime.Now;

        /// <summary>Dirección IP desde la cual se originó la petición.</summary>
        public string? IpAddress { get; set; }
    }
}
