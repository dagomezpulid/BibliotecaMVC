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

### 2. 🌐 Ecosistema de Biblioteca 100% Digital
- **Visor Inmersivo Integrado (Web Reader):** Capacidad de leer archivos nativos (como PDFs) a pantalla completa directamente desde el ecosistema sin requerir descargas.
- **Túneles de Privacidad (DRM Backend):** Los libros digitales yacen alojados en una Bóveda Confidencial (Vault). El acceso se proporciona mediante URLs temporales, imposibilitando los ataques de "Fuga Pública" o desvío de hipervínculos de archivos.
- **Motor AI de Metadatos Bibliográficos:** La carga de nuevos ejemplares ahora cuenta con una integración a Base de Datos Mundial mediante ISBN (Google Books API). Implementa un barrido de información masiva que mapea autores, categorías estandarizadas, sinopsis limpiadas de HTMl y portadas adaptativas.
- **Garbage Collector Discreto:** Detecta automáticamente ficheros obsoletos en el servidor cuando el usuario sustituye un libro antiguo y lo elimina para asegurar el uso responsable del espacio en disco duro.
- **Soporte Multiformato Seguro:** Plataforma de carga de volúmenes con validación estricta de documentos PDF, EPUB y Microsoft Word Docx. Almacenados inteligentemente bajo inyecciones GUID anti-colisión.

### 3. 📖 Experiencia de Usuario Interactiva
- **Catálogo Inteligente:** Motor de búsqueda dinámica que filtra por Título, Autor, Categoría o ISBN con paginación segmentada ultrarrápida.
- **⭐ Sistema de Reseñas:** Calificación numérica (1-5 estrellas) y comentarios sobre cada obra con protección anti-spam.
- **Muros de Confirmación Dinámica:** Las transacciones de Préstamo despliegan portadas en alta definición y resúmenes de sinopsis anticipando la experiencia para el solicitante.
- **💡 Motor de Recomendaciones:** Algoritmo integrado que sugiere lecturas similares basándose en la categoría y el autor del libro visualizado.
- **❤️ Wishlist (Favoritos) de un-clic:** Persistencia de selecciones mediante operaciones AJAX en vivo para una lectura futura.
- **🔔 Centro de Notificaciones:** Sistema interno de alertas persistentes para comunicar estados de préstamos, multas y mensajes del sistema.

### 4. 🔄 Motor Avanzado de Préstamos y Multas
- **Gestión de Stock en Tiempo Real:** Validación estricta que impide préstamos sin existencias y restaura stock automáticamente en devoluciones.
- **🛡️ Concurrencia Optimista:** Implementación de tokens de concurrencia (`RowVersion`) para evitar colisiones de datos y asegurar la integridad del inventario en accesos simultáneos.
- **Límites de Uso:** Máximo 3 préstamos activos por usuario para garantizar la rotación del inventario.
- **Calculadora de Mora y Amnistía:** Procesamiento automático de días de retraso con generación de recargos financieros, con potestad del Administrador para dictaminar "Amnistía" global o particular en devoluciones complicadas.

### 5. 📲 Infraestructura de Mensajería (WhatsApp/Cron Jobs)
- **Mensajería Twilio WhatsApp/SMS:** Envío transaccional en tiempo real para generar huella física fuera de la aplicación (solicitudes, atrasos agresivos).
- **Vigilante Nocturno Automatizado (Cron Job):** BackgroundWorker (`IHostedService`) que patrulla la base de datos de manera silenciosa detectando deudores furtivos y disparando alertas sincronizadas.

### 6. 🛠️ Excelencia Técnica y Auditoría (Hardening)
- **Hardening de Identidad:** Requisitos de contraseñas de alta complejidad y bloqueo agresivo de cuentas tras 5 intentos fallidos o moras severas.
- **Unificación Premium Global:** Implementación de Aspect-Ratios (Proporción aurea) de CSS universales e integrados globalmente que desvían renders rotos, entregando la mejor apariencia bajo Tema Oscuro o Claro automáticamente.
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

