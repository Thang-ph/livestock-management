using BusinessObjects.ConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class SearchHistoryBatchImport
    {
        public DateTime CreatedAt { get; set; }
        public string InspectionCode { get; set; }
        public string SpecieName { get; set; }
        public string MedicineId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public livestock_gender? Gender { get; set; } = livestock_gender.ĐỰC;
        public string? Color { get; set; } = "Nâu";
        public decimal? Weight { get; set; } = 100;
        public DateTime? Dob { get; set; } = null;
    }

    public class ListSearchHistory : ResponseListModel<SearchHistoryBatchImport>
    {

    }
}
