using Newtonsoft.Json;
using static BusinessObjects.Constants.LmsConstants;
using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public class InspectionCodeRange : BaseEntity
    {
        public string StartCode { get; set; }
        public string EndCode { get; set; }
        public string CurrentCode { get; set; }
        public int Quantity { get; set; }
        public int OrderNumber { get; set; }

        // SpecieTypes chỉ là một chuỗi JSON
        public string SpecieTypes { get; set; }
        public inspection_code_range_status Status { get; set; }
        public virtual ICollection<InspectionCodeCounter> InspectionCodeCounters { get; set; }
    }
}
