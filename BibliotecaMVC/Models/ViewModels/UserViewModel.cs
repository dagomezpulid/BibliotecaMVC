using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EsAdmin { get; set; }
    }
}
