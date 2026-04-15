using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Representa un archivo digital individual asociado a un libro.
    /// Permite que un solo título tenga múltiples versiones o formatos (PDF, Word, etc).
    /// </summary>
    public class LibroArchivo
    {
        public int Id { get; set; }

        /// <summary>
        /// ID del libro al que pertenece este archivo.
        /// </summary>
        public int LibroId { get; set; }
        public Libro? Libro { get; set; }

        /// <summary>
        /// Nombre único del archivo almacenado en el Vault (GUID).
        /// </summary>
        [Required]
        public string Ruta { get; set; } = string.Empty;

        /// <summary>
        /// Extensión u formato del archivo (ej. .pdf, .docx).
        /// </summary>
        public string? Formato { get; set; }

        /// <summary>
        /// Fecha en la que se subió el archivo al sistema.
        /// </summary>
        public DateTime FechaCarga { get; set; } = DateTime.Now;
    }
}
