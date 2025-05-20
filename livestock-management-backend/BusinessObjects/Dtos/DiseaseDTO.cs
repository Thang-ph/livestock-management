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
    /// <summary>
    /// Thông tin tóm tắt của bệnh
    /// </summary>
    public class DiseaseSummary
    {
        /// <summary>
        /// ID của bệnh
        /// </summary>
        /// <example>dis-001</example>
        public string Id { get; set; }

        /// <summary>
        /// Tên bệnh
        /// </summary>
        /// <example>Lở mồm long móng</example>
        public string Name { get; set; }
    }

    /// <summary>
    /// Danh sách bệnh với thông tin tổng số bệnh
    /// </summary>
    public class ListDiseases : ResponseListModel<DiseaseSummary>
    {
    }

    /// <summary>
    /// Thông tin chi tiết của bệnh
    /// </summary>
    public class DiseaseDTO
    {
        /// <summary>
        /// ID của bệnh
        /// </summary>
        /// <example>dis-001</example>
        public string Id { get; set; }

        /// <summary>
        /// Tên bệnh
        /// </summary>
        /// <example>Lở mồm long móng</example>
        public string Name { get; set; }

        /// <summary>
        /// Triệu chứng của bệnh
        /// </summary>
        /// <example>Sốt cao, lở loét miệng và móng</example>
        public string Symptom { get; set; }

        /// <summary>
        /// Mô tả chi tiết về bệnh
        /// </summary>
        /// <example>Bệnh truyền nhiễm nguy hiểm ở trâu bò</example>
        public string? Description { get; set; }

        /// <summary>
        /// Loại bệnh
        /// </summary>
        /// <example>TRUYỀN_NHIỄM_NGUY_HIỂM</example>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public disease_type Type { get; set; }
    }

    /// <summary>
    /// Thông tin cập nhật bệnh
    /// </summary>
    public class DiseaseUpdateDTO
    {
        /// <summary>
        /// Tên bệnh (yêu cầu từ 2-280 ký tự)
        /// </summary>
        /// <example>Lở mồm long móng loại A</example>
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Triệu chứng của bệnh (yêu cầu từ 2-280 ký tự)
        /// </summary>
        /// <example>Sốt cao, lở loét miệng và móng</example>
        [MinLength(2, ErrorMessage = "Triệu chứng phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Triệu chứng không thể dài quá 280 ký tự")]
        public string Symptom { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết về bệnh (tối đa 280 ký tự)
        /// </summary>
        /// <example>Bệnh truyền nhiễm nguy hiểm ở trâu bò</example>
        [MaxLength(280, ErrorMessage = "Mô tả không thể dài quá 280 ký tự")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Loại bệnh (bắt buộc)
        /// </summary>
        /// <example>TRUYỀN_NHIỄM_NGUY_HIỂM</example>
        [Required(ErrorMessage = "Loại bệnh không được để trống")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public disease_type Type { get; set; }

        /// <summary>
        /// Người yêu cầu cập nhật (tùy chọn)
        /// </summary>
        /// <example>admin</example>
        public string requestedBy { get; set; }
    }
}
