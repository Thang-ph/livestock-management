namespace BusinessObjects.Models
{
    public class Customer : BaseEntity
    {
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
