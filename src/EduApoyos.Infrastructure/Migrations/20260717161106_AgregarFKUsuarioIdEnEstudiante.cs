using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduApoyos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFKUsuarioIdEnEstudiante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Estudiantes_UsuarioId",
                table: "Estudiantes");

            migrationBuilder.AlterColumn<Guid>(
                name: "UsuarioId",
                table: "Estudiantes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_UsuarioId",
                table: "Estudiantes",
                column: "UsuarioId",
                unique: true,
                filter: "[UsuarioId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Estudiantes_Usuarios_UsuarioId",
                table: "Estudiantes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estudiantes_Usuarios_UsuarioId",
                table: "Estudiantes");

            migrationBuilder.DropIndex(
                name: "IX_Estudiantes_UsuarioId",
                table: "Estudiantes");

            migrationBuilder.AlterColumn<Guid>(
                name: "UsuarioId",
                table: "Estudiantes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_UsuarioId",
                table: "Estudiantes",
                column: "UsuarioId",
                unique: true);
        }
    }
}
