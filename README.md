# 📚 BibliotecaMVC: Centro de Gestión Bibliográfica de Vanguardia

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://docs.microsoft.com/ef/)
[![UI Design](https://img.shields.io/badge/UX-Premium%20Design-FF69B4)](file:///c:/Repos/BibliotecaMVC)
[![Status](https://img.shields.io/badge/Status-Production%20Ready-success)](file:///c:/Repos/BibliotecaMVC)

**BibliotecaMVC** es una plataforma de gestión bibliotecaria de alta gama que redefine la experiencia de préstamo digital. Diseñada bajo un estándar de **Estética Industrial**, integra analíticas avanzadas, un motor de lectura inteligente de última generación y una arquitectura de servicios desacoplada para una robustez de nivel empresarial.

---

## ✨ Características Premium

### 🎨 1. Estética Industrial y UX Adaptativa
*   **Diseño de Vanguardia**: Implementación de **Glassmorphism** (Efecto Cristal) y micro-animaciones dinámicas con **Animate.css**.
*   **Modo Oscuro Dinámico**: Interfaz 100% armonizada mediante variables CSS. Los componentes cambian orgánicamente basándose en las preferencias del sistema.
*   **Feedback Visual Real**: La campana de notificaciones reacciona físicamente (efecto *swing*) ante nuevos eventos en tiempo real.

### 🚀 2. Notificaciones en Tiempo Real (SignalR)
*   **Push Engine**: Integración con **SignalR** para la entrega inmediata de alertas sin necesidad de recarga de página.
*   **Reactividad Instantánea**: Los contadores de multas, préstamos y notificaciones se actualizan en el acto al producirse cambios en el servidor.
*   **Notificaciones Omnicanal**: Sincronización entre la interfaz web, alertas de base de datos y **Twilio SMS** para comunicaciones críticas.

### 🏛️ 3. Arquitectura de Servicios Reforzada
*   **Capa de Negocio (Service Layer)**: Desacoplamiento total de la lógica de controladores. Servicios especializados como `IPrestamoService`, `ILibroService` y `INotificationService` gestionan la integridad del sistema.
*   **Observabilidad y Logging**: Registro estructurado de eventos críticos mediante `ILogger`, permitiendo una trazabilidad profesional de transacciones y errores.
*   **Documentación XML Integral**: Código 100% documentado siguiendo estándares de la industria para facilitar el mantenimiento y la extensibilidad.

### 📖 4. Smart Reading Engine Multi-Formato
*   **Lector Word (.docx) & PDF**: Renderizado nativo en navegador con estética de "hoja física" y guardado persistente del progreso de lectura.
*   **Búsqueda Inteligente (AJAX)**: Filtrado asíncrono ultra-rápido por múltiples criterios.
*   **Digital Vault (DRM)**: Almacenamiento seguro de activos digitales fuera de la ruta pública para protección de derechos de autor.

### 📊 5. Inteligencia de Negocio
*   **Dashboards con Chart.js**: Visualizaciones reactivas al tema actual para control administrativo total de préstamos, multas y tendencias.

---

## 🏛️ Arquitectura del Ecosistema

```mermaid
graph TD
    User((Usuario / Lector)) -->|HTTPS| WebServer[ASP.NET Core 10 Engine]
    
    subgraph "Navegador (Frontend UX)"
        User -.-> UX[Interfaz Premium / Animate.css]
        UX -.-> SignalRClient[SignalR Connection]
        UX -.-> Reader[Smart Reading Engine]
        Reader --> PDF[Visor PDF Nativo]
        Reader --> Word[docx-preview]
    end

    subgraph "Núcleo del Servidor (Backend)"
        WebServer --> Auth[Identity Hardened]
        WebServer --> SignalRHub[NotificationHub]
        WebServer --> Controllers[Controladores MVC]
        
        subgraph "Service Layer (Negocio)"
            Controllers --> IPS[IPrestamoService]
            Controllers --> ILS[ILibroService]
            IPS --> INS[INotificationService]
            ILS --> INS
        end
        
        INS --> SignalRHub
        IPS --> EF[Entity Framework Core 10]
        EF --> DB[(SQL Server / LocalDB)]
    end

    subgraph "Integraciones Externas"
        INS -->|SMS| Twilio[Twilio SMS API]
        INS -->|Email| SMTP[SMTP Relay]
    end

    subgraph "Gestión de Activos"
        ILS -->|Control DRM| Vault[(Digital Vault - Folder)]
    end
```

---

## 🛠️ Stack Tecnológico
*   **Backend**: C# 13, .NET 10.0, EF Core 10.
*   **Real-Time**: ASP.NET Core SignalR.
*   **Frontend**: Bootstrap 5, JS ES2022, Animate.css, Chart.js.
*   **Mensajería**: Twilio SMS API.
*   **Seguridad**: Identity con Lockout, PhysicalFile Streaming (DRM), Auditoría de IP.

---

## 📁 Estructura de la Solución
*   **Hubs/**: Punto de entrada para comunicaciones en tiempo real.
*   **Services/**: Capa de servicios que contiene la médula de la lógica de negocio (Préstamos, Libros, Notificaciones).
*   **Controllers/**: Gestión de flujos de navegación y endpoints API.
*   **Models/**: Definición de entidades y contexto de datos EF Core.
*   **BibliotecaLibros_Vault/**: Repositorio físico de libros digitales protegido.

---

> [!IMPORTANT]
> **Aviso de Excelencia**: Esta plataforma ha sido refactorizada para cumplir con los más altos estándares de arquitectura de software, garantizando un código limpio, desacoplado y orientado al rendimiento.

*Desarrollado con arquitectura premium y pasión tecnológica.*
