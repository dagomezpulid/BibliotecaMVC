using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Models
{
    /// <summary>
    /// Contexto de acceso a datos que hereda de IdentityDbContext.
    /// Define la estructura de tablas, relaciones Fluent API y semillas de datos (Seeding).
    /// </summary>
    public class BibliotecaContext : IdentityDbContext<ApplicationUser>
    {
        public BibliotecaContext(DbContextOptions<BibliotecaContext> options)
            : base(options)
        {
        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<Multa> Multas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Resena> Resenas { get; set; }

        /// <summary>
        /// Configuración avanzada del modelo de datos mediante Fluent API.
        /// Define relaciones 1:1, 1:N y M:N, además de datos iniciales.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Prestamo>()
                .ToTable("Prestamos");

            modelBuilder.Entity<Prestamo>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId);

            modelBuilder.Entity<Multa>()
                .HasOne(m => m.Prestamo)
                .WithOne(p => p.Multa)
                .HasForeignKey<Multa>(m => m.PrestamoId);

            // Seed Categorías
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Ficción" },
                new Categoria { Id = 2, Nombre = "Ciencia" },
                new Categoria { Id = 3, Nombre = "Tecnología" },
                new Categoria { Id = 4, Nombre = "Historia" },
                new Categoria { Id = 5, Nombre = "Biografía" },
                new Categoria { Id = 6, Nombre = "Fantasía" },
                new Categoria { Id = 7, Nombre = "Misterio" }
            );
        }
    }
}
