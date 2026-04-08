using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaMVC.Controllers
{
    [Route("[controller]")]
    public class ValidationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ValidationController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [AcceptVerbs("GET", "POST")]
        [Route("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromQuery(Name = "Input.Email")] string email)
        {
            if (string.IsNullOrEmpty(email)) return Json(true);
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return Json("Ya existe una cuenta registrada con este correo electrónico.");
            }
            return Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        [Route("VerifyPhone")]
        public IActionResult VerifyPhone([FromQuery(Name = "Input.PhoneNumber")] string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return Json(true);
            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
            if (user != null)
            {
                return Json("Ya existe una cuenta registrada con este número telefónico.");
            }
            return Json(true);
        }
    }
}
