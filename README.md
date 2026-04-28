# 🏛️ BibliotecaMVC: Sistema de Gestión de Vanguardia

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512bd4.svg)](https://dotnet.microsoft.com/download)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-blue.svg)](https://docs.microsoft.com/ef/core/)
[![SignalR](https://img.shields.io/badge/RealTime-SignalR-orange.svg)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**BibliotecaMVC** es un ecosistema digital de vanguardia diseñado para la gestión de activos bibliográficos de alto rendimiento. Combina una arquitectura desacoplada en **ASP.NET Core 10** con una interfaz ultra-moderna basada en **Glassmorphism**, ofreciendo una experiencia reactiva y segura para la era digital.

---

## 🏗️ Arquitectura Técnica Detallada

El sistema implementa una **Arquitectura en Capas** reforzada con el patrón de Inyección de Dependencias, garantizando un mantenimiento modular y escalabilidad horizontal.

```mermaid
graph TB
    subgraph Client ["🖥️ Interfaz de Usuario (Frontend)"]
        UI["Vistas Razor + Bootstrap 5 (Glassmorphism)"]
        JS["JavaScript ES2022 / SignalR Client"]
        VC["ViewComponents (Widgets Modulares)"]
        CH["Chart.js (Analítica Dinámica)"]
    end

    subgraph App ["⚙️ ASP.NET Core 10 (Backend)"]
        subgraph Controllers ["Controladores & Endpoints"]
            LC["LibrosController (Catálogo)"]
            PC["PrestamosController (Circular)"]
            NH["NotificationHub (SignalR)"]
            AC["AdminController (Dashboard BI)"]
        end

        subgraph Services ["Capa de Negocio (Service Layer)"]
            LS["ILibroService (Vault V2)"]
            PS["IPrestamoService (Rules Engine)"]
            NS["INotificationService (SignalR/SMS)"]
        end

        subgraph Workers ["🤖 Procesamiento Background"]
            SW["SmsBackgroundWorker (Vigilante Nocturno)"]
        end

        subgraph Security ["Seguridad & Auditoría"]
            IC["Identity Core (RBAC)"]
            JT["Jitter Engine (Anti-Enumeration)"]
            XV["XSS Validator (JSON Serialize)"]
        end
    end

    subgraph Storage ["💾 Persistencia & Recursos"]
        DB[("SQL Server / EF Core 10")]
        Vault["Digital Vault (Secure Storage)"]
    end

    subgraph External ["🌐 Integraciones Externas"]
        GBA["Google Books API"]
        TWI["Twilio SMS/WhatsApp"]
    end

    %% Conexiones
    Client -- "AJAX / SignalR" --> Controllers
    Controllers -- "DI" --> Services
    Services -- "Job Dispatch" --> Workers
    Workers -- "Auto-Alert" --> External
    Services -- "ORM" --> DB
    Services -- "Metadata" --> GBA
    Services -- "File I/O" --> Vault
```

---

## 🌟 Características de Élite

### 1. 🧬 Inteligencia de Datos ISBN (Resiliencia Multi-Capa)
Motor de autocompletado inteligente con estrategia de fallback:
- **Google Books Primary**: Recupera títulos, autores, categorías y portadas de alta resolución.
- **OpenLibrary Fallback**: Garantiza la disponibilidad de metadatos incluso si las cuotas de Google se agotan.
- **Normalización Inteligente**: Sanitización de ISBNs y manejo de descripciones estructuradas para evitar errores de renderizado.

### 2. 🛡️ Bóveda Digital Segura (Vault System V2)
- **Segregación Física**: Los archivos (`BibliotecaLibros_Vault`) se almacenan fuera del `wwwroot`, impidiendo el acceso directo por URL.
- **Validación Estricta**: Whitelist de extensiones (.pdf, .epub, .docx, .txt) para prevenir la subida de scripts maliciosos.
- **Limpieza Automática**: Motor de gestión de almacenamiento que elimina archivos huérfanos al actualizar libros, optimizando el espacio en disco.
- **DRM Proactivo**: El acceso requiere un préstamo activo validado en tiempo real. Auditoría completa de cada descarga/lectura.

### 3. 📊 Analítica Predictiva y Real-Time
- **SignalR Push Engine**: Alertas instantáneas al dashboard administrativo y notificaciones de usuario.
- **Omnicanalidad**: Notificaciones vía **Twilio SMS** (con soporte para WhatsApp) y **SMTP Transaccional**.
- **BI Integrado**: Dashboards dinámicos con Chart.js para monitoreo de morosidad (Top Morosos), popularidad de títulos y tendencias de préstamos en los últimos 6 meses.
- **Mitigación de Ataques**: Implementación de **Jitter** (retraso aleatorio) en endpoints de validación para prevenir la enumeración de cuentas por bots y anonimización de datos sensibles en logs.

### 4. 🤖 Procesamiento Asíncrono (Vigilante Nocturno)
El sistema incluye un **Worker en Segundo Plano** (`SmsBackgroundWorker`) que:
- **Patrullaje Diario**: Escanea la base de datos cada 24 horas buscando préstamos vencidos.
- **Alertas Proactivas**: Envía mensajes de texto (SMS) automáticos a usuarios con libros en mora, incentivando la devolución sin intervención manual del administrador.
- **Optimización de Recursos**: Utiliza inyección de dependencias mediante scopes transitorios para garantizar la estabilidad de la conexión a base de datos en tareas largas.

### 5. 💎 Estética y Experiencia de Usuario (Glassmorphism)
- **Interfaz Premium**: Diseño basado en transparencia, desenfoque de fondo (backdrop-filter) y bordes sutiles.
- **Micro-animaciones**: Transiciones fluidas usando Animate.css y CSS3 dinámico para una sensación de aplicación moderna y "viva".
- **Dashboards Modulares**: Uso de **ViewComponents** para encapsular widgets complejos (como el resumen de multas del perfil) mejorando la reutilización de código.
- **Rendimiento Optimizado**: Resolución de problemas de **N+1** mediante carga impaciente (Eager Loading) en el dashboard BI, permitiendo tiempos de respuesta instantáneos incluso con miles de registros.

---

## 🛠️ Stack Tecnológico

| Capa | Tecnologías |
| :--- | :--- |
| **Backend** | .NET 10.0, C# 13, SignalR, Identity Core |
| **Persistencia** | SQL Server (Express), EF Core 10 (Migrations) |
| **Frontend** | Bootstrap 5, JS ES2022, Animate.css, Chart.js |
| **Servicios** | Twilio API, Google Books API, OpenLibrary API |

---

## 🚀 Instalación y Configuración

### 1. Requisitos Previos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **SQL Server Express** (Instancia local `.\SQLEXPRESS`)

### 2. Despliegue Inicial
```bash
git clone https://github.com/dagomezpulid/BibliotecaMVC.git
cd BibliotecaMVC
dotnet restore
```

### 3. Configuración de Secretos (Crítico)
Para que el sistema funcione correctamente (especialmente en entornos nuevos), configura los **User Secrets**:

```bash
dotnet user-secrets init

# Administrador Inicial (Se creará al arrancar la app)
dotnet user-secrets set "AdminSettings:Email" "tu-email@ejemplo.com"
dotnet user-secrets set "AdminSettings:Password" "TuPasswordSeguro123!"

# Twilio (SMS/WhatsApp)
dotnet user-secrets set "TwilioSettings:AccountSid" "tu_sid"
dotnet user-secrets set "TwilioSettings:AuthToken" "tu_token"
dotnet user-secrets set "TwilioSettings:FromPhoneNumber" "+123456789"

# Email (SMTP Transaccional)
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:Port" "587"
dotnet user-secrets set "EmailSettings:Username" "tu-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "tu-app-password"
```

### 4. Base de Datos y Ejecución
```bash
dotnet ef database update
dotnet run
```

---

## 🔒 Privacidad y "Derecho al Olvido" (GDPR Ready)

El sistema implementa un flujo de **Anonimización Segura** para usuarios que solicitan la eliminación de su cuenta:
- **Preservación de Métricas**: Se eliminan nombres, apellidos, correos y claves, pero se mantienen los registros de préstamos y multas anonimizados para no romper la analítica histórica del sistema.
- **Bloqueo Permanente**: Las cuentas eliminadas quedan inaccesibles mediante la invalidación de sus credenciales y bloqueo administrativo total.
- **Limpieza de PII**: El `PhoneNumber` y otros datos de identificación personal son purgados del servidor de forma definitiva.

---

## 📁 Estructura del Proyecto

- `Controllers`: Endpoints desacoplados de la lógica de negocio.
- `Services`: Implementaciones de `ILibroService` y `IPrestamoService`.
- `Models`: Entidades POCO con anotaciones de validación y Fluent API.
- `Hubs`: WebSocket endpoints para notificaciones real-time.
- `BibliotecaLibros_Vault`: Almacenamiento seguro de archivos digitales (Generado automáticamente).

---