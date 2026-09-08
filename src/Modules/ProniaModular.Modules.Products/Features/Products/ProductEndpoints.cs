using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record UpdateProductRequest(string Name, decimal Price, string? Description, long CategoryId);

    public static class ProductEndpoints
    {
        public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/products").WithTags("Products");

            group.MapGet("/", GetAllAsync);
            group.MapGet("/{id:long}", GetByIdAsync);
            group.MapPost("/", CreateAsync);
            group.MapPut("/{id:long}", UpdateAsync);
            group.MapDelete("/{id:long}", DeleteAsync);
            group.MapPost("/{id:long}/restore", RestoreAsync);
            group.MapDelete("/{id:long}/hard", HardDeleteAsync);

            return app;
        }

        // [AsParameters] keeps the signature clean: search, filters, sorting and paging
        // all arrive inside one object instead of nine separate arguments.
        private static async Task<IResult> GetAllAsync(
            [AsParameters] ProductQueryParameters parameters,
            IProductService service,
            CancellationToken ct)
        {
            var products = await service.GetAllAsync(parameters, ct);
            return Results.Ok(products);
        }

        private static async Task<IResult> GetByIdAsync(long id, bool? includeDeleted, IProductService service, CancellationToken ct)
        {
            var product = await service.GetByIdAsync(id, includeDeleted ?? false, ct);
            return Results.Ok(product);
        }

        private static async Task<IResult> CreateAsync(
            CreateProductCommand command,
            IValidator<CreateProductCommand> validator,
            IProductService service,
            CancellationToken ct)
        {
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var product = await service.CreateAsync(command, ct);
            return Results.Created($"/api/products/{product.Id}", product);
        }

        private static async Task<IResult> UpdateAsync(
            long id,
            UpdateProductRequest request,
            IValidator<UpdateProductCommand> validator,
            IProductService service,
            CancellationToken ct)
        {
            var command = new UpdateProductCommand(id, request.Name, request.Price, request.Description, request.CategoryId);
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var product = await service.UpdateAsync(command, ct);
            return Results.Ok(product);
        }

        private static async Task<IResult> DeleteAsync(long id, IProductService service, CancellationToken ct)
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }

        private static async Task<IResult> RestoreAsync(long id, IProductService service, CancellationToken ct)
        {
            var product = await service.RestoreAsync(id, ct);
            return Results.Ok(product);
        }

        private static async Task<IResult> HardDeleteAsync(long id, IProductService service, CancellationToken ct)
        {
            await service.HardDeleteAsync(id, ct);
            return Results.NoContent();
        }
    }
}
