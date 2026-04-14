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
- **🛡️ Concurrencia Optimista:** Implementación de tokens de concurrencia (`RowVersion`) para evitar colisiones de datos y asegurar la integridad del inventario en accesos simultáneos.
- **Paginación Dinámica:** Catálogo segmentado mediante AJAX para garantizar una carga ultrarrápida independientemente del tamaño de la colección.
- **Límites de Uso:** Máximo 3 préstamos activos por usuario para garantizar la rotación del inventario.
- **Calculadora de Mora:** Procesamiento automático de días de retraso con generación de recargos financieros.

### 4. 📲 Infraestructura de Mensajería (WhatsApp/Cron Jobs)
- **Motor Twilio WhatsApp:** Notificaciones inmediatas al rentar un libro o al generarse una multa.
- **Vigilante Nocturno Automatizado (Cron Job):** `IHostedService` que patrulla la base de datos cada 24 horas detectando deudores y disparando alertas preventivas.

### 5. 🛠️ Excelencia Técnica y Auditoría (Hardening)
- **Hardening de Identidad:** Requisitos de contraseñas de alta complejidad (Símbolos obligatorios) y bloqueo agresivo de cuentas tras 5 intentos fallidos.
- **Validación Universal (ISBN):** Motor de expresiones regulares (Regex) para garantizar que los metadatos bibliográficos cumplan los estándares internacionales.
- **Blindaje de Secretos:** Implementación de **User Secrets** para llaves de API, credenciales de servidor y configuración administrativa raíz.
- **Defensa CSRF/XSS:** Tokens de verificación sincronizados globalmente y sanitización automática de Razor.

---

## 💻 Instalación y Configuración Paso a Paso

Sigue este orden exacto para poner en marcha el proyecto:

### 1. Preparación de Archivos
Clona el repositorio en tu máquina local:
```bash
git clone [URL-del-repositorio]
```

### 2. Acceso al Corazón del Proyecto (VITAL) 🚩
Sitúate en la carpeta del código fuente para ejecutar los comandos:
```bash
cd BibliotecaMVC/BibliotecaMVC
```

### 3. Configuración de Secretos (Identity & APIs)
Ejecuta estos comandos en la terminal para configurar tus credenciales privadas:

```bash
# A. Inicializa el gestor de secretos
dotnet user-secrets init

# B. Configura tu servidor de correo (SMTP)
dotnet user-secrets set "EmailSettings:Username" "tu_correo@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "tu_app_password"

# C. Configura las llaves de Twilio (WhatsApp/SMS)
dotnet user-secrets set "TwilioSettings:AccountSid" "tu_sid"
dotnet user-secrets set "TwilioSettings:AuthToken" "tu_token"
dotnet user-secrets set "TwilioSettings:FromPhoneNumber" "+1234567890"

# D. Configura la Identidad Maestra (Administrador Raíz)
dotnet user-secrets set "AdminSettings:Email" "tu-admin@ejemplo.com"
dotnet user-secrets set "AdminSettings:Password" "TuPasswordAdmin123!"
```

### 4. Preparación de la Base de Datos
Asegúrate de tener **SQL Server** iniciado y ejecuta las migraciones:
```bash
dotnet ef database update
```

### 5. Lanzamiento
```bash
dotnet run
```

---

## 🔐 Acceso Administrativo
El sistema crea automáticamente un administrador inicial basado en tus secretos del **Paso 3-D**:
- **Usuario:** El configurado en `AdminSettings:Email` (Default: `dgomezpulid@outlook.com`)
- **Contraseña:** La configurada en `AdminSettings:Password`.

---
*Desarrollado con estándares de Clean Code, MVC Patterns y auditoría de seguridad preventiva Fase 3.*

