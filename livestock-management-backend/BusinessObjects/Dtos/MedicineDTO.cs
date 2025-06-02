using BusinessObjects.ConfigModels;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class MedicineDTO
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Tên không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Loại không được để trống")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public medicine_type Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100, ErrorMessage = "Tên người tạo không được vượt quá 100 ký tự")]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [MaxLength(100, ErrorMessage = "Tên người cập nhật không được vượt quá 100 ký tự")]
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class MedicineSummary
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public medicine_type Type { get; set; }
        public DateTime CreatedAt { get; set; }

    }

    public class ListMedicine : ResponseListModel<MedicineSummary> { }
    public class CreateMedicineDTO
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại không được để trống")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public medicine_type Type { get; set; }

        [MaxLength(100, ErrorMessage = "Tên người tạo không được vượt quá 100 ký tự")]
        public string CreatedBy { get; set; } = string.Empty;
        public string DisiseaId { get; set; }
    }

    public class UpdateMedicineDTO
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại không được để trống")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public medicine_type Type { get; set; }

        [MaxLength(100, ErrorMessage = "Tên người cập nhật không được vượt quá 100 ký tự")]
        public string UpdatedBy { get; set; } = string.Empty;
        public string DiseaseId { get; set; }
    }
    public class MedicinesFliter : CommonListFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public medicine_type? Type { get; set; } = null;
    }
}
