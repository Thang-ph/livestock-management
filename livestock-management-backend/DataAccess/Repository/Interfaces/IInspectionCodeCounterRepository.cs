using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Interfaces
{
    public interface IInspectionCodeCounterRepository
    {
        //Khi có con vật nuôi mới được khởi tạo thì bảng này cũng sẽ được khởi tạo theo
        Task<InspectionCodeCounter> CreateInspectionCodeCouter(CreateInspectionCodeCouterDto inspectionCodeCouterDto);
        // Lấy mã hiện tại theo mã vật nuôi
        Task<InspectionCodeCounter> GetInspectionCodeBySpecie(specie_type type);
        //Update khi thêm mới hoặc dùng mã
        Task<String> UpdateCurrentNumberInspectionCode(String type);
    }
}
