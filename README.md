# 🏛️ BibliotecaMVC: Sistema de Gestión de Vanguardia

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512bd4.svg)](https://dotnet.microsoft.com/download)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-blue.svg)](https://docs.microsoft.com/ef/core/)
[![SignalR](https://img.shields.io/badge/RealTime-SignalR-orange.svg)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**BibliotecaMVC** no es solo un gestor de libros; es un ecosistema digital diseñado para bibliotecas de alto rendimiento. Combina una arquitectura robusta en **ASP.NET Core 10** con una interfaz ultra-moderna inspirada en el **Glassmorphism** y una experiencia de usuario fluida y reactiva.

---

## 🏗️ Arquitectura Técnica Detallada

El sistema sigue un patrón de **Arquitectura en Capas** con desacoplamiento mediante inyección de dependencias, lo que permite una escalabilidad y mantenimiento superior.

```mermaid
graph TB
    subgraph Client ["🖥️ Interfaz de Usuario (Frontend)"]
        UI["Vistas Razor + Bootstrap 5 (Glassmorphism)"]
        JS["JavaScript ES2022 / SignalR Client"]
        QR["Lector de Metadatos ISBN"]
    end

    subgraph App ["⚙️ ASP.NET Core 10 (Backend)"]
        subgraph Controllers ["Controladores & Endpoints"]
            LC["LibrosController (Gestión Catálogo)"]
            PC["PrestamosController (Lógica Circular)"]
            NH["NotificationHub (SignalR Push)"]
            DC["DashboardController (Analítica)"]
        end

        subgraph Services ["Capa de Negocio (Service Layer)"]
            LS["ILibroService (Persistencia y Vault)"]
            PS["IPrestamoService (Reglas de Negocio)"]
            ES["IEmailSender (Notificaciones SMTP)"]
            TS["ITwilioSmsSender (Alertas SMS)"]
        end

        subgraph Security ["Seguridad & Auditoría"]
            IC["Identity Core (RBAC - Admin/Usuario)"]
            AL["Audit Logger (Seguimiento de Activos)"]
        end
    end

    subgraph Storage ["💾 Persistencia & Recursos"]
        DB[("SQL Server / EF Core 10")]
        Vault["Digital Vault (File System Protegido)"]
    end

    subgraph External ["🌐 Integraciones Externas"]
        GBA["Google Books API (Metadata Primary)"]
        OLA["OpenLibrary API (Metadata Fallback)"]
        TWI["Twilio API Gateway (SMS)"]
    end

    %% Conexiones Técnicas
    Client -- "HTTP / HTTPS Requests" --> Controllers
    JS <--> NH
    Controllers -- "Inyección de Dependencias" --> Services
    Services --> Security
    Services -- "Consulta Metadata" --> GBA & OLA
    Services -- "Notificaciones Globales" --> NH & ES & TS
    TS --> TWI
    Security -- "Persistencia de Logs" --> DB
    Services -- "ORM Mapping" --> DB
    LS & PC -- "I/O Stream (Read/Write)" --> Vault
```

---

## 🌟 Características de Élite

### 1. 🧬 Inteligencia de Datos ISBN (Multi-Servicio)
El corazón del registro de libros ahora cuenta con una estrategia de **búsqueda persistente multi-capa**:
- **Google Books API Integration**: Recuperación automática de metadatos técnicos (sinopsis, autores, categorías).
- **OpenLibrary Fallback**: Si Google Books no responde o tiene límites de cuota, el sistema alterna inteligentemente a OpenLibrary para asegurar el autocompletado.
- **Normalización Agresiva**: Limpieza automática de ISBNs (guiones, espacios, caracteres especiales) para una precisión del 100%.

### 2. 🛡️ Bóveda Digital y DRM (Digital Rights Management)
Seguridad absoluta para tus activos digitales:
- **Digital Vault**: Almacenamiento físico segregado (`BibliotecaLibros_Vault`) fuera de la raíz web para prevenir accesos directos.
- **Auditoría Proactiva**: Registro detallado de quién, cuándo y desde qué IP se descarga o lee un libro digital.
- **Streaming de Contenido**: Visor inmersivo que sirve archivos directamente desde la bóveda con soporte para rangos de bytes.

### 3. 📊 Dashboard Predictivo y Real-Time
- **SignalR Push Engine**: Notificaciones en tiempo real sobre préstamos próximos a vencer y nuevas adquisiciones.
- **Visualización con Chart.js**: Gráficos dinámicos que analizan la rotación del catálogo y preferencias de los lectores.
- **Notificaciones Multi-Canal**: Integración nativa con **Twilio SMS** y envío de correos electrónicos transaccionales.

---

## 🛠️ Stack Tecnológico

| Capa | Tecnologías |
| :--- | :--- |
| **Backend** | .NET 10.0, C# 13, SignalR, Identity Core |
| **Persistencia** | SQL Server, Entity Framework Core 10 (Code First) |
| **Frontend** | Bootstrap 5, JavaScript ES2022, Animate.css, Chart.js |
| **Servicios** | Twilio SMS API, Google Books API, OpenLibrary API |
| **Arquitectura** | Service Layer, Repository Pattern (Lite), Audit Logging |

---

## 🚀 Instalación Rápida

1. **Clonar y Restaurar**:
   ```bash
   git clone https://github.com/dagomezpulid/BibliotecaMVC.git
   cd BibliotecaMVC
   dotnet restore
   ```

2. **Configuración de Secretos**:
   Para evitar exponer información sensible en `appsettings.json`, utiliza la herramienta de **User Secrets**:

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "AdminEmail" "admin@biblioteca.com"
   dotnet user-secrets set "AdminPassword" "TuPasswordSeguro123!"
   dotnet user-secrets set "Twilio:AccountSid" "tu_sid"
   dotnet user-secrets set "Twilio:AuthToken" "tu_token"
   dotnet user-secrets set "Twilio:FromNumber" "tu_numero"
   dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
   dotnet user-secrets set "EmailSettings:Port" "587"
   dotnet user-secrets set "EmailSettings:Username" "tu-email@gmail.com"
   dotnet user-secrets set "EmailSettings:Password" "tu-app-password"
   ```

3. **Base de Datos**:
   Las migraciones están listas. Ejecuta:
   ```bash
   dotnet ef database update
   ```

4. **Ejecutar**:
   ```bash
   dotnet run
   ```

---

## 📁 Estructura del Proyecto

- `BibliotecaMVC/Services`: Lógica de negocio pura (Préstamos, Libros, SMS, Email).
- `BibliotecaMVC/Hubs`: Canales de comunicación SignalR.
- `BibliotecaMVC/Models`: Entidades ricas y auditoría de logs.
- `BibliotecaLibros_Vault`: Carpeta protegida para PDFs y EPUBs (se crea automáticamente).

---

## 👨‍💻 Contribuciones
Este proyecto es una muestra de ingeniería de software moderna. Siéntete libre de clonarlo y proponer mejoras en la capa de IA o integración de lectores PDF.js avanzados.

**Desarrollado con ❤️ para la comunidad de ASP.NET**
