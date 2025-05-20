using BusinessObjects.Attribute;
using BusinessObjects.ConfigModels;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class ListImportBatchesFilter : CommonListFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_import_status? Status { get; set; }
    }

    public class ImportBatchSummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int EstimatedQuantity { get; set; } = 0;
        public int ImportedQuantity { get; set; } = 0;
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_import_status Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

    }

    public class ListImportBatchesForAdmin : ResponseListModel<ImportSum>
    {

    }

    public class ImportSum
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int EstimatedQuantity { get; set; } = 0;
        public int ImportedQuantity { get; set; } = 0;
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_import_status Status { get; set; }
        public string PinnedId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ListImportBatches : ResponseListModel<ImportBatchSummary>
    {

    }

    public class ImportBatchDetails : ImportBatchSummary
    {
        public string OriginLocation { get; set; }
        public string ImportToBarn { get; set; }
        public ListImportedLivestocks ListImportedLivestocks { get; set; }
    }

    public class ListImportedLivestocks : ResponseListModel<ImportedLivestockInfo>
    {

    }

    public class ImportedLivestockInfo
    {
        public string Id { get; set; }
        public string LivestockId { get; set; }
        public string InspectionCode { get; set; }
        public string? SpecieId { get; set; }
        public string? SpecieName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type? SpecieType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImportedBy { get; set; }
        public DateTime? ImportedDate { get; set; }
        public decimal? WeightImport { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_status Status { get; set; }
    }

    public class BatchImportStatus
    {
        public string Id { get; set; }
        public string RequestedBy { get; set; }
    }
    public class BatchImportCreate
    {
        [Required(ErrorMessage = "Tên không được để trống.")]
        public string Name { get; set; }

        [GreaterThanZero(ErrorMessage = "Số lượng ước tính phải lớn hơn 0.")]
        public int EstimatedQuantity { get; set; }

        [Required(ErrorMessage = "Ngày hoàn thành dự kiến không được để trống.")]
        [FutureDateAttribute(ErrorMessage = "Ngày hoàn thành dự kiến phải từ hôm nay trở đi.")]
        public DateTime? ExpectedCompletionDate { get; set; }

        [Required(ErrorMessage = "Vị trí xuất xứ không được để trống.")]
        public string OriginLocation { get; set; }

        [Required(ErrorMessage = "Mã chuồng không được để trống.")]
        public string BarnId { get; set; }

        [Required(ErrorMessage = "Người tạo không được để trống.")]
        public string CreatedBy { get; set; }
    }
    public class BatchImportUpdate
    {
        [Required(ErrorMessage = "Tên không được để trống.")]
        public string Name { get; set; }

        [GreaterThanZero(ErrorMessage = "Số lượng ước tính phải lớn hơn 0.")]
        public int EstimatedQuantity { get; set; }

        [Required(ErrorMessage = "Ngày hoàn thành dự kiến không được để trống.")]
        [FutureDate(ErrorMessage = "Ngày hoàn thành dự kiến phải từ hôm nay trở đi.")]
        public DateTime? ExpectedCompletionDate { get; set; }

        [Required(ErrorMessage = "Vị trí xuất xứ không được để trống.")]
        public string OriginLocation { get; set; }

        [Required(ErrorMessage = "Mã chuồng không được để trống.")]
        public string BarnId { get; set; }


        [Required(ErrorMessage = "Người cập nhật không được để trống.")]
        public string UpdatedBy { get; set; }
    }
    public class BatchImportGet
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public int EstimatedQuantity { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public string OriginLocation { get; set; }

        public string BarnId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_import_status Status { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    //public class GetInspectionCodeRequest
    //{
    //    public string BatchImportId { get; set; }
    //    public string LivestockId { get; set; } //id of the QR code
    //    [JsonConverter(typeof(JsonStringEnumConverter))]
    //    public specie_type LivestockType { get; set; }
    //}

    //public class InspectionCodeCheck
    //{
    //    public string message { get; set; }
    //    public string InspectionCode { get; set; }
    //    public string LivestockId { get; set; } //id of the QR code
    //    [JsonConverter(typeof(JsonStringEnumConverter))]
    //    public specie_type LivestockType { get; set; }
    //    public int NumberLivestockAdded { get; set; } = 0;
    //    [JsonConverter(typeof(JsonStringEnumConverter))]
    //    public request_status CheckStatus { get; set; }
    //}

    
    //public class InspectionCodeDTO
    //{
    //    public string InspectionCode { get; set; }
    //    public string BatchImportName { get; set; }
    //    [JsonConverter(typeof(JsonStringEnumConverter))]
    //    public specie_type SpeciesType { get; set; }
    //    public int TotalConfirm { get; set; }
    //}

    //public class BatchImportScanDTO
    //{
    //    public string BatchImportId { get; set; }
    //    public string BatchImportName { get; set; }
    //    public int Taken {  get; set; }
    //    public int Total {  get; set; }
    //}


    public class ListPinnedImportBatches : ResponseListModel<PinnedBatchImportDTO>
    {

    }

    public class ListOverDueImportBatches : ResponseListModel<OverDueBatchImportDTO>
    {

    }

    public class ListNearDueImportBatches : ResponseListModel<NearDueBatchImportDTO>
    {

    }

    public class ListMissingImportBatches : ResponseListModel<MissingBatchImportDTO>
    {

    }

    public class ListUpcomingImportBatches : ResponseListModel<UpcomingBatchImportDTO>
    {

    }
    public class PinnedBatchImportDTO
    {
        public string Id { get; set; }
        public string BatchImportId { get; set; }
        public string BatchImportName { get; set; }
        public DateTime? BatchImportCompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OverDueBatchImportDTO
    {
        public string BatchImportId { get; set; }
        public string BatchImportName { get; set; }
        public string DayOver { get; set; }
        public DateTime? BatchImportCompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NearDueBatchImportDTO
    {
        public string BatchImportId { get; set; }
        public string BatchImportName { get; set; }
        public string Dayleft { get; set; }
        public DateTime? BatchImportCompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MissingBatchImportDTO
    {
        public string BatchImportId { get; set; }
        public string BatchImportName { get; set; }
        public string TotalMissing { get; set; }
        public DateTime? BatchImportCompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpcomingBatchImportDTO
    {
        public string BatchImportId { get; set; }
        public string BatchImportName { get; set; }
        public string Dayleft { get; set; }
        public DateTime? BatchImportCompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddLivestockToImportBatch
    {
        public string BatchImportId { get; set; }
        public string LivestockId { get; set; } //id of the QR code
        public string InspectionCode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type LivestockType { get; set; }
    }

    public class AddLivestockToImportBatchResult
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type LivestockType { get; set; }
        public string message { get; set; }
        public int NumberLivestockAdded { get; set; } = 0;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public request_status AddStatus { get; set; }
    }

}
