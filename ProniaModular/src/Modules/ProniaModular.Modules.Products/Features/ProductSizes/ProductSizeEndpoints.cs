using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProniaModular.Modules.Products.Common.Results;

namespace ProniaModular.Modules.Products.Features.ProductSizes
{
    public static class ProductSizeEndpoints
    {
        public static IEndpointRouteBuilder MapProductSizeEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/products/{productId:long}/sizes").WithTags("ProductSizes");

            group.MapGet("/", async (
                long productId,
                IValidator<GetProductSizesQuery> validator,
                GetProductSizesQueryHandler handler,
                CancellationToken ct) =>
            {
                var query = new GetProductSizesQuery(productId);
                var validation = await validator.ValidateAsync(query, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

            group.MapPost("/{sizeId:long}", async (
                long productId,
                long sizeId,
                IValidator<AddProductSizeCommand> validator,
                AddProductSizeCommandHandler handler,
                CancellationToken ct) =>
            {
                var command = new AddProductSizeCommand(productId, sizeId);
                var validation = await validator.ValidateAsync(command, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(command, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/products/{productId}/sizes", result.Value)
                    : result.ToProblem();
            });

            group.MapDelete("/{sizeId:long}", async (
                long productId,
                long sizeId,
                IValidator<RemoveProductSizeCommand> validator,
                RemoveProductSizeCommandHandler handler,
                CancellationToken ct) =>
            {
                var command = new RemoveProductSizeCommand(productId, sizeId);
                var validation = await validator.ValidateAsync(command, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(command, ct);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem();
            });

            return app;
        }
    }
}
