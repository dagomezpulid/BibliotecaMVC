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

        public SmsBackgroundWorker(
            IServiceProvider serviceProvider,
            ILogger<SmsBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[CRON JOB EMPEZADO] Motor Automatico de SMS patrullando en 2do plano.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await EnviarAlertasAutomaticas(stoppingToken);
                
                // El centinela se duerme por 24 horas y despierta mañana
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

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
