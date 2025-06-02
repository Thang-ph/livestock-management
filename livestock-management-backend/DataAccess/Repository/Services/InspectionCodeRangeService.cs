using AutoMapper;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services
{
    public class InspectionCodeRangeService : IInspectionCodeRangeRepository
    {
        private readonly LmsContext _context;
        private readonly IMapper _mapper;
        private readonly IInspectionCodeCounterRepository _counterService;

        public InspectionCodeRangeService(LmsContext context, IMapper mapper, IInspectionCodeCounterRepository counterService)
        {
            _context = context;
            _mapper = mapper;
            _counterService = counterService;
        }

        public async Task<bool> CheckExistsInspectionCodeRange(CreateInspectionCodeRangeDTO createInspectionCodeRangeDto)
        {
            // Ensure SpecieTypeList is a collection of strings, 
            // and adjust the intersection accordingly
            var codeRanges = await _context.InspectionCodeRanges
                .Where(o => o.SpecieTypes.Any(specieType =>
                    createInspectionCodeRangeDto.SpecieTypeList.Contains(specieType.ToString())))
                .ToListAsync();

            foreach (var codeRange in codeRanges)
            {
                int startCode = int.Parse(createInspectionCodeRangeDto.StartCode);
                int endCode = int.Parse(createInspectionCodeRangeDto.EndCode);
                int rangeStartCode = int.Parse(codeRange.StartCode);
                int rangeEndCode = int.Parse(codeRange.EndCode);

                // Check if the new range overlaps with the existing range
                if ((startCode >= rangeStartCode && startCode <= rangeEndCode) ||
                    (endCode >= rangeStartCode && endCode <= rangeEndCode) ||
                    (startCode <= rangeStartCode && endCode >= rangeEndCode))
                {
                    return false; // Overlap found
                }
            }
            return true; // No overlaps
        }

        public async Task<InspectionCodeRange> CreateInspectionCodeRange(CreateInspectionCodeRangeDTO createInspectionCodeRangeDto)
        {
            // Map DTO to model
            var codeRangeModel = _mapper.Map<InspectionCodeRange>(createInspectionCodeRangeDto);
            var codeRange = _mapper.Map<InspectionCodeRangeDTO>(codeRangeModel);
            
            //Check dữ liệu truyền vào
            var invalidSpecieTypes = codeRange.SpecieTypeList.Where(type => !Enum.IsDefined(typeof(specie_type), type)).ToList();
            if (invalidSpecieTypes.Any())
            {
                throw new Exception("Loại vật nuôi không hợp lệ");
            }

            if (!IsValidCodeFormat(createInspectionCodeRangeDto.StartCode) || !IsValidCodeFormat(createInspectionCodeRangeDto.EndCode))
            {
                throw new FormatException("Định dạng mã phải là 6 chữ số");
            }

            if (int.Parse(createInspectionCodeRangeDto.StartCode) >= int.Parse(createInspectionCodeRangeDto.EndCode))
            {
                throw new Exception("Số bắt đầu phải nhỏ hơn số cuối");
            }

            List<InspectionCodeRange> list = new List<InspectionCodeRange>();

            foreach (var type in codeRange.SpecieTypeList)
            {
                var dataInspectionCode = _context.InspectionCodeRanges.Where(p => p.SpecieTypes.Contains(type)).ToList();
                list.AddRange(dataInspectionCode);
            }

            // check xem mã đã tồn tại hay chưa
            foreach (var data in list)
            {
                int startCode = int.Parse(createInspectionCodeRangeDto.StartCode);
                int endCode = int.Parse(createInspectionCodeRangeDto.EndCode);
                int rangeStartCode = int.Parse(data.StartCode);
                int rangeEndCode = int.Parse(data.EndCode);

                // Check if the new range overlaps with the existing range
                if ((startCode >= rangeStartCode && startCode <= rangeEndCode) ||
                    (endCode >= rangeStartCode && endCode <= rangeEndCode) ||
                    (startCode <= rangeStartCode && endCode >= rangeEndCode))
                {
                    throw new Exception("Mã này đã tồn tại!");
                }
            }
           
            
            var filter = new InspectionCodeRangeFilter
            {
                SpecieTypes = codeRange.SpecieTypeList,
                Status = new List<inspection_code_range_status> 
                {
                    inspection_code_range_status.MỚI,
                    inspection_code_range_status.ĐANG_SỬ_DỤNG
                }
            };
            var index = await _context.InspectionCodeRanges.ToListAsync();
            codeRangeModel.OrderNumber = index.Count + 1;

            try
            {
                // Calculate the quantity (including both the start and end codes)
                codeRangeModel.Quantity = int.Parse(codeRangeModel.EndCode) - int.Parse(codeRangeModel.StartCode) + 1;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi chuyển đổi dữ liệu, Yêu cầu nhập đúng định dạng", ex);
            }

            //Kết thúc check dữ liệu
            
            // Set other properties
            codeRangeModel.Id = SlugId.New();
            codeRangeModel.CurrentCode = codeRangeModel.StartCode;
            codeRangeModel.StartCode = codeRangeModel.StartCode;
            codeRangeModel.EndCode = codeRangeModel.EndCode;
            codeRangeModel.Status = inspection_code_range_status.MỚI;
            codeRangeModel.CreatedAt = DateTime.Now;
            codeRangeModel.CreatedBy = codeRangeModel.CreatedBy?.Trim() ?? "System"; // Ensure CreatedBy is not null
            codeRangeModel.UpdatedAt = codeRangeModel.CreatedAt;
            codeRangeModel.UpdatedBy = codeRangeModel.CreatedBy;

            // Set SpecieTypes (if provided in DTO)
            codeRangeModel.SpecieTypes = codeRangeModel.SpecieTypes;
           
            
           
            // Add to database
            await _context.InspectionCodeRanges.AddAsync(codeRangeModel);
            await _context.SaveChangesAsync();

            // Return the created InspectionCodeRange entity
            return codeRangeModel;
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
                    .ToList();    
            }

            
            result.Items = codeRangesMap.OrderBy(o => o.OrderNumber);
            result.Total = codeRangesMap.Count;
            // Map to DTO list and return
            return result;
        }
        public async Task<InspectionCodeRangeDTO?> GetInspcetionCodeRangeById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("Id truyền vào không hợp lệ");
            }

            var data = await _context.InspectionCodeRanges
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();
            if(data == null)
            {
                throw new Exception("Không tìm thấy");
            }
            var result = _mapper.Map<InspectionCodeRangeDTO>(data);
            return result;

        }

    

        private bool IsValidCodeFormat(string code)
        {
            return code.Length == 6 && code.All(c => char.IsDigit(c));
        }

        public async Task<InspectionCodeRangeDTO?> UpdateInspectionCodeRange(String id, CreateInspectionCodeRangeDTO request)
        {
            // Tìm dữ liệu hiện tại trong database theo Id
            var data = await _context.InspectionCodeRanges
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();



            // Nếu không tìm thấy dữ liệu, ném ra lỗi
            if (data == null)
            {
                throw new Exception("Không tìm thấy khoảng bạn muốn cập nhật");
            }

            // Kiểm tra trạng thái hiện tại của thẻ
            if (data.Status.ToString().Equals(inspection_code_range_status.MỚI.ToString()))
            {
                var codeRangeModel = _mapper.Map<InspectionCodeRange>(request);
                var codeRange = _mapper.Map<InspectionCodeRangeDTO>(codeRangeModel);

                //Check dữ liệu truyền vào
                var invalidSpecieTypes = codeRange.SpecieTypeList.Where(type => !Enum.IsDefined(typeof(specie_type), type)).ToList();
                if (invalidSpecieTypes.Any())
                {
                    throw new Exception("Loại vật nuôi không hợp lệ");
                }

                // Kiểm tra StartCode và EndCode có hợp lệ không
                if (!IsValidCodeFormat(request.StartCode) || !IsValidCodeFormat(request.EndCode))
                {
                    throw new Exception("Định dạng mã phải là 6 chữ số");
                }

                // Kiểm tra StartCode phải nhỏ hơn EndCode
                if (int.Parse(request.StartCode) >= int.Parse(request.EndCode))
                {
                    throw new Exception("StartCode phải nhỏ hơn EndCode");
                }

                // Cập nhật lại StartCode và EndCode
                data.StartCode = request.StartCode;
                data.EndCode = request.EndCode;

                // Tính lại Quantity (bao gồm cả StartCode và EndCode)
                try
                {
                    data.Quantity = int.Parse(data.EndCode) - int.Parse(data.StartCode) + 1;
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi khi tính toán Quantity. Vui lòng nhập đúng định dạng mã", ex);
                }
                data.UpdatedAt = DateTime.Now;
                // Cập nhật lại SpecieTypes từ request
                data.SpecieTypes = JsonConvert.SerializeObject(request.SpecieTypeList);

                // Lưu thay đổi vào database
                _context.InspectionCodeRanges.Update(data);
                await _context.SaveChangesAsync();

                // Map lại đối tượng để trả về dưới dạng DTO
                var result = _mapper.Map<InspectionCodeRangeDTO>(data);
                return result;
            }
            else
            {
                // Nếu trạng thái không phải "MỚI", ném ra lỗi
                throw new Exception("Thẻ đã sử dụng hết hoặc đang được sử dụng");
            }
        }

        public async Task<LstInspectionCodeRangesDto?> GetListInspectionByType()
        {
            List<LivestocksInspectionCodeRanges> dataResponse = new List<LivestocksInspectionCodeRanges>();
            var specieTypes = Enum.GetValues(typeof(specie_type))
                                       .Cast<specie_type>()
                                       .Select(e => e.ToString())  // Chuyển giá trị enum thành chuỗi
                                       .ToList();
            var listInspectionCodeRange =await _context.InspectionCodeRanges.ToListAsync();
            var codeRangesMap = _mapper.Map<List<InspectionCodeRangeDTO>>(listInspectionCodeRange);
            foreach (var item in specieTypes)
            {
                var LivestockRange = codeRangesMap.Where(o => o.SpecieTypeList.Contains(item)).ToList();
                // var LivestockRange = codeRangesMap.Where(o => o.SpecieTypeList.Intersect(item).Any()).ToList(); // Kiểm tra nếu có phần tử chung

                if (LivestockRange.Count() > 0)
                {
                    LivestockRange = LivestockRange.Where(p => !p.Status.Equals(inspection_code_range_status.DÙNG_HẾT.ToString())).ToList();
                    var totalQuantity = LivestockRange.Sum(s => s.Quantity);
                    var dv = new LivestocksInspectionCodeRanges
                    {
                        SpecieType = item,
                        Quantity = totalQuantity
                    };
                    dataResponse.Add(dv);
                }
            }

            LstInspectionCodeRangesDto response = new LstInspectionCodeRangesDto
            {
                Total = 0
            };
            response.Items = dataResponse;
            response.Total = dataResponse.Count;

            return response;
        }


        public async Task<LstInspectionCodeRangesDto?> GetListWarrningInspection()
        {
            List<LivestocksInspectionCodeRanges> dataResponse = new List<LivestocksInspectionCodeRanges>();
            var specieTypes = Enum.GetValues(typeof(specie_type))
                                       .Cast<specie_type>()
                                       .Select(e => e.ToString())  // Chuyển giá trị enum thành chuỗi
                                       .ToList();
            var listInspectionCodeRange = await _context.InspectionCodeRanges.ToListAsync();
            var codeRangesMap = _mapper.Map<List<InspectionCodeRangeDTO>>(listInspectionCodeRange);
            foreach (var item in specieTypes)
            {
                var LivestockRange = codeRangesMap.Where(o => o.SpecieTypeList.Contains(item)).ToList();
                // var LivestockRange = codeRangesMap.Where(o => o.SpecieTypeList.Intersect(item).Any()).ToList(); // Kiểm tra nếu có phần tử chung

                if (LivestockRange.Count() > 0)
                {
                    LivestockRange = LivestockRange.Where(p => !p.Status.Equals(inspection_code_range_status.DÙNG_HẾT.ToString())).ToList();
                    var totalQuantity = LivestockRange.Sum(s => s.Quantity);
                    if(totalQuantity < 50)
                    {
                        var dv = new LivestocksInspectionCodeRanges
                        {
                            SpecieType = item,
                            Quantity = totalQuantity
                        };
                        dataResponse.Add(dv);
                    }
                    
                }
            }

            LstInspectionCodeRangesDto response = new LstInspectionCodeRangesDto
            {
                Total = 0
            };
            response.Items = dataResponse;
            response.Total = dataResponse.Count;

            return response;
        }

        public async Task<InfoSpecieInspectionCodeRangeDto> GetInfoSpecieInspectionCodeRange(string specieType)
        {
            var result = new InfoSpecieInspectionCodeRangeDto()
            {
                Total = 0
            };

           var codeInfo = _context.InspectionCodeCounters.Where(x => x.SpecieType.Equals(specieType)).Include(x => x.InspectionCodeRange).FirstOrDefault();

            if (codeInfo == null)
            {
                return result;
            }

            var data = new InfoSpecieInspectionCodeRange()
            {
                CurrentID = codeInfo.InspectionCodeRange.Id,
                CurrentNumber = codeInfo.InspectionCodeRange.CurrentCode,
                FromNumber = codeInfo.InspectionCodeRange.StartCode,
                ToNumber = codeInfo.InspectionCodeRange.EndCode,
                Quantity = codeInfo.InspectionCodeRange.Quantity,
                SpecieType = specieType
            };

            var responseData = new List<InfoSpecieInspectionCodeRange>()
            {
                data
            };

            result.Items = responseData;
            result.Total = responseData.Count;
            return result;

        }

        public async Task<ListInspectionCodeRanges> GetListInspectionCodeRangeBySpecie(string specie)
        {
            var result = new ListInspectionCodeRanges()
            {
                Total = 0
            };

            // Retrieve InspectionCodeRanges filtered by the given SpecieType
            var codeRanges = await _context.InspectionCodeRanges
                .Where(x => x.SpecieTypes.Contains(specie) && x.Status.Equals(inspection_code_range_status.MỚI))
                .ToListAsync();

            if (codeRanges == null || !codeRanges.Any())
            {
                return result;
            }
            var codeRangesMap = _mapper.Map<List<InspectionCodeRangeDTO>>(codeRanges);
            codeRangesMap = codeRangesMap.OrderBy(x => x.OrderNumber).ToList();
            


            result.Items = codeRangesMap.OrderBy(o => o.OrderNumber);
            result.Total = codeRangesMap.Count;
            // Map to DTO list and return
            return result;
        }
    }
}
