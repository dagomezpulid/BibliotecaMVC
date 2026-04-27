using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de libros.
    /// Maneja la persistencia en base de datos y el almacenamiento físico en el Vault.
    /// </summary>
    public class LibroService : ILibroService
    {
        private readonly BibliotecaContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LibroService> _logger;
        private readonly string[] _allowedExtensions = { ".pdf", ".epub", ".docx", ".txt" };

        /// <summary>
        /// Inicializa el servicio inyectando el contexto de base de datos, el entorno de hosting para manejo de archivos
        /// y el sistema de logging para auditoría de errores.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework Core.</param>
        /// <param name="env">Entorno de ejecución (usado para rutas físicas de archivos).</param>
        /// <param name="logger">Instancia de Logger para registro de eventos técnicos.</param>
        public LibroService(BibliotecaContext context, IWebHostEnvironment env, ILogger<LibroService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<(List<Libro> Libros, int TotalPages)> GetPagedLibrosAsync(string? query, int page, int pageSize)
        {
            var librosQuery = _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .Include(l => l.Archivos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var keywords = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in keywords)
                {
                    librosQuery = librosQuery.Where(l =>
                        (l.Titulo != null && l.Titulo.ToLower().Contains(word)) ||
                        (l.Autor != null && l.Autor.Nombre.ToLower().Contains(word)) ||
                        l.Categorias.Any(c => c.Nombre.ToLower().Contains(word)) ||
                        (l.ISBN != null && l.ISBN.Contains(word))
                    );
                }
            }

            int totalLibros = await librosQuery.CountAsync();
            var libros = await librosQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var libroIds = libros.Select(l => l.Id).ToList();
            var ratings = await _context.Resenas
                .Where(r => libroIds.Contains(r.LibroId))
                .GroupBy(r => r.LibroId)
                .Select(g => new { LibroId = g.Key, Avg = g.Average(r => r.Puntuacion) })
                .ToDictionaryAsync(x => x.LibroId, x => (double)x.Avg);

            foreach (var l in libros)
            {
                l.RatingCalculadoEager = ratings.ContainsKey(l.Id) ? Math.Round(ratings[l.Id], 1) : 0;
            }

            return (libros, (int)Math.Ceiling(totalLibros / (double)pageSize));
        }

        /// <inheritdoc />
        public async Task<Libro?> GetLibroDetailsAsync(int id)
        {
            return await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .Include(l => l.Resenas).ThenInclude(r => r.Usuario)
                .Include(l => l.Archivos)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <inheritdoc />
        public async Task<List<Libro>> GetRecommendedLibrosAsync(int currentLibroId, int? autorId, List<int> categoriaIds)
        {
            return await _context.Libros
                .Include(l => l.Autor)
                .Where(l => l.Id != currentLibroId && (l.AutorId == autorId || l.Categorias.Any(c => categoriaIds.Contains(c.Id))))
                .OrderByDescending(l => l.Resenas.Average(r => (double?)r.Puntuacion) ?? 0)
                .Take(8)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<(bool Success, string ErrorMessage)> CreateLibroAsync(Libro libro, int[] categoriaIds, IFormFileCollection archivos)
        {
            try
            {
                if (await _context.Libros.AnyAsync(l => l.Titulo.ToLower() == libro.Titulo.ToLower()))
                    return (false, "El título ya está registrado en el catálogo.");

                if (categoriaIds != null)
                {
                    foreach (var id in categoriaIds)
                    {
                        var cat = await _context.Categorias.FindAsync(id);
                        if (cat != null) libro.Categorias.Add(cat);
                    }
                }

                if (archivos != null && archivos.Count > 0)
                {
                    var vaultFolder = GetVaultPath();
                    foreach (var file in archivos)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLower();

                        // SEGURIDAD: Validación de lista blanca (Whitelist)
                        // Evita la subida de archivos maliciosos (ej. .exe, .asp, .php) que podrían comprometer el servidor.
                        if (!_allowedExtensions.Contains(extension))
                            return (false, $"El formato {extension} no está permitido. Use: PDF, EPUB o DOCX.");

                        // SEGURIDAD: Nombre de archivo aleatorio
                        // Se usa un GUID para evitar ataques de colisión de nombres y ataques de "Directory Traversal".
                        var uniqueName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(vaultFolder, uniqueName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        libro.Archivos.Add(new LibroArchivo { Ruta = uniqueName, Formato = extension });
                    }
                }

                _context.Libros.Add(libro);
                await _context.SaveChangesAsync();
                return (true, "Libro creado exitosamente.");
            }
            catch (Exception ex)
            {
                // SEGURIDAD: Error Genérico
                // No devolvemos ex.Message para evitar "Information Disclosure" (revelar rutas o nombres de tablas).
                _logger.LogError(ex, "Error al crear libro: {Titulo}", libro.Titulo);
                return (false, "Ocurrió un error interno en el servidor al intentar crear el libro.");
            }
        }

        /// <inheritdoc />
        public async Task<(bool Success, string ErrorMessage)> UpdateLibroAsync(Libro libro, int[] categoriaIds, IFormFileCollection nuevosArchivos)
        {
            try
            {
                var libroToUpdate = await _context.Libros
                    .Include(l => l.Categorias)
                    .Include(l => l.Archivos)
                    .FirstOrDefaultAsync(l => l.Id == libro.Id);

                if (libroToUpdate == null) return (false, "El libro no fue encontrado en la base de datos.");

                libroToUpdate.Titulo = libro.Titulo;
                libroToUpdate.AutorId = libro.AutorId;
                libroToUpdate.ISBN = libro.ISBN;
                libroToUpdate.ImagenUrl = libro.ImagenUrl;
                libroToUpdate.Descripcion = libro.Descripcion;

                libroToUpdate.Categorias.Clear();
                if (categoriaIds != null)
                {
                    foreach (var catId in categoriaIds)
                    {
                        var cat = await _context.Categorias.FindAsync(catId);
                        if (cat != null) libroToUpdate.Categorias.Add(cat);
                    }
                }

                if (nuevosArchivos != null && nuevosArchivos.Count > 0)
                {
                    var vaultFolder = GetVaultPath();

                    foreach (var viejo in libroToUpdate.Archivos.ToList())
                    {
                        var oldPath = Path.Combine(vaultFolder, viejo.Ruta);
                        if (File.Exists(oldPath)) File.Delete(oldPath);
                        _context.Set<LibroArchivo>().Remove(viejo);
                    }
                    libroToUpdate.Archivos.Clear();

                    foreach (var file in nuevosArchivos)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLower();

                        if (!_allowedExtensions.Contains(extension))
                            return (false, $"El formato {extension} no está permitido.");

                        var uniqueName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(vaultFolder, uniqueName);

                        using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
                        libroToUpdate.Archivos.Add(new LibroArchivo { Ruta = uniqueName, Formato = extension });
                    }
                }

                _context.Update(libroToUpdate);
                await _context.SaveChangesAsync();
                return (true, "Libro actualizado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar libro ID {LibroId}", libro.Id);
                return (false, "Ocurrió un error interno al intentar actualizar los datos del libro.");
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteLibroAsync(int id)
        {
            try
            {
                var libro = await _context.Libros.Include(l => l.Archivos).FirstOrDefaultAsync(l => l.Id == id);
                if (libro == null) return false;

                var vaultFolder = GetVaultPath();
                foreach (var archivo in libro.Archivos)
                {
                    var filePath = Path.Combine(vaultFolder, archivo.Ruta);
                    if (File.Exists(filePath)) File.Delete(filePath);
                }

                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar libro ID {LibroId}", id);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task PostResenaAsync(int libroId, string userId, int puntuacion, string comentario)
        {
            var resena = new Resena
            {
                LibroId = libroId,
                UsuarioId = userId,
                Puntuacion = puntuacion,
                Comentario = comentario,
                FechaPublicacion = DateTime.Now
            };
            _context.Resenas.Add(resena);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene y asegura la existencia de la ruta del Vault de libros digitales.
        /// </summary>
        private string GetVaultPath()
        {
            var path = Path.Combine(_env.ContentRootPath, "BibliotecaLibros_Vault");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }
}
