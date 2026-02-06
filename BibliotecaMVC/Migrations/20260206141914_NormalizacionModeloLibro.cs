using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaMVC.Migrations
{
    /// <inheritdoc />
    public partial class NormalizacionModeloLibro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libros_Autores_AutorID",
                table: "Libros");

            migrationBuilder.DropForeignKey(
                name: "FK_Prestamos_Libros_LibroID",
                table: "Prestamos");

            migrationBuilder.RenameColumn(
                name: "LibroID",
                table: "Prestamos",
                newName: "LibroId");

            migrationBuilder.RenameColumn(
                name: "PrestamoID",
                table: "Prestamos",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Prestamos_LibroID",
                table: "Prestamos",
                newName: "IX_Prestamos_LibroId");

            migrationBuilder.RenameColumn(
                name: "AutorID",
                table: "Libros",
                newName: "AutorId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Libros",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Libros_AutorID",
                table: "Libros",
                newName: "IX_Libros_AutorId");

            migrationBuilder.RenameColumn(
                name: "AutorID",
                table: "Autores",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Libros",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Libros_Autores_AutorId",
                table: "Libros",
                column: "AutorId",
                principalTable: "Autores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamos_Libros_LibroId",
                table: "Prestamos",
                column: "LibroId",
                principalTable: "Libros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libros_Autores_AutorId",
                table: "Libros");

            migrationBuilder.DropForeignKey(
                name: "FK_Prestamos_Libros_LibroId",
                table: "Prestamos");

            migrationBuilder.RenameColumn(
                name: "LibroId",
                table: "Prestamos",
                newName: "LibroID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Prestamos",
                newName: "PrestamoID");

            migrationBuilder.RenameIndex(
                name: "IX_Prestamos_LibroId",
                table: "Prestamos",
                newName: "IX_Prestamos_LibroID");

            migrationBuilder.RenameColumn(
                name: "AutorId",
                table: "Libros",
                newName: "AutorID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Libros",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_Libros_AutorId",
                table: "Libros",
                newName: "IX_Libros_AutorID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Autores",
                newName: "AutorID");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Libros",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddForeignKey(
                name: "FK_Libros_Autores_AutorID",
                table: "Libros",
                column: "AutorID",
                principalTable: "Autores",
                principalColumn: "AutorID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamos_Libros_LibroID",
                table: "Prestamos",
                column: "LibroID",
                principalTable: "Libros",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
