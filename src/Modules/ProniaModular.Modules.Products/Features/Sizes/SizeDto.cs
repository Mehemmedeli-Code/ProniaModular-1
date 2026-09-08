namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record SizeDto(
        long Id,
        string Name,
        int ProductCount,
        bool IsDeleted,
        DateTime CreatedAt);
}
