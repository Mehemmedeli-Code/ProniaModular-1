using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Features.Categories;
using ProniaModular.Modules.Products.Features.ProductSizes;
using ProniaModular.Modules.Products.Features.Products;
using ProniaModular.Modules.Products.Features.Sizes;

namespace ProniaModular.Modules.Products
{
    public static class ProductsModuleServiceCollectionExtensions
    {
        public static IServiceCollection AddProductsModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ProductsDbContext>(options => options.UseSqlServer(connectionString));

            services.AddValidatorsFromAssemblyContaining<ProductsDbContext>();

            // Categories
            services.AddScoped<CreateCategoryCommandHandler>();
            services.AddScoped<UpdateCategoryCommandHandler>();
            services.AddScoped<DeleteCategoryCommandHandler>();
            services.AddScoped<GetCategoryByIdQueryHandler>();
            services.AddScoped<GetCategoriesQueryHandler>();

            // Sizes
            services.AddScoped<CreateSizeCommandHandler>();
            services.AddScoped<UpdateSizeCommandHandler>();
            services.AddScoped<DeleteSizeCommandHandler>();
            services.AddScoped<GetSizeByIdQueryHandler>();
            services.AddScoped<GetSizesQueryHandler>();

            // Products
            services.AddScoped<CreateProductCommandHandler>();
            services.AddScoped<UpdateProductCommandHandler>();
            services.AddScoped<DeleteProductCommandHandler>();
            services.AddScoped<GetProductByIdQueryHandler>();
            services.AddScoped<GetProductsQueryHandler>();

            // ProductSizes
            services.AddScoped<AddProductSizeCommandHandler>();
            services.AddScoped<RemoveProductSizeCommandHandler>();
            services.AddScoped<GetProductSizesQueryHandler>();

            return services;
        }
    }
}
