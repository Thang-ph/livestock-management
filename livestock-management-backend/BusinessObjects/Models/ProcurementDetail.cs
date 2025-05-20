namespace BusinessObjects.Models
{
    public class ProcurementDetail : BaseEntity
    {
        public string ProcurementPackageId { get; set; }
        public string SpeciesId { get; set; }
        public decimal? RequiredWeightMax { get; set; }
        public decimal? RequiredWeightMin { get; set; }
        public int? RequiredAgeMin { get; set; }
        public int? RequiredAgeMax { get; set; }
        public string? Description { get; set; }
        public int? RequiredQuantity { get; set; }
        public int? RequiredInsurance { get; set; }

        public virtual ProcurementPackage ProcurementPackage { get; set; }
        public virtual Species Species { get; set; }
        public virtual ICollection<VaccinationRequirement> VaccinationRequirements { get; set; }
        public virtual ICollection<BatchVaccinationProcurement> BatchVaccinationProcurement { get; set; }

    }
}
