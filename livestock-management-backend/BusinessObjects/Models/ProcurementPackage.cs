using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class ProcurementPackage : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Owner { get; set; }
        public int? ExpiredDuration { get; set; }
        public DateTime? SuccessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public procurement_status Status { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<BatchExport> BatchExports { get; set; }
        public virtual ICollection<LivestockProcurement> LivestockProcurements { get; set; }
        public virtual ICollection<ProcurementDetail> ProcurementDetails { get; set; }
        public virtual ICollection<InsuranceRequest> InsuranceRequests { get; set; }

    }
}
