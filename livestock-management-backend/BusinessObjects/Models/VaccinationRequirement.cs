using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class VaccinationRequirement : BaseEntity
    {
        public int InsuranceDuration { get; set; }
        public string DiseaseId { get; set; }
        public string? ProcurementDetailId { get; set; }
        public string? OrderRequirementId { get; set; }
        public virtual Disease Disease { get; set; }
        public virtual ProcurementDetail ProcurementDetail { get; set; }
        public virtual OrderRequirement OrderRequirement { get; set; }
    }
}
