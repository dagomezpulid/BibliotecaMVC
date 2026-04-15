using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Contrato para el servicio de validación de credenciales de usuario.
    /// </summary>
    public interface IUserValidationService
    {
        /// <summary>Verifica si el email ya está registrado en el sistema.</summary>
        /// <param name="email">Correo electrónico a comprobar.</param>
        /// <returns>Mensaje de error si existe, null si es válido.</returns>
        Task<string> CheckDuplicateEmailAsync(string email);

        /// <summary>Verifica si el teléfono ya está en uso por otra cuenta.</summary>
        /// <param name="phoneNumber">Número de teléfono a comprobar.</param>
        /// <returns>Mensaje de error si existe, null si es válido.</returns>
        string CheckDuplicatePhone(string phoneNumber);
    }

    /// <summary>
    /// Servicio centralizado de validación de identidad.
    /// Encargado de verificar la unicidad de credenciales (Email/Teléfono) antes del registro.
    /// </summary>
    public class UserValidationService : IUserValidationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserValidationService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Verifica si el email ya está registrado en el sistema.
        /// </summary>
        /// <param name="email">Correo electrónico a verificar.</param>
        /// <returns>Mensaje de error localizado si existe un duplicado, null si el email está disponible.</returns>
        public async Task<string> CheckDuplicateEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null)
            {
                return "Ya existe una cuenta registrada con este correo electrónico.";
            }
            return null;
        }

        /// <summary>
        /// Verifica si el número de teléfono ya está en uso dentro del sistema.
        /// </summary>
        /// <param name="phoneNumber">Número de teléfono a verificar.</param>
        /// <returns>Mensaje de error localizado si existe un duplicado, null si el teléfono está disponible.</returns>
        public string CheckDuplicatePhone(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;

            var existingPhone = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
            if (existingPhone != null)
            {
                return "Ya existe una cuenta registrada con este número telefónico.";
            }
            return null;
        }
    }
}
