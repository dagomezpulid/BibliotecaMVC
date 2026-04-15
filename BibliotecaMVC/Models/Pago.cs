using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Representa una transacción financiera de pago de multa.
    /// Almacena datos para auditoría y validación del método de pago.
    /// </summary>
    public class Pago
    {
        public int Id { get; set; }

        /// <summary>
        /// ID de la multa que se está liquidando.
        /// </summary>
        public int MultaId { get; set; }
        public Multa? Multa { get; set; }

        /// <summary>
        /// ID del usuario que realizó la transacción.
        /// </summary>
        public string? UsuarioId { get; set; }
        public ApplicationUser? Usuario { get; set; }

        /// <summary>
        /// Cantidad exacta pagada en el momento de la transacción.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Timestamp exacto de la aprobación del pago.
        /// </summary>
        public DateTime FechaPago { get; set; } = DateTime.Now;

        /// <summary>
        /// Descripción del medio utilizado (ej. Tarjeta de Crédito, Débito).
        /// </summary>
        [StringLength(50)]
        public string MetodoPago { get; set; } = "Tarjeta de Crédito";

        /// <summary>
        /// Referencia para validación manual: Últimos 4 dígitos de la tarjeta.
        /// </summary>
        [StringLength(4)]
        public string UltimosDigitosTarjeta { get; set; } = string.Empty;
    }
}
