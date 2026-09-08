using ProniaModular.Modules.Products.Entities.Common;

namespace ProniaModular.Modules.Products.Entities
{
    public class Product : BaseAccountableEntity
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Description { get; set; }

        public long CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
    }
}
