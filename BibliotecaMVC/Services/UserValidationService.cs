using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaMVC.Services
{
    public interface IUserValidationService
    {
        Task<string> CheckDuplicateEmailAsync(string email);
        string CheckDuplicatePhone(string phoneNumber);
    }

    public class UserValidationService : IUserValidationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserValidationService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

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
