using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduApoyos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarActivoAEstudiante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Estudiantes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Estudiantes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Estudiantes");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Estudiantes");
        }
    }
}
