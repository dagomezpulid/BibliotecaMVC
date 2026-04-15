using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Representa una sanción financiera aplicada a un préstamo devuelto tarde.
    /// Solo puede existir una multa por préstamo.
    /// </summary>
    public class Multa
    {
        public int Id { get; set; }

        /// <summary>
        /// Identificador del préstamo que originó la multa.
        /// </summary>
        public int PrestamoId { get; set; }
        public Prestamo? Prestamo { get; set; }

        /// <summary>
        /// Cantidad total a pagar calculada en base a los días de mora.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Indica si la penalización ya fue liquidada económicamente.
        /// </summary>
        public bool Pagada { get; set; }

        /// <summary>
        /// Fecha en que se detectó la mora y se creó el registro.
        /// </summary>
        public DateTime FechaGenerada { get; set; }

        /// <summary>
        /// Fecha real en que el usuario realizó el pago.
        /// </summary>
        public DateTime? FechaPago { get; set; }
    }
}
