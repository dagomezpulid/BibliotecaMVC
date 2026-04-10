using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Perfil de usuario extendido basado en ASP.NET Core Identity.
    /// Almacena datos personales y estados de sanción administrativa.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Primer nombre del lector.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido paterno/materno del lector.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Bandera de seguridad: Si es true, el usuario no puede realizar nuevos préstamos
        /// debido a deudas o devoluciones tardías persistentes.
        /// </summary>
        public bool BloqueadoParaPrestamos { get; set; } = false;

        /// <summary>
        /// Atributo de lectura que concatena nombre y apellido.
        /// </summary>
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
