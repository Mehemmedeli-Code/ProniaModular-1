using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record UpdateSizeRequest(string Name);

    public static class SizeEndpoints
    {
        public static IEndpointRouteBuilder MapSizeEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/sizes").WithTags("Sizes");

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
            [AsParameters] SizeQueryParameters parameters,
            ISizeService service,
            CancellationToken ct)
        {
            var sizes = await service.GetAllAsync(parameters, ct);
            return Results.Ok(sizes);
        }

        private static async Task<IResult> GetByIdAsync(long id, bool? includeDeleted, ISizeService service, CancellationToken ct)
        {
            var size = await service.GetByIdAsync(id, includeDeleted ?? false, ct);
            return Results.Ok(size);
        }

        private static async Task<IResult> CreateAsync(
            CreateSizeCommand command,
            IValidator<CreateSizeCommand> validator,
            ISizeService service,
            CancellationToken ct)
        {
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var size = await service.CreateAsync(command, ct);
            return Results.Created($"/api/sizes/{size.Id}", size);
        }

        private static async Task<IResult> UpdateAsync(
            long id,
            UpdateSizeRequest request,
            IValidator<UpdateSizeCommand> validator,
            ISizeService service,
            CancellationToken ct)
        {
            var command = new UpdateSizeCommand(id, request.Name);
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var size = await service.UpdateAsync(command, ct);
            return Results.Ok(size);
        }

        private static async Task<IResult> DeleteAsync(long id, ISizeService service, CancellationToken ct)
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }

        private static async Task<IResult> RestoreAsync(long id, ISizeService service, CancellationToken ct)
        {
            var size = await service.RestoreAsync(id, ct);
            return Results.Ok(size);
        }

        private static async Task<IResult> HardDeleteAsync(long id, ISizeService service, CancellationToken ct)
        {
            await service.HardDeleteAsync(id, ct);
            return Results.NoContent();
        }
    }
}
