using BibliotecaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Servicio en segundo plano (Cron Job) que patrulla la base de datos diariamente.
    /// Detecta préstamos vencidos que aún no han sido notificados y envía una alerta SMS automática.
    /// </summary>
    public class SmsBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SmsBackgroundWorker> _logger;

        /// <summary>
        /// Inicializa el worker con el proveedor de servicios y el logger.
        /// Se usa IServiceProvider (en vez de inyección directa) porque BackgroundService
        /// tiene ciclo de vida Singleton y necesita crear scopes transitorios para acceder al contexto.
        /// </summary>
        /// <param name="serviceProvider">Fábrica de scopes de DI.</param>
        /// <param name="logger">Logger para diagnóstico de la tarea en segundo plano.</param>
        public SmsBackgroundWorker(
            IServiceProvider serviceProvider,
            ILogger<SmsBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Ciclo principal del worker. Se ejecuta de forma continua mientras la aplicación esté activa,
        /// disparando el escaneo de mora cada 24 horas.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación que permite terminar el ciclo de limpiamente al apagar el servidor.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[CRON JOB EMPEZADO] Motor Automatico de SMS patrullando en 2do plano.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await EnviarAlertasAutomaticas(stoppingToken);
                    
                    var now = DateTime.Now;
                    var target = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0); // 8:00 AM
                    if (now >= target)
                    {
                        target = target.AddDays(1);
                    }
                    var delay = target - now;

                    // Salvaguarda: Si el delay es muy pequeño o negativo por deriva del reloj, esperar al menos 1 minuto.
                    if (delay.TotalSeconds <= 0) delay = TimeSpan.FromMinutes(1);

                    _logger.LogInformation("[CRON JOB] Vigilante en reposo. Próximo escaneo a las {Target:g} (en {Delay:N1} horas).", target, delay.TotalHours);

                    await Task.Delay(delay, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Captura el cierre normal de la aplicación para evitar reportes de errores en la consola/depurador
                _logger.LogInformation("[CRON JOB FINALIZADO] El motor de SMS se ha detenido correctamente.");
            }
        }

        /// <summary>
        /// Escanea la base de datos buscando préstamos vencidos sin notificar
        /// y envía un SMS de alerta urgente a cada usuario infractor.
        /// Marca la bandera AlertaMoraEnviada para evitar mensajes repetidos.
        /// </summary>
        /// <param name="stoppingToken">Token para interrumpir la operación si el servidor se está apagando.</param>
        private async Task EnviarAlertasAutomaticas(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    _logger.LogInformation("[VIGILANTE NOCTURNO] Escaneando Base de Datos buscando deudores fugitivos...");

                    // Encontrar los préstamos que expiran hoy o antes y su flag 'Enviada' está apagada
                    var prestamosVencidos = await context.Prestamos
                        .Include(p => p.Usuario)
                        .Include(p => p.Libro)
                        .Where(p => p.FechaDevolucionReal == null 
                                 && DateTime.Now > p.FechaDevolucionProgramada 
                                 && p.AlertaMoraEnviada == false)
                        .ToListAsync(stoppingToken);

                    int enviadosContador = 0;

                    foreach (var p in prestamosVencidos)
                    {
                        if (p.Usuario != null && !string.IsNullOrEmpty(p.Usuario.PhoneNumber))
                        {
                            try 
                            {
                                string titulo = p.Libro?.Titulo ?? "desconocido";
                                string date = p.FechaDevolucionProgramada.ToShortDateString();
                                
                                string smsBody = $"🔴 BibliotecaMVC (URGENTE): Tu préstamo del libro '{titulo}' expiró el {date}. " +
                                                 $"Entrégalo HOY a la central para detener la acumulación de MULTAS diarias.";

                                await notificationService.SendSmsAsync(p.Usuario, titulo, $"Tu préstamo expiró el {date}. Entrégalo pronto.");
                                await notificationService.CreateNotificationAsync(p.UsuarioId!, "⚠️ Mora Detectada", $"Tu préstamo de '{titulo}' ha vencido.", "warning");
                                
                                // Activar la bandera y persistir inmediatamente para evitar doble envío en caso de colapso posterior
                                p.AlertaMoraEnviada = true;
                                await context.SaveChangesAsync(stoppingToken); 
                                enviadosContador++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "[VIGILANTE] Error al procesar notificación para Préstamo ID {PrestamoId}", p.Id);
                            }
                        }
                    }

                    if (enviadosContador > 0)
                    {
                        _logger.LogInformation("[VIGILANTE NOCTURNO] Termino patrulla: Se notificó exitosamente a {Contador} morosos.", enviadosContador);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FATAL ERROR] Colapso del Motor SMS Automatico.");
            }
        }
    }
}
