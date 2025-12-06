using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestionLaboresAcademicas.Migrations
{
    /// <inheritdoc />
    public partial class AddVinculoPadreEstudiante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "PendienteAsignacionCurso",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "PendienteVinculoEstudiantes",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                name: "VinculosPadreEstudiante");

            migrationBuilder.DropColumn(
                name: "PendienteVinculoEstudiantes",
                table: "Usuarios");

            migrationBuilder.AlterColumn<bool>(
                name: "PendienteAsignacionCurso",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}
