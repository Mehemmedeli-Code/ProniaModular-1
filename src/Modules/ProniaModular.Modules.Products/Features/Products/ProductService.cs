using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Exceptions;
using ProniaModular.Modules.Products.Common.Queries;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Features.Products
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default);
        Task<ProductDto> GetByIdAsync(long id, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<ProductDto> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default);
        Task<ProductDto> UpdateAsync(UpdateProductCommand command, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<ProductDto> RestoreAsync(long id, CancellationToken cancellationToken = default);
        Task HardDeleteAsync(long id, CancellationToken cancellationToken = default);
    }

    public sealed class ProductService(ProductsDbContext dbContext) : IProductService
    {
        public async Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Products
                .AsNoTracking()
                .ApplyDeletedFilter(parameters.IncludeDeleted);

            // ---- Searching ----
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var pattern = parameters.Search.ToLikePattern();

                query = query.Where(p =>
                    EF.Functions.Like(p.Name, pattern) ||
                    (p.Description != null && EF.Functions.Like(p.Description, pattern)) ||
                    EF.Functions.Like(p.Category.Name, pattern));
            }

            // ---- Filtering (every filter is combined with AND) ----
            if (parameters.CategoryId is > 0)
                query = query.Where(p => p.CategoryId == parameters.CategoryId);

            if (parameters.SizeId is > 0)
                query = query.Where(p => p.ProductSizes.Any(ps => ps.SizeId == parameters.SizeId));

            if (parameters.MinPrice.HasValue)
                query = query.Where(p => p.Price >= parameters.MinPrice.Value);

            if (parameters.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

            // ---- Sorting ----
            query = ApplySorting(query, parameters);

            // ---- Paging ----
            return await query
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Description,
                    p.CategoryId,
                    p.Category != null ? p.Category.Name : string.Empty,
                    p.IsDeleted,
                    p.CreatedAt))
                .ToPagedResultAsync(parameters.Page, parameters.PageSize, cancellationToken);
        }

        private static IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductQueryParameters parameters)
        {
            var sorted = parameters.SortBy?.Trim().ToLowerInvariant() switch
            {
                "price" => parameters.Desc
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),

                "createdat" => parameters.Desc
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),

                "category" => parameters.Desc
                    ? query.OrderByDescending(p => p.Category.Name)
                    : query.OrderBy(p => p.Category.Name),

                "id" => parameters.Desc
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id),

                _ => parameters.Desc
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name)
            };

            // Stable paging: rows with an equal sort key always keep the same order.
            return sorted.ThenBy(p => p.Id);
        }

        public async Task<ProductDto> GetByIdAsync(long id, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var product = await dbContext.Products
                .AsNoTracking()
                .ApplyDeletedFilter(includeDeleted)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Description,
                    p.CategoryId,
                    p.Category != null ? p.Category.Name : string.Empty,
                    p.IsDeleted,
                    p.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            return product ?? throw new NotFoundException($"Product with id {id} was not found.");
        }

        public async Task<ProductDto> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken)
                ?? throw new NotFoundException($"Category with id {command.CategoryId} was not found.");

            var nameExists = await dbContext.Products.AnyAsync(p => p.Name == command.Name, cancellationToken);
            if (nameExists)
                throw new ConflictException($"A product named '{command.Name}' already exists.");

            var product = new Product
            {
                Name = command.Name,
                Price = command.Price,
                Description = command.Description,
                CategoryId = command.CategoryId
                // CreatedAt / CreatedBy are filled by ProductsDbContext.SaveChangesAsync
            };

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new ProductDto(product.Id, product.Name, product.Price, product.Description,
                product.CategoryId, category.Name, product.IsDeleted, product.CreatedAt);
        }

        public async Task<ProductDto> UpdateAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
                ?? throw new NotFoundException($"Product with id {command.Id} was not found.");

            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken)
                ?? throw new NotFoundException($"Category with id {command.CategoryId} was not found.");

            var nameTaken = await dbContext.Products
                .AnyAsync(p => p.Name == command.Name && p.Id != command.Id, cancellationToken);
            if (nameTaken)
                throw new ConflictException($"A product named '{command.Name}' already exists.");

            product.Name = command.Name;
            product.Price = command.Price;
            product.Description = command.Description;
            product.CategoryId = command.CategoryId;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new ProductDto(product.Id, product.Name, product.Price, product.Description,
                product.CategoryId, category.Name, product.IsDeleted, product.CreatedAt);
        }

        // Soft delete: Remove() is turned into "IsDeleted = 1" by the DbContext.
        public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Product with id {id} was not found.");

            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Only reachable with IgnoreQueryFilters(), because the row is invisible to normal queries.
        public async Task<ProductDto> RestoreAsync(long id, CancellationToken cancellationToken = default)
        {
            var product = await dbContext.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Product with id {id} was not found.");

            if (!product.IsDeleted)
                throw new ConflictException($"Product with id {id} is not deleted.");

            var nameTaken = await dbContext.Products
                .AnyAsync(p => p.Name == product.Name && p.Id != product.Id, cancellationToken);
            if (nameTaken)
                throw new ConflictException($"A product named '{product.Name}' already exists.");

            product.IsDeleted = false;
            await dbContext.SaveChangesAsync(cancellationToken);

            var categoryName = await dbContext.Categories
                .IgnoreQueryFilters()
                .Where(c => c.Id == product.CategoryId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

            return new ProductDto(product.Id, product.Name, product.Price, product.Description,
                product.CategoryId, categoryName, product.IsDeleted, product.CreatedAt);
        }

        // Real DELETE. ExecuteDelete bypasses SaveChanges, so the soft delete logic is skipped.
        public async Task HardDeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var affected = await dbContext.Products
                .IgnoreQueryFilters()
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            if (affected == 0)
                throw new NotFoundException($"Product with id {id} was not found.");
        }
    }
}
