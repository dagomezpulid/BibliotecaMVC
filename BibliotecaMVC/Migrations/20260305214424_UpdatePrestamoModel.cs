using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaMVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrestamoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Devuelto",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "DiasRetraso",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "FechaDevolucion",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "Multa",
                table: "Prestamos");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Prestamos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDevolucionProgramada",
                table: "Prestamos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "FechaDevolucionProgramada",
                table: "Prestamos");

            migrationBuilder.AddColumn<bool>(
                name: "Devuelto",
                table: "Prestamos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DiasRetraso",
                table: "Prestamos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDevolucion",
                table: "Prestamos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Multa",
                table: "Prestamos",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }
    }
}
