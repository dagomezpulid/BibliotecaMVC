# 📚 BibliotecaMVC: Centro de Gestión Bibliográfica de Vanguardia

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4)](https://docs.microsoft.com/ef/)
[![UI Design](https://img.shields.io/badge/UX-Premium%20Design-FF69B4)](file:///c:/Repos/BibliotecaMVC)
[![Status](https://img.shields.io/badge/Status-Production%20Ready-success)](file:///c:/Repos/BibliotecaMVC)

**BibliotecaMVC** es una plataforma de gestión bibliotecaria de alta gama que redefine la experiencia de préstamo digital. Diseñada bajo un estándar de **Estética Industrial**, integra analíticas avanzadas, un motor de lectura inteligente de última generación y una arquitectura de seguridad robusta para el manejo de activos digitales.

---

## ✨ Características Premium

### 🎨 1. Estética Industrial y UX Adaptativa
*   **Diseño de Vanguardia**: Implementación de **Glassmorphism** (Efecto Cristal) en tarjetas de libros para una profundidad visual superior que detecta y se adapta al tema actual.
*   **Modo Oscuro Dinámico**: Interfaz 100% armonizada mediante variables CSS y `backdrop-filter`. Los componentes cambian orgánicamente basándose en las preferencias del sistema o del usuario.
*   **Diferenciación de Interacción**: Jerarquía visual clara entre "Acciones" (botones sólidos, `rounded-3`) y "Estados" (badges de cápsula, traslúcidos), eliminando la carga cognitiva.

### 📊 2. Inteligencia de Negocio y Analíticas
*   **Insights en Tiempo Real**: Dashboards administrativos potenciados por `Chart.js` con visualización adaptativa (textos y grillas que cambian de color automáticamente según el tema).
*   **Tendencias de Préstamos**: Análisis gráfico de la actividad mensual para la toma de decisiones basada en datos históricos.
*   **Control de Morosidad**: Monitorización visual de deudas y días de retraso mediante gráficos de impacto para una supervisión administrativa eficiente.

### 📖 3. Smart Reading Engine & Búsqueda
*   **Búsqueda Inteligente (AJAX)**: Filtrado en tiempo real sin recarga de página por título, autor o categoría, garantizando una exploración fluida.
*   **Navegación Centralizada**: El sistema utiliza el Catálogo de Libros como eje central, asegurando que todos los botones de retorno y accesos directos lleven al corazón del ecosistema.
*   **Visor Inmersivo con Memoria**: Experiencia de lectura fluida que guarda automáticamente la página exacta donde se detuvo el usuario, sincronizando el progreso de forma persistente.

### 🔔 4. Centro de Notificaciones y Mensajería
*   **Centro de Alertas**: Un buzón de notificaciones en tiempo real (vía polling suave) que informa sobre multas generadas, préstamos confirmados y recordatorios del sistema.
*   **Notificaciones Omnicanal**: Integración nativa con **Twilio SMS** para enviar alertas críticas directo al teléfono del usuario cuando se detecta morosidad.

### 🛡️ 5. Ingeniería de Software y Seguridad
*   **Documentación de Grado Industrial**: Todo el backend cuenta con una infraestructura completa de **XML Documentation (Summaries)**, asegurando que cada controlador, servicio y entidad esté debidamente documentada para su escalabilidad.
*   **Auditoría de Seguridad (IDOR/CSRF)**: Implementación de validaciones estrictas de propiedad de datos en acciones transaccionales y protección endurecida contra ataques de falsificación de peticiones.
*   **Integridad Relacional**: Lógica avanzada de borrado que valida la coexistencia de registros y migraciones a "usuarios fantasma" para preservar la integridad de las analíticas históricas.

---

## 🏛️ Arquitectura del Ecosistema

```mermaid
graph TD
    User((Usuario/Admin)) -->|HTTPS| WebServer[ASP.NET Core MVC Engine]
    subgraph "Core Business Logic"
        WebServer --> Controllers[Controllers]
        Controllers --> Services[Business Services]
        Services --> EF[Entity Framework Core]
        EF --> DB[(SQL Server / LocalDB)]
    end
    subgraph "Advanced Integrations"
        Services -->|SMS/WhatsApp| Twilio[Twilio Messaging]
        Services -->|Analytics| Charts[Chart.js Adaptive Engine]
        Controllers -->|Real-time| Notif[Notification System]
    end
    subgraph "Asset Management"
        Controllers -->|Access Control| Vault[Digital Vault Infrastructure]
        WebServer -->|Auth| Identity[ASP.NET Identity Hardened]
    end
    Worker[SmsBackgroundWorker] -->|IHostedService| Services
```

---

## 🛠️ Stack Tecnológico
*   **Backend**: C# 12, ASP.NET Core 8.0+, Entity Framework Core.
*   **Frontend**: Bootstrap 5 + CSS Custom Properties, JavaScript ES6 (Fetch/Async), Chart.js.
*   **Cloud**: Twilio SMS API para notificaciones transaccionales.
*   **Seguridad**: Identity con políticas de bloqueo, PhysicalFile Streaming para DRM y validaciones de IDOR multi-nivel.
*   **Estándar**: Documentación XML Integral y Clean Code Architecture.

---

## 💻 Guía de Despliegue y Configuración

### 1. Gestión de Secretos
El sistema protege tus credenciales críticas mediante `dotnet user-secrets`. Configura tus llaves antes de iniciar:

```powershell
# Iniciar gestión de secretos en el proyecto
dotnet user-secrets init

# Configuración Administrativa
dotnet user-secrets set "AdminSettings:Email" "admin@bibliotecamvc.com"
dotnet user-secrets set "AdminSettings:Password" "TuPasswordSeguro123!"

# Configuración Twilio (SMS)
dotnet user-secrets set "TwilioSettings:AccountSid" "ACXXXXXXXXXX"
dotnet user-secrets set "TwilioSettings:AuthToken" "tu_token_aqui"
dotnet user-secrets set "TwilioSettings:FromPhoneNumber" "+123456789"
```

### 2. Inicialización del Ecosistema
```powershell
# Aplicar esquema de base de datos
dotnet ef database update

# Ejecutar el servidor
dotnet run
```

---

## 📁 Estructura de la Solución
*   **BibliotecaLibros_Vault/**: Repositorio físico protegido (DRM) fuera de la ruta pública.
*   **Services/**: Motores de SMS, Notificaciones y Lógica de Negocio.
*   **Controllers/**: Flujos de Administración, Préstamos y Catálogo AJAX.
*   **ViewComponents/**: Componentes de UI modulares (ej. Alertas de Multa).

---

> [!IMPORTANT]
> **Aviso de Cumplimiento**: Esta plataforma implementa validaciones de seguridad multicapa (IDOR, CSRF) y un diseño orientado a la excelencia operacional.

*Desarrollado con arquitectura premium y pasión tecnológica.*
