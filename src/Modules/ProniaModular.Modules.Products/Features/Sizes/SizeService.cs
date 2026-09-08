using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Exceptions;
using ProniaModular.Modules.Products.Common.Queries;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public interface ISizeService
    {
        Task<PagedResult<SizeDto>> GetAllAsync(SizeQueryParameters parameters, CancellationToken cancellationToken = default);
        Task<SizeDto> GetByIdAsync(long id, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<SizeDto> CreateAsync(CreateSizeCommand command, CancellationToken cancellationToken = default);
        Task<SizeDto> UpdateAsync(UpdateSizeCommand command, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<SizeDto> RestoreAsync(long id, CancellationToken cancellationToken = default);
        Task HardDeleteAsync(long id, CancellationToken cancellationToken = default);
    }

    public sealed class SizeService(ProductsDbContext dbContext) : ISizeService
    {
        public async Task<PagedResult<SizeDto>> GetAllAsync(SizeQueryParameters parameters, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Sizes
                .AsNoTracking()
                .ApplyDeletedFilter(parameters.IncludeDeleted);

            // ---- Searching ----
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var pattern = parameters.Search.ToLikePattern();
                query = query.Where(s => EF.Functions.Like(s.Name, pattern));
            }

            // ---- Filtering ----
            if (parameters.OnlyAssigned == true)
                query = query.Where(s => s.ProductSizes.Any());

            // ---- Sorting ----
            query = ApplySorting(query, parameters);

            // ---- Paging ----
            return await query
                .Select(s => new SizeDto(s.Id, s.Name, s.ProductSizes.Count, s.IsDeleted, s.CreatedAt))
                .ToPagedResultAsync(parameters.Page, parameters.PageSize, cancellationToken);
        }

        private static IQueryable<Size> ApplySorting(IQueryable<Size> query, SizeQueryParameters parameters)
        {
            var sorted = parameters.SortBy?.Trim().ToLowerInvariant() switch
            {
                "productcount" => parameters.Desc
                    ? query.OrderByDescending(s => s.ProductSizes.Count)
                    : query.OrderBy(s => s.ProductSizes.Count),

                "createdat" => parameters.Desc
                    ? query.OrderByDescending(s => s.CreatedAt)
                    : query.OrderBy(s => s.CreatedAt),

                "id" => parameters.Desc
                    ? query.OrderByDescending(s => s.Id)
                    : query.OrderBy(s => s.Id),

                _ => parameters.Desc
                    ? query.OrderByDescending(s => s.Name)
                    : query.OrderBy(s => s.Name)
            };

            return sorted.ThenBy(s => s.Id);
        }

        public async Task<SizeDto> GetByIdAsync(long id, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var size = await dbContext.Sizes
                .AsNoTracking()
                .ApplyDeletedFilter(includeDeleted)
                .Where(s => s.Id == id)
                .Select(s => new SizeDto(s.Id, s.Name, s.ProductSizes.Count, s.IsDeleted, s.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            return size ?? throw new NotFoundException($"Size with id {id} was not found.");
        }

        public async Task<SizeDto> CreateAsync(CreateSizeCommand command, CancellationToken cancellationToken = default)
        {
            var nameExists = await dbContext.Sizes.AnyAsync(s => s.Name == command.Name, cancellationToken);
            if (nameExists)
                throw new ConflictException($"A size named '{command.Name}' already exists.");

            var size = new Size
            {
                Name = command.Name
                // CreatedAt / CreatedBy are filled by ProductsDbContext.SaveChangesAsync
            };

            dbContext.Sizes.Add(size);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new SizeDto(size.Id, size.Name, 0, size.IsDeleted, size.CreatedAt);
        }

        public async Task<SizeDto> UpdateAsync(UpdateSizeCommand command, CancellationToken cancellationToken = default)
        {
            var size = await dbContext.Sizes.FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken)
                ?? throw new NotFoundException($"Size with id {command.Id} was not found.");

            var nameTaken = await dbContext.Sizes
                .AnyAsync(s => s.Name == command.Name && s.Id != command.Id, cancellationToken);
            if (nameTaken)
                throw new ConflictException($"A size named '{command.Name}' already exists.");

            size.Name = command.Name;

            await dbContext.SaveChangesAsync(cancellationToken);

            var productCount = await dbContext.ProductSizes.CountAsync(ps => ps.SizeId == size.Id, cancellationToken);

            return new SizeDto(size.Id, size.Name, productCount, size.IsDeleted, size.CreatedAt);
        }

        public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var size = await dbContext.Sizes.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Size with id {id} was not found.");

            // The ProductSize filter hides links of soft deleted products, so only live
            // assignments block the delete.
            var isAssigned = await dbContext.ProductSizes.AnyAsync(ps => ps.SizeId == id, cancellationToken);
            if (isAssigned)
                throw new ConflictException("Size cannot be deleted while it is still assigned to products.");

            dbContext.Sizes.Remove(size);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<SizeDto> RestoreAsync(long id, CancellationToken cancellationToken = default)
        {
            var size = await dbContext.Sizes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Size with id {id} was not found.");

            if (!size.IsDeleted)
                throw new ConflictException($"Size with id {id} is not deleted.");

            var nameTaken = await dbContext.Sizes
                .AnyAsync(s => s.Name == size.Name && s.Id != size.Id, cancellationToken);
            if (nameTaken)
                throw new ConflictException($"A size named '{size.Name}' already exists.");

            size.IsDeleted = false;
            await dbContext.SaveChangesAsync(cancellationToken);

            var productCount = await dbContext.ProductSizes.CountAsync(ps => ps.SizeId == size.Id, cancellationToken);

            return new SizeDto(size.Id, size.Name, productCount, size.IsDeleted, size.CreatedAt);
        }

        public async Task HardDeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var affected = await dbContext.Sizes
                .IgnoreQueryFilters()
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            if (affected == 0)
                throw new NotFoundException($"Size with id {id} was not found.");
        }
    }
}
