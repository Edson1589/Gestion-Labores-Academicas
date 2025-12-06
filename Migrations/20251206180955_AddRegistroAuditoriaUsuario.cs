using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionLaboresAcademicas.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistroAuditoriaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioAfectadoId",
                table: "RegistrosAuditoriaUsuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Origen",
                table: "RegistrosAuditoriaUsuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Detalle",
                table: "RegistrosAuditoriaUsuarios",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Accion",
                table: "RegistrosAuditoriaUsuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "RegistrosAuditoriaUsuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "RegistrosAuditoriaUsuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoriaUsuarios_UsuarioId",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoriaUsuarios_UsuarioId1",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioAfectadoId",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioAfectadoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioId",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioId1",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioAfectadoId",
                table: "RegistrosAuditoriaUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioId",
                table: "RegistrosAuditoriaUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioId1",
                table: "RegistrosAuditoriaUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosAuditoriaUsuarios_UsuarioId",
                table: "RegistrosAuditoriaUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosAuditoriaUsuarios_UsuarioId1",
                table: "RegistrosAuditoriaUsuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "RegistrosAuditoriaUsuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "RegistrosAuditoriaUsuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Origen",
                table: "RegistrosAuditoriaUsuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Detalle",
                table: "RegistrosAuditoriaUsuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Accion",
                table: "RegistrosAuditoriaUsuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosAuditoriaUsuarios_Usuarios_UsuarioAfectadoId",
                table: "RegistrosAuditoriaUsuarios",
                column: "UsuarioAfectadoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
