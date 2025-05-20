using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BusinessObjects.ConfigModels;
using BusinessObjects.Models;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class InsurenceRequestOverviewDTO
    {
        public int Quantity { get; set; }
        public String Status { get; set; }
    }
    public class ListInsurenceRequestOverviewDTO : ResponseListModel<InsurenceRequestOverviewDTO> { }
    public class ListInsuranceStatusDTO : ResponseListModel<string> { }
    public class InsurenceRequestDTO : BaseEntity
    {
        public string RequestLivestockId { get; set; }
        public string DiseaseName { get; set; }
        public string? NewLivestockId { get; set; }
        public String Status { get; set; }
        public string? ProcurementDetailId { get; set; }
        public string? OrderRequirementId { get; set; }
        public string? InsurenceRequestName { get; set; }
        public string? RequestLivestockStatus {get; set;}
        public string? InspectionCodeRequest {  get; set;}
        public string? InspectionCodeNew {get;set;}
    }

    public class ListInsurenceRequestDTO : ResponseListModel<InsurenceRequestDTO> { }

    public class InsurenceRequestInfoDTO : BaseEntity
    {
        public string RequestLivestockId { get; set; }
        public string DiseaseId { get; set; }
        public string? OtherReason { get; set; }
        public string ImageUris { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public insurance_request_status Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? ProcessingAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? NewLivestockId { get; set; }
        public string? RejectReason { get; set; }
        public string? Note { get; set; }
        public string? ProcurementDetailId { get; set; }
        public string? OrderRequirementId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public insurance_request_livestock_status RequestLivestockStatus { get; set; } 
        public bool IsLivestockReturn { get; set; }
        public string? Species { get; set; }
        public decimal? ExportWeight { get; set; }
        public decimal? ExportWeightReturn { get; set; }
        public string? InspectionCodeRequest { get; set; }
        public string? InspectionCodeNew { get; set; }
        public string? DiseaseName { get; set; }
    }
        public class InsurenceLiveStockDTO
        {
            public string LivestockId { get; set; }
            public string LivestockName { get; set; }
            public string LivestockType { get; set; }
            public string Id { get; set; }
        }
        public class CreateInsurenceDTO
        {
            public string Type { get; set; } //Loại hợp đồng
            public string LivestockId { get; set; }
            public string DiseaseId { get; set; }
            public string? OtherReason { get; set; }
            public string ImageUris { get; set; }
            public string SpecieType { get; set; }
            public string CreatedBy { get; set; } = "HieuNT";
        
        }
    public class CreateInsurenceQrDTO
    {
        public string LivestockId { get; set; }
        public string CreatedBy { get; set; } = "HieuNT";

    }

    public class ChangeStatusInsuranceDto
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string UpdatedBy { get; set; } = "HieuNT";
    }
    public class InsurenceFilter : CommonListFilterModel
        {
            public string? LivestockId { get; set;}
            public string? ProcurmentId { get; set; }
            public string? Status { get; set; }
        }

    public class UpdateInsuranceLivestockDto
    {
        public decimal Weight { get; set; } = 0;
        public string LivestockId { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty ;
        public string Id { get; set; } = string.Empty;
    }

    public class RemoveInsuranceLivestockDto
    {
        public string UpdatedBy { get; set; }
        public string Id { get; set; }
    }

    public class RejectInsuranceDto 
    {
        public string UpdatedBy { get; set; }
        public string Id { get; set; }
        public string? reasonReject {  get; set; }
    }

    public class UpdateInsuranceRequestInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string? DiseaseId { get; set; } = string.Empty;
        public string? Note {  get; set; }
        public string? OtherReason { get; set; }
        public String? RequestLivestockStatus { get; set; }
    }

    public class VaccinationDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int DateOfInsurance { get; set; }
    }

    public class VaccinationProcurmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public List<VaccinationDto> vaccinationRequirements { get; set; }
    }

    public class InsuranceRequestVancinationDto
    {
        public string Id { get; set; }
    }
}
