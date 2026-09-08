using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ProniaModular.Modules.Products.Features.ProductSizes
{
    public static class ProductSizeEndpoints
    {
        public static IEndpointRouteBuilder MapProductSizeEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/products/{productId:long}/sizes").WithTags("ProductSizes");

            group.MapGet("/", GetByProductIdAsync);
            group.MapPost("/{sizeId:long}", AddAsync);
            group.MapDelete("/{sizeId:long}", RemoveAsync);

            return app;
        }

        private static async Task<IResult> GetByProductIdAsync(long productId, bool? includeDeleted, IProductSizeService service, CancellationToken ct)
        {
            var sizes = await service.GetByProductIdAsync(productId, includeDeleted ?? false, ct);
            return Results.Ok(sizes);
        }

        private static async Task<IResult> AddAsync(long productId, long sizeId, IProductSizeService service, CancellationToken ct)
        {
            var productSize = await service.AddAsync(productId, sizeId, ct);
            return Results.Created($"/api/products/{productId}/sizes", productSize);
        }

        private static async Task<IResult> RemoveAsync(long productId, long sizeId, IProductSizeService service, CancellationToken ct)
        {
            await service.RemoveAsync(productId, sizeId, ct);
            return Results.NoContent();
        }
    }
}
