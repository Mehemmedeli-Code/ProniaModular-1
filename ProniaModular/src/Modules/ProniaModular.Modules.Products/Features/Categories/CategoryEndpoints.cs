using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProniaModular.Modules.Products.Common.Results;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record UpdateCategoryRequest(string Name);

    public static class CategoryEndpoints
    {
        public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/categories").WithTags("Categories");

            group.MapGet("/", async (GetCategoriesQueryHandler handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new GetCategoriesQuery(), ct);
                return Results.Ok(result.Value);
            });

            group.MapGet("/{id:long}", async (long id, GetCategoryByIdQueryHandler handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new GetCategoryByIdQuery(id), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

            group.MapPost("/", async (
                CreateCategoryCommand command,
                IValidator<CreateCategoryCommand> validator,
                CreateCategoryCommandHandler handler,
                CancellationToken ct) =>
            {
                var validation = await validator.ValidateAsync(command, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(command, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/categories/{result.Value!.Id}", result.Value)
                    : result.ToProblem();
            });

            group.MapPut("/{id:long}", async (
                long id,
                UpdateCategoryRequest request,
                IValidator<UpdateCategoryCommand> validator,
                UpdateCategoryCommandHandler handler,
                CancellationToken ct) =>
            {
                var command = new UpdateCategoryCommand(id, request.Name);
                var validation = await validator.ValidateAsync(command, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

            group.MapDelete("/{id:long}", async (
                long id,
                IValidator<DeleteCategoryCommand> validator,
                DeleteCategoryCommandHandler handler,
                CancellationToken ct) =>
            {
                var command = new DeleteCategoryCommand(id);
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
