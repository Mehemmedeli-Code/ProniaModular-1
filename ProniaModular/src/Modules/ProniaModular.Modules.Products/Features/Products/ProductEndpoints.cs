using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProniaModular.Modules.Products.Common.Results;

namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record UpdateProductRequest(string Name, decimal Price, string? Description, long CategoryId);

    public static class ProductEndpoints
    {
        public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/products").WithTags("Products");

            group.MapGet("/", async (GetProductsQueryHandler handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new GetProductsQuery(), ct);
                return Results.Ok(result.Value);
            });

            group.MapGet("/{id:long}", async (long id, GetProductByIdQueryHandler handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new GetProductByIdQuery(id), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

            group.MapPost("/", async (
                CreateProductCommand command,
                IValidator<CreateProductCommand> validator,
                CreateProductCommandHandler handler,
                CancellationToken ct) =>
            {
                var validation = await validator.ValidateAsync(command, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(command, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/products/{result.Value!.Id}", result.Value)
                    : result.ToProblem();
            });

            group.MapPut("/{id:long}", async (
                long id,
                UpdateProductRequest request,
                IValidator<UpdateProductCommand> validator,
                UpdateProductCommandHandler handler,
                CancellationToken ct) =>
            {
                var command = new UpdateProductCommand(id, request.Name, request.Price, request.Description, request.CategoryId);
                var validation = await validator.ValidateAsync(command, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

            group.MapDelete("/{id:long}", async (
                long id,
                IValidator<DeleteProductCommand> validator,
                DeleteProductCommandHandler handler,
                CancellationToken ct) =>
            {
                var command = new DeleteProductCommand(id);
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
