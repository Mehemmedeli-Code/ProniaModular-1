using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Exceptions;
using ProniaModular.Modules.Products.Common.Queries;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryDto>> GetAllAsync(CategoryQueryParameters parameters, CancellationToken cancellationToken = default);
        Task<CategoryDto> GetByIdAsync(long id, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<CategoryDto> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default);
        Task<CategoryDto> UpdateAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<CategoryDto> RestoreAsync(long id, CancellationToken cancellationToken = default);
        Task HardDeleteAsync(long id, CancellationToken cancellationToken = default);
    }

    public sealed class CategoryService(ProductsDbContext dbContext) : ICategoryService
    {
        public async Task<PagedResult<CategoryDto>> GetAllAsync(CategoryQueryParameters parameters, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Categories
                .AsNoTracking()
                .ApplyDeletedFilter(parameters.IncludeDeleted);

            // ---- Searching ----
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var pattern = parameters.Search.ToLikePattern();
                query = query.Where(c => EF.Functions.Like(c.Name, pattern));
            }

            // ---- Filtering ----
            if (parameters.MinProductCount is > 0)
                query = query.Where(c => c.Products.Count >= parameters.MinProductCount);

            // ---- Sorting ----
            query = ApplySorting(query, parameters);

            // ---- Paging ----
            return await query
                .Select(c => new CategoryDto(c.Id, c.Name, c.Products.Count, c.IsDeleted, c.CreatedAt))
                .ToPagedResultAsync(parameters.Page, parameters.PageSize, cancellationToken);
        }

        private static IQueryable<Category> ApplySorting(IQueryable<Category> query, CategoryQueryParameters parameters)
        {
            var sorted = parameters.SortBy?.Trim().ToLowerInvariant() switch
            {
                "productcount" => parameters.Desc
                    ? query.OrderByDescending(c => c.Products.Count)
                    : query.OrderBy(c => c.Products.Count),

                "createdat" => parameters.Desc
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt),

                "id" => parameters.Desc
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id),

                _ => parameters.Desc
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name)
            };

            return sorted.ThenBy(c => c.Id);
        }

        public async Task<CategoryDto> GetByIdAsync(long id, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories
                .AsNoTracking()
                .ApplyDeletedFilter(includeDeleted)
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto(c.Id, c.Name, c.Products.Count, c.IsDeleted, c.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            return category ?? throw new NotFoundException($"Category with id {id} was not found.");
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var nameExists = await dbContext.Categories.AnyAsync(c => c.Name == command.Name, cancellationToken);
            if (nameExists)
                throw new ConflictException($"A category named '{command.Name}' already exists.");

            var category = new Category
            {
                Name = command.Name
                // CreatedAt / CreatedBy are filled by ProductsDbContext.SaveChangesAsync
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new CategoryDto(category.Id, category.Name, 0, category.IsDeleted, category.CreatedAt);
        }

        public async Task<CategoryDto> UpdateAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken)
                ?? throw new NotFoundException($"Category with id {command.Id} was not found.");

            var nameTaken = await dbContext.Categories
                .AnyAsync(c => c.Name == command.Name && c.Id != command.Id, cancellationToken);
            if (nameTaken)
                throw new ConflictException($"A category named '{command.Name}' already exists.");

            category.Name = command.Name;

            await dbContext.SaveChangesAsync(cancellationToken);

            var productCount = await dbContext.Products.CountAsync(p => p.CategoryId == category.Id, cancellationToken);

            return new CategoryDto(category.Id, category.Name, productCount, category.IsDeleted, category.CreatedAt);
        }

        // Thanks to the global query filter, already soft deleted products no longer block the delete.
        public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Category with id {id} was not found.");

            var hasProducts = await dbContext.Products.AnyAsync(p => p.CategoryId == id, cancellationToken);
            if (hasProducts)
                throw new ConflictException("Category cannot be deleted while it still has products assigned to it.");

            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<CategoryDto> RestoreAsync(long id, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Category with id {id} was not found.");

            if (!category.IsDeleted)
                throw new ConflictException($"Category with id {id} is not deleted.");

            var nameTaken = await dbContext.Categories
                .AnyAsync(c => c.Name == category.Name && c.Id != category.Id, cancellationToken);
            if (nameTaken)
                throw new ConflictException($"A category named '{category.Name}' already exists.");

            category.IsDeleted = false;
            await dbContext.SaveChangesAsync(cancellationToken);

            var productCount = await dbContext.Products.CountAsync(p => p.CategoryId == category.Id, cancellationToken);

            return new CategoryDto(category.Id, category.Name, productCount, category.IsDeleted, category.CreatedAt);
        }

        public async Task HardDeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var exists = await dbContext.Categories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == id, cancellationToken);

            if (!exists)
                throw new NotFoundException($"Category with id {id} was not found.");

            // The FK is Restrict, so even a soft deleted product would break the delete:
            // IgnoreQueryFilters() is required to see them here.
            var hasAnyProduct = await dbContext.Products
                .IgnoreQueryFilters()
                .AnyAsync(p => p.CategoryId == id, cancellationToken);

            if (hasAnyProduct)
                throw new ConflictException("Category cannot be permanently deleted while products still reference it.");

            await dbContext.Categories
                .IgnoreQueryFilters()
                .Where(c => c.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
