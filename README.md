# 📚 Biblioteca Digital (MVC)

Plataforma de gestión de préstamos, inventario y usuarios construida bajo la arquitectura **ASP.NET Core MVC**. Diseñada con un enfoque en *Clean Code*, reglas de negocio centralizadas y una interfaz gráfica Premium, completamente responsiva.

---

## 🚀 Tecnologías Principales
- **Backend:** C# / .NET 10.0
- **Frontend:** HTML5, CSS3, Razor Pages, Bootstrap 5 (UI enriquecida con Componentes de Tarjeta, Gradientes y Sombras).
- **Base de Datos:** SQL Server gestionado mediante **Entity Framework Core**.
- **Autenticación:** ASP.NET Core Identity (Autenticación por Cookies, Roles y Perfiles Extendidos).
- **Notificaciones:** Integración con **Twilio API** (WhatsApp/SMS) y **SmtpClient** (Email).

---

## ✨ Características y Funcionalidades

### 1. 🛡️ Sistema de Roles y Seguridad (Identity)
- **Rol Administrador:** Acceso al Panel de Control (Dashboard central), gestión de usuarios, auditoría global y capacidad de "Amnistía" para desbloqueo de cuentas.
- **Rol Usuario (Lector):** Acceso al catálogo, panel "Mi Perfil" (autoservicio de datos) y gestión personal de préstamos/favoritos.
- **Verificación SMTP Real:** Implementación para el envío asíncrono de enlaces de recuperación y confirmaciones de cuenta.
- **Bloqueo Restrictivo (Mora):** Suspensión automática del privilegio de préstamos si se detectan entregas tardías, rehabilitable únicamente por un administrador.

### 2. 📖 Experiencia de Usuario Interactiva
- **Catálogo Inteligente:** Motor de búsqueda dinámica que filtra por Título, Autor, Categoría o ISBN.
- **⭐ Sistema de Reseñas:** Calificación numérica (1-5 estrellas) y comentarios sobre cada obra con protección anti-spam (una reseña por usuario).
- **💡 Motor de Recomendaciones:** Algoritmo integrado que sugiere lecturas similares basándose en la categoría y el autor del libro visualizado.
- **❤️ Wishlist (Favoritos):** Los usuarios pueden marcar libros de su interés para acceso rápido desde el catálogo.
- **🔔 Centro de Notificaciones:** Sistema interno de alertas persistentes para comunicar estados de préstamos, multas y mensajes del sistema.

### 3. 🔄 Motor Avanzado de Préstamos y Multas
- **Gestión de Stock en Tiempo Real:** Validación estricta que impide préstamos sin existencias y restaura stock automáticamente en devoluciones.
- **Duración Proactiva:** Selección de tiempo de préstamo (2 a 20 días) sincronizada mediante JavaScript.
- **Límites de Uso:** Máximo 3 préstamos activos por usuario para garantizar la rotación del inventario.
- **Calculadora de Mora:** Procesamiento automático de días de retraso y generación de recargos financieros.
- **Checkout Seguro (Simulación):** Pasarela de pagos integrada para la liquidación de deudas y restablecimiento de estado de cuenta.

### 4. 📲 Infraestructura de Mensajería (WhatsApp/Cron Jobs)
- **Motor Twilio WhatsApp:** Notificaciones inmediatas al rentar un libro o al generarse una multa, evadiendo mallas anti-spam internacionales.
- **Vigilante Nocturno Automatizado (Cron Job):** `IHostedService` que patrulla la base de datos cada 24 horas detectando deudores y disparando alertas preventivas automáticas.
- **Persistencia de Alertas:** Mecanismo `AlertaMoraEnviada` en la base de datos para evitar duplicidad de mensajes y garantizar una comunicación profesional.

### 5. 🛠️ Excelencia Técnica y Auditoría
- **Hardening de Secretos:** Implementación de **User Secrets** para llaves de API y credenciales de servidor.
- **Defensa Técnica:** Blindaje contra ataques IDOR, Double-Submit y CSRF mediante el uso estricto de tokens de verificación y validaciones de ID de usuario.
- **Limpieza en Cascada:** Eliminación recursiva de datos sensibles para mantener la integridad referencial y cumplir con estándares de privacidad.

---

## 💻 Instalación y Configuración Paso a Paso

Sigue este orden exacto para poner en marcha el proyecto en cualquier computador:

### 1. Preparación de Archivos
Clona el repositorio en tu máquina local:
```bash
git clone [URL-del-repositorio]
```

### 2. Acceso al Corazón del Proyecto (VITAL) 🚩
La mayoría de los errores ocurren por intentar ejecutar comandos en la carpeta raíz. Antes de continuar, **debes situarte en la carpeta donde está el código fuente**:
```bash
cd BibliotecaMVC/BibliotecaMVC
```
> [!IMPORTANT]
> Todos los comandos de los pasos 3, 4 y 5 deben ejecutarse **dentro** de esta carpeta. Si recibes el error *"Could not find a MSBuild project file"*, verifica que hiciste este `cd`.

### 3. Configuración de Secretos (Identity & APIs)
Los secretos (passwords y llaves) no se suben a GitHub por seguridad. Ejecuta estos comandos en orden:

```bash
# A. Inicializa el gestor de secretos (Solo se hace una vez)
dotnet user-secrets init

# B. Configura tu servidor de correo (SMTP)
dotnet user-secrets set "EmailSettings:Username" "tu_correo@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "tu_app_password"

# C. Configura las llaves de Twilio (WhatsApp/SMS)
dotnet user-secrets set "TwilioSettings:AccountSid" "tu_sid"
dotnet user-secrets set "TwilioSettings:AuthToken" "tu_token"
dotnet user-secrets set "TwilioSettings:FromPhoneNumber" "+1234567890"

# D. Define la contraseña maestra del Administrador
dotnet user-secrets set "AdminSettings:Password" "TuPasswordAdmin123!"
```

### 4. Preparación de la Base de Datos
Asegúrate de tener **SQL Server** iniciado y ejecuta la migración para crear las tablas:
```bash
dotnet ef database update
```

### 5. Lanzamiento
Inicia el servidor de desarrollo:
```bash
dotnet run
```

---

## 🔐 Acceso Administrativo
Una vez que la aplicación esté corriendo, el sistema crea automáticamente un administrador inicial con las credenciales que configuraste:
- **Usuario:** `dgomezpulid@outlook.com`
- **Contraseña:** La que definiste en el **Paso 3-D**.

---
*Desarrollado con estándares de Clean Code, MVC Patterns y auditoría de seguridad preventiva.*

