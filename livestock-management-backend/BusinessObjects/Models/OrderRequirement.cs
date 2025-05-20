namespace BusinessObjects.Models
{
    public class OrderRequirement : BaseEntity
    {
        public string OrderId { get; set; }
        public string SpecieId { get; set; }
        public decimal? Weight { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public virtual Order Order { get; set; }
        public virtual Species Species { get; set; }
        public virtual ICollection<VaccinationRequirement> VaccinationRequirements { get; set; }
    }
}
