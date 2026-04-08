using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers
{
    [Route("[controller]")]
    public class ValidationController : Controller
    {
        private readonly IUserValidationService _validationService;

        public ValidationController(IUserValidationService validationService)
        {
            _validationService = validationService;
        }

        [AcceptVerbs("GET", "POST")]
        [Route("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromQuery(Name = "Input.Email")] string email)
        {
            var error = await _validationService.CheckDuplicateEmailAsync(email);
            return error != null ? Json(error) : Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        [Route("VerifyPhone")]
        public IActionResult VerifyPhone([FromQuery(Name = "Input.PhoneNumber")] string phoneNumber)
        {
            var error = _validationService.CheckDuplicatePhone(phoneNumber);
            return error != null ? Json(error) : Json(true);
        }
    }
}
