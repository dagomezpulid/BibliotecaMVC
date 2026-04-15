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
                    _logger.LogInformation($"[CRON JOB] Vigilante en reposo. Próximo escaneo a las {target:g} (en {delay.TotalHours:N1} horas).");

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
                    var smsSender = scope.ServiceProvider.GetRequiredService<ISmsSender>();

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
                            string titulo = p.Libro?.Titulo ?? "desconocido";
                            string date = p.FechaDevolucionProgramada.ToShortDateString();
                            
                            string smsBody = $"🔴 BibliotecaMVC (URGENTE): Tu préstamo del libro '{titulo}' expiró el {date}. " +
                                             $"Entrégalo HOY a la central para detener la acumulación de MULTAS diarias.";

                            await smsSender.SendSmsAsync(p.Usuario.PhoneNumber, smsBody);
                            
                            // Activar la bandera de Memoria para no volver a escribirle mañana
                            p.AlertaMoraEnviada = true; 
                            enviadosContador++;
                        }
                    }

                    if (enviadosContador > 0)
                    {
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"[VIGILANTE NOCTURNO] Termino patrulla: Castigo/Notifico a {enviadosContador} morosos nuevos.");
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
