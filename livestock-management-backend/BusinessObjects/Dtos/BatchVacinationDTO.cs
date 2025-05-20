using BusinessObjects.Attribute;
using BusinessObjects.ConfigModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class BatchVacinationCreate
    {
        [Required(ErrorMessage = "Ngày dự kiến không được để trống")]
        [FutureDate(ErrorMessage = "Ngày dự kiến phải từ hôm nay trở đi.")]
        public DateTime? DateSchedule { get; set; }
        [Required(ErrorMessage = "Tên không được để trống")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Người thực hiện không được để trống")]
        [StringLength(100, ErrorMessage = "Tên người thực hiện không thể dài quá 100 ký tự")]
        public string ConductedBy { get; set; }

        [Required(ErrorMessage = "ID Vaccine không được để trống")]
        public string VaccineId { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Loại tiêm chủng không được để trống")]
        [EnumDataType(typeof(batch_vaccination_type), ErrorMessage = "Loại tiêm chủng không hợp lệ")]
        public batch_vaccination_type Type { get; set; }

        [Required(ErrorMessage = "Trạng thái tiêm chủng không được để trống")]
        [EnumDataType(typeof(batch_vaccination_status), ErrorMessage = "Trạng thái không hợp lệ")]
        public batch_vaccination_status Status { get; set; }
        [Required(ErrorMessage = "Người tạo không được để trống")]
        public string CreatedBy { get; set; }
    }
    public class BatchVacinationUpdate
    {
       
        [Required(ErrorMessage = "Tên không được để trống")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Ngày dự kiến không được để trống")]
        [FutureDate(ErrorMessage = "Ngày dự kiến phải từ hôm nay trở đi.")]
        public DateTime? DateSchedule { get; set; }

        [StringLength(100, ErrorMessage = "Tên người thực hiện không thể dài quá 100 ký tự")]
        [Required(ErrorMessage = "Người thực hiện không được để trống")]
        public string ConductedBy { get; set; }

        [Required(ErrorMessage = "ID Vaccine không được để trống")]
        public string VaccineId { get; set; }
        
        public string? Description { get; set; }

        [Required(ErrorMessage = "Loại tiêm chủng không được để trống")]
        [EnumDataType(typeof(batch_vaccination_type), ErrorMessage = "Loại tiêm chủng không hợp lệ")]
        public batch_vaccination_type Type { get; set; }

       
        [Required(ErrorMessage = "Người cập nhật không được để trống")]
        public string UpdatedBy { get; set; }
    }
    public class VaccinationSummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public medicine_type MedcicalType { get; set; }
        public string Symptom { get; set; }
        public DateTime? DateSchedule { get; set; }
        public string ConductedBy { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_vaccination_status Status { get; set; }
        public DateTime? DateConduct { get; set; }
        public DateTime CreatedAt { get; set; }

    }

    public class ListVaccination : ResponseListModel<VaccinationSummary>{ }

    public class ListLivestocksVaccination : ResponseListModel<LivestockVaccineInfo> { }

    public class VaccinationGeneral
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_vaccination_type VaccinationType { get; set; }
        public string MedicineName { get; set; }
        public string Symptom { get; set; }
        public DateTime? DateSchedule { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_vaccination_status Status { get; set; }
        public string ConductedBy { get; set; }
        public DateTime? DateConduct { get; set; }
        public string? Note { get; set; }
    }
    public class ListVaccinationsFliter : CommonListFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_vaccination_status? Status { get; set; }
    }

    public class ListLivestockVaccination : CommonListFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_status? Status { get; set; }
    }

    public class ChangeVaccinationBatchStatus
    {
        public string VaccinationBatchId { get; set; }
        public string RequestedBy { get; set; }
    }

    public class LivestockVaccinationInfo
    {
        public string LivestockId { get; set; }
        public string InspectionCode { get; set; }
        public string SpecieName { get; set; }
        public string Color { get; set; } = "N/A";
        public IEnumerable<VaccinationInfo> VaccinationInfos { get; set; }
    }

    public class VaccinationInfo
    {
        public string DiseaseName { get; set; }
        public int NumberOfVaccination { get; set; } = 0;
    }

    public class AddLivestockToVaccinationBatch
    {
        public string VaccinationBatchId { get; set; } 
        public string LivestockId { get; set; }
     
    }
   
    public class ListRequireVaccinationProcurement
    {
        public string ProcurementId { get; set; }
        public string ProcurementName { get; set; }
        public string ProcurementCode { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int LivestockQuantity { get; set; }
        public List<DiseaseRequire> diseaseRequires { get; set; }
    }
    public class ListSuggestReVaccination
    {
        public string BatchVaccinationId { get; set; }
        public string BatchVaccinationName { get; set; }
        public int LivestockQuantity { get; set; }
        public DateTime? LastDate { get; set; }
        public string MedicineName { get; set; }
        public string DiseasaName { get; set; }
    }
    public class ListFutureBatchVaccination
    {
        public string BatchVaccinationId { get; set; }
        public string BatchVaccinationName { get; set; }
        public string ConductName { get; set; }
        public string MedicineName { get; set; }
        public string DiseaseName { get; set; }
        public DateTime? SchedulteTime { get; set; }
    }
    public class DiseaseRequire
    {
        public string DiseaseName { get; set; }

        public int HasDone { get; set; }

    }
    public class RequireVaccinationProcurementDetail
    {
        public string ProcurementId { get; set; }
        public string ProcurementName { get; set; }
        public string ProcurementCode { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int LivestockQuantity { get; set; }
        public List<DiseaseRequireForSpecie> diseaseRequiresForSpecie { get; set; }
    }
    public class LivestockRequireVaccinationProcurement
    {
        public string LivestockId { get; set; }
        public string InspectionCode { get; set; }

        public string SpecieName { get; set; }
        public string Color { get; set; }
        public List<LivestockRequireDisease> livestockRequireDisease { get; set; }

    }
    public class LivestockRequireDisease
    {
        public string DiseaseName { get; set; }
        public string MedicineName { get; set; }
        public string BatchVaccinationId { get; set; }
    }
    public class DiseaseRequireForSpecie
    {
        public string DiseaseName { get; set; }
        public string SpecieName { get; set; }
        public string BatchVaccinationId { get; set; }
        public int HasDone { get; set; }
        public string MedicineName { get; set; }    
        public IsCreated isCreated { get; set; }
        public int TotalQuantity { get; set; }
    }
}
