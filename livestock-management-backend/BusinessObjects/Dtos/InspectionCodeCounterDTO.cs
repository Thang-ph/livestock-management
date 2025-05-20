using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class InspectionCodeCounterDTO
    {
        public string CurrentCode { get; set; }
        public string MaxCode { get; set; }
        public int QuantityCode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public string Type { get; set; }
        public string? CurrentRangeId { get; set; }
        public string? NextRangeId { get; set; }
    }

    public class CreateInspectionCodeCouterDto
    {
        public string SpecieType { get; set; }
        public string? CurrentRangeId { get; set; }
    }

}
