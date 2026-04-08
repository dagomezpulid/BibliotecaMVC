using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BibliotecaMVC.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
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
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres, contener mayúsculas, minúsculas, y números.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe confirmar su contraseña.")]
            [Display(Name = "Confirmar contraseña")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Verificación de correo electrónico duplicado
            var existingEmail = await _userManager.FindByEmailAsync(Input.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Input.Email", "Ya existe una cuenta registrada con este correo electrónico.");
            }

            // Verificación de número telefónico duplicado
            var existingPhone = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == Input.PhoneNumber);
            if (existingPhone != null)
            {
                ModelState.AddModelError("Input.PhoneNumber", "Ya existe una cuenta registrada con este número telefónico.");
            }

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

