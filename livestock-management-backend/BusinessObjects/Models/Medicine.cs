using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class Medicine : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public medicine_type Type { get; set; }

        public virtual ICollection<MedicalHistory> MedicalHistories { get; set; }
        public virtual ICollection<BatchVaccination> BatchVaccinations { get; set; }
        public virtual ICollection<DiseaseMedicine> DiseaseMedicines { get; set; }
        public virtual ICollection<SingleVaccination> SingleVaccinations { get; set; }

    }
}
