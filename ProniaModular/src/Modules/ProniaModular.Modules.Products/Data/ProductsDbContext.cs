using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Data
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Size> Sizes => Set<Size>();
        public DbSet<ProductSize> ProductSizes => Set<ProductSize>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
