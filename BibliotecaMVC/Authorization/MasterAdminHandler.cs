using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BibliotecaMVC.Authorization
{
    /// <summary>
    /// Requerimiento para la política de Master Admin.
    /// </summary>
    public class MasterAdminRequirement : IAuthorizationRequirement { }

    /// <summary>
    /// Evaluador de la política Master Admin.
    /// Valida que el usuario autenticado sea el administrador raíz configurado en el sistema.
    /// </summary>
    public class MasterAdminHandler : AuthorizationHandler<MasterAdminRequirement>
    {
        private readonly IConfiguration _configuration;

        public MasterAdminHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MasterAdminRequirement requirement)
        {
            var userEmail = context.User.FindFirstValue(ClaimTypes.Email);
            var masterAdminEmail = _configuration["AdminSettings:Email"] ?? "dgomezpulid@outlook.com";

            if (!string.IsNullOrEmpty(userEmail) && userEmail.Equals(masterAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
