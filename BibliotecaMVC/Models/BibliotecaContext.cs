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
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<LibroArchivo> LibroArchivos { get; set; }
        public DbSet<ProgresoLectura> ProgresosLectura { get; set; }
        public DbSet<LogAuditoria> LogsAuditoria { get; set; }

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

            modelBuilder.Entity<Multa>()
                .Property(m => m.Monto)
                .HasPrecision(18, 2); // Garantiza que no haya pérdida de decimales en montos de dinero (SQL Server decimal(18,2))

            modelBuilder.Entity<Pago>()
                .Property(p => p.Monto)
                .HasPrecision(18, 2);

            // Seed Categorías (Expandido Fase 4)
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Ficción" },
                new Categoria { Id = 2, Nombre = "Ciencia" },
                new Categoria { Id = 3, Nombre = "Tecnología" },
                new Categoria { Id = 4, Nombre = "Historia" },
                new Categoria { Id = 5, Nombre = "Biografía" },
                new Categoria { Id = 6, Nombre = "Fantasía" },
                new Categoria { Id = 7, Nombre = "Misterio" },
                new Categoria { Id = 8, Nombre = "Novela Histórica" },
                new Categoria { Id = 9, Nombre = "Novela Romántica" },
                new Categoria { Id = 10, Nombre = "Novela Policíaca" },
                new Categoria { Id = 11, Nombre = "Suspenso" },
                new Categoria { Id = 12, Nombre = "Ciencia Ficción" },
                new Categoria { Id = 13, Nombre = "Distopía" },
                new Categoria { Id = 14, Nombre = "Cuentos y Relatos" },
                new Categoria { Id = 15, Nombre = "Poesía" },
                new Categoria { Id = 16, Nombre = "Teatro y Drama" },
                new Categoria { Id = 17, Nombre = "Cómic y Novela Gráfica" },
                new Categoria { Id = 18, Nombre = "Autoayuda" },
                new Categoria { Id = 19, Nombre = "Negocios y Finanzas" },
                new Categoria { Id = 20, Nombre = "Salud y Bienestar" },
                new Categoria { Id = 21, Nombre = "Arte y Fotografía" },
                new Categoria { Id = 22, Nombre = "Religión y Espiritualidad" },
                new Categoria { Id = 23, Nombre = "Infantil y Juvenil" },
                new Categoria { Id = 24, Nombre = "Consulta y Referencia" },
                new Categoria { Id = 25, Nombre = "Libros de Texto" },
                new Categoria { Id = 26, Nombre = "Cocina" },
                new Categoria { Id = 27, Nombre = "Manualidades" },
                new Categoria { Id = 28, Nombre = "Viajes y Guías" },
                new Categoria { Id = 29, Nombre = "Terror" },
                new Categoria { Id = 30, Nombre = "Novela Autobiográfica" },
                new Categoria { Id = 31, Nombre = "Relato de Memorias" },
                new Categoria { Id = 32, Nombre = "Novela Negra" },
                new Categoria { Id = 33, Nombre = "Novela Oratorio" },
                new Categoria { Id = 34, Nombre = "Crónica Documental" },
                new Categoria { Id = 35, Nombre = "Crónica" }
            );
        }
    }
}
