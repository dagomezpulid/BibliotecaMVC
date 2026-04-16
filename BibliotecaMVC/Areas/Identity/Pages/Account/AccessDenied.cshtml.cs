// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BibliotecaMVC.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Gestiona la visualización cuando un usuario intenta acceder a un recurso 
    /// para el cual no tiene los roles o permisos suficientes.
    /// </summary>
    public class AccessDeniedModel : PageModel
    {
        /// <summary>
        /// Ejecuta la lógica inicial al cargar la página de acceso denegado.
        /// </summary>
        public void OnGet()
        {
        }
    }
}
