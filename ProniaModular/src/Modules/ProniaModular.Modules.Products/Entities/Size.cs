using ProniaModular.Modules.Products.Entities.Common;

namespace ProniaModular.Modules.Products.Entities
{
    public class Size : BaseAccountableEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
    }
}
