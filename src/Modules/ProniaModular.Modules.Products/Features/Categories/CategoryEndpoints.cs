using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record UpdateCategoryRequest(string Name);

    public static class CategoryEndpoints
    {
        public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/categories").WithTags("Categories");

            group.MapGet("/", GetAllAsync);
            group.MapGet("/{id:long}", GetByIdAsync);
            group.MapPost("/", CreateAsync);
            group.MapPut("/{id:long}", UpdateAsync);
            group.MapDelete("/{id:long}", DeleteAsync);
            group.MapPost("/{id:long}/restore", RestoreAsync);
            group.MapDelete("/{id:long}/hard", HardDeleteAsync);

            return app;
        }

        private static async Task<IResult> GetAllAsync(
            [AsParameters] CategoryQueryParameters parameters,
            ICategoryService service,
            CancellationToken ct)
        {
            var categories = await service.GetAllAsync(parameters, ct);
            return Results.Ok(categories);
        }

        private static async Task<IResult> GetByIdAsync(long id, bool? includeDeleted, ICategoryService service, CancellationToken ct)
        {
            var category = await service.GetByIdAsync(id, includeDeleted ?? false, ct);
            return Results.Ok(category);
        }

        private static async Task<IResult> CreateAsync(
            CreateCategoryCommand command,
            IValidator<CreateCategoryCommand> validator,
            ICategoryService service,
            CancellationToken ct)
        {
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var category = await service.CreateAsync(command, ct);
            return Results.Created($"/api/categories/{category.Id}", category);
        }

        private static async Task<IResult> UpdateAsync(
            long id,
            UpdateCategoryRequest request,
            IValidator<UpdateCategoryCommand> validator,
            ICategoryService service,
            CancellationToken ct)
        {
            var command = new UpdateCategoryCommand(id, request.Name);
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var category = await service.UpdateAsync(command, ct);
            return Results.Ok(category);
        }

        private static async Task<IResult> DeleteAsync(long id, ICategoryService service, CancellationToken ct)
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }

        private static async Task<IResult> RestoreAsync(long id, ICategoryService service, CancellationToken ct)
        {
            var category = await service.RestoreAsync(id, ct);
            return Results.Ok(category);
        }

        private static async Task<IResult> HardDeleteAsync(long id, ICategoryService service, CancellationToken ct)
        {
            await service.HardDeleteAsync(id, ct);
            return Results.NoContent();
        }
    }
}
