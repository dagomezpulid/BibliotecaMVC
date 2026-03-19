using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalLibros { get; set; }
        public int TotalAutores { get; set; }
        public int PrestamosActivos { get; set; }
        public decimal TotalMultasPendientes { get; set; }
        public List<UserViewModel> Usuarios { get; set; } = new List<UserViewModel>();
    }
}
