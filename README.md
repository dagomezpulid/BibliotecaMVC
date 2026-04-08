# 📚 Biblioteca Digital (MVC)

Plataforma de gestión de préstamos, inventario y usuarios construida bajo la arquitectura **ASP.NET Core MVC**. Diseñada con un enfoque en *Clean Code*, reglas de negocio centralizadas y una interfaz gráfica Premium, completamente responsiva.

## 🚀 Tecnologías Principales
- **Backend:** C# / .NET 10.0
- **Frontend:** HTML5, CSS3, Razor Pages, Bootstrap 5 (UI basada en Componentes de Tarjeta y Sombras).
- **Base de Datos:** SQL Server gestionado mediante **Entity Framework Core**.
- **Autenticación:** ASP.NET Core Identity (Autenticación por Cookies, Roles y Configuración Override).

## ✨ Características y Funcionalidades

### 1. 🛡️ Sistema de Roles y Seguridad (Identity)
- **Rol Administrador:** Acceso irrestricto al Panel de Control (Dashboard central), y capacidades de gestión y auditoría globales.
- **Rol Usuario (Lector):** Acceso al catálogo digital, panel unificado "Mi Perfil" (autoservicio de actualización de correo y contraseñas) y al flujo personal de préstamos.
- **Verificación SMTP Real:** Configuración integral de un Servidor de Correos (vía SmtpClient) para el envío asíncrono y real de enlaces de uso único al momento de solicitar o confirmar cambios en el correo electrónico.
- **Datos Expandibles:** El perfil está preparado nativamente para almacenar el Teléfono del usuario (`PhoneNumber`), abriendo paso a futuras integraciones (Ej. Bots de Twilio o Autenticación 2FA).
- **Bloqueo Restrictivo (Mora):** Sistema implacable de "Suspensión Automática" sobre el privilegio de préstamos si un usuario entrega un libro tarde. El infractor conserva acceso parcial a su historial, pero solo un Administrador puede conceder la "Amnistía" (Desbloqueo) desde el panel central.

### 2. 📖 Catálogo Dinámico y Validaciones
- **Gestión de Stock Rigurosa:** El sistema valida e impide el préstamo si el libro alcanza un stock de `0`. El stock transita orgánicamente (se reduce al prestar, se restaura al devolver).
- **Escudo Anti-Duplicados:** Validaciones asíncronas de base de datos (`AnyAsync`) previenen errores humanos impidiendo la creación de Autores o Libros repetidos, manteniendo el catálogo pulcro.

### 3. 🔄 Motor Avanzado de Préstamos
- **Duración Dinámica (User-Choice):** Los usuarios rigen sus propios tiempos eligiendo la duración de sus préstamos mediante un doble input sincronizado por JavaScript (Selector de Fecha Calendario y Selector de Días). Condicionado a reglas de negocio (Min: 2 días, Max: 20 días).
- **Reglas Base Simultáneas:**
  - Máximo 3 préstamos activos en simultáneo.
  - Intercepción inmediata bloqueando préstamos si hay multas sin pagar.
  - Prevención de duplicidad de copias de un mismo título.
- **Defensa Técnica Integral:** El controlador está completamente blindado contra vulnerabilidades de referencia (IDOR) y ataques por doble envío de peticiones (Double-Submit), asegurando que el stock sea impenetrable.

### 4. 💰 Híbrido de Multas y Pasarela (Mock)
- Calculadora temporal para deducir **Días de Mora** procesados al segundo de efectuar la Devolución Real.
- Generación automática de recargos financieros para entregas tardías.
- **Checkout Seguro (Simulación):** Pasarela digital moderna para liquidación de deudas, la cual interactúa con modelos de Pagos, limpia la mora y restablece la moralidad del usuario.

### 5. 📊 Dashboard Administrativo 
- Tarjetas de monitoreo en tiempo real (Usuarios, Libros, Autores, Préstamos, Deuda Acumulada).
- Control de Usuarios *End-to-End*: Privilegios para promover cuentas, eliminación en cascada de datos sensibles y el ansiado **botón de "Perdonar / Rehabilitar"** cuentas con candados por mora.

### 6. 🛡️ Ciberseguridad Defensiva y Pulimento UI (Auditoría Cero Fisuras)
- **Prevención Over-Posting (Asignación Masiva):** Extrema precaución en los Controladores delimitando mediante el atributo `[Bind]` exactamente qué propiedades viajan a la Base de Datos, previniendo inyecciones de identificadores u otros parámetros ocultos.
- **Escudo Antibots CSRF:** Inyección estricta de `[ValidateAntiForgeryToken]` en todas las transacciones y controladores sensibles de post-administración, evadiendo ataques de Falsificación de Petición en Sitios Cruzados.
- **Interfaz Pulcra:** Sistema de Notificaciones Flash (`TempData`) centralizado topológicamente en el *Layout* principal para garantizar cero alertas repetidas; y toda la arquitectura frontal (incluyendo menús autogenerados de Identity) traducida meticulosamente al español.

### 7. 📲 Infraestructura de Mensajería Celular (Integración Twilio)
- **Captura Rigurosa Cero-Evasión:** Reescritura del modelo nativo de Registro de Identity convirtiendo el contacto móvil en un componente **obligatorio** para garantizar que ningún lector ingrese de forma anónima al ecosistema físico.
- **Motor de Refracción a la Realidad (Fire-and-Forget):** Inyección de Dependencias `ISmsSender` ligada a la SDK global de **Twilio**. Cuando el controlador de préstamos dicta una sentencia de Mora y suspende una cuenta, el servidor automáticamente lanza un hilo de ejecución independiente para despachar un **SMS de Infracción** al mundo real, alertando inmediatamente al usuario al vibrar su bolsillo.

---

## 💻 Instalación y Configuración Local

1. Clona el repositorio e instálate en el directorio raíz.
2. Asegúrate de tener **.NET 10.0 SDK** y **SQL Server** activos.
3. Actualiza el archivo `appsettings.json` o confía en el `DefaultConnection` localdb de tu ecosistema virtual.
4. Aplica las migraciones de tabla para construir la BD:
   ```bash
   dotnet ef database update
   ```
5. Compila e inicia el servidor de desarrollo:
   ```bash
   dotnet run
   ```
6. **(Credencial Maestra Automática):** Al primer inicio, el sistema *seedeará* la cuenta super-admin:
   - **Correo:** `dgomezpulid@outlook.com` 
   - **Contraseña:** `Admin_123`

---
*Desarrollado y mantenido con estándares de Clean Code, MVC Patterns y metodologías ágiles.*
