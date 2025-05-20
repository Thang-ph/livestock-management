using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class BatchImport : BaseEntity
    {
        public string Name { get; set; }
        public int EstimatedQuantity { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public batch_import_status Status { get; set; }
        public string OriginLocation { get; set; }
        public string BarnId { get; set; }
        public virtual Barn Barn { get; set; }
        public virtual ICollection<BatchImportDetail> BatchImportDetails { get; set; }
        public virtual ICollection<PinnedBatchImport> PinnedBatchImports { get; set; }
        public virtual ICollection<SingleVaccination> SingleVaccinations { get; set; }

    }
}
