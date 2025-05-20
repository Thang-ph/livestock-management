using BusinessObjects.ConfigModels;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class BarnInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class ListBarns : ResponseListModel<BarnInfo>
    {

    }

    public class CreateBarnDTO()
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [MinLength(2, ErrorMessage = "Địa chỉ phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Địa chỉ không thể dài quá 280 ký tự")]
        public string Address { get; set; }
        public string? RequestedBy { get; set; }
    }

    public class UpdateBarnDTO()
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Tên không thể dài quá 280 ký tự")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [MinLength(2, ErrorMessage = "Địa chỉ phải có ít nhất 2 ký tự")]
        [MaxLength(280, ErrorMessage = "Địa chỉ không thể dài quá 280 ký tự")]
        public string Address { get; set; }
        public string? RequestedBy { get; set; }
    }
}
