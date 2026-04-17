using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace BibliotecaMVC.Hubs
{
    /// <summary>
    /// Hub de SignalR para gestionar notificaciones en tiempo real.
    /// Solo permite conexiones de usuarios autenticados.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Se ejecuta cuando un usuario se conecta. 
        /// SignalR asocia automáticamente el UserIdentifier del usuario autenticado.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
