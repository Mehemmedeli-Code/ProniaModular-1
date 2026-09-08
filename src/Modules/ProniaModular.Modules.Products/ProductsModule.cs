using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProniaModular.Modules.Products.Common.Exceptions;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Features.Categories;
using ProniaModular.Modules.Products.Features.Products;
using ProniaModular.Modules.Products.Features.ProductSizes;
using ProniaModular.Modules.Products.Features.Sizes;

namespace ProniaModular.Modules.Products
{
    // One entry point for the whole module: the host only calls these three methods.
    public static class ProductsModule
    {
        public const string ConnectionStringName = "ProductsDb";

        public static IServiceCollection AddProductsModule(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' was not found in the configuration.");

            services.AddDbContext<ProductsDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sql =>
                {
                    // Migrations live inside the module, not in the API project.
                    sql.MigrationsAssembly(typeof(ProductsDbContext).Assembly.GetName().Name);
                    sql.MigrationsHistoryTable(ProductsDbContext.MigrationsHistoryTable, ProductsDbContext.Schema);
                    sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                });
            });

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISizeService, SizeService>();
            services.AddScoped<IProductSizeService, ProductSizeService>();

            // Picks up every validator of this module in one call.
            services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();

            services.AddExceptionHandler<ProductsModuleExceptionHandler>();

            return services;
        }

        public static IEndpointRouteBuilder MapProductsModule(this IEndpointRouteBuilder app)
        {
            app.MapCategoryEndpoints();
            app.MapProductEndpoints();
            app.MapSizeEndpoints();
            app.MapProductSizeEndpoints();

            return app;
        }

        // Applies only this module's migrations, using this module's history table.
        public static async Task MigrateProductsModuleAsync(this IApplicationBuilder app, CancellationToken cancellationToken = default)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
