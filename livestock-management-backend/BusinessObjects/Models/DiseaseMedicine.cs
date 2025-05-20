using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class DiseaseMedicine : BaseEntity
    {
        public string DiseaseId { get; set; }
        public string MedicineId { get; set; }
        public virtual Disease Disease {  get; set; }
        public virtual Medicine Medicine { get; set; }
    }
}
