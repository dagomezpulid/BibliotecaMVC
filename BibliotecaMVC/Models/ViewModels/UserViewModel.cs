using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Models.ViewModels
{
    /// <summary>
    /// Representación simplificada de un usuario para visualización en el Panel Admin.
    /// Desacopla la entidad de base de datos de la capa de vista.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>Identificador único de Identity del usuario.</summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>Nombre completo concatenado (Nombre + Apellido).</summary>
        public string NombreCompleto { get; set; } = string.Empty;
        /// <summary>Correo electrónico de la cuenta del usuario.</summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Lista de roles asignados (Admin, Usuario, etc).
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Refleja el estado BloqueadoParaPrestamos de la entidad real.
        /// </summary>
        public bool EstaBloqueado { get; set; }
    }
}

