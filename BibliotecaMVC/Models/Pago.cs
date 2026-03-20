using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    public class Pago
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        public int MultaId { get; set; }
        public Multa? Multa { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string MetodoPago { get; set; } = "Tarjeta de Crédito";

        [StringLength(4)]
        public string UltimosDigitosTarjeta { get; set; } = string.Empty;
    }
}
