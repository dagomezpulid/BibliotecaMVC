using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Entidad para gestionar comunicaciones internas y alertas del sistema hacia los usuarios.
    /// Permite el seguimiento de mensajes leídos/no leídos de forma persistente.
    /// </summary>
    public class Notificacion
    {
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario destinatario.
        /// </summary>
        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        /// <summary>
        /// Encabezado breve de la notificación (Ej: "Multa Generada").
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Cuerpo detallado del mensaje.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Contenido { get; set; } = string.Empty;

        /// <summary>
        /// Marca temporal de creación.
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Estado de visualización por parte del usuario.
        /// </summary>
        public bool Leida { get; set; } = false;

        /// <summary>
        /// Opcional: Icono o tipo de notificación (Info, Warning, Success).
        /// </summary>
        public string Tipo { get; set; } = "info";
    }
}
