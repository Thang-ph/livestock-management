using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class Species : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? GrowthRate { get; set; }
        public decimal? DressingPercentage { get; set; }
        public specie_type Type { get; set; }

        public virtual ICollection<Livestock> Livestocks { get; set; }
        public virtual ICollection<ProcurementDetail> ProcurementDetails { get; set; }
        public virtual ICollection<OrderRequirement> OrderRequirements { get; set; }
    }
}
