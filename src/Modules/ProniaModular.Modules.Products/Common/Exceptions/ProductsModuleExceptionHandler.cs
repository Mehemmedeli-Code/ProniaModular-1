using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace ProniaModular.Modules.Products.Common.Exceptions
{
    public sealed class ProductsModuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var statusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                _ => 0
            };

            if (statusCode == 0)
                return false;

            httpContext.Response.StatusCode = statusCode;

            await Results.Problem(detail: exception.Message, statusCode: statusCode)
                .ExecuteAsync(httpContext);

            return true;
        }
    }
}
