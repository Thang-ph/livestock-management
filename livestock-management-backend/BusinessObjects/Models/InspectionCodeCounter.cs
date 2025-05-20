using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class InspectionCodeCounter : BaseEntity
    {

        public String SpecieType { get; set; }
        public string? CurrentRangeId { get; set; }
        public virtual InspectionCodeRange InspectionCodeRange { get; set; }
    }
}