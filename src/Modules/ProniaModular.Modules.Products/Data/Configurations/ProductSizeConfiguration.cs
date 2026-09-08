using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Data.Configurations
{
    public class ProductSizeConfiguration : IEntityTypeConfiguration<ProductSize>
    {
        public void Configure(EntityTypeBuilder<ProductSize> builder)
        {
            builder.HasKey(ps => new { ps.ProductId, ps.SizeId });

            builder.HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSizes)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ps => ps.Size)
                .WithMany(s => s.ProductSizes)
                .HasForeignKey(ps => ps.SizeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductSize has no IsDeleted column, but both of its required navigations are
            // filtered. Without this filter EF Core raises the
            // "required navigation with query filter" warning and links of soft deleted
            // products/sizes would still leak into the results.
            builder.HasQueryFilter(ps => !ps.Product.IsDeleted && !ps.Size.IsDeleted);
        }
    }
}
