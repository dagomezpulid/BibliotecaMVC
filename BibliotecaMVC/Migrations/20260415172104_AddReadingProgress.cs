using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgresosLectura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LibroId = table.Column<int>(type: "int", nullable: false),
                    PaginaActual = table.Column<int>(type: "int", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresosLectura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgresosLectura_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgresosLectura_Libros_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosLectura_LibroId",
                table: "ProgresosLectura",
                column: "LibroId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosLectura_UsuarioId",
                table: "ProgresosLectura",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgresosLectura");
        }
    }
}
