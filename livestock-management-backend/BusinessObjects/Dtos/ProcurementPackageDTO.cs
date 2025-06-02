using BusinessObjects.ConfigModels;
using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class ProcurementPackageDto
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Tên gói thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Tên gói thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; }
        public string Status { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; }
        public virtual ICollection<BatchExport>? BatchExports { get; set; }
        public virtual ICollection<LivestockProcurement>? LivestockProcurements { get; set; }
        public virtual ICollection<ProcurementDetail>? ProcurementDetails { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100, ErrorMessage = "Tên người tạo không được vượt quá 100 ký tự")]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [MaxLength(100, ErrorMessage = "Tên người cập nhật không được vượt quá 100 ký tự")]
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class ProcurementPackageQuery
    {
        public string? Name { get; set; } = null;
        public string? Status { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? CreatedBy { get; set; } = null;
        public string? UpdatedBy { get; set; } = null;
        public string? SortBy { get; set; } = null;
        public bool IsDecsending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UpdateProcurementPackageDTO
    {
        [Required(ErrorMessage = "Tên gói thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Tên gói thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; }

        public string Status { get; set; }
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; }
        [MaxLength(100, ErrorMessage = "Tên người cập nhật không được vượt quá 100 ký tự")]
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class CreateProcurementPackageDTO
    {
        [Required(ErrorMessage = "Tên gói thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Tên gói thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; }

        public string Status { get; set; }
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

    }

    public class ListProcurementsFilter : CommonListFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public procurement_status? Status { get; set; }
    }

    public class ProcurementSummary
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime? SuccessDate { get; set; } = null;
        public DateTime? ExpirationDate { get; set; } = null;
        public DateTime? CompletionDate { get; set; } = null;
        public int TotalExported { get; set; } = 0;
        public int TotalRequired { get; set; } = 0;
        public int TotalSelected { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public procurement_status Status { get; set; }

        public HandoverInformation Handoverinformation { get; set; }
    }
    public class HandOverProcessProcurement
    {
        public string ProcurementId { get; set; }  
        public DateTime? SuccessDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int TotalRequired { get; set; }
        public int TotalHandover { get; set; }
    }
    public class HandoverInformation
    {
        public int totalSelected { get; set; } = 0;

        public int completeCount { get; set; }
        public int totalCount { get; set; }
    }

    public class ListProcurements : ResponseListModel<ProcurementSummary>
    {

    }

    public class ProcurementGeneralInfo : ProcurementSummary
    {
        public string Owner { get; set; }
        public int? ExpiredDuration { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? CreatedBy { get; set; } = "N/A";
        public IEnumerable<ProcurementDetails> Details { get; set; }
    }
    public class ProcurementOverview
    {
        public int bidding { get; set; }
        public int waitHandOver { get; set; }
        public int handOver { get; set; }
        public int complete { get; set; }
        public int cancel { get; set; }
        public int waitSelect { get; set; }
    }
   
    public class ProcurementDetails
    {
        public string Id { get; set; }
        public string ProcurementName { get; set; }
        public string SpeciesId { get; set; }
        public string SpeciesName { get; set; }
        public decimal? RequiredWeightMax { get; set; } = null;
        public decimal? RequiredWeightMin { get; set; } = null;
        public int? RequiredAgeMin { get; set; } = null;
        public int? RequiredAgeMax { get; set; } = null;
        public string? Description { get; set; } = "N/A";
        public int? RequiredQuantity { get; set; } = null;
        public int? RequiredInsurance { get; set; } = null;
        public List<VaccinationRequireProcurementDetail> vaccinationRequire { get; set; }
    }
    public class VaccinationRequireProcurementDetail
    {
       public string DiseaseName { get; set; }
        public int  InsuranceDuration { get; set; } 
    }
    public class CreateProcurementPackageRequest
    {
        [Required(ErrorMessage = "Mã gói thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Mã gói thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Mã không thể dài quá 280 ký tự")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Tên gói thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Tên gói thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Tên bên mời thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Tên bên mời thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Owner { get; set; }
        [Range(0, 1000, ErrorMessage = "Thời hạn gói thầu phải nằm trong khoảng 0 tới 100")]
        public int? ExpiredDuration { get; set; } = null;
        [MinLength(2, ErrorMessage = "Mô tả gói thầu phải có ít nhất 2 ký tự")]
        public string? Description { get; set; } = null;
        [Required(ErrorMessage = "Yêu cầu kỹ thuật không thể để trống")]
        public IEnumerable<CreateProcurementDetailsRequest> Details { get; set; }
        public string? RequestedBy { get; set; } = null;
    }

    public class CreateProcurementDetailsRequest
    {
        public string SpeciesId { get; set; }
        [Range(0, 1000, ErrorMessage = "Yêu cầu trọng lượng nhỏ nhất phải nằm trong khoảng từ 0 tới 1000")]
        public decimal? RequiredWeightMin { get; set; } = null;
        [Range(0, 1000, ErrorMessage = "Yêu cầu trọng lượng lớn nhất phải nằm trong khoảng từ 0 tới 1000")]
        public decimal? RequiredWeightMax { get; set; } = null;
        [Range(0, 1000, ErrorMessage = "Yêu cầu tuổi nhỏ nhất phải nằm trong khoảng từ 0 tới 1000")]
        public int? RequiredAgeMin { get; set; } = null;
        [Range(0, 1000, ErrorMessage = "Yêu cầu tuổi lớn nhất phải nằm trong khoảng từ 0 tới 1000")]
        public int? RequiredAgeMax { get; set; } = null;
        [Range(0, 100, ErrorMessage = "Yêu cầu thời hạn bảo hành phải nằm trong khoảng từ 0 tới 100")]
        public int? RequiredInsuranceDuration { get; set; } = null;
        [Required(ErrorMessage = "Yêu cầu số lượng không được để trống")]
        [Range(0, 1000, ErrorMessage = "Yêu cầu số lượng phải nằm trong khoảng từ 0 tới 1000")]
        public int RequiredQuantity { get; set; }
        [MinLength(2, ErrorMessage = "Mô tả gói thầu phải có ít nhất 2 ký tự")]
        public string? Description { get; set; } = null;
        public List<VaccinationRequireProcurementDetailCreate> vaccinationRequireProcurementDetailCreates { get; set; }
    }
    public class VaccinationRequireProcurementDetailCreate
    {

        public string DiseaseId { get; set; }
        public int InsuranceDuration { get; set; }
    }
    public class UpdateProcurementPackageRequest : CreateProcurementPackageRequest
    {
        [Required(ErrorMessage = "ID gói thầu không được để trống")]
        public string Id { get; set; }
        [Required(ErrorMessage = "Mã gói thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Mã gói thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Mã không thể dài quá 280 ký tự")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Tên gói thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Tên gói thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Tên bên mời thầu không được để trống")]
        [MinLength(2, ErrorMessage = "Tên bên mời thầu phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Owner { get; set; }
        [Range(0, 1000, ErrorMessage = "Thời hạn gói thầu phải nằm trong khoảng 0 tới 100")]
        public int? ExpiredDuration { get; set; } = null;
        [MinLength(2, ErrorMessage = "Mô tả gói thầu phải có ít nhất 2 ký tự")]
        public string? Description { get; set; } = null;
        [Required(ErrorMessage = "Yêu cầu kỹ thuật không thể để trống")]
        public IEnumerable<UpdateProcurementDetailsRequest> Details { get; set; }
        public string? RequestedBy { get; set; } = null;
    }

    public class UpdateProcurementDetailsRequest
    {
        [Required(ErrorMessage = "ID gói thầu không được để trống")]
        public string Id { get; set; }
        public string SpeciesId { get; set; }
        [Range(0, 1000, ErrorMessage = "Yêu cầu trọng lượng nhỏ nhất phải nằm trong khoảng từ 0 tới 1000")]
        public decimal? RequiredWeightMin { get; set; } = null;
        [Range(0, 1000, ErrorMessage = "Yêu cầu trọng lượng lớn nhất phải nằm trong khoảng từ 0 tới 1000")]
        public decimal? RequiredWeightMax { get; set; } = null;
        [Range(0, 1000, ErrorMessage = "Yêu cầu tuổi nhỏ nhất phải nằm trong khoảng từ 0 tới 1000")]
        public int? RequiredAgeMin { get; set; } = null;
        [Range(0, 1000, ErrorMessage = "Yêu cầu tuổi lớn nhất phải nằm trong khoảng từ 0 tới 1000")]
        public int? RequiredAgeMax { get; set; } = null;
        [Range(0, 100, ErrorMessage = "Yêu cầu thời hạn bảo hành phải nằm trong khoảng từ 0 tới 100")]
        public int? RequiredInsurance { get; set; } = null;
        [Required(ErrorMessage = "Yêu cầu số lượng không được để trống")]
        [Range(0, 1000, ErrorMessage = "Yêu cầu số lượng phải nằm trong khoảng từ 0 tới 1000")]
        public int RequiredQuantity { get; set; }
        [MinLength(2, ErrorMessage = "Mô tả gói thầu phải có ít nhất 2 ký tự")]
        public string? Description { get; set; } = null;
        public List<VaccinationRequireProcurementDetailUpdate> vaccinationRequireProcurementDetailUpdates { get; set; }
    }
    public class VaccinationRequireProcurementDetailUpdate
    {
        public string DiseaseId { get; set; }
        public int InsuranceDuration { get; set; }
    }
    public class CreateExportDetail
    {
        public string BatchExportId { get; set; }
        public string LivestockId { get; set; }
        public string RequestedBy { get; set; }
    }

    public class UpdateExportDetail : CreateExportDetail
    {
        public string BatchExportDetailId { get; set; }
    }

    public class ExportDetail
    {
        public string BatchExportDetailId { get; set; }
        public string LivestockId { get; set; }
        public string? InspectionCode { get; set; } = "N/A";
        public decimal? WeightExport {  get; set; }
        public DateTime? HandoverDate { get; set; }
        public DateTime? ExportDate { get; set; }
        public DateTime? ExpiredInsuranceDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_export_status Status { get; set; }
    }

    public class ListExportDetails : ResponseListModel<ExportDetail>
    {
        public string BatchExportId { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerPhone { get; set; } = "N/A";
        public string? CustomerAddress { get; set; } = "N/A";
        public string? CustomerNote { get; set; } = "N/A";
        public int TotalLivestocks {  get; set; }
        public int Received { get; set; }
        public int Remaining { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_export_status Status { get; set; }
    }

    public class ProcurementStatus
    {
        public string Id { get; set; }
        public string RequestedBy { get; set; }

    }
}

