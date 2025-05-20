using AutoMapper;
using Azure.Core;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Services
{
    public class InspectionCodeCounterService : IInspectionCodeCounterRepository
    {
        private readonly LmsContext _context;
        private readonly IInspectionCodeRangeRepository _services;
        private readonly IMapper _mapper;

        public InspectionCodeCounterService(LmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<InspectionCodeCounter> CreateInspectionCodeCouter(CreateInspectionCodeCouterDto inspectionCodeCouterDto)
        {
            if (inspectionCodeCouterDto == null)
                throw new Exception("Lỗi khi tạo mới bảng đếm cho vật nuôi");
            var isDuplicate = await _context.InspectionCodeCounters
                .AnyAsync(o => o.SpecieType.Equals(inspectionCodeCouterDto.SpecieType.ToString()));
            if (isDuplicate)
            {
                throw new Exception("Vật nuôi đã được khởi tạo.");
                //Reset lại current vật nuôi hoặc không
            }
            else
            {
                var result = new InspectionCodeCounter
                {
                    Id = SlugId.New(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = "SYS",
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = "SYS",
                    SpecieType = inspectionCodeCouterDto.SpecieType,
                    CurrentRangeId = inspectionCodeCouterDto.CurrentRangeId,
                };
                try
                {
                    await _context.InspectionCodeCounters.AddAsync(result);
                    return result;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

            }
        }

        public async Task<InspectionCodeCounter> GetInspectionCodeBySpecie(LmsConstants.specie_type type)
        {
            var inspectionCodeCounter = await _context.InspectionCodeCounters
                .FirstOrDefaultAsync(o => o.SpecieType.Equals(type.ToString()));

            if (inspectionCodeCounter == null)
            {
                throw new Exception("Không tìm thấy mã kiểm tra cho loại vật nuôi này.");
            }
            return inspectionCodeCounter;
        }

        public async Task<string> UpdateCurrentNumberInspectionCode(String type)
        {
            /*
             * Hàm được dùng khi khách muốn lấy mã thẻ tai để sử dụng cho vật nuôi nào đó
             * Hàm sẽ tự động cập nhật mã thẻ tai tiếp theo
             */
            var inspectionCodeCounter = await _context.InspectionCodeCounters
                .Include(o => o.InspectionCodeRange)
                .FirstOrDefaultAsync(o => o.SpecieType.Equals(type.ToString()));

            if (inspectionCodeCounter == null)
            {
                throw new Exception("Không tìm thấy mã kiểm tra cho loại vật nuôi này.");
            }
            //Khai báo biến để lưu giá trị trả về trước khi thay đổi
            var currentCodeNow = "";

            // Chuyển đổi CurrentCode và MaxCode sang int
            int currentCode = int.Parse(inspectionCodeCounter.InspectionCodeRange.CurrentCode);
            int maxCode = int.Parse(inspectionCodeCounter.InspectionCodeRange.EndCode);
            InspectionCodeRangeFilter filter = new InspectionCodeRangeFilter();
            var inspectionCodeRangeList = await GetListInspectionCodeRange(filter);
            // Kiểm tra nếu CurrentCode nhỏ hơn MaxCode
            if (currentCode < maxCode)
            {
                // Tăng CurrentCode lên 1 và giữ lại định dạng string
                //currentCode += 1;
                inspectionCodeCounter.InspectionCodeRange.CurrentCode = currentCode.ToString("D" + inspectionCodeCounter.InspectionCodeRange.CurrentCode.Length);
            }
            else
            {
                // Nếu CurrentCode bằng MaxCode, kiểm tra NextRangeId
                // Đã đạt giới hạn phải chuyển sang range tiếp theo

                var nextInspectionCodeRange = inspectionCodeRangeList.Items.Where(o => o.OrderNumber == (inspectionCodeCounter.InspectionCodeRange.OrderNumber + 1)).SingleOrDefault();
                try
                {
                    while (int.Parse(nextInspectionCodeRange.CurrentCode) >= int.Parse(nextInspectionCodeRange.EndCode))
                    {
                        // Kiem tra xem trong khoang tiep theo co gia tri dung duoc hay khong
                        nextInspectionCodeRange = inspectionCodeRangeList.Items.Where(o => o.OrderNumber == (nextInspectionCodeRange.OrderNumber + 1)).SingleOrDefault();
                    }
                }
                catch
                {
                    throw new Exception("Không đủ mã thẻ tai cho vật nuôi " + inspectionCodeCounter.SpecieType);
                }


                if (nextInspectionCodeRange != null)
                {


                    if (nextInspectionCodeRange.CurrentCode != null)
                    {
                        currentCodeNow = nextInspectionCodeRange.CurrentCode;
                        inspectionCodeCounter.CurrentRangeId = nextInspectionCodeRange.Id;
                        // Chuyển đổi NextCurrentCode sang int và tăng lên 1
                        int nextCurrentCode = int.Parse(nextInspectionCodeRange.CurrentCode);
                        nextCurrentCode += 1;

                        // Cập nhật CurrentCode của NextRangeId và giữ lại định dạng string
                        nextInspectionCodeRange.CurrentCode = nextCurrentCode.ToString("D" + nextInspectionCodeRange.CurrentCode.Length);
                        await _context.SaveChangesAsync();

                        return currentCodeNow;
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy mã kiểm tra cho NextRangeId.");
                    }
                }
                else
                {
                    throw new Exception("Không đủ mã thẻ tai cho vật nuôi " + inspectionCodeCounter.SpecieType);
                }
            }


            // xu ly neu current lon hon max code
            try
            {
                currentCodeNow = inspectionCodeCounter.InspectionCodeRange.CurrentCode;
                int nextCurrentCode = int.Parse(inspectionCodeCounter.InspectionCodeRange.CurrentCode);
                nextCurrentCode += 1;

                // Cập nhật CurrentCode của NextRangeId và giữ lại định dạng string
                inspectionCodeCounter.InspectionCodeRange.CurrentCode = nextCurrentCode.ToString("D" + inspectionCodeCounter.InspectionCodeRange.CurrentCode.Length);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi cập nhật mã kiểm tra: " + e.Message);
            }

            return currentCodeNow;
        }

        public async Task<ListInspectionCodeRanges> GetListInspectionCodeRange(InspectionCodeRangeFilter filter)
        {
            var result = new ListInspectionCodeRanges()
            {
                Total = 0
            };

            // Retrieve InspectionCodeRanges filtered by the given SpecieType
            var codeRanges = await _context.InspectionCodeRanges
                .ToListAsync();

            if (codeRanges == null || !codeRanges.Any())
            {
                return result;
            }
            var codeRangesMap = _mapper.Map<List<InspectionCodeRangeDTO>>(codeRanges);

            if (filter != null)
            {
                if (filter.SpecieTypes != null)
                {
                    codeRangesMap = codeRangesMap.Where(o => o.SpecieTypeList.Intersect(filter.SpecieTypes).Any()) // Kiểm tra nếu có phần tử chung
       .ToList();
                }
                if (filter.Status != null && filter.Status.Any())
                {


                    codeRangesMap = codeRangesMap.Where(o => o.SpecieTypeList.All(specie => filter.SpecieTypes.Contains(specie)))
                                 .ToList();
                }

                // Xử lý StartCode và EndCode
                if ((filter.StartCode != null && filter.StartCode.Any()) || (filter.EndCode != null && filter.EndCode.Any()))
                {
                    try
                    {
                        // Kiểm tra và xử lý định dạng StartCode và EndCode
                        string startCode = filter.StartCode ?? "000000"; // Nếu không có StartCode thì dùng "000000"
                        string endCode = filter.EndCode ?? "999999";     // Nếu không có EndCode thì dùng "999999"

                        if (int.Parse(startCode) >= int.Parse(endCode))
                        {
                            throw new Exception("StartCode phải nhỏ hơn EndCode");
                        }

                        // Kiểm tra định dạng 6 chữ số
                        if (!IsValidCodeFormat(startCode) || !IsValidCodeFormat(endCode))
                        {
                            throw new Exception("Định dạng mã phải là 6 chữ số");
                        }

                        // Tạo ra các mã cần so sánh với dữ liệu trong database
                        codeRangesMap = codeRangesMap.Where(o =>
                            string.Compare(o.StartCode, startCode) >= 0 && string.Compare(o.EndCode, endCode) <= 0)
                            .ToList();
                    }
                    catch (FormatException ex)
                    {
                        // Xử lý lỗi nếu định dạng mã không hợp lệ
                        throw new Exception("Dữ liệu truyền vào không hợp lệ");
                    }
                    catch (Exception ex)
                    {
                        // Xử lý các lỗi khác
                        throw new Exception($"{ex.Message}");
                    }
                }
                codeRangesMap = codeRangesMap
                    .OrderBy(o => o.OrderNumber)
                    .Skip(filter.Skip)
                    .Take(filter.Take)
                    .ToList();
            }


            result.Items = codeRangesMap.OrderBy(o => o.OrderNumber);
            result.Total = codeRangesMap.Count;
            // Map to DTO list and return
            return result;
        }
        private bool IsValidCodeFormat(string code)
        {
            return code.Length == 6 && code.All(c => char.IsDigit(c));
        }
    }
}
