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
    public class BatchExportDTO
    {
        public class BatchExportHandover
        {
            public string livestockId { get; set; }
            public string UpdatedBy { get; set; }

        }
        public class BatchExportHandoverByInspection
        {
            public string inspectionCode { get; set; }
            public specie_type specieType { get; set; }
            public string UpdatedBy { get; set; }
        }
        public string Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; } 
        public string? CustomerAddress { get; set; }
        public string? CustomerNote { get; set; }
        public int Total { get; set; }
        public int Remaining { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]

        public batch_export_status Status { get; set; }

        public DateTime? CreatedAt { get; set; }
    }

    public class ListCustomers : ResponseListModel<BatchExportDTO> { }

    public class CustomersFliter : CommonListFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public batch_export_status? Status { get; set; }
    }
  public class UpdateCustomerBatchExportDTO
    {
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được quá 100 ký tự")]
        public string CustomerName { get; set; } = string.Empty;
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại không hợp lệ ( chỉ gồm 10 số)")]
        public string? CustomerPhone { get; set; }
        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string? CustomerAddress { get; set; }
        [StringLength(200, ErrorMessage = "Ghi chú không được quá 200 ký tự")]
        public string? CustomerNote { get; set; }
        [Required(ErrorMessage = "Người cập nhật không được để trống")]
        public string UpdatedBy { get; set; }
    }
    public class AddCustomerBatchExportDTO
    {
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được quá 100 ký tự")]
        public string CustomerName { get; set; } = string.Empty;
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại không hợp lệ ( chỉ gồm 10 số)")]
        public string? CustomerPhone { get; set; }
        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string? CustomerAddress { get; set; }
        [StringLength(200, ErrorMessage = "Ghi chú không được quá 200 ký tự")]
        public string? CustomerNote { get; set; }
        [Required(ErrorMessage = "Người cập nhật không được để trống")]
        public string CreatedBy { get; set; }
        [GreaterThanZeroAttribute(ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Total { get; set; }
        [Required(ErrorMessage = "Id gói thầu không được để trống")]
        public string ProcurementPackageId { get; set; }
    }
  
}
