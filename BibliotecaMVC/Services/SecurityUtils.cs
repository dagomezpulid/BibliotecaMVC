using System.Text.RegularExpressions;
using System.Text;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Utilidades centralizadas para operaciones de seguridad.
    /// Incluye enmascaramiento de PII, sanitización de entrada y limpieza de nombres de archivos.
    /// </summary>
    public static class SecurityUtils
    {
        /// <summary>
        /// Enmascara un número de teléfono para proteger la privacidad en los logs.
        /// Ejemplo: +573001234567 -> +57*******4567
        /// </summary>
        public static string MaskPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return "N/A";
            
            phoneNumber = phoneNumber.Trim();
            if (phoneNumber.Length <= 4) return "****";

            // Mostrar los últimos 4 dígitos y el prefijo si tiene '+'
            string lastFour = phoneNumber.Substring(phoneNumber.Length - 4);
            if (phoneNumber.StartsWith("+"))
            {
                // Asumimos código de país corto (ej. +57)
                string prefix = phoneNumber.Substring(0, 3); 
                return $"{prefix}*******{lastFour}";
            }

            return $"*******{lastFour}";
        }

        /// <summary>
        /// Sanitiza una cadena para ser utilizada de forma segura como nombre de archivo.
        /// </summary>
        public static string SanitizeFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "archivo_sin_nombre";

            // Reemplazar caracteres no permitidos
            string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()) + " ");
            string sanitized = Regex.Replace(fileName, "[" + invalidChars + "]", "_");

            // Limitar longitud para evitar problemas en algunos sistemas de archivos
            if (sanitized.Length > 100) sanitized = sanitized.Substring(0, 100);

            return sanitized;
        }

        /// <summary>
        /// Sanitiza un texto eliminando etiquetas HTML potencialmente peligrosas.
        /// </summary>
        public static string SanitizeHtml(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // En un entorno productivo real se usaría una librería como HtmlSanitizer.
            // Para este proyecto, realizamos un escape básico para prevenir ejecución de scripts.
            return System.Net.WebUtility.HtmlEncode(input);
        }
    }
}
