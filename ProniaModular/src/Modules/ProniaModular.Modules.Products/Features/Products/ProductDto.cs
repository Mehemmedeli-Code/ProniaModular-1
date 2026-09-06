namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record ProductDto(long Id, string Name, decimal Price, string? Description, long CategoryId, string CategoryName);
}
