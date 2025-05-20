using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class BatchImportDetail : BaseEntity
    {
        public string BatchImportId { get; set; }
        public string LivestockId { get; set; }
        public decimal? PriceImport { get; set; }
        public decimal? WeightImport { get; set; }
        public batch_import_status Status { get; set; }
        public int? InsuranceDuration { get; set; }
        public DateTime? ExpiredInsuranceDate { get; set; }
        public DateTime? ImportedDate { get; set; }


        public virtual BatchImport BatchImport { get; set; }
        public virtual Livestock Livestock { get; set; }
    }
}
