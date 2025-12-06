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
                    RolId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "RegistrosAuditoriaUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioAfectadoId = table.Column<int>(type: "integer", nullable: true),
                    ActorId = table.Column<int>(type: "integer", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Accion = table.Column<string>(type: "text", nullable: false),
                    Detalle = table.Column<string>(type: "text", nullable: false),
                    Origen = table.Column<string>(type: "text", nullable: false)
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
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_RegistrosAuditoriaUsuarios_ActorId",
                table: "RegistrosAuditoriaUsuarios",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoriaUsuarios_UsuarioAfectadoId",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioAfectadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DocumentoCI",
                table: "Usuarios",
                column: "DocumentoCI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CredencialesAcceso");

            migrationBuilder.DropTable(
                name: "PoliticasSeguridad");

            migrationBuilder.DropTable(
                name: "RegistrosAuditoriaUsuarios");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
