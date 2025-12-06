using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionLaboresAcademicas.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioAsignatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Asignaturas_AsignaturaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_AsignaturaId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AsignaturaId",
                table: "Usuarios");

            migrationBuilder.AddColumn<string>(
                name: "ItemDocente",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PendienteAsignarAsignaturas",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.InsertData(
                table: "Asignaturas",
                columns: new[] { "Id", "Area", "Nombre" },
                values: new object[,]
                {
                    { 1, "Ciencias Exactas", "Matemática" },
                    { 2, "Comunicación", "Lenguaje" },
                    { 3, "Ciencias Exactas", "Física" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioAsignatura_DocentesId",
                table: "UsuarioAsignatura",
                column: "DocentesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioAsignatura");

            migrationBuilder.DeleteData(
                table: "Asignaturas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Asignaturas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Asignaturas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "ItemDocente",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PendienteAsignarAsignaturas",
                table: "Usuarios");

            migrationBuilder.AddColumn<int>(
                name: "AsignaturaId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AsignaturaId",
                table: "Usuarios",
                column: "AsignaturaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Asignaturas_AsignaturaId",
                table: "Usuarios",
                column: "AsignaturaId",
                principalTable: "Asignaturas",
                principalColumn: "Id");
        }
    }
}
