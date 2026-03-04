using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Models
{
    public class Multa
    {
        public int Id { get; set; }

        public int PrestamoId { get; set; }
        public Prestamo Prestamo { get; set; }

        public decimal Monto { get; set; }

        public bool Pagada { get; set; }

        public DateTime FechaGenerada { get; set; }

        public DateTime? FechaPago { get; set; }
    }
}
