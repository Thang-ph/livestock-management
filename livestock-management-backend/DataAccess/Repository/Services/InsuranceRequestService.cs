using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services
{
    public class InsuranceRequestService : IInsuranceRequestRepository
    {
        private readonly LmsContext _context;
        private readonly IMapper _mapper;
       

        public InsuranceRequestService(LmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            
        }

        public async Task<InsurenceRequestDTO> ApproveInsuranceRequest(RemoveInsuranceLivestockDto data)
        {
            try
            {
                var insurance = await _context.InsuranceRequests.Where(x => x.Id.Equals(data.Id)).SingleOrDefaultAsync();
                if (insurance == null) throw new Exception("Mã bảo hành không hợp lệ");
                insurance.Status = insurance_request_status.ĐANG_CHUẨN_BỊ;
                insurance.ApprovedAt = DateTime.Now;
                insurance.UpdatedBy = data.UpdatedBy;
                await _context.SaveChangesAsync();
                var response = _mapper.Map<InsurenceRequestDTO>(insurance);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi dữ liệu");
            }
        }

        public async Task<InsurenceRequestDTO> ChangeStatusInsurance(ChangeStatusInsuranceDto insuranceDto)
        {
            var info = await _context.InsuranceRequests.Where( p => p.Id.Equals(insuranceDto.Id) ).SingleOrDefaultAsync();
            if (info != null)
            {
                try
                {
                    info.Status = (insurance_request_status)Enum.Parse(typeof(insurance_request_status), insuranceDto.Status);
                    info.UpdatedAt = DateTime.Now;
                    info.UpdatedBy = insuranceDto.UpdatedBy;
                    await _context.SaveChangesAsync();
                    var response = _mapper.Map<InsurenceRequestDTO>(info);
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception("Dữ liệu truyền vào không hợp lệ");
                }
                
            }
            else { throw new Exception("Mã bảo hành không tồn tại."); }
            
        }

        public async Task<InsurenceRequestDTO> CreateInsurenceRequest(CreateInsurenceDTO createDto)
        {
            if (createDto == null) throw new Exception("Không thể tạo với thông tin rỗng");
            if(createDto.Type.Equals("0") || createDto.Type.Equals("ĐƠN_MUA_LẺ"))
            {
                throw new Exception("Đang phát triển");
            }
            else
            {
                
                

                //Kiểm tra xem con vật có tồn tại trong hệ thống hay không
                //Lý do: Vì có nhiều loại con vật có thể cùng mã kiểm dịch nên mưới phải check thế này để đảm bảo
                var livestock = await _context.Livestocks.Where(p => p.InspectionCode.Equals(createDto.LivestockId) && p.SpeciesId.Equals(createDto.SpecieType)).SingleOrDefaultAsync();
                if (livestock == null) throw new Exception("Mã kiểm dịch không chính xác");


                var procurmentLivestock = await _context.LivestockProcurements.Where(x => x.LivestockId.Equals(livestock.Id)).Include(x => x.ProcurementPackage).SingleOrDefaultAsync();
                if (procurmentLivestock == null) throw new Exception("Vật nuôi này không có trong hợp đồng !");
                var procurment = await _context.ProcurementPackages.Where(x => x.Id.Equals(procurmentLivestock.ProcurementPackageId)).Include(x => x.ProcurementDetails).SingleOrDefaultAsync();

                //Kiểm tra hợp đồng có hợp lệ hay không
                if (procurment == null) throw new Exception("Mã hợp đồng không hợp lệ");

                //Kiểm tra loại vật nuôi có hợp lệ hay không
                var checkSpecie = procurment.ProcurementDetails.Where(p => p.SpeciesId.Contains(createDto.SpecieType)).FirstOrDefault();
                if (checkSpecie == null) throw new Exception("Loại vật nuôi không có trong hợp đồng này");

                //Kiểm tra xem con vật này có phải của hợp đồng này không
                var isExitsLivestock = procurment.LivestockProcurements.Where(p => p.LivestockId.Equals(livestock.Id)).SingleOrDefault();
                if (isExitsLivestock == null) throw new Exception("Vật nuôi không nằm trong hợp đồng được chọn");

                //Kiểm tra xem loại bệnh có được trong loại bệnh bảo hành hay không
                var diseases = await _context.VaccinationRequirement.Where(p => p.ProcurementDetailId.Equals(checkSpecie.Id)).ToListAsync();
                var isDisease = diseases.Where(p => p.DiseaseId.Equals(createDto.DiseaseId)).SingleOrDefault();
                if (isDisease == null) throw new Exception("Loại bệnh này không được bảo hành với hợp đồng này!");


                InsuranceRequest requestData = new InsuranceRequest()
                {
                    Id = SlugId.New(),
                    RequestLivestockId = isExitsLivestock.LivestockId,
                    DiseaseId = createDto.DiseaseId,
                    OtherReason = createDto.OtherReason,
                    ImageUris = createDto.ImageUris,
                    Status = insurance_request_status.CHỜ_DUYỆT,
                    IsLivestockReturn = false,
                    ProcurementId = procurment.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = createDto.CreatedBy,
                    UpdatedBy = createDto.CreatedBy,
                    RequestLivestockStatus = insurance_request_livestock_status.KHÔNG_THU_HỒI
                };
                try
                {
                    _context.InsuranceRequests.Add(requestData);
                    await _context.SaveChangesAsync();
                    var responseData = _mapper.Map<InsurenceRequestDTO>(requestData);
                    return responseData;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + " Lỗi khi lưu vào database");
                }
                
                
            }
        }

        public Task<InsurenceRequestDTO> CreateInsurenceRequestWithScan(CreateInsurenceQrDTO createDto)
        {
            throw new NotImplementedException();
        }

        public ListInsuranceStatusDTO GetAllStatusInsurence()
        {
             var specieTypes = Enum.GetValues(typeof(insurance_request_status))
                                       .Cast<insurance_request_status>()
                                       .Select(e => e.ToString())  // Chuyển giá trị enum thành chuỗi
                                       .ToList();
            var data = new ListInsuranceStatusDTO()
            {
                Total = 0,
            };

            data.Items = specieTypes;
            data.Total = specieTypes.Count;

            return data;
        }

        public async Task<VaccinationProcurmentDto> GetDataProcurmentByInsurance(string id)
        {
            VaccinationProcurmentDto response = new VaccinationProcurmentDto();
            List<VaccinationDto> vaccinationRequirements = new List<VaccinationDto>();

            var insurance = await _context.InsuranceRequests.Where( x => x.Id.Equals(id) ).SingleOrDefaultAsync();
            if (insurance == null) throw new Exception("Mã bảo hành không hợp lệ");
            var livestock = await _context.Livestocks.Where(x => x.Id.Equals(insurance.RequestLivestockId)).SingleOrDefaultAsync();
            if (insurance.OrderId != null)
            {
                var order = await _context.OrderRequirements.Where(x => x.OrderId.Equals(insurance.OrderId) && x.SpecieId.Equals(livestock.SpeciesId)).SingleOrDefaultAsync();
                var vaccination = await _context.VaccinationRequirement.Where(x => x.OrderRequirementId.Equals(order.Id)).Include(x => x.Disease).ToListAsync();
                response.Name = order.Id;
                foreach(var d in vaccination)
                {
                    var vacxin = new VaccinationDto()
                    {
                        Id = d.DiseaseId,
                        DateOfInsurance = d.InsuranceDuration,
                        Name = d.Disease.Name
                    };
                    vaccinationRequirements.Add(vacxin);
                }
            }
            else 
            {
                var procurment = await _context.ProcurementDetails.Where(x => x.ProcurementPackageId.Equals(insurance.ProcurementId) && x.SpeciesId.Equals(livestock.SpeciesId)).Include(x => x.ProcurementPackage).SingleOrDefaultAsync();
                var vaccination = await _context.VaccinationRequirement.Where(x => x.ProcurementDetailId.Equals(procurment.Id)).Include(x => x.Disease).ToListAsync();
                response.Name = procurment.ProcurementPackage.Name;
                response.CustomerName = procurment.ProcurementPackage.Owner;
                foreach (var d in vaccination)
                {
                    var vacxin = new VaccinationDto()
                    {
                        Id = d.Id,
                        DateOfInsurance = d.InsuranceDuration,
                        Name = d.Disease.Name
                    };
                    vaccinationRequirements.Add(vacxin);
                }
            }
            response.vaccinationRequirements = vaccinationRequirements;
            return response;

        }

        public async Task<ListInsurenceRequestDTO> GetInsurenceList(InsurenceFilter filter)
        {
            var result = new ListInsurenceRequestDTO()
            {
                Total = 0
            };

            var listInsurance = await _context.InsuranceRequests.Include(x => x.Disease).Include(x => x.ProcurementPackage).Include(x => x.Order).ToListAsync();
            
            var dtoList = listInsurance.Select(x => new InsurenceRequestDTO
            {
                Id = x.Id,
                DiseaseName = x.Disease.Name,
                RequestLivestockId = x.RequestLivestockId,
                NewLivestockId = x.NewLivestockId,
                Status = x.Status.ToString(),
                InsurenceRequestName = x.ProcurementId != null ? x.ProcurementPackage.Name : x.Order.Id.ToString(),
                ProcurementDetailId = x.ProcurementId,
                OrderRequirementId = x.OrderId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CreatedBy= x.CreatedBy,
                UpdatedBy = x.UpdatedBy
                
            }).ToList();

            

            foreach (var d in dtoList)
            {
                var newLiveStock = await _context.Livestocks.SingleOrDefaultAsync(x => x.Id.Equals(d.NewLivestockId));
                var requestLiveStock = await _context.Livestocks.SingleOrDefaultAsync(x => x.Id.Equals(d.RequestLivestockId));

                if (newLiveStock != null)
                {
                    d.InspectionCodeNew = newLiveStock.InspectionCode;
                }

                if (requestLiveStock != null)
                {
                    d.InspectionCodeRequest = requestLiveStock.InspectionCode;
                }
            }

            if (filter != null)
            {
                if(filter.LivestockId != null)
                {            
                    dtoList = dtoList.Where(x => x.InspectionCodeRequest.Contains(filter.LivestockId)).ToList();
                }
                if(filter.Status != null)
                {
                    dtoList = dtoList.Where(x => x.Status.Equals(filter.Status)).ToList();
                }
                if(filter.ProcurmentId != null)
                {
                    dtoList = dtoList.Where( x => x.ProcurementDetailId.Equals(filter.ProcurmentId)).ToList();
                }
            }

           
            
            
            
            dtoList = dtoList.OrderBy(x => x.UpdatedAt)
                    .Skip(filter.Skip)
                    .Take(filter.Take)
                    .ToList();

            result.Items = dtoList;
            result.Total = dtoList.Count();
            return result;

        }

        public async Task<InsurenceRequestInfoDTO> GetInsurenceRequestInfo(string id)
        {
            if (id == null) throw new Exception("Id không đúng định dạng");
            var insuranceRequest = await _context.InsuranceRequests.Where( x => x.Id.Equals(id)).Include(x => x.Disease).SingleOrDefaultAsync();
            if ((insuranceRequest==null))
            {
                throw new Exception("Id không hợp lệ");
                
            }
            var requestLivestock = await _context.Livestocks.Where(p => p.Id.Equals(insuranceRequest.RequestLivestockId)).SingleOrDefaultAsync();
            var returnLivestock = await _context.Livestocks.Where(p => p.Id.Equals(insuranceRequest.NewLivestockId)).SingleOrDefaultAsync();
            var specie = await _context.Species.Where(x => x.Id.Equals(requestLivestock.SpeciesId)).SingleOrDefaultAsync();
            try
            {
                var insuranceInfo = new InsurenceRequestInfoDTO()
                {
                    Id = insuranceRequest.Id,
                    ApprovedAt = insuranceRequest.ApprovedAt,
                    CancelledAt = insuranceRequest.CancelledAt,
                    CompletedAt = insuranceRequest.CompletedAt,
                    CreatedAt = insuranceRequest.CreatedAt,
                    CreatedBy = insuranceRequest.CreatedBy,
                    DiseaseId = insuranceRequest.DiseaseId,
                    ImageUris = insuranceRequest.ImageUris,
                    IsLivestockReturn = insuranceRequest.IsLivestockReturn,
                    Note = insuranceRequest.Note,
                    RejectedAt = insuranceRequest.RejectedAt,
                    OtherReason = insuranceRequest.OtherReason,
                    NewLivestockId = insuranceRequest.NewLivestockId,
                    RequestLivestockId = insuranceRequest.RequestLivestockId,
                    OrderRequirementId = insuranceRequest.OrderId,
                    ProcurementDetailId = insuranceRequest.ProcurementId,
                    Species = specie.Name,
                    ExportWeight = requestLivestock.WeightOrigin,
                    InspectionCodeRequest = requestLivestock.InspectionCode,
                    DiseaseName = insuranceRequest.Disease.Name
                };
                if (returnLivestock != null)
                {
                    insuranceInfo.ExportWeightReturn = returnLivestock.WeightExport;
                    insuranceInfo.InspectionCodeNew = returnLivestock.InspectionCode;
                }

                return insuranceInfo;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi dữ liệu");
            }
          

            
        }

        public async Task<ListInsurenceRequestOverviewDTO> GetPendingOverviewList()
        {
            var lst = new ListInsurenceRequestOverviewDTO()
            {
                Total = 0
            };

            var listInsurance = new List<InsurenceRequestOverviewDTO>();

            var listStatus = Enum.GetValues(typeof(insurance_request_status))
                                       .Cast<insurance_request_status>()
                                       .Select(e => e)  // Chuyển giá trị enum thành chuỗi
                                       .ToList();
            for (int i = 0; i < 6; i++)
            {
                var quantity = await _context.InsuranceRequests
                    .Where(x => x.Status == listStatus[i])
                    .ToListAsync();

                var soLuong = quantity.Count();

                listInsurance.Add(new InsurenceRequestOverviewDTO()
                {
                    Status = listStatus[i].ToString(), 
                    Quantity = soLuong
                });
            }

            lst.Items = listInsurance;
            lst.Total = listInsurance.Sum(x => x.Quantity); // Cập nhật tổng số lượng

            return lst; // <-- Quan trọng: TRẢ VỀ GIÁ TRỊ
        }

        public async Task<InsurenceRequestDTO> RejectInsuranceRequest(RejectInsuranceDto data)
        {
            try
            {
                var insurance = await _context.InsuranceRequests.Where(x => x.Id.Equals(data.Id)).SingleOrDefaultAsync();
                if (insurance == null) throw new Exception("Mã bảo hành không hợp lệ");
                insurance.Status = insurance_request_status.TỪ_CHỐI;
                insurance.CancelledAt = DateTime.Now;
                insurance.RejectReason = data.reasonReject;
                if(insurance.NewLivestockId != null)
                {
                    var newLivestock = await _context.Livestocks.Where(x => x.Id.Equals(insurance.NewLivestockId)).SingleOrDefaultAsync();
                    newLivestock.Status = livestock_status.KHỎE_MẠNH;
                }
                await _context.SaveChangesAsync();
                var response = _mapper.Map<InsurenceRequestDTO>(insurance);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi dữ liệu");
            }
        }

        public async Task<InsurenceRequestDTO> RemoveNewLivestockInsuranceRequest(RemoveInsuranceLivestockDto updateDto)
        {
            var insurance = await _context.InsuranceRequests.Where(x => x.Id.Equals(updateDto.Id)).SingleOrDefaultAsync();
            if (insurance == null) throw new Exception("Mã bảo hành không hợp lệ");
            if (insurance.NewLivestockId == null) throw new Exception("Không có vật nuôi để xóa");
            var oldNewLivestock = await _context.Livestocks.Where(x => x.Id.Equals(insurance.NewLivestockId)).SingleOrDefaultAsync();
            try
            {
              
                insurance.NewLivestockId = null;
                insurance.UpdatedAt = DateTime.Now;
                insurance.UpdatedBy = updateDto.UpdatedBy;
                oldNewLivestock.Status = livestock_status.KHỎE_MẠNH;
                oldNewLivestock.UpdatedAt = DateTime.Now;
                oldNewLivestock.UpdatedBy = updateDto.UpdatedBy;
                insurance.Status = insurance_request_status.ĐANG_CHUẨN_BỊ;
                await _context.SaveChangesAsync();

                var data = _mapper.Map<InsurenceRequestDTO>(insurance);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Lỗi dữ liệu");
            }
        }

        public async Task<InsurenceRequestDTO> TransferInsuranceRequest(RemoveInsuranceLivestockDto data)
        {
            var insurance = await _context.InsuranceRequests.Where(x => x.Id.Equals(data.Id)).SingleOrDefaultAsync();
            if (insurance.NewLivestockId == null) throw new Exception("Chưa có vật nuôi, Không thể bàn giao");
            try
            {

                var newLivestock = await _context.Livestocks.Where(x => x.Id.Equals(insurance.NewLivestockId)).SingleOrDefaultAsync();
                newLivestock.Status = livestock_status.ĐÃ_XUẤT;
                newLivestock.WeightExport = newLivestock.WeightOrigin;
                newLivestock.UpdatedAt = DateTime.Now;
                newLivestock.UpdatedBy = data.UpdatedBy;
                insurance.Status = insurance_request_status.HOÀN_THÀNH;
                insurance.CompletedAt = DateTime.Now;
                insurance.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                var response = _mapper.Map<InsurenceRequestDTO>(insurance);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi dữ liệu");
            }
        }

        public async Task<InsurenceRequestDTO> UpdateInfoInsuranceRequest(UpdateInsuranceRequestInfoDto updateDto)
        {
            var insurance = await _context.InsuranceRequests.Where(x => x.Id.Equals(updateDto.Id)).SingleOrDefaultAsync();
            if (insurance == null) throw new Exception("Không tìm thấy mã hợp đồng");
            try
            {
                
                if (!string.IsNullOrEmpty(updateDto.Note))
                {
                    insurance.Note = updateDto.Note;
                }
                if(!string.IsNullOrEmpty(updateDto.OtherReason)) insurance.OtherReason = updateDto.OtherReason;

                if (!string.IsNullOrEmpty(updateDto.RequestLivestockStatus))
                {
                    if (Enum.TryParse(typeof(inspection_code_range_status), updateDto.RequestLivestockStatus, out var status))
                    {
                        insurance.RequestLivestockStatus = (insurance_request_livestock_status)status;
                    }
                    else
                    {
                        // Xử lý trường hợp không chuyển đổi thành công nếu cần
                        // Ví dụ: ghi log hoặc gán giá trị mặc định
                    }
                }
                if (!string.IsNullOrEmpty(updateDto.DiseaseId)) insurance.DiseaseId = updateDto.DiseaseId;
                insurance.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                var data = _mapper.Map<InsurenceRequestDTO>(insurance);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Lỗi cập nhật");
            }
        }

        public async Task<InsurenceRequestDTO> UpdateNewLivestockInsurenceRequest(UpdateInsuranceLivestockDto updateDto)
        {
            try
            {
                var insurance = await _context.InsuranceRequests.Where( p => p.Id.Equals(updateDto.Id)).SingleOrDefaultAsync();
                if (insurance == null) throw new Exception("Mã bảo hành không hợp lệ");
                
                
                    var requestLivestock = await _context.Livestocks.Where(x => x.Id.Equals(insurance.RequestLivestockId)).SingleOrDefaultAsync();
                    var livestockNew = await _context.Livestocks.Where(x => x.InspectionCode.Equals(updateDto.LivestockId) && x.SpeciesId.Equals(requestLivestock.SpeciesId)).SingleOrDefaultAsync();
                    if (livestockNew == null) throw new Exception("Mã vật nuôi hoặc loại vật nuôi không chính xác");
                    insurance.Status = insurance_request_status.CHỜ_BÀN_GIAO;
                    insurance.UpdatedAt = DateTime.Now;
                    insurance.UpdatedBy = updateDto.UpdatedBy;
                    livestockNew.UpdatedBy = updateDto.UpdatedBy;
                    livestockNew.UpdatedAt = DateTime.Now;
                    livestockNew.WeightOrigin = updateDto.Weight;
                    livestockNew.Status = livestock_status.CHỜ_XUẤT;
                    if (insurance.NewLivestockId == null)
                    {
                        insurance.NewLivestockId = livestockNew.Id;
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var oldNewLivestock =  await _context.Livestocks.Where(x => x.Id.Equals(insurance.NewLivestockId)).SingleOrDefaultAsync();
                        insurance.NewLivestockId = updateDto.LivestockId;
                        oldNewLivestock.Status = livestock_status.KHỎE_MẠNH;
                        oldNewLivestock.UpdatedAt = DateTime.Now;
                        oldNewLivestock.UpdatedBy = updateDto.UpdatedBy;
                        await _context.SaveChangesAsync();
                    }
                    
                    var data = _mapper.Map<InsurenceRequestDTO>(insurance);
                return data;
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "Lỗi lấy dữ liệu");
            }
        }
    }
}
