using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestionLaboresAcademicas.Migrations
{
    /// <inheritdoc />
    public partial class AddAprobaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacionRoles_RolSolicitadoId",
                table: "SolicitudesAprobacionRoles",
                column: "RolSolicitadoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacionRoles_UsuarioSolicitadoId",
                table: "SolicitudesAprobacionRoles",
                column: "UsuarioSolicitadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesAprobacionRoles");
        }
    }
}
