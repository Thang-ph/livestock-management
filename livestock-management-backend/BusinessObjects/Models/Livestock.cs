using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class Livestock : BaseEntity
    {
        public string? InspectionCode { get; set; }  // Unique (cần cấu hình thêm qua Fluent API nếu cần)
        public livestock_status Status { get; set; }
        public DateTime? Dob { get; set; }
        public livestock_gender? Gender { get; set; }
        public string? Color { get; set; }
        public string? Origin { get; set; }
        public string? SpeciesId { get; set; }
        public decimal? WeightOrigin { get; set; }
        public decimal? WeightExport { get; set; }
        public decimal? WeightEstimate { get; set; }
        public string BarnId { get; set; }

        // Navigation properties
        public virtual Species Species { get; set; }
        public virtual Barn Barn { get; set; }
        public virtual ICollection<MedicalHistory> MedicalHistories { get; set; }
        public virtual ICollection<BatchImportDetail> BatchImportDetails { get; set; }
        public virtual ICollection<BatchExportDetail> BatchExportDetails { get; set; }
        public virtual ICollection<LivestockVaccination> LivestockVaccinations { get; set; }
        public virtual ICollection<LivestockProcurement> LivestockProcurements { get; set; }
        public virtual ICollection<SearchHistory> SearchHistories { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<InsuranceRequest> InsuranceRequests { get; set; }
        public virtual ICollection<SingleVaccination> SingleVaccinations { get; set; }

    }
}
