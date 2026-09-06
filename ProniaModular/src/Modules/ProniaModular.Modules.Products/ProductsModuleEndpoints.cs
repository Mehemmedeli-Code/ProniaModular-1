using Microsoft.AspNetCore.Routing;
using ProniaModular.Modules.Products.Features.Categories;
using ProniaModular.Modules.Products.Features.ProductSizes;
using ProniaModular.Modules.Products.Features.Products;
using ProniaModular.Modules.Products.Features.Sizes;

namespace ProniaModular.Modules.Products
{
    public static class ProductsModuleEndpoints
    {
        public static IEndpointRouteBuilder MapProductsModule(this IEndpointRouteBuilder app)
        {
            app.MapCategoryEndpoints();
            app.MapSizeEndpoints();
            app.MapProductEndpoints();
            app.MapProductSizeEndpoints();

            return app;
        }
    }
}
