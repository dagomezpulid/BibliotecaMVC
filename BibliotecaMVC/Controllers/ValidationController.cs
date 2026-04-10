using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Provee endpoints de validación asíncrona (AJAX) para formularios.
    /// Utilizado principalmente por jQuery Validation Unobtrusive para validaciones en tiempo real.
    /// Incluye medidas para mitigar ataques de enumeración.
    /// </summary>
    [Route("[controller]")]
    public class ValidationController : Controller
    {
        private readonly IUserValidationService _validationService;

        public ValidationController(IUserValidationService validationService)
        {
            _validationService = validationService;
        }

        /// <summary>
        /// Valida si un correo electrónico ya está registrado en el sistema.
        /// </summary>
        /// <param name="email">Correo a verificar.</param>
        /// <returns>JSON con el mensaje de error o true si es válido.</returns>
        [AcceptVerbs("GET", "POST")]
        [Route("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromQuery(Name = "Input.Email")] string email)
        {
            // Mitigación básica: Solo responder a peticiones AJAX/Fetch
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest" && !Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return BadRequest();
            }

            var error = await _validationService.CheckDuplicateEmailAsync(email);
            return error != null ? Json(error) : Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        [Route("VerifyPhone")]
        public IActionResult VerifyPhone([FromQuery(Name = "Input.PhoneNumber")] string phoneNumber)
        {
            // Mitigación básica: Solo responder a peticiones AJAX/Fetch
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest" && !Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return BadRequest();
            }

            var error = _validationService.CheckDuplicatePhone(phoneNumber);
            return error != null ? Json(error) : Json(true);
        }
    }
}
