using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BibliotecaMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {









            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecursoId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detalles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogsAuditoria_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });





            migrationBuilder.CreateTable(
                name: "LibroArchivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LibroId = table.Column<int>(type: "int", nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Formato = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibroArchivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibroArchivos_Libros_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });


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
                name: "IX_LibroArchivos_LibroId",
                table: "LibroArchivos",
                column: "LibroId");


            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_UsuarioId",
                table: "LogsAuditoria",
                column: "UsuarioId");







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
                name: "LibroArchivos");

            migrationBuilder.DropTable(
                name: "LogsAuditoria");



            migrationBuilder.DropTable(
                name: "ProgresosLectura");








        }
    }
}
