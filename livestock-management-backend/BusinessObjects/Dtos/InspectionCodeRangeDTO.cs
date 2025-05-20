using System.Collections.Generic;
using System.Text.Json.Serialization;
using BusinessObjects.ConfigModels;
using BusinessObjects.Models;
using Newtonsoft.Json;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class InspectionCodeRangeFilter : CommonListFilterModel
    {
        public IEnumerable<String>? SpecieTypes { get; set; }
        public IEnumerable<inspection_code_range_status>? Status {  get; set; }
        public string? StartCode { get; set; }
        public string? EndCode { get; set; }
    }

    public class InspectionCodeRangeDTO : BaseEntity
    {
        
        public string StartCode { get; set; }
        public string EndCode { get; set; }
        public string CurrentCode { get; set; } // Thêm CurrentCode
        public int Quantity { get; set; }
        public int OrderNumber { get; set; }

        // Property này sẽ trả về chuỗi JSON từ SpecieTypes
        [System.Text.Json.Serialization.JsonIgnore]
        public string SpecieTypes { get; set; }

       
        public List<String> SpecieTypeList {  get; set; }

        public inspection_code_range_status Status { get; set; }
        public virtual ICollection<InspectionCodeCounter> InspectionCodeCounters { get; set; }
    }

    public class ListInspectionCodeRanges : ResponseListModel<InspectionCodeRangeDTO> { }
    public class CreateInspectionCodeRangeDTO
    {
        public string StartCode { get; set; }
        public string EndCode { get; set; }

        // Property này sẽ trả về chuỗi JSON từ SpecieTypes

        // Không serialize, sử dụng thuoc tính SpecieTypeList
        [Newtonsoft.Json.JsonConverter(typeof(JsonStringEnumConverter))]
        public List<string> SpecieTypeList { get; set; }
    }

    public class SpecieTypesDto : ResponseListModel<string> { }
   
    public class LivestocksInspectionCodeRanges
    {
        public string SpecieType { get; set; }
        public int Quantity { get; set; }
    }

    public class LstInspectionCodeRangesDto : ResponseListModel<LivestocksInspectionCodeRanges> { }

    public class InfoSpecieInspectionCodeRange
    {
        public string SpecieType { get; set; }
        public int Quantity { get; set; }
        public string CurrentID { get; set; }
        public string CurrentNumber { get; set; }
        public string FromNumber { get; set; }
        public string ToNumber { get; set; }
    }

    public class InfoSpecieInspectionCodeRangeDto : ResponseListModel<InfoSpecieInspectionCodeRange> { }
}
