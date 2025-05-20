using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class LivestockVaccination : BaseEntity
    {
        public string LivestockId { get; set; }
        public string BatchVaccinationId { get; set; }

        public virtual Livestock Livestock { get; set; }
        public virtual BatchVaccination BatchVaccination { get; set; }
    }
}
