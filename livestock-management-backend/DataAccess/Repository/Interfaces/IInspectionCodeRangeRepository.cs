using BusinessObjects.Dtos;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Interfaces
{
    public interface IInspectionCodeRangeRepository
    {
        //Khi tạo mới một lô thẻ tai
        Task<InspectionCodeRange> CreateInspectionCodeRange(CreateInspectionCodeRangeDTO createInspcetionCodeRangeDto);
        //Lấy code theo Specie Type
        Task<ListInspectionCodeRanges> GetListInspectionCodeRange(InspectionCodeRangeFilter filter);
        //Lấy khoảng hiện tại dựa vào bảng curent(phục vụ bảng current)
        Task<InspectionCodeRangeDTO?> GetInspcetionCodeRangeById(string id);
        Task<InspectionCodeRangeDTO?> UpdateInspectionCodeRange(String id, CreateInspectionCodeRangeDTO request);
        Task<LstInspectionCodeRangesDto?> GetListInspectionByType();
        Task<LstInspectionCodeRangesDto?> GetListWarrningInspection();
        Task<InfoSpecieInspectionCodeRangeDto> GetInfoSpecieInspectionCodeRange(string specieType);
        Task<ListInspectionCodeRanges> GetListInspectionCodeRangeBySpecie(string specie);
    }
}
