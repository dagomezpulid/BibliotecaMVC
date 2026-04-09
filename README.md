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

### 6. 🛡️ Ciberseguridad Defensiva (Auditoría Cero Fisuras)
- **Hardening de Secretos:** Eliminación de toda credencial real en el código fuente. Implementación de **User Secrets** para el manejo de llaves de API (Twilio), contraseñas SMTP y el acceso maestro administrativo.
- **Prevención de Enumeración:** Los endpoints de validación asíncrona han sido blindados para responder únicamente a peticiones legítimas de la aplicación (AJAX/Fetch), mitigando intentos de barrido o cosecha de datos por parte de atacantes externos.
- **Limpieza de Datos (Cascada Segura):** El flujo de baja de usuarios ha sido optimizado para limpiar recursivamente historiales de **Pagos**, Multas y Préstamos, evitando errores de integridad referencial y fugas de datos huérfanos.
- **Escudo Antibots CSRF:** Uso estricto de `[ValidateAntiForgeryToken]` en todas las transacciones sensibles.

### 7. 📲 Infraestructura de Mensajería (Twilio / WhatsApp)
- **Captura Rigurosa Cero-Evasión:** Reescritura del modelo nativo de Registro de Identity convirtiendo el contacto móvil en un componente **obligatorio** para garantizar que ningún lector ingrese de forma anónima al ecosistema físico.
- **Transmutación a WhatsApp (Sandbox):** El núcleo de telecomunicaciones fue elevado insertando el prefijo de protocolo `whatsapp:` a la API de Twilio, evadiendo las rigurosas mallas Anti-Spam (A2P 10DLC) de SMS internacionales y logrando una entregabilidad celular del 100% hacia cualquier país.
- **Motor de Refracción (Fire-and-Forget):** Inyección de Dependencias `ISmsSender` ligada a la SDK global de **Twilio**. El sistema despacha notificaciones a WhatsApp de manera asíncrona sin congelar la Interfaz Web en dos escenarios críticos:
  - **Confirmación Predictiva:** Notificación inmediata al rentar un libro dictando las reglas y la fecha de vencimiento.
  - **Multa Reactiva:** Notificación de sanción al devolver un libro con retraso temporal, anunciando la suspensión y la deuda financiera.
  - **Vigilante Nocturno Automatizado (Cron Job):** Diseño e implementación de una arquitectura en segundo plano (`IHostedService`). Un motor autónomo patrulla la base de datos cada 24 horas detectando deudores evadidos y disparando alertas preventivas automáticas, respaldado por mecanismos booleanos de seguridad en SQL Server (`AlertaMoraEnviada`) para impedir ciclos de acoso o mensajería duplicada.

### 8. 🧹 Refactorización y Rendimiento
- **Gestión de Memoria:** Eliminación de variables de estado redundantes y optimización de consultas Eager Loading para reducir el consumo de recursos en el servidor.
- **D.R.Y (Don't Repeat Yourself):** Consolidación de procesos de sembrado de datos (Seeding) y validaciones de identidad en servicios inyectados.

---

## 💻 Instalación y Configuración Local

1. **Clonación:** Clona el repositorio e instálate en el directorio raíz.
2. **Requisitos:** Asegúrate de tener **.NET 10.0 SDK** y **SQL Server** activos (LocalDB o Express).
3. **Configuración de Secretos (CRÍTICO):** Al eliminar las credenciales del `appsettings.json`, debes configurar tus llaves locales para que el sistema funcione:
   ```bash
   # Dentro de la carpeta BibliotecaMVC/BibliotecaMVC/
   dotnet user-secrets set "EmailSettings:Username" "tu_gmail@gmail.com"
   dotnet user-secrets set "EmailSettings:Password" "tu_app_password"
   dotnet user-secrets set "TwilioSettings:AccountSid" "tu_sid"
   dotnet user-secrets set "TwilioSettings:AuthToken" "tu_token"
   dotnet user-secrets set "AdminSettings:Password" "TuPasswordAdmin123!"
   ```
4. **Base de Datos:** Aplica las migraciones para construir el esquema:
   ```bash
   dotnet ef database update
   ```
5. **Ejecución:** Inicia el servidor de desarrollo:
   ```bash
   dotnet run
   ```

## 🔐 Acceso Administrativo
Al iniciar por primera vez, el sistema creará automáticamente el administrador raíz:
- **Usuario:** `dgomezpulid@outlook.com`
- **Contraseña:** La que hayas configurado en el comando `dotnet user-secrets set "AdminSettings:Password" ...` (Paso 3).

---
*Desarrollado con estándares de Clean Code, MVC Patterns y auditoría de seguridad preventiva.*
