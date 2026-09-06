using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProniaModular.Modules.Products.Common.Results;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record UpdateSizeRequest(string Name);

    public static class SizeEndpoints
    {
        public static IEndpointRouteBuilder MapSizeEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/sizes").WithTags("Sizes");

            group.MapGet("/", async (GetSizesQueryHandler handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new GetSizesQuery(), ct);
                return Results.Ok(result.Value);
            });

            group.MapGet("/{id:long}", async (long id, GetSizeByIdQueryHandler handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new GetSizeByIdQuery(id), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

            group.MapPost("/", async (
                CreateSizeCommand command,
                IValidator<CreateSizeCommand> validator,
                CreateSizeCommandHandler handler,
                CancellationToken ct) =>
            {
                var validation = await validator.ValidateAsync(command, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(command, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/sizes/{result.Value!.Id}", result.Value)
                    : result.ToProblem();
            });

            group.MapPut("/{id:long}", async (
                long id,
                UpdateSizeRequest request,
                IValidator<UpdateSizeCommand> validator,
                UpdateSizeCommandHandler handler,
                CancellationToken ct) =>
            {
                var command = new UpdateSizeCommand(id, request.Name);
                var validation = await validator.ValidateAsync(command, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await handler.Handle(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

            group.MapDelete("/{id:long}", async (
                long id,
                IValidator<DeleteSizeCommand> validator,
                DeleteSizeCommandHandler handler,
                CancellationToken ct) =>
            {
                var command = new DeleteSizeCommand(id);
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
