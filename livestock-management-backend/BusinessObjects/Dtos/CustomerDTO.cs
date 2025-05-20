using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos
{
    public class CustomerInfo
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class AddCustomerDTO
    {
        [Required(ErrorMessage = "Tên không được để trống.")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có từ 10 đến 11 chữ số.")]
        public string Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? RequestedBy { get; set; }
    }
}
