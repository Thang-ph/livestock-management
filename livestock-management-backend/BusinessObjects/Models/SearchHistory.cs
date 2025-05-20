namespace BusinessObjects.Models
{
    public class SearchHistory : BaseEntity
    {
        public string LivestockId { get; set; }
        public virtual Livestock Livestock { get; set; }
    }
}
