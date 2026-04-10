using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Registro de una transacción económica para liquidar una multa.
    /// Almacena información de auditoría del pago realizado.
    /// </summary>
    public class Pago
    {
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario que realizó el pago.
        /// </summary>
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        /// <summary>
        /// ID de la multa que se está liquidando.
        /// </summary>
        public int MultaId { get; set; }
        public Multa? Multa { get; set; }

        /// <summary>
        /// Cantidad de dinero procesada en la transacción.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Fecha y hora exacta del procesamiento del pago.
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
