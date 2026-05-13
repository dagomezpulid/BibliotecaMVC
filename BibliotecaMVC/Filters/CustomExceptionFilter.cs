using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BibliotecaMVC.Filters
{
    /// <summary>
    /// Filtro global para la captura y gestión de excepciones no controladas.
    /// Registra el error detallado en el log y redirige al usuario a una vista amigable
    /// sin exponer información sensible del servidor.
    /// </summary>
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilter> _logger;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IModelMetadataProvider modelMetadataProvider)
        {
            _logger = logger;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public void OnException(ExceptionContext context)
        {
            // 1. Registro del error real en el sistema de logging (Auditoría interna)
            _logger.LogError(context.Exception, "Error no controlado detectado en {Action}", context.ActionDescriptor.DisplayName);

            // 2. Si es una petición AJAX/API, devolver JSON en lugar de una vista HTML
            if (IsAjaxRequest(context.HttpContext.Request))
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = "Ocurrió un error interno al procesar la solicitud. Por favor, intente más tarde."
                })
                {
                    StatusCode = 500
                };
            }
            else
            {
                // 3. Redirección a la vista de error amigable (Home/Error)
                var result = new ViewResult { ViewName = "Error" };
                result.ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
                
                // Opcional: Pasar un mensaje genérico si se desea
                result.ViewData["ErrorMessage"] = "Hemos experimentado un problema técnico. Nuestro equipo ha sido notificado.";
                
                context.Result = result;
            }

            // 4. Marcar la excepción como controlada para evitar que ASP.NET muestre la página de error por defecto
            context.ExceptionHandled = true;
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
