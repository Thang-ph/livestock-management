using AutoMapper;
using Azure.Core;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

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

            var specieTypes = Enum.GetValues(typeof(specie_type))
           .Cast<specie_type>()
           .Select(e => e.ToString())  // Chuyển giá trị enum thành chuỗi
           .ToList();
            var inspectionCouter = await _context.InspectionCodeCounters.ToListAsync();
            if (specieTypes.Count() == inspectionCouter.Count())
            {
                
            }
            else
            {
                foreach (var inspection in inspectionCouter)
                {
                    if (specieTypes.Contains(inspection.SpecieType))
                    {
                        var i = inspection.SpecieType;
                        specieTypes.Remove(inspection.SpecieType);
                    }
                }
                List<InspectionCodeCounter> list = new List<InspectionCodeCounter>();
                foreach (var code in specieTypes)
                {
                    InspectionCodeCounter inCo = new InspectionCodeCounter()
                    {
                        SpecieType = code,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        Id = SlugId.New()
                    };
                    list.Add(inCo);
                }
                _context.InspectionCodeCounters.AddRangeAsync(list);
                await _context.SaveChangesAsync();
            }


            var inspectionCodeCounter = await _context.InspectionCodeCounters
                .FirstOrDefaultAsync(o => o.SpecieType.Equals(type.ToString()));
            
            if (inspectionCodeCounter == null)
            {
                throw new Exception("Không tìm thấy mã kiểm tra cho loại vật nuôi này.");
            }
            InspectionCodeRange codeRange = new InspectionCodeRange();
            var listCodeRange = await _context.InspectionCodeRanges
                   .Where(x => x.SpecieTypes.Contains(type) && !x.Status.Equals(inspection_code_range_status.DÙNG_HẾT)).OrderBy(x => x.OrderNumber).ToListAsync();
            if (inspectionCodeCounter.CurrentRangeId == null)
            {
               
                var id = listCodeRange.FirstOrDefault();
                if (id != null)
                {
                    inspectionCodeCounter.CurrentRangeId = id.Id;
                    
                }
                else
                {
                    throw new Exception("Chưa có bảng mã nào của vật nuôi này");
                }
            }

            codeRange = await _context.InspectionCodeRanges.Where(x => x.Id.Equals(inspectionCodeCounter.CurrentRangeId)).SingleOrDefaultAsync();
            //Khai báo biến để lưu giá trị trả về trước khi thay đổi
            var currentCodeNow = "";

            // Chuyển đổi CurrentCode và MaxCode sang int
             int currentCode = int.Parse(codeRange.CurrentCode);

            //Check Inspection Is used or not
            specie_type parsedType = (specie_type)Enum.Parse(typeof(specie_type), type, ignoreCase: true);
            var livestock = await _context.Livestocks
                .Include(s => s.Species)
                .FirstOrDefaultAsync(x => x.InspectionCode == (currentCode - 1).ToString() && x.Species.Type == parsedType);
            if (livestock == null)
                currentCode -= 1;

            int maxCode = int.Parse(codeRange.EndCode);
            InspectionCodeRangeFilter filter = new InspectionCodeRangeFilter();
            var inspectionCodeRangeList = await GetListInspectionCodeRange(filter);
            // Kiểm tra nếu CurrentCode nhỏ hơn MaxCode
            int nextCurrentCode = 0;
            
            if (currentCode <= maxCode)
            {
                // Tăng CurrentCode lên 1 và giữ lại định dạng string
                //currentCode += 1;
                codeRange.CurrentCode = currentCode.ToString("D" + codeRange.CurrentCode.Length);
                nextCurrentCode = currentCode + 1;
                codeRange.CurrentCode = nextCurrentCode.ToString("D" + codeRange.CurrentCode.Length);
            }
            else
            {
                // Nếu CurrentCode bằng MaxCode, kiểm tra NextRangeId
                // Đã đạt giới hạn phải chuyển sang range tiếp theo
                try
                {
                    var newCode = listCodeRange.Where(x => !x.Id.Equals(inspectionCodeCounter.CurrentRangeId)).FirstOrDefault();
                    if (newCode != null)
                    {
                        inspectionCodeCounter.CurrentRangeId = newCode.Id;
                        currentCode = int.Parse(newCode.CurrentCode);
                        newCode.Status = inspection_code_range_status.ĐANG_SỬ_DỤNG;
                        codeRange.Status = inspection_code_range_status.DÙNG_HẾT;
                        nextCurrentCode = currentCode + 1;
                        newCode.CurrentCode = nextCurrentCode.ToString("D" + newCode.CurrentCode.Length);
                    }
                    else
                    {
                        throw new Exception("Không còn bảng mã nào có thể xử dụng");
                    }
                } catch { throw new Exception("Không còn bảng mã nào có thể xử dụng"); };
                

            }


            // xu ly neu current lon hon max code
            try
            {

                // Cập nhật CurrentCode của NextRangeId và giữ lại định dạng string
                currentCodeNow = currentCode.ToString("D" + codeRange.CurrentCode.Length);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi cập nhật mã kiểm tra: " + e.Message);
            }

            return currentCodeNow;
        }

        public async Task GenInspectionCodeCounter()
        {
            var specieTypes = Enum.GetValues(typeof(specie_type))
                       .Cast<specie_type>()
                       .Select(e => e.ToString())  // Chuyển giá trị enum thành chuỗi
                       .ToList();
            var inspectionCouter = await _context.InspectionCodeCounters.ToListAsync();
            if (specieTypes.Count() == inspectionCouter.Count())
            {
                return;
            }
            else
            {
                foreach (var inspection in inspectionCouter)
                {
                    if (specieTypes.Contains(inspection.SpecieType))
                    {
                        var i = inspection.SpecieType;
                        specieTypes.Remove(inspection.SpecieType);
                    }
                }
                List<InspectionCodeCounter> list = new List<InspectionCodeCounter>();
                foreach (var code in specieTypes)
                {
                    InspectionCodeCounter inCo = new InspectionCodeCounter()
                    {
                        SpecieType = code,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        Id = SlugId.New()
                    };
                    list.Add(inCo);
                }
                _context.InspectionCodeCounters.AddRangeAsync(list);
                await _context.SaveChangesAsync();
            }
          
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
