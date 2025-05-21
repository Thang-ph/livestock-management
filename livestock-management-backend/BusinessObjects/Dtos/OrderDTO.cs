using BusinessObjects.ConfigModels;
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
    public class ListOrders : ResponseListModel<OrderSummary>
    {

    }

    public class OrderSummary
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public int Total { get; set; }
        public int Received { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public order_status Status { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public order_type Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }


    public class OrderDetailsDTO
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public int Total { get; set; }
        public int Imported { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public order_status Status { get; set; }
        public string Code { get; set; }
        public IEnumerable<OrderRequirementdetails> Details { get; set; }
    }


    public class OrderRequirementdetails
    {
        public string OrderRequirementId { get; set; }
        public string SpecieId { get; set; }
        public string SpecieName { get; set; }
        public decimal? WeightFrom { get; set; }
        public decimal? WeightTo { get; set; }
        public int Total { get; set; }
        public string? Description { get; set; }
        public List<VaccinationRequireOrderDetail>? VaccintionRequirement { get; set; }
    }

    public class VaccinationRequireOrderDetail
    {
        public string VaccinationRequirementId { get; set; }
        public string DiseaseId { get; set; }
        public string DiseaseName { get; set; }
        public int InsuranceDuration { get; set; }
    }

    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "Tên Khách hàng không được để trống.")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có từ 10 đến 11 chữ số.")]
        public string Phone { get; set; }
        public string? Addrress { get; set; }
        public string? Email { get; set; }
        public IEnumerable<CreateOrderDetailsRequirement> Details { get; set; }
        public string? RequestedBy { get; set; }

    }    
    

    public class CreateOrderDetailsRequirement 
    {
        [Required(ErrorMessage = "Loài vật không được để trống.")]
        public string SpecieId { get; set; }
        public decimal? WeightFrom { get; set; }
        public decimal? WeightTo { get; set; }
        [Required(ErrorMessage = "Số lượng loài vật không được để trống.")]
        public int Total { get; set; }
        public string? Description { get; set; }
        public List<VaccinationRequireOrderDetailCreate>? VaccintionRequirement { get; set; }
    }

    public class VaccinationRequireOrderDetailCreate
    {
        [Required(ErrorMessage = "Bệnh không được để trống.")]
        public string DiseaseId { get; set; }
        public int? InsuranceDuration { get; set; } = 21;
    }

    //public class AddOrderRequirementDTO
    //{
    //    public string SpecieId { get; set; }
    //    public decimal WwightFrom {  get; set; }
    //    public decimal WwightTo { get; set; }
    //    public int Total { get; set; }
    //    public string Description { get; set; }
    //    public string VaccinationRequirementId { get; set; }
    //}

    public class UpdateOrderDTO
    {
        [Required(ErrorMessage = "Tên Khách hàng không được để trống.")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có từ 10 đến 11 chữ số.")]
        public string Phone { get; set; }
        public string? Addrress { get; set; }
        public string? Email { get; set; }
        public IEnumerable<UpdateOrderDetailsRequirement> Details { get; set; }
        public string? RequestedBy { get; set; }

    }

    public class UpdateOrderDetailsRequirement
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Loài vật không được để trống.")]
        public string SpecieId { get; set; }
        public decimal? WeightFrom { get; set; }
        public decimal? WeightTo { get; set; }
        [Required(ErrorMessage = "Số lượng loài vật không được để trống.")]
        public int Total { get; set; }
        public string? Description { get; set; }
        public List<VaccinationRequireOrderDetailUpdate>? VaccintionRequirement { get; set; }
    }

    public class VaccinationRequireOrderDetailUpdate
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Bệnh không được để trống.")]
        public string DiseaseId { get; set; }
        public int? InsuranceDuration { get; set; } = 21;
    }

    public class AddLivestockToOrderDTO
    {
        public string LivestockId {  get; set; }
        public string? RequestedBy { get; set; }
    }


    public class ListOrderFilter : CommonListFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public order_status? Status { get; set; }
    }

    public class ListLivestockFilter : CommonListFilterModel
    {

    }

}
