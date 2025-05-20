using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class InsuranceRequest : BaseEntity
    {
        public string RequestLivestockId { get; set; }
        public string DiseaseId { get; set; }
        public string? OtherReason { get; set; }
        public string ImageUris { get; set; }
        public insurance_request_status Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? ProcessingAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? NewLivestockId { get; set; }
        public string? RejectReason { get; set; }
        public string? Note { get; set; }
        public string? ProcurementId { get; set; }
        public string? OrderId { get; set; }
        public bool IsLivestockReturn { get; set; }
        public insurance_request_livestock_status RequestLivestockStatus { get; set; }
        public virtual ProcurementPackage ProcurementPackage { get; set; }
        public virtual Order Order { get; set; }
        public virtual Livestock Livestock { get; set; }
        public virtual Disease Disease { get; set; }
    }
}
