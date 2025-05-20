using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class BatchExport : BaseEntity
    {
        public string? BarnId { get; set; }
        public string ProcurementPackageId { get; set; }
        public batch_export_status Status { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerNote { get; set; }
        public int Total {  get; set; }
        public int Remaining {  get; set; }
        public virtual Barn Barn { get; set; }
        public virtual ProcurementPackage ProcurementPackage { get; set; }
        public virtual ICollection<BatchExportDetail> BatchExportDetails { get; set; }
    }
}
