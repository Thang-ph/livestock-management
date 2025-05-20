using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Models
{
    public class Disease : BaseEntity
    {
        public string Name { get; set; }   
        public string Symptom { get; set; }
        public string? Description { get; set; }
        public int? DefaultInsuranceDuration { get; set; }
        public disease_type Type {  get; set; }  
        public virtual ICollection<DiseaseMedicine> DiseaseMedicines { get; set; }
        public virtual ICollection<MedicalHistory> MedicalHistories { get; set; }
        public virtual ICollection<VaccinationRequirement> VaccinationRequirements { get; set; }
        public virtual ICollection<InsuranceRequest> InsuranceRequests { get; set; }
    }
}
