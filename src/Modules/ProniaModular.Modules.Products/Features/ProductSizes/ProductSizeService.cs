using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Exceptions;
using ProniaModular.Modules.Products.Common.Queries;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Features.ProductSizes
{
    public interface IProductSizeService
    {
        Task<List<ProductSizeDto>> GetByProductIdAsync(long productId, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<ProductSizeDto> AddAsync(long productId, long sizeId, CancellationToken cancellationToken = default);
        Task RemoveAsync(long productId, long sizeId, CancellationToken cancellationToken = default);
    }

    public sealed class ProductSizeService(ProductsDbContext dbContext) : IProductSizeService
    {
        public async Task<List<ProductSizeDto>> GetByProductIdAsync(long productId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var productExists = await dbContext.Products
                .ApplyDeletedFilter(includeDeleted)
                .AnyAsync(p => p.Id == productId, cancellationToken);

            if (!productExists)
                throw new NotFoundException($"Product with id {productId} was not found.");

            return await dbContext.ProductSizes
                .AsNoTracking()
                .ApplyDeletedFilter(includeDeleted)
                .Where(ps => ps.ProductId == productId)
                .OrderBy(ps => ps.Size.Name)
                .Select(ps => new ProductSizeDto(ps.ProductId, ps.Product.Name, ps.SizeId, ps.Size.Name))
                .ToListAsync(cancellationToken);
        }

        public async Task<ProductSizeDto> AddAsync(long productId, long sizeId, CancellationToken cancellationToken = default)
        {
            // Soft deleted products/sizes are invisible here, so a deleted row can never be linked.
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken)
                ?? throw new NotFoundException($"Product with id {productId} was not found.");

            var size = await dbContext.Sizes.FirstOrDefaultAsync(s => s.Id == sizeId, cancellationToken)
                ?? throw new NotFoundException($"Size with id {sizeId} was not found.");

            var alreadyAssigned = await dbContext.ProductSizes
                .IgnoreQueryFilters()
                .AnyAsync(ps => ps.ProductId == productId && ps.SizeId == sizeId, cancellationToken);
            if (alreadyAssigned)
                throw new ConflictException("This size is already assigned to the product.");

            var productSize = new ProductSize
            {
                ProductId = productId,
                SizeId = sizeId
            };

            dbContext.ProductSizes.Add(productSize);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new ProductSizeDto(productId, product.Name, sizeId, size.Name);
        }

        // ProductSize is not a BaseEntity, so this stays a real DELETE.
        public async Task RemoveAsync(long productId, long sizeId, CancellationToken cancellationToken = default)
        {
            var productSize = await dbContext.ProductSizes
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.SizeId == sizeId, cancellationToken)
                ?? throw new NotFoundException("This size is not assigned to the product.");

            dbContext.ProductSizes.Remove(productSize);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
