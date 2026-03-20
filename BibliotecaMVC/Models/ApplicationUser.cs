using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        public bool BloqueadoParaPrestamos { get; set; } = false;

        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
