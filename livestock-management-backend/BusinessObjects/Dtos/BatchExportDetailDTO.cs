using BusinessObjects.Attribute;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class BatchExportDetailAddDTO
    {
        [Required(ErrorMessage = "ID lô xuất không được để trống")]
        public string BatchExportId { get; set; }
        [Required(ErrorMessage = "Mã gia súc không được để trống.")]
        public string LivestockId { get; set; }


        [FutureDate(ErrorMessage = "Ngày hết hạn bảo hiểm phải từ hôm nay trở đi.")]
        public DateTime? ExpiredInsuranceDate { get; set; }
        [Required(ErrorMessage = "Người tạo không được để trống")]
        public string CreatedBy { get; set; }

    }
    public class BatchExportDetailChangeDTO
    {
        [Required(ErrorMessage = "ID lô xuất không được để trống")]
        public string BatchExportId { get; set; }
        [Required(ErrorMessage = "Mã gia súc không được để trống.")]
        public string LivestockId { get; set; }
    
        [Required(ErrorMessage = "Người cập nhật không được để trống")]
        public string UpdatedBy { get; set; }

    }
    public class BatchExportDetailUpdateDTO
    {
        [Required(ErrorMessage = "ID lô xuất không được để trống")]
        public string BatchExportId { get; set; }
        [Required(ErrorMessage = "Mã gia súc không được để trống.")]
        public string LivestockId { get; set; }
        [GreaterThanZero(ErrorMessage = "Giá xuất phải lớn hơn 0.")]
        public decimal? PriceExport { get; set; }
        [GreaterThanZero(ErrorMessage = "Khối lượng xuất phải lớn hơn 0.")]
        public decimal? WeightExport { get; set; }

        [FutureDate(ErrorMessage = "Ngày hết hạn bảo hiểm phải từ hôm nay trở đi.")]
        public DateTime? ExpiredInsuranceDate { get; set; }
        [Required(ErrorMessage = "Người cập nhật không được để trống")]
        public string UpdatedBy { get; set; }
        [FutureDate(ErrorMessage = "Ngày bàn giao phải từ hôm nay trở đi.")]
        public DateTime? HandoverDate { get; set; }
        [FutureDate(ErrorMessage = "Ngày chọn phải từ hôm nay trở đi.")]
        public DateTime? ExportDate { get; set; }
    }
}
