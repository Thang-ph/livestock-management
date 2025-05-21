namespace BusinessObjects.Models
{
    public class OrderDetail : BaseEntity
    {
        public string OrderId { get; set; }
        public string LivestockId { get; set; }
        public DateTime? ExportedDate { get; set; }
        public virtual Order Order { get; set; }
        public virtual Livestock Livestock { get; set; }
    }
}
