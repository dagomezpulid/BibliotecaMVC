using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Models
{
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Prestamo>()
                .ToTable("Prestamos");

            modelBuilder.Entity<Prestamo>()
                .Property(p => p.Multa)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Prestamo>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId);
            modelBuilder.Entity<Multa>()
                .HasOne(m => m.Prestamo)
                .WithOne(p => p.Multas)
                .HasForeignKey<Multa>(m => m.PrestamoId);
        }
    }
}
