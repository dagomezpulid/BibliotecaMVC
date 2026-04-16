// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BibliotecaMVC.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Gestiona la finalización de la sesión del usuario.
    /// Limpia las cookies de autenticación y redirige al inicio.
    /// </summary>
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Procesa la solicitud de cierre de sesión mediante el método GET.
        /// </summary>
        public async Task<IActionResult> OnGet()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión mediante GET.");
            return LocalRedirect("~/");
        }

        /// <summary>
        /// Procesa la solicitud de cierre de sesión mediante el método POST.
        /// </summary>
        /// <param name="returnUrl">URL a la cual redirigir tras el cierre de sesión.</param>
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión mediante POST.");
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirección forzada inmediata hacia Home
                return LocalRedirect("~/");
            }
        }
    }
}
