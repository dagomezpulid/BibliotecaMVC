// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BibliotecaMVC.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Gestiona la autenticación de usuarios registrados.
    /// Valida credenciales, gestiona el estado de persistencia de la sesión 
    /// y redirige a la URL de origen tras un acceso exitoso.
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>Modelo de entrada para los datos del formulario.</summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>Lista de proveedores de autenticación externos configurados.</summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>URL de redirección post-autenticación.</summary>
        public string ReturnUrl { get; set; }

        /// <summary>Mensaje de error para mostrar en la interfaz.</summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>Estructura de datos para el login de usuario.</summary>
        public class InputModel
        {
            /// <summary>Correo electrónico del usuario.</summary>
            [Required(ErrorMessage = "Debe proporcionar un correo electrónico.")]
            [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
            public string Email { get; set; }

            /// <summary>Contraseña de acceso.</summary>
            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>Indicador para mantener la sesión activa.</summary>
            [Display(Name = "¿Recordarme?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Limpiar la cookie externa existente para asegurar un proceso de login limpio
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Los fallos de login no disparan el bloqueo de cuenta por defecto
                // Para habilitar el bloqueo por intentos fallidos, establecer lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
                    return Page();
                }
            }

            // Si llegamos aquí, algo falló; recargar la página con los errores del modelo
            return Page();
        }
    }
}
