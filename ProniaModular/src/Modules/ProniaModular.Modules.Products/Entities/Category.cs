using ProniaModular.Modules.Products.Entities.Common;

namespace ProniaModular.Modules.Products.Entities
{
    public sealed class Category : BaseAccountableEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
