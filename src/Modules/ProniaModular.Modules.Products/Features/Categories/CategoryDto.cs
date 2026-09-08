namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record CategoryDto(
        long Id,
        string Name,
        int ProductCount,
        bool IsDeleted,
        DateTime CreatedAt);
}
