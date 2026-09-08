using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Filtered unique index: a soft deleted row must not block the same name from being reused.
            builder.HasIndex(p => p.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(6,2)");

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.CreatedBy).HasMaxLength(100);
            builder.Property(p => p.UpdatedBy).HasMaxLength(100);
        }
    }
}
