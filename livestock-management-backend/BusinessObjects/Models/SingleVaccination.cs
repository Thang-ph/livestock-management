using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class SingleVaccination : BaseEntity
    {
      public  string? BatchImportId { get; set; }
        public string LivestockId { get; set; }
        public string MedicineId { get; set; }
        public virtual Livestock Livestock { get; set; }
        public virtual BatchImport? BatchImport { get; set; }
        public virtual Medicine Medicine { get; set; }
    }
}
