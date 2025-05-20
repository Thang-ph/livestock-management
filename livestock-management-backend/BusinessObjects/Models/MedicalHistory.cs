using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class MedicalHistory : BaseEntity
    {
        public string LivestockId { get; set; }
        public string? Symptom { get; set; }
        public string DiseaseId { get; set; }
        public string Status { get; set; }
        public string MedicineId { get; set; }

        public virtual Livestock Livestock { get; set; }
        public virtual Medicine Medicine { get; set; }
        public virtual Disease Disease { get; set; }
    }

}
