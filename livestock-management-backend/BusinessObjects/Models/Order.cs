using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class Order : BaseEntity
    {
        public string CustomerId { get; set; }
        public string Code { get; set; }
        public order_type Type { get; set; }
        public order_status Status { get; set; }
        public DateTime? StartPrepareAt { get; set; }
        public DateTime? AwaitDeliverAt { get; set; }
        public DateTime? StartDeliverAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<OrderRequirement> OrderRequirements { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<InsuranceRequest> InsuranceRequests { get; set; }
    }
}
