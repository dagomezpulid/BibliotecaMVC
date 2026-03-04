using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Models
{
    public class Pago
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; }
        public ApplicationUser Usuario { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        public string MetodoPago { get; set; } = "Efectivo";
    }
}
