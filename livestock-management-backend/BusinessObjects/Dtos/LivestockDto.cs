using BusinessObjects.ConfigModels;
using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class ListLivestocksFilter : CommonListFilterModel
    {
        public decimal? MinWeight { get; set; }
        public decimal? MaxWeight { get; set; }
        public IEnumerable<string>? SpeciesIds { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IEnumerable<livestock_status>? Statuses { get; set; }
        public IEnumerable<string>? DiseaseIds { get; set; }
        public IEnumerable<string>? DiseaseVaccinatedIds { get; set; }
        public bool? IsMissingInformation { get; set; } = true;
    }

    public class LivestockIdFindDTO
    {
        public string InspectionCode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type SpecieType { get; set; }
    }

    public class LivestockSummary
    {
        public string Id { get; set; }
        public string InspectionCode { get; set; }
        public string Species { get; set; }
        public decimal? Weight { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_gender? Gender { get; set; } = null;
        public string? Color { get; set; } = "N/A";
        public string? Origin { get; set; } = "N/A";
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_status Status { get; set; }
    }

    public class LivestockBatchImportInfo
    {
        public string Id { get; set; }
        public string InspectionCode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type SpecieType { get; set; }
        public string SpecieName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_status LiveStockStatus { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_gender? Gender { get; set; } = null;
        public string? Color { get; set; } = "N/A";
        public decimal? Weight { get; set; }
        public DateTime? Dob { get; set; } = null;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_import_status ImportStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ImportedDate { get; set; } = null;
    }

    public class LivestockBatchImportScanInfo
    {
        public string LivestockId { get; set; }
        public string BatchImportName { get; set; }
        public string InspectionCode { get; set; }
        public string SpecieName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_gender? Gender { get; set; } = null;
        public string? Color { get; set; } = "N/A";
        public decimal? Weight { get; set; }
        public DateTime? Dob { get; set; } = null;
        public DateTime CreatedAt { get; set; }
        public int Total { get; set; }
        public int Imported { get; set; }
    }

    public class AddImportLivestockDTO
    {
        [Required(ErrorMessage = "ID trong thẻ tai không được trống.")]
        public string Id { get; set; }
        [Required(ErrorMessage = "Mã kiểm dịch không được để trống.")]
        public string InspectionCode { get; set; }
        [Required(ErrorMessage = "Loại vật nuôi không được để trống.")]
        public string SpecieId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_status? LivestockStatus { get; set; } = livestock_status.KHỎE_MẠNH;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_gender? Gender { get; set; } = livestock_gender.ĐỰC;
        public string? Color { get; set; } = "Nâu";
        [Range(0.1, double.MaxValue, ErrorMessage = "Khối lượng phải lớn hơn 0.")]
        public decimal? Weight { get; set; } = 100;
        public DateTime? Dob { get; set; } = null;
        public string? MedicineId { get; set; }
        public string? RequestedBy { get; set; } = "SYS";
    }

    public class UpdateImportLivestockDTO
    {
        [Required(ErrorMessage = "Tên vật nuôi không được để trống.")]
        public string? SpecieId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_gender? Gender { get; set; } = livestock_gender.ĐỰC;
        public string? Color { get; set; } = "Nâu";
        [Range(0.1, double.MaxValue, ErrorMessage = "Khối lượng phải lớn hơn 0.")]
        public decimal? Weight { get; set; } = 100;
        public DateTime? Dob { get; set; } = null;
        public string? RequestedBy { get; set; } = "SYS";
    }

    public class ListLivestocks : ResponseListModel<LivestockSummary> { }
    
    public class LivestockVaccineInfo
    {
        public string Id { get; set; }
        public string InspectionCode { get; set; }
        public string Species { get; set; }
        public string? Color { get; set; } = "N/A";
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_status Status { get; set; } 
        public DateTime? DateConduct { get; set; }
        public string ConductedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class LivestockVaccineInfoById
    {
        public string Id { get; set; }
        public string InspectionCode { get; set; }
        public string Species { get; set; }
        public string? Color { get; set; } = "N/A";
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_status Status { get; set; }
        public List<LivestockViccineInfoByIdDetail> Injections_count { get; set; }
        public DateTime? DateConduct { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class LivestockViccineInfoByIdDetail
    {
        public int Injections_count { get; set; } = 0;
        public string DiseaseName { get; set; }
        public string Name { get; set; }
        public string ConductedBy { get; set; }

        public string? Description { get; set; }
    }

    public class LivestockGeneralInfo : LivestockSummary
    {
        public string? InspectionCode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_status Status { get; set; }
        public DateTime? Dob { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_gender? Gender { get; set; }
        public string? Color { get; set; }
        public string? Origin { get; set; }
        public string? SpeciesId { get; set; }
        public decimal? WeightOrigin { get; set; }
        public decimal? WeightExport { get; set; }
        public decimal? WeightEstimate { get; set; }
        public string BarnId { get; set; }
    }

    public class VaccinationDetail
    {
        public DateTime DateTime { get; set; }
        public string VaccineId { get; set; }
        public string VaccineName { get; set; }
        public string? Description { get; set; } = "N/A";
    }

    public class LivestockVaccinationHistory : ResponseListModel<VaccinationDetail>
    {
        public string LivestockId { get; set; }
        public string InspectionCode { get; set; }
        public List<LivestockVaccinationDetail> vaccineHistory { get; set; }
    }
    public class LivestockVaccinationDetail
    {
        public DateTime createdAt { get; set; }
        public string vaccine { get; set; }
        public string description { get; set; }
    }
    public class SicknessDetail
    {
        public DateTime DateTime { get; set; }
        public string? MedicineId { get; set; } = null;
        public string? MedicineName { get; set; } = null;
        public string Disease { get; set; } = "N/A";
        public string Symptom { get; set; } = "N/A";
        public string? Description { get; set; } = "N/A";
    }

    public class LivestockSicknessHistory : ResponseListModel<SicknessDetail>
    {
        public string LivestockId { get; set; }
        public string InspectionCode { get; set; }
        public List<LivestockSicknessDetail> disease { get; set; }
    }
   public class LivestockSicknessDetail
    {
        public string? Symptom { get; set; }
        public string Disease { get; set; }
        public DateTime dateOfRecord { get; set; }
        public string MedicineName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public medical_history_status Status { get; set; }

    }

    public class ScanLivestockQrCode
    {
        public string? LivestockId { get; set; }
        public string? InspectionCode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type SpecieType { get; set; }

    }

    public class DashboardLivestock
    {
        public DiseaseRatioSummary DiseaseRatioSummary { get; set; }
        public VaccinationRatioSummary VaccinationRatioSummary { get; set; }
        public int TotalDisease { get; set; } = 0;
        public int TotalLivestockMissingInformation { get; set; } = 0;
        public InspectionCodeQuantitySummary InspectionCodeQuantitySummary { get; set; }
        public SpecieRatioSummary SpecieRatioSummary { get; set; }
        public WeightRatioSummary WeightRatioSummary { get; set; }
    }

    public class DiseaseRatio 
    {
        public string DiseaseId { get; set; }
        public string DiseaseName { get; set; }
        public int Quantity { get; set; } = 0;
        public decimal Ratio { get; set; } = 0;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public severity Severity { get; set; } = severity.MEDIUM;
    }

    public class DiseaseRatioSummary : ResponseListModel<DiseaseRatio>
    {
        public decimal TotalRatio { get; set; } = 0;
    }

    public class VaccinationRatio
    {
        public string DiseaseId { get; set; }
        public string DiseaseName { get; set; }
        public decimal Ratio { get; set; } = 0;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public severity Severity { get; set; } = severity.MEDIUM;
    }

    public class VaccinationRatioSummary : ResponseListModel<VaccinationRatio>
    {

    }

    public class SpecieRatio
    {
        public string SpecieId { get; set; }
        public string SpecieName { get; set; }
        public int Quantity { get; set; } = 0;
        public decimal Ratio { get; set; } = 0;
    }

    public class SpecieRatioSummary : ResponseListModel<SpecieRatio>
    {

    }

    public class WeightRatio
    {
        public string WeightRange { get; set; }
        public int Quantity { get; set; } = 0;
        public decimal Ratio { get; set; } = 0;
    }

    public class WeightRatioBySpecie
    {
        public string SpecieId { get; set; }
        public string SpecieName { get; set; }
        public IEnumerable<WeightRatio> WeightRatios { get; set; }
        public int TotalQuantity { get; set; } = 0;
    }

    public class WeightRatioSummary : ResponseListModel<WeightRatioBySpecie>
    {

    }

    public class InspectionCodeQuantityBySpecie
    {
        public specie_type Specie_Type { get; set; }
        public int TotalQuantity { get; set; } = 0;
        public int RemainingQuantity { get; set; } = 0;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public severity Severity { get; set; } = severity.MEDIUM;
    }
    public class InspectionCodeQuantitySummary : ResponseListModel<InspectionCodeQuantityBySpecie>
    {

    }
}
