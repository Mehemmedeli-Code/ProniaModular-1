using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Entities;
using ProniaModular.Modules.Products.Entities.Common;
using System.Linq.Expressions;

namespace ProniaModular.Modules.Products.Data
{
    public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
    {
        // Every module owns its own schema + its own migrations history table,
        // so modules never collide inside the same physical database.
        public const string Schema = "products";
        public const string MigrationsHistoryTable = "__ProductsMigrationsHistory";

        private const string SystemUser = "Admin";

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            // typeof(...).Assembly is safer than Assembly.GetExecutingAssembly() in a modular host:
            // it always points to THIS module, no matter who triggers the model building.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);

            ApplySoftDeleteQueryFilters(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        // Global Query Filter: every entity derived from BaseEntity automatically gets
        // "WHERE IsDeleted = 0", so services never repeat that condition.
        private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
            {
                if (entityType.BaseType is not null)
                    continue;

                if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                    continue;

                // e => !e.IsDeleted
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeleted = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(isDeleted), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        // Audit columns + soft delete are handled in one place instead of in every service.
        private void ApplyAuditAndSoftDelete()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.IsDeleted = false;

                        if (entry.Entity is BaseAccountableEntity added)
                        {
                            if (added.CreatedAt == default)
                                added.CreatedAt = now;

                            added.CreatedBy ??= SystemUser;
                        }
                        break;

                    case EntityState.Modified:
                        if (entry.Entity is BaseAccountableEntity modified)
                        {
                            modified.UpdatedAt = now;
                            modified.UpdatedBy ??= SystemUser;

                            // CreatedAt / CreatedBy must never be overwritten by an update.
                            entry.Property(nameof(BaseAccountableEntity.CreatedAt)).IsModified = false;
                            entry.Property(nameof(BaseAccountableEntity.CreatedBy)).IsModified = false;
                        }
                        break;

                    case EntityState.Deleted:
                        // Remove() no longer produces a DELETE statement: it becomes an UPDATE.
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;

                        if (entry.Entity is BaseAccountableEntity deleted)
                        {
                            deleted.UpdatedAt = now;
                            deleted.UpdatedBy ??= SystemUser;

                            entry.Property(nameof(BaseAccountableEntity.CreatedAt)).IsModified = false;
                            entry.Property(nameof(BaseAccountableEntity.CreatedBy)).IsModified = false;
                        }
                        break;
                }
            }
        }
    }
}
