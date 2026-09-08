namespace ProniaModular.Modules.Products.Entities.Common
{
    public abstract class BaseAccountableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        protected BaseAccountableEntity()
        {
            CreatedBy = "Admin";
        }
    }
}
