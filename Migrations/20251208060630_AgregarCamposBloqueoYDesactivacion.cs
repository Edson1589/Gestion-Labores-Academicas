using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestionLaboresAcademicas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposBloqueoYDesactivacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueadoHasta",
                table: "Usuarios",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BloqueadoPor",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DesactivadoEl",
                table: "Usuarios",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DesactivadoPor",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBloqueo",
                table: "Usuarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoDesactivacion",
                table: "Usuarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoBloqueo",
                table: "Usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionIP",
                table: "RegistrosAuditoriaUsuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistorialEstadosUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioAfectadoId = table.Column<int>(type: "integer", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: true),
                    EstadoAnterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EstadoNuevo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoCambio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DireccionIP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadosUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialEstadosUsuarios_Usuarios_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadosUsuarios_Usuarios_UsuarioAfectadoId",
                        column: x => x.UsuarioAfectadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_BloqueadoPor",
                table: "Usuarios",
                column: "BloqueadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DesactivadoPor",
                table: "Usuarios",
                column: "DesactivadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosUsuarios_ActorId",
                table: "HistorialEstadosUsuarios",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosUsuarios_UsuarioAfectadoId",
                table: "HistorialEstadosUsuarios",
                column: "UsuarioAfectadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Usuarios_BloqueadoPor",
                table: "Usuarios",
                column: "BloqueadoPor",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Usuarios_DesactivadoPor",
                table: "Usuarios",
                column: "DesactivadoPor",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Usuarios_BloqueadoPor",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Usuarios_DesactivadoPor",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "HistorialEstadosUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_BloqueadoPor",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_DesactivadoPor",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "BloqueadoHasta",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "BloqueadoPor",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DesactivadoEl",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DesactivadoPor",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoBloqueo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoDesactivacion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TipoBloqueo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DireccionIP",
                table: "RegistrosAuditoriaUsuarios");
        }
    }
}
