using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionLaboresAcademicas.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asignaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asignaturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nivel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Grado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Paralelo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Turno = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Gestion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosAcademicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Gestion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NombrePeriodo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosAcademicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PoliticasSeguridad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LongitudMinimaPassword = table.Column<int>(type: "integer", nullable: false),
                    RequiereMayusculas = table.Column<bool>(type: "boolean", nullable: false),
                    RequiereMinusculas = table.Column<bool>(type: "boolean", nullable: false),
                    RequiereNumero = table.Column<bool>(type: "boolean", nullable: false),
                    RequiereCaracterEspecial = table.Column<bool>(type: "boolean", nullable: false),
                    IntentosMaximosFallidos = table.Column<int>(type: "integer", nullable: false),
                    MinutosBloqueo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticasSeguridad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    RequiereAprobacion = table.Column<bool>(type: "boolean", nullable: false),
                    AlcanceMaximo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentoCI = table.Column<string>(type: "text", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    TipoUsuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstadoCuenta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    CursoId = table.Column<int>(type: "integer", nullable: true),
                    PendienteAsignacionCurso = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PendienteVinculoEstudiantes = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ItemDocente = table.Column<string>(type: "text", nullable: true),
                    PendienteAsignarAsignaturas = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BitacorasConsulta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FiltrosJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    TiposIndicador = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Exito = table.Column<bool>(type: "boolean", nullable: false),
                    MensajeError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacorasConsulta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BitacorasConsulta_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CredencialesAcceso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    EsTemporal = table.Column<bool>(type: "boolean", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RequiereCambioPrimerLogin = table.Column<bool>(type: "boolean", nullable: false),
                    IntentosFallidos = table.Column<int>(type: "integer", nullable: false),
                    BloqueadaHasta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredencialesAcceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CredencialesAcceso_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatosAcademicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodoAcademicoId = table.Column<int>(type: "integer", nullable: false),
                    CursoId = table.Column<int>(type: "integer", nullable: false),
                    AsignaturaId = table.Column<int>(type: "integer", nullable: false),
                    EstudianteId = table.Column<int>(type: "integer", nullable: false),
                    TipoDato = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Asistencia_Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Nota = table.Column<decimal>(type: "numeric", nullable: true),
                    TipoEvaluacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FechaInscripcion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Matricula_Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FechaPrestamo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FechaDevolucion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PrestamoBiblioteca_Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatosAcademicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatosAcademicos_Asignaturas_AsignaturaId",
                        column: x => x.AsignaturaId,
                        principalTable: "Asignaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatosAcademicos_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatosAcademicos_PeriodosAcademicos_PeriodoAcademicoId",
                        column: x => x.PeriodoAcademicoId,
                        principalTable: "PeriodosAcademicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatosAcademicos_Usuarios_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoriaUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioAfectadoId = table.Column<int>(type: "integer", nullable: true),
                    ActorId = table.Column<int>(type: "integer", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Detalle = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Origen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoriaUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosAuditoriaUsuarios_Usuarios_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioAfectadoId",
                        column: x => x.UsuarioAfectadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesAprobacionRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioSolicitadoId = table.Column<int>(type: "integer", nullable: false),
                    RolSolicitadoId = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MotivoRechazo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesAprobacionRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacionRoles_Roles_RolSolicitadoId",
                        column: x => x.RolSolicitadoId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacionRoles_Usuarios_UsuarioSolicitadoId",
                        column: x => x.UsuarioSolicitadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioAsignatura",
                columns: table => new
                {
                    AsignaturasId = table.Column<int>(type: "integer", nullable: false),
                    DocentesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioAsignatura", x => new { x.AsignaturasId, x.DocentesId });
                    table.ForeignKey(
                        name: "FK_UsuarioAsignatura_Asignaturas_AsignaturasId",
                        column: x => x.AsignaturasId,
                        principalTable: "Asignaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioAsignatura_Usuarios_DocentesId",
                        column: x => x.DocentesId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VinculosPadreEstudiante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PadreId = table.Column<int>(type: "integer", nullable: false),
                    EstudianteId = table.Column<int>(type: "integer", nullable: false),
                    Relacion = table.Column<string>(type: "text", nullable: false),
                    EsTutorLegal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VinculosPadreEstudiante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VinculosPadreEstudiante_Usuarios_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VinculosPadreEstudiante_Usuarios_PadreId",
                        column: x => x.PadreId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Asignaturas",
                columns: new[] { "Id", "Area", "Nombre" },
                values: new object[,]
                {
                    { 1, "Ciencias Exactas", "Matemática" },
                    { 2, "Comunicación", "Lenguaje" },
                    { 3, "Ciencias Exactas", "Física" }
                });

            migrationBuilder.InsertData(
                table: "PeriodosAcademicos",
                columns: new[] { "Id", "Estado", "FechaFin", "FechaInicio", "Gestion", "NombrePeriodo" },
                values: new object[] { 1, "Activo", new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gestión 2025", "Gestión anual 2025" });

            migrationBuilder.InsertData(
                table: "PoliticasSeguridad",
                columns: new[] { "Id", "IntentosMaximosFallidos", "LongitudMinimaPassword", "MinutosBloqueo", "RequiereCaracterEspecial", "RequiereMayusculas", "RequiereMinusculas", "RequiereNumero" },
                values: new object[] { 1, 5, 8, 15, true, true, true, true });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "AlcanceMaximo", "Descripcion", "Nombre", "RequiereAprobacion" },
                values: new object[,]
                {
                    { 1, "Institucion", "Director de la institución", "Director", true },
                    { 2, "Institucion", "Secretaría académica", "Secretaria", false },
                    { 3, "SusCursos", "Docente", "Docente", false },
                    { 4, "SuCurso", "Estudiante", "Estudiante", false },
                    { 5, "CursoHijos", "Padre de familia", "Padre", false },
                    { 6, "Institucion", "Regente de disciplina", "Regente", true },
                    { 7, "Biblioteca", "Bibliotecario", "Bibliotecario", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BitacorasConsulta_UsuarioId",
                table: "BitacorasConsulta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CredencialesAcceso_Username",
                table: "CredencialesAcceso",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CredencialesAcceso_UsuarioId",
                table: "CredencialesAcceso",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DatosAcademicos_AsignaturaId",
                table: "DatosAcademicos",
                column: "AsignaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_DatosAcademicos_CursoId",
                table: "DatosAcademicos",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_DatosAcademicos_EstudianteId",
                table: "DatosAcademicos",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_DatosAcademicos_PeriodoAcademicoId",
                table: "DatosAcademicos",
                column: "PeriodoAcademicoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoriaUsuarios_ActorId",
                table: "RegistrosAuditoriaUsuarios",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoriaUsuarios_UsuarioAfectadoId",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioAfectadoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoriaUsuarios_UsuarioId",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoriaUsuarios_UsuarioId1",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacionRoles_RolSolicitadoId",
                table: "SolicitudesAprobacionRoles",
                column: "RolSolicitadoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacionRoles_UsuarioSolicitadoId",
                table: "SolicitudesAprobacionRoles",
                column: "UsuarioSolicitadoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioAsignatura_DocentesId",
                table: "UsuarioAsignatura",
                column: "DocentesId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CursoId",
                table: "Usuarios",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DocumentoCI",
                table: "Usuarios",
                column: "DocumentoCI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculosPadreEstudiante_EstudianteId",
                table: "VinculosPadreEstudiante",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculosPadreEstudiante_PadreId",
                table: "VinculosPadreEstudiante",
                column: "PadreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitacorasConsulta");

            migrationBuilder.DropTable(
                name: "CredencialesAcceso");

            migrationBuilder.DropTable(
                name: "DatosAcademicos");

            migrationBuilder.DropTable(
                name: "PoliticasSeguridad");

            migrationBuilder.DropTable(
                name: "RegistrosAuditoriaUsuarios");

            migrationBuilder.DropTable(
                name: "SolicitudesAprobacionRoles");

            migrationBuilder.DropTable(
                name: "UsuarioAsignatura");

            migrationBuilder.DropTable(
                name: "VinculosPadreEstudiante");

            migrationBuilder.DropTable(
                name: "PeriodosAcademicos");

            migrationBuilder.DropTable(
                name: "Asignaturas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
