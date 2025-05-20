using BusinessObjects.Attribute;
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
    public class SpecieDTO
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Tên không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; } = string.Empty;
        [GreaterThanZero]
        public decimal GrowthRate { get; set; } = 0;
        [GreaterThanZero]
        public decimal DressingPercentage { get; set; } = 0;
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        [Required(ErrorMessage = "Specie type không được để trống")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type Type { get; set; }
    }
    public class SpecieCreate
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; } = string.Empty;
        [GreaterThanZero]
        public decimal GrowthRate { get; set; } = 0;
        [GreaterThanZero]
        public decimal DressingPercentage { get; set; } = 0;
        [Required(ErrorMessage = "Specie type không được để trống")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type Type { get; set; }
        public string? RequestedBy { get; set; }
    }
    public class SpecieUpdate
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MinLength(2, ErrorMessage = "Mô tả phải có ít nhất 2 ký tự")]
        public string Description { get; set; } = string.Empty;
        [GreaterThanZero]
        public decimal GrowthRate { get; set; } = 0;
        [GreaterThanZero]
        public decimal DressingPercentage { get; set; } = 0;
        [Required(ErrorMessage = "Specie type không được để trống")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type Type { get; set; }
        public string? RequestedBy { get; set; }
    }
    public class SpecieType
    {
        public int TypeNumber { get; set; }
        public string TypeString { get; set; }

    }
    public class SpecieName
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
