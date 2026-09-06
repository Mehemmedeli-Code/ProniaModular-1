namespace ProniaModular.Modules.Products.Common.Results
{
    public static class ResultHttpExtensions
    {
        public static Microsoft.AspNetCore.Http.IResult ToProblem<T>(this Result<T> result)
        {
            var error = result.Error ?? Error.Validation("Unknown error.");

            var statusCode = error.Type switch
            {
                ErrorType.NotFound => Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound,
                ErrorType.Conflict => Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict,
                ErrorType.Validation => Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest,
                _ => Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest
            };

            return Microsoft.AspNetCore.Http.Results.Problem(detail: error.Message, statusCode: statusCode);
        }
    }
}
