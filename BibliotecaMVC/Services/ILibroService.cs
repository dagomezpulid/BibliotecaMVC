using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Http;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión del catálogo de libros.
    /// Define operaciones para búsqueda, gestión de archivos y recomendaciones.
    /// </summary>
    public interface ILibroService
    {
        /// <summary>
        /// Obtiene una lista paginada de libros basada en una consulta de búsqueda.
        /// </summary>
        /// <param name="query">Texto de búsqueda (título, autor, ISBN, categoría).</param>
        /// <param name="page">Número de página actual.</param>
        /// <param name="pageSize">Cantidad de elementos por página.</param>
        /// <returns>Tupla con la lista de libros y el total de páginas.</returns>
        Task<(List<Libro> Libros, int TotalPages)> GetPagedLibrosAsync(string? query, int page, int pageSize);

        /// <summary>
        /// Obtiene los detalles técnicos y archivos de un libro específico.
        /// </summary>
        /// <param name="id">ID del libro.</param>
        Task<Libro?> GetLibroDetailsAsync(int id);

        /// <summary>
        /// Obtiene una lista de libros recomendados basados en autor y categorías.
        /// </summary>
        /// <param name="currentLibroId">ID del libro actual (para excluirlo).</param>
        /// <param name="autorId">ID del autor para recomendaciones por autor.</param>
        /// <param name="categoriaIds">Lista de IDs de categorías para recomendaciones temáticas.</param>
        Task<List<Libro>> GetRecommendedLibrosAsync(int currentLibroId, int? autorId, List<int> categoriaIds);
        
        /// <summary>
        /// Crea un nuevo título en el catálogo y procesa la carga de archivos al Vault.
        /// </summary>
        /// <param name="libro">Entidad con los metadatos del libro.</param>
        /// <param name="categoriaIds">IDs de las categorías asociadas.</param>
        /// <param name="archivos">Colección de archivos digitales subidos.</param>
        Task<(bool Success, string ErrorMessage)> CreateLibroAsync(Libro libro, int[] categoriaIds, IFormFileCollection archivos);

        /// <summary>
        /// Actualiza los metadatos de un libro y permite añadir nuevos archivos.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateLibroAsync(Libro libro, int[] categoriaIds, IFormFileCollection nuevosArchivos);

        /// <summary>
        /// Elimina un libro del catálogo y sus archivos físicos asociados en el Vault.
        /// </summary>
        /// <param name="id">ID del libro a eliminar.</param>
        Task<bool> DeleteLibroAsync(int id);
        
        /// <summary>
        /// Registra una nueva reseña y calificación de un usuario para un libro.
        /// </summary>
        Task PostResenaAsync(int libroId, string userId, int puntuacion, string comentario);
    }
}
