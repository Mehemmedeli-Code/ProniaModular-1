namespace ProniaModular.Modules.Products.Entities
{
    public class ProductSize
    {
        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public long SizeId { get; set; }
        public Size Size { get; set; } = null!;
    }
}
