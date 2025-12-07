using GestionLaboresAcademicas.Models;
using GestionLaboresAcademicas.Models.DatosAcademicos;
using GestionLaboresAcademicas.Models.Estadisticas;
using Microsoft.EntityFrameworkCore;

namespace GestionLaboresAcademicas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<CredencialAcceso> CredencialesAcceso => Set<CredencialAcceso>();
        public DbSet<PoliticaSeguridad> PoliticasSeguridad => Set<PoliticaSeguridad>();
        public DbSet<RegistroAuditoriaUsuario> RegistrosAuditoriaUsuarios => Set<RegistroAuditoriaUsuario>();
        public DbSet<Curso> Cursos => Set<Curso>();
        public DbSet<Asignatura> Asignaturas => Set<Asignatura>();
        public DbSet<VinculoPadreEstudiante> VinculosPadreEstudiante => Set<VinculoPadreEstudiante>();
        public DbSet<SolicitudAprobacionRol> SolicitudesAprobacionRoles => Set<SolicitudAprobacionRol>();
        public DbSet<PeriodoAcademico> PeriodosAcademicos => Set<PeriodoAcademico>();

        public DbSet<DatoAcademico> DatosAcademicos => Set<DatoAcademico>();
        public DbSet<Calificacion> Calificaciones => Set<Calificacion>();
        public DbSet<Asistencia> Asistencias => Set<Asistencia>();
        public DbSet<Matricula> Matriculas => Set<Matricula>();
        public DbSet<PrestamoBiblioteca> PrestamosBiblioteca => Set<PrestamoBiblioteca>();

        public DbSet<BitacoraConsulta> BitacorasConsulta => Set<BitacoraConsulta>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.DocumentoCI).IsUnique();
                entity.HasIndex(u => u.Correo).IsUnique();

                entity.Property(u => u.Nombres).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Apellidos).HasMaxLength(100).IsRequired();
                entity.Property(u => u.TipoUsuario).HasMaxLength(50).IsRequired();
                entity.Property(u => u.EstadoCuenta).HasMaxLength(20).IsRequired();

                entity.Property(u => u.PendienteAsignacionCurso).HasDefaultValue(false);
                entity.Property(u => u.PendienteVinculoEstudiantes).HasDefaultValue(false);
                entity.Property(u => u.PendienteAsignarAsignaturas).HasDefaultValue(false);

                entity
                    .HasMany(u => u.Asignaturas)
                    .WithMany(a => a.Docentes)
                    .UsingEntity(j => j.ToTable("UsuarioAsignatura"));
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
                entity.HasIndex(r => r.Nombre).IsUnique();
            });

            modelBuilder.Entity<CredencialAcceso>(entity =>
            {
                entity.HasIndex(c => c.Username).IsUnique();
                entity.Property(c => c.Username).HasMaxLength(50).IsRequired();

                entity.HasOne(c => c.Usuario)
                      .WithOne(u => u.CredencialAcceso)
                      .HasForeignKey<CredencialAcceso>(c => c.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RegistroAuditoriaUsuario>(entity =>
            {
                entity.Property(r => r.Accion)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(r => r.Origen)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(r => r.Detalle)
                      .HasMaxLength(2000)
                      .IsRequired();

                entity.HasOne(r => r.UsuarioAfectado)
                      .WithMany()
                      .HasForeignKey(r => r.UsuarioAfectadoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Actor)
                      .WithMany()
                      .HasForeignKey(r => r.ActorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Director", Descripcion = "Director de la institución", RequiereAprobacion = true, AlcanceMaximo = "Institucion" },
                new Rol { Id = 2, Nombre = "Secretaria", Descripcion = "Secretaría académica", RequiereAprobacion = false, AlcanceMaximo = "Institucion" },
                new Rol { Id = 3, Nombre = "Docente", Descripcion = "Docente", RequiereAprobacion = false, AlcanceMaximo = "SusCursos" },
                new Rol { Id = 4, Nombre = "Estudiante", Descripcion = "Estudiante", RequiereAprobacion = false, AlcanceMaximo = "SuCurso" },
                new Rol { Id = 5, Nombre = "Padre", Descripcion = "Padre de familia", RequiereAprobacion = false, AlcanceMaximo = "CursoHijos" },
                new Rol { Id = 6, Nombre = "Regente", Descripcion = "Regente de disciplina", RequiereAprobacion = true, AlcanceMaximo = "Institucion" },
                new Rol { Id = 7, Nombre = "Bibliotecario", Descripcion = "Bibliotecario", RequiereAprobacion = true, AlcanceMaximo = "Biblioteca" }
            );

            modelBuilder.Entity<PoliticaSeguridad>().HasData(
                new PoliticaSeguridad
                {
                    Id = 1,
                    LongitudMinimaPassword = 8,
                    RequiereMayusculas = true,
                    RequiereMinusculas = true,
                    RequiereNumero = true,
                    RequiereCaracterEspecial = true,
                    IntentosMaximosFallidos = 5,
                    MinutosBloqueo = 15
                }
            );

            modelBuilder.Entity<Curso>(entity =>
            {
                entity.Property(c => c.Nivel).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Grado).HasMaxLength(20).IsRequired();
                entity.Property(c => c.Paralelo).HasMaxLength(5).IsRequired();
                entity.Property(c => c.Turno).HasMaxLength(20).IsRequired();
                entity.Property(c => c.Gestion).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Asignatura>(entity =>
            {
                entity.Property(a => a.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Area).HasMaxLength(100);
            });

            modelBuilder.Entity<Asignatura>().HasData(
                new Asignatura { Id = 1, Nombre = "Matemática", Area = "Ciencias Exactas" },
                new Asignatura { Id = 2, Nombre = "Lenguaje", Area = "Comunicación" },
                new Asignatura { Id = 3, Nombre = "Física", Area = "Ciencias Exactas" }
            );

            modelBuilder.Entity<VinculoPadreEstudiante>(entity =>
            {
                entity.HasOne(v => v.Padre)
                      .WithMany(u => u.VinculosComoPadre)
                      .HasForeignKey(v => v.PadreId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Estudiante)
                      .WithMany(u => u.VinculosComoEstudiante)
                      .HasForeignKey(v => v.EstudianteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SolicitudAprobacionRol>(entity =>
            {
                entity.Property(s => s.Estado)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.HasOne(s => s.UsuarioSolicitado)
                      .WithMany(u => u.SolicitudesRol)
                      .HasForeignKey(s => s.UsuarioSolicitadoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.RolSolicitado)
                      .WithMany()
                      .HasForeignKey(s => s.RolSolicitadoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PeriodoAcademico>(entity =>
            {
                entity.Property(p => p.Gestion)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(p => p.NombrePeriodo)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(p => p.Estado)
                      .HasMaxLength(20)
                      .IsRequired();
            });

            modelBuilder.Entity<DatoAcademico>(entity =>
            {
                entity.HasDiscriminator<string>("TipoDato")
                      .HasValue<Calificacion>("Calificacion")
                      .HasValue<Asistencia>("Asistencia")
                      .HasValue<Matricula>("Matricula")
                      .HasValue<PrestamoBiblioteca>("PrestamoBiblioteca");

                entity.HasOne(d => d.PeriodoAcademico)
                      .WithMany(p => p.DatosAcademicos)
                      .HasForeignKey(d => d.PeriodoAcademicoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Curso)
                      .WithMany()
                      .HasForeignKey(d => d.CursoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Asignatura)
                      .WithMany()
                      .HasForeignKey(d => d.AsignaturaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Estudiante)
                      .WithMany()
                      .HasForeignKey(d => d.EstudianteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Calificacion>(entity =>
            {
                entity.Property(c => c.TipoEvaluacion)
                      .HasMaxLength(50)
                      .IsRequired();
            });

            modelBuilder.Entity<Asistencia>(entity =>
            {
                entity.Property(a => a.Estado)
                      .HasMaxLength(20)
                      .IsRequired();
            });

            modelBuilder.Entity<Matricula>(entity =>
            {
                entity.Property(m => m.Estado)
                      .HasMaxLength(20)
                      .IsRequired();
            });

            modelBuilder.Entity<PrestamoBiblioteca>(entity =>
            {
                entity.Property(p => p.Estado)
                      .HasMaxLength(20)
                      .IsRequired();
            });

            modelBuilder.Entity<BitacoraConsulta>(entity =>
            {
                entity.Property(b => b.Accion)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(b => b.Rol)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(b => b.FiltrosJson)
                      .HasMaxLength(4000)
                      .IsRequired();

                entity.Property(b => b.TiposIndicador)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(b => b.MensajeError)
                      .HasMaxLength(1000);

                entity.HasOne(b => b.Usuario)
                      .WithMany()
                      .HasForeignKey(b => b.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PeriodoAcademico>().HasData(
                new PeriodoAcademico
                {
                    Id = 1,
                    Gestion = "Gestión 2025",
                    NombrePeriodo = "Gestión anual 2025",
                    FechaInicio = new DateTime(2025, 1, 1),
                    FechaFin = new DateTime(2025, 12, 31),
                    Estado = "Activo"
                }
            );
        }
    }
}
