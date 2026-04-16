using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Gestiona la lógica de autoregistro de nuevos lectores en el sistema.
    /// Implementa validaciones de fortaleza de clave y control de duplicidad (Email/Teléfono).
    /// </summary>
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUserValidationService _validationService;

        /// <summary>
        /// Inicializa el modelo de registro con los servicios de identidad, roles, logs, email y validación.
        /// </summary>
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IUserValidationService validationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _validationService = validationService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe registrar un Apellido.")]
            public string Apellido { get; set; } = string.Empty;

            [Required(ErrorMessage = "El número telefónico es obligatorio de proveer.")]
            [Phone]
            [Display(Name = "Teléfono Móvil / WhatsApp")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe proporcionar un correo electrónico.")]
            [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [Display(Name = "Contraseña")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 8)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas y números.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe confirmar su contraseña.")]
            [Display(Name = "Confirmar contraseña")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        /// <summary>
        /// Prepara la vista de registro.
        /// </summary>
        public void OnGet()
        {
        }

        /// <summary>
        /// Procesa el formulario de registro, valida la unicidad de datos y crea un nuevo usuario con rol de 'Usuario'.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Verificación de correo electrónico duplicado
            var emailError = await _validationService.CheckDuplicateEmailAsync(Input.Email);
            if (emailError != null)
                ModelState.AddModelError("Input.Email", emailError);

            // Verificación de número telefónico duplicado
            var phoneError = _validationService.CheckDuplicatePhone(Input.PhoneNumber);
            if (phoneError != null)
                ModelState.AddModelError("Input.PhoneNumber", phoneError);

            if (!ModelState.IsValid)
                return Page();

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                Nombre = Input.Nombre,
                Apellido = Input.Apellido,
                PhoneNumber = Input.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Usuario"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Usuario"));
                }

                await _userManager.AddToRoleAsync(user, "Usuario");

                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return RedirectToAction("Index", "Home");
        }
    }
}

