using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Data.Configurations
{
    public class SizeConfiguration : IEntityTypeConfiguration<Size>
    {
        public void Configure(EntityTypeBuilder<Size> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Filtered unique index: a soft deleted row must not block the same name from being reused.
            builder.HasIndex(s => s.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(s => s.CreatedAt).IsRequired();
            builder.Property(s => s.CreatedBy).HasMaxLength(100);
            builder.Property(s => s.UpdatedBy).HasMaxLength(100);
            builder.Property(s => s.IsDeleted).HasDefaultValue(false);
        }
    }
}
