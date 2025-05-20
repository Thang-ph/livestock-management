using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class LivestockProcurement : BaseEntity
    {
        public string LivestockId { get; set; }
        public string ProcurementPackageId { get; set; }

        public virtual Livestock Livestock { get; set; }
        public virtual ProcurementPackage ProcurementPackage { get; set; }
    }
}
