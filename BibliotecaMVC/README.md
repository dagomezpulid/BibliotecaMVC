# Biblioteca MVC - Prueba Técnica Junior .NET

Aplicación web desarrollada en ASP.NET Core MVC para la gestión básica de una biblioteca.  
Permite agregar autores, registrar libros y visualizar la lista de libros con sus respectivos autores.

El sistema cuenta con dos entidades principales: Autor y Libro.
La relación entre ambas es de uno a muchos, donde un autor puede estar asociado a uno o varios libros, mientras que cada libro pertenece a un único autor.
Esta relación se implementa mediante la clave foránea AutorID en la entidad Libro.

---

## Tecnologías utilizadas

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server
- Bootstrap

## Funcionalidades

### Gestión de Autores
- Crear autores
- Listar autores
- Editar autores
- Eliminar autores (validando que no tengan libros asociados)

### Gestión de Libros
- Crear libros
- Asociar libros a autores
- Editar libros
- Eliminar libros
- Control de stock (cantidad de copias disponibles)

### Módulo de Préstamos
- Registrar el préstamo de un libro
- Ingresar nombre del solicitante
- Ingresar fecha estimada de devolución
- Validar que no se pueda prestar un libro sin stock disponible
- Disminuir el stock automáticamente al prestar

### Devolución de Libros
- Listar préstamos activos
- Registrar la devolución de un libro
- Incrementar el stock automáticamente al devolver

---

Entidades principales:
- Autor
- Libro
- Préstamo

## Configuración del proyecto

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/dagomezpulid/BibliotecaMVC.git
2. Abrir el proyecto en Visual Studio
3. Configurar la cadena de conexión en appsettings.json:
	"ConnectionStrings": {
  "BibliotecaConnection": "Server=.\\SQLEXPRESS;Database=BibliotecaDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
4. Ejecutar migraciones:
   -Add-Migration InitialCreate
   -Update-Database
5. Ejecutar la aplicación.



	