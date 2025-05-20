using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class BatchVaccinationProcurement : BaseEntity
    {
        public string BatchVaccinationId { get; set; }
        public string ProcurementDetailId { get; set; }

        public virtual BatchVaccination BatchVaccination { get; set; }
        public virtual ProcurementDetail ProcurementDetail { get; set; }
    }
}
