using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionLaboresAcademicas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposEliminacionUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EliminadoEl",
                table: "Usuarios",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EliminadoPor",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoEliminacion",
                table: "Usuarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EliminadoPor",
                table: "Usuarios",
                column: "EliminadoPor");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Usuarios_EliminadoPor",
                table: "Usuarios",
                column: "EliminadoPor",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Usuarios_EliminadoPor",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EliminadoPor",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EliminadoEl",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoEliminacion",
                table: "Usuarios");
        }
    }
}
