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
    public class LivestockVaccinationAdd
    {
        [Required(ErrorMessage = "Id con vật không được để trống")]
        public string LivestockId { get; set; }
        [Required(ErrorMessage = "Id lô tiêm không được để trống")]
        public string BatchVaccinationId { get; set; }

      
        [Required(ErrorMessage = "Người tạo không được để trống")]
        public string  CreatedBy { get; set; }
    }
    public class LivestockVaccinationAddByInspectionCode
    {
        [Required(ErrorMessage = "Mã kiểm dịch không được để trống")]
        public string InspectionCoded { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public specie_type Specie_Type { get; set; }
        [Required(ErrorMessage = "Id lô tiêm không được để trống")]
        public string BatchVaccinationId { get; set; }
        

        [Required(ErrorMessage = "Người tạo không được để trống")]
        public string CreatedBy { get; set; }
    }
}
