using GestionLaboresAcademicas.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionLaboresAcademicas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TipoUsuario> TiposUsuario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.CI)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<TipoUsuario>().HasData(
                new TipoUsuario { Id = 1, Nombre = "Director" },
                new TipoUsuario { Id = 2, Nombre = "Secretaria" },
                new TipoUsuario { Id = 3, Nombre = "Docente" },
                new TipoUsuario { Id = 4, Nombre = "Estudiante" },
                new TipoUsuario { Id = 5, Nombre = "Padre de familia" },
                new TipoUsuario { Id = 6, Nombre = "Regente" },
                new TipoUsuario { Id = 7, Nombre = "Bibliotecario" }
            );
        }
    }
}
