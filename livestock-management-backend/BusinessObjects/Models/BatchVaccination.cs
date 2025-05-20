using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class BatchVaccination : BaseEntity
    {
        public DateTime? DateSchedule { get; set; }
        public DateTime? DateConduct { get; set; }
        public string Name { get; set; }
        public string ConductedBy{ get; set; }
        public string VaccineId { get; set; }
        public string? Description { get; set; }
        public batch_vaccination_type Type { get; set; }
        public batch_vaccination_status Status { get; set; }

        public virtual Medicine Vaccine { get; set; }
        public virtual ICollection<LivestockVaccination> LivestockVaccinations { get; set; }
        public virtual ICollection<BatchVaccinationProcurement> BatchVaccinationProcurement { get; set; }

    }
}
