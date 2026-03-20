# 📚 Biblioteca Digital (MVC)

Plataforma de gestión de préstamos, inventario y usuarios construida bajo la arquitectura **ASP.NET Core MVC**. Diseñada con un enfoque en *Clean Code*, reglas de negocio centralizadas y una interfaz gráfica Premium, completamente responsiva.

## 🚀 Tecnologías Principales
- **Backend:** C# / .NET 10.0
- **Frontend:** HTML5, CSS3, Razor Pages, Bootstrap 5 (UI basada en Componentes de Tarjeta y Sombras).
- **Base de Datos:** SQL Server gestionado mediante **Entity Framework Core**.
- **Autenticación:** ASP.NET Core Identity (Autenticación por Cookies, Roles y Validaciones de Seguridad).

## ✨ Características y Funcionalidades

### 1. 🛡️ Sistema de Roles y Seguridad (Identity)
- **Rol Administrador:** Acceso irrestricto al Panel de Control (Dashboard central), y capacidades de gestión y auditoría globales.
- **Rol Usuario (Lector):** Acceso al catálogo digital y al flujo personal de préstamos/multas.
- **Bloqueo Inteligente (Auto-Ban):** Restricción automática del inicio de sesión (LockoutEnd) para aquellos usuarios que mantengan préstamos expirados por más de 8 días.

### 2. 📖 Catálogo Dinámico (Libros y Autores)
- **Gestión de Stock:** El sistema valida e impide el préstamo si el libro alcanza un stock de `0`. El stock se reduce al prestar y se restaura al devolver (Control riguroso usando FKs hacia Autores).
- **Interfaz Premium:** Catálogo modernizado en formato de tabla inmersiva ocultando o deshabilitando acciones según el perfil (Ej. el Admin no puede prestarse libros a sí mismo).

### 3. 🔄 Motor Central de Préstamos
- **Reglas de Negocio Estrictas:**
  - Máximo 3 préstamos activos en simultáneo por usuario.
  - Préstamos bloqueados si el usuario posee multas pendientes.
  - El sistema detecta automáticamente si el usuario intenta solicitar el mismo libro dos veces.
- **Auditoría de Devoluciones:** Las fechas reales y programadas se evalúan al segundo para generar o exceptuar cobros adicionales.

### 4. 💰 Sistema Híbrido de Multas y Pasarela de Pagos (Mock)
- Calculadora interna de **Días de Mora** integrada estandarizadamente en el modelo `Prestamo.cs`. 
- Generación automática de recargos transaccionales para entregas tardías (Ej. $1,000 por día de retraso).
- **Checkout Seguro Simulador:** Pasarela digital para la liquidación de multas generada en un entorno ficticio altamente pulido (acepta tarjetas simuladas, registra el modelo de `Pago` y cierra automáticamente el ciclo de la Multa liberando la cuenta).

### 5. 📊 Panel de Control Administrativo (Dashboard)
- Listado unificado con las estadísticas en tiempo real (Usuarios, Libros, Autores, Préstamos Activos, Dinero en Deuda).
- Control E2E de usuarios: los administradores tienen el privilegio de promocionar usuarios a Administrador, remover cuentas y auditar estados de bloqueos con un clic.

---

## 💻 Instalación y Configuración Local

1. Clona el repositorio e instálate en el directorio raíz.
2. Asegúrate de tener **.NET 10.0 SDK** y **SQL Server** activos.
3. Actualiza el archivo `appsettings.json` o confía en el `DefaultConnection` localdb de tu sistema.
4. Genera la base de datos corriendo el comando:
   ```bash
   dotnet ef database update
   ```
5. Inicia el servidor:
   ```bash
   dotnet run
   ```
6. **(Credencial Maestra):** La base de datos, en caso de no poseer ningún administrador, genera dinámicamente al primer inicio en `Program.cs` la cuenta:
   - **Correo:** `dgomezpulid@outlook.com` 
   - **Contraseña:** `Admin_123`

---
*Desarrollado y mantenido con estándares de Clean Code, MVC Patterns y DRY.*
