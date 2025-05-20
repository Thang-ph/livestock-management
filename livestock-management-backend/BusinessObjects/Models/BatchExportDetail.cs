using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class BatchExportDetail : BaseEntity
    {
        public string BatchExportId { get; set; }
        public string LivestockId { get; set; }
        public decimal? PriceExport { get; set; }
        public decimal? WeightExport { get; set; }
        public DateTime? HandoverDate { get; set; }
        public DateTime? ExportDate { get; set; }
        public batch_export_status Status { get; set; }
        public DateTime? ExpiredInsuranceDate { get; set; }

        public virtual BatchExport BatchExport { get; set; }
        public virtual Livestock Livestock { get; set; }
    }
}
