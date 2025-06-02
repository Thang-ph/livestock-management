using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services
{
    public class BatchExportService : IBatchExportRepository
    {
        private readonly LmsContext _context;

        public BatchExportService(LmsContext context)
        {
            _context = context;
        }

        public async Task<BatchExport> AddCustomerFromBatchExport(AddCustomerBatchExportDTO addCustomerBatchExportDTO)
        {
            BatchExport batchExport = new BatchExport();
            List<BatchExport> batchExports = _context.BatchExports.Where(x => x.ProcurementPackageId == addCustomerBatchExportDTO.ProcurementPackageId).ToList();

            List<ProcurementDetail> procurementDetails = _context.ProcurementDetails.Where(x => x.ProcurementPackageId == addCustomerBatchExportDTO.ProcurementPackageId).ToList();
            int Total = 0;
            int RequiredQuantity = 0;
            foreach (BatchExport batch in batchExports)
            {
                Total += batch.Total;
            }
            foreach (ProcurementDetail procurementDetail in procurementDetails)
            {
                RequiredQuantity += procurementDetail.RequiredQuantity ?? 0;
            }
            if (Total+addCustomerBatchExportDTO.Total> RequiredQuantity)
            {
                throw new Exception($"Số lượng bàn giao vượt quá tổng số lượng trong gói thầu {Total+addCustomerBatchExportDTO.Total-RequiredQuantity} con");
            }
            batchExport.Id = SlugId.New();
            batchExport.CreatedBy = addCustomerBatchExportDTO.CreatedBy;
            batchExport.UpdatedAt = DateTime.Now;
            batchExport.CreatedAt = DateTime.Now;
            batchExport.UpdatedBy = addCustomerBatchExportDTO.CreatedBy;
            batchExport.CustomerAddress = addCustomerBatchExportDTO.CustomerAddress;
            batchExport.CustomerName = addCustomerBatchExportDTO.CustomerName;
            batchExport.CustomerPhone = addCustomerBatchExportDTO.CustomerPhone;
            batchExport.CustomerNote = addCustomerBatchExportDTO.CustomerNote;
            batchExport.Total = addCustomerBatchExportDTO.Total;
            batchExport.Remaining = batchExport.Total;
            ProcurementPackage procurementPackage=_context.ProcurementPackages.AsNoTracking().FirstOrDefault(x=>x.Id==addCustomerBatchExportDTO.ProcurementPackageId);
            if (procurementPackage == null)
            {
                throw new Exception("Không tìm thấy gói thầu này");
            }
            batchExport.ProcurementPackageId = addCustomerBatchExportDTO.ProcurementPackageId;
            batchExport.Status = batch_export_status.CHỜ_BÀN_GIAO;
            await _context.BatchExports.AddAsync(batchExport);
            await _context.SaveChangesAsync();
            return batchExport;
        }

        public async Task<bool> AddLivestockToBatchExportDetail( BatchExportDetailAddDTO batchExportDetailAddDTO)
        {
            var batchExport = await _context.BatchExports
  
     .FirstOrDefaultAsync(x => x.Id == batchExportDetailAddDTO.BatchExportId);
            
            if (batchExport == null)
            {
                throw new Exception("Không tìm thấy lô xuất");
            }
            Livestock livestock = await _context.Livestocks
           .FirstOrDefaultAsync(o => o.Id == batchExportDetailAddDTO.LivestockId
               && o.Status != livestock_status.CHẾT
                && o.Status != livestock_status.XUẤT_BÁN_THỊT
               && o.Status != livestock_status.ĐÃ_XUẤT) ??
           throw new Exception("Không tìm thấy vật nuôi hoặc vật nuôi đã chết chờ xuất hoặc đã xuất");
            //ProcurementDetail procurementDetail = _context.ProcurementDetails.FirstOrDefault(x => x.ProcurementPackageId == batchExport.ProcurementPackageId && x.SpeciesId == livestock.SpeciesId);
            //int ageInDays = (int)(DateTime.Now.Date - livestock.Dob!.Value.Date).TotalDays;
            //decimal? weight = livestock.WeightEstimate ?? livestock.WeightOrigin;
            //if (ageInDays < procurementDetail.RequiredAgeMin)
            //{
            //    throw new Exception($"Tuổi vật nuôi ({ageInDays} ngày) nhỏ hơn tuổi tối thiểu yêu cầu ({procurementDetail.RequiredAgeMin} ngày).");
            //}
            //if (ageInDays > procurementDetail.RequiredAgeMax)
            //{
            //    throw new Exception($"Tuổi vật nuôi ({ageInDays} ngày) lớn hơn tuổi tối đa cho phép ({procurementDetail.RequiredAgeMax} ngày).");
            //}
            //if (weight < procurementDetail.RequiredWeightMin)
            //{
            //    throw new Exception($"Khối lượng vật nuôi ({weight} kg) nhỏ hơn khối lượng tối thiểu yêu cầu ({procurementDetail.RequiredWeightMin} kg).");
            //}
            //if (weight > procurementDetail.RequiredWeightMax)
            //{
            //    throw new Exception($"Khối lượng vật nuôi ({weight} kg) vượt quá khối lượng tối đa cho phép ({procurementDetail.RequiredWeightMax} kg).");
            //}

            IdentityUser user = _context.Users.FirstOrDefault(x => x.Id == batchExportDetailAddDTO.CreatedBy);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người cập nhật");
            }
            if (batchExport.Remaining == 0) {
                throw new Exception("Người này đã nhận tối đa số vật nuôi");
            }
            livestock.Status = livestock_status.CHỜ_XUẤT;
            _context.Livestocks.Update(livestock);
            await _context.SaveChangesAsync();
            BatchExportDetail batchExportDetail = new BatchExportDetail();
            batchExportDetail.Id = SlugId.New();
            batchExportDetail.LivestockId = batchExportDetailAddDTO.LivestockId;
            batchExportDetail.BatchExportId = batchExportDetailAddDTO.BatchExportId;
            batchExportDetail.Status = batch_export_status.CHỜ_BÀN_GIAO;
            batchExportDetail.CreatedAt= DateTime.Now;
            batchExportDetail.UpdatedAt=DateTime.Now;
            batchExportDetail.CreatedBy = batchExportDetailAddDTO.CreatedBy;
            batchExportDetail.UpdatedBy = batchExportDetailAddDTO.CreatedBy;
            batchExportDetail.ExpiredInsuranceDate = batchExportDetailAddDTO.ExpiredInsuranceDate;
            batchExportDetail.ExportDate=DateTime.Now;
            batchExport.Remaining--;
            if (batchExport.Remaining == 0)
            {
                batchExport.Status = batch_export_status.CHỜ_BÀN_GIAO;
            }
            List<BatchExportDetail> batchExportDetails = _context.BatchExportDetails.Where(x => x.BatchExport.Id == batchExport.Id).ToList();
            //foreach (BatchExportDetail batch in batchExportDetails)
            //{
            //    batch.Status = batch_export_status.ĐÃ_BÀN_GIAO;
            //}
            ProcurementPackage procurementPackage = _context.ProcurementPackages.FirstOrDefault(x => x.Id == batchExport.ProcurementPackageId);
            procurementPackage.Status = procurement_status.CHỜ_BÀN_GIAO;
            _context.ProcurementPackages.Update(procurementPackage);
            _context.SaveChanges(); 
            batchExport.UpdatedBy= batchExportDetailAddDTO.CreatedBy;
            batchExport.UpdatedAt=DateTime.Now;
            _context.BatchExports.Update(batchExport);
            await _context.SaveChangesAsync();
            await  _context.BatchExportDetails.AddAsync(batchExportDetail);
            await _context.SaveChangesAsync();
            batchExportDetail.BatchExport = null;
            return true;
        }

        public async Task<bool> CanAddCustomerInBatchExport(string procurementId)
        {
            ProcurementPackage procurementPackage= _context.ProcurementPackages.FirstOrDefault(x=>x.Id==procurementId);
            if (procurementPackage == null)
            {
                throw new Exception("Không tìm thấy gói thầu này");
            }
            List<BatchExport> batchExports = _context.BatchExports.Where(x => x.ProcurementPackageId == procurementId).ToList();

            List<ProcurementDetail> procurementDetails =_context.ProcurementDetails.Where(x => x.ProcurementPackageId == procurementId).ToList();
            int Total = 0;
            int RequiredQuantity = 0;
            foreach (BatchExport batchExport in batchExports)
            {
                Total += batchExport.Total;
            }
            foreach (ProcurementDetail procurementDetail in procurementDetails)
            {
                RequiredQuantity += procurementDetail.RequiredQuantity??0;
            }
            if(RequiredQuantity ==0)
            {
                return false;
            }
            if (Total == RequiredQuantity)
            {
                return false;
            }
            return true;
        }
        public async Task<BatchExportDetail> ChangeLivestockToBatchExportDetail(string batchExportDetailId, BatchExportDetailChangeDTO batchExportDetailChangeDTO)
        {
            BatchExport batchExport = _context.BatchExports.FirstOrDefault(x => x.Id == batchExportDetailChangeDTO.BatchExportId);
            if (batchExport == null)
            {
                throw new Exception("Không tìm thấy lô xuất");
            }
           
            IdentityUser user = _context.Users.FirstOrDefault(x => x.Id == batchExportDetailChangeDTO.UpdatedBy);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người cập nhật");
            }
            Livestock livestock = await _context.Livestocks.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == batchExportDetailChangeDTO.LivestockId
                && o.Status != livestock_status.CHẾT
                 && o.Status != livestock_status.CHỜ_XUẤT
                && o.Status != livestock_status.ĐÃ_XUẤT) ??
            throw new Exception("Không tìm thấy vật nuôi hoặc vật nuôi đã bán hoặc chết");
            List<BatchExportDetail> existLivestock = _context.BatchExportDetails.Where(x => x.LivestockId == batchExportDetailChangeDTO.LivestockId).ToList();
            if (existLivestock.Any())
            {
                throw new Exception("Vật nuôi đã thuộc 1 lô xuất khác");
            }
            BatchExportDetail batchExportDetail = _context.BatchExportDetails.FirstOrDefault(x => x.Id == batchExportDetailId);
            if (batchExportDetail == null)
            {
                throw new Exception("không tìm thấy batch export detail");
            }
            batchExportDetail.LivestockId = batchExportDetailChangeDTO.LivestockId;
            batchExportDetail.BatchExportId = batchExportDetailChangeDTO.BatchExportId;
            batchExportDetail.UpdatedAt = DateTime.Now;
            batchExportDetail.UpdatedBy = batchExportDetailChangeDTO.UpdatedBy;
            _context.BatchExportDetails.Update(batchExportDetail);
            await _context.SaveChangesAsync();
            batchExportDetail.BatchExport = null;
            return batchExportDetail;
        }
        public async Task<BatchExportDetail> UpdateBatchExportDetail(BatchExportDetailUpdateDTO batchExportDetailChangeDTO)
        {
            BatchExport batchExport = _context.BatchExports.Include(x=>x.BatchExportDetails).FirstOrDefault(x => x.Id == batchExportDetailChangeDTO.BatchExportId);
            if (batchExport == null)
            {
                throw new Exception("Không tìm thấy lô xuất");
            }
          

            IdentityUser user = _context.Users.FirstOrDefault(x => x.Id == batchExportDetailChangeDTO.UpdatedBy);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người cập nhật");
            }
            BatchExportDetail batchExportDetail = _context.BatchExportDetails.FirstOrDefault(x => x.BatchExportId == batchExportDetailChangeDTO.BatchExportId);
            batchExportDetail.PriceExport = batchExportDetailChangeDTO.PriceExport;
            batchExportDetail.WeightExport = batchExportDetailChangeDTO.WeightExport;
            batchExportDetail.LivestockId = batchExportDetailChangeDTO.LivestockId;
            batchExportDetail.BatchExportId = batchExportDetailChangeDTO.BatchExportId;
            batchExportDetail.UpdatedAt = DateTime.Now;
            batchExportDetail.UpdatedBy = batchExportDetailChangeDTO.UpdatedBy;
            batchExportDetail.ExpiredInsuranceDate = batchExportDetailChangeDTO.ExpiredInsuranceDate;
            batchExportDetail.ExportDate = batchExportDetailChangeDTO.ExportDate;
            batchExportDetail.HandoverDate=batchExportDetailChangeDTO.HandoverDate;
             _context.BatchExportDetails.Update(batchExportDetail);
            await _context.SaveChangesAsync();
            return batchExportDetail;
        }

        public async Task<bool> DeleteCustomerFromBatchExport(string batchExportID)
        {
            BatchExport batchExport = _context.BatchExports.FirstOrDefault(x => x.Id == batchExportID);
            if (batchExport == null)
            {
                throw new Exception("Không tìm thấy batch export này");
            }

            List<BatchExportDetail> batchExportDetails = _context.BatchExportDetails.Where(x => x.BatchExportId == batchExportID).ToList();
            if (batchExportDetails.Any())
            {
                throw new Exception("Khách hàng này đã trong quá trình bàn giao");
            }
            if (batchExport.Status == batch_export_status.ĐÃ_BÀN_GIAO)
            {
                throw new Exception("Lô xuất này đã hoàn thành");
            }
             _context.BatchExports.Remove(batchExport);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> DeleteLivestockFromBatchExportDetail(string batchExportDetailId)
        {
            BatchExportDetail batchExportDetail = _context.BatchExportDetails.FirstOrDefault(x => x.Id == batchExportDetailId);
            if (batchExportDetail == null)
            {
                throw new Exception("Không tìm thấy thông tin chi tiết lô xuất này");
            }
            _context.BatchExportDetails.Remove(batchExportDetail);
            await _context.SaveChangesAsync();
            BatchExport batchExport = _context.BatchExports.FirstOrDefault(x => x.Id == batchExportDetail.BatchExportId);
            batchExport.Remaining++;
            _context.BatchExports.Update(batchExport);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ListCustomers> GetListCustomers(string id, CustomersFliter filter)
        {
            var result = new ListCustomers()
            {
                Total = 0
            };

            var customersList = await _context.BatchExports.Where(x => x.ProcurementPackageId == id).ToArrayAsync();
            if (customersList == null || !customersList.Any()) return result;
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    customersList = customersList
                        .Where(v => v.CustomerName.ToUpper().Contains(filter.Keyword.Trim().ToUpper()))
                        .ToArray();
                }
                if (filter.FromDate != null && filter.FromDate != DateTime.MinValue)
                {
                    customersList = customersList
                        .Where(v => v.CreatedAt >= filter.FromDate)
                        .ToArray();
                }
                if (filter.ToDate != null && filter.ToDate != DateTime.MinValue)
                {
                    customersList = customersList
                       .Where(v => v.CreatedAt <= filter.ToDate)
                       .ToArray();
                }
                if (filter.Status != null)
                {
                    customersList = customersList
                        .Where(v => v.Status == filter.Status)
                        .ToArray();
                }
            }
            if (!customersList.Any()) return result;
            customersList = customersList
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();

            result.Items = customersList
                .Select(v => new BatchExportDTO
                {
                    Id = v.Id,
                    CustomerName = v.CustomerName,
                    CustomerPhone = v.CustomerPhone,
                    CustomerAddress = v.CustomerAddress,
                    CustomerNote = v.CustomerNote,
                    Total = v.Total,
                    Remaining = v.Remaining,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();
            result.Total = customersList.Length;

            return result;
        }

        public async Task ImportCustomer(string procurementId, string requestedBy, IFormFile file)
        {

            var procurementPackage = await _context.ProcurementPackages.FindAsync(procurementId) ??
                throw new Exception("Không tìm thấy gói thầu");
            //remove existing list customers if possible
            var currentListExports = await _context.BatchExports
                .Where(o => o.ProcurementPackageId == procurementId)
                .ToArrayAsync();
            if (currentListExports != null && currentListExports.Any())
            {
                var currentExportDetails = await _context.BatchExportDetails
                    .Where(o => currentListExports.Select(x => x.Id).Contains(o.BatchExportId))
                    .ToArrayAsync();
                if (currentExportDetails != null && currentExportDetails.Any())
                    throw new Exception("Gói thầu đang trong quá trình bàn giao, không thể tải thêm danh sách khách hàng");
                _context.BatchExports.RemoveRange(currentListExports);
                await _context.SaveChangesAsync();
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (file == null || file.Length == 0) throw new Exception("File không hợp lệ.");

            var uploadsFolder = $"{Directory.GetCurrentDirectory()}\\Uploads\\";
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, file.Name);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Lấy BarnId thứ 2 trong bảng Barns
            var barn = await _context.Barns.OrderBy(b => b.Id).Skip(1).FirstOrDefaultAsync();
            if (barn == null) throw new Exception("Không tìm thấy Barn thứ 2.");
            int count = 0;

            //validate procurement package info

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) throw new Exception("Không tìm thấy sheet hợp lệ trong file Excel.");

                var procurementCodeStr = worksheet.Cells["A1"].Value?.ToString();
                if (string.IsNullOrEmpty(procurementCodeStr))
                    throw new Exception("Mã gói thầu không hợp lệ");

                var code = procurementCodeStr.Trim().Split(":").LastOrDefault()?.Trim();
                if (string.IsNullOrEmpty(code))
                    throw new Exception("Mã gói thầu không hợp lệ");

                if (!string.Equals(procurementPackage.Code, code, StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"Không tìm thấy mã gói thầu {code}");

                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            bool isHeaderSkipped = false;
                            while (reader.Read())
                            {
                                if (!isHeaderSkipped || count < 6)
                                {
                                    isHeaderSkipped = true;
                                    count++;
                                    continue;
                                }
                                BatchExport b = new BatchExport();
                                b.Id = SlugId.New();
                                b.CustomerName = reader.GetValue(0).ToString();
                                b.CustomerAddress = reader.GetValue(1).ToString();
                                b.CustomerPhone = reader.GetValue(2).ToString();
                                b.CustomerNote = reader.GetValue(3).ToString();
                                b.Total = Convert.ToInt32(reader.GetValue(4).ToString());
                                b.Remaining = b.Total;
                                b.ProcurementPackageId = procurementId;
                                b.BarnId = barn.Id;
                                b.CreatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy;
                                b.UpdatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy;
                                b.UpdatedAt = b.CreatedAt = DateTime.Now;
                                _context.BatchExports.Add(b);
                                await _context.SaveChangesAsync();
                            }
                        } while (reader.NextResult());
                    }
                }
            }
            return;
        }

        public async Task<BatchExport> UpdateCustomerFromBatchExport(string batchExportId,UpdateCustomerBatchExportDTO updateCustomerBatchExportDTO)
        {
            BatchExport batchExport = _context.BatchExports.FirstOrDefault(x => x.Id == batchExportId);
            if (batchExport == null)
            {
                throw new Exception("Không tìm thấy lô xuất");
            }
            batchExport.UpdatedBy = updateCustomerBatchExportDTO.UpdatedBy;
            batchExport.UpdatedAt = DateTime.Now;
            batchExport.CustomerAddress= updateCustomerBatchExportDTO.CustomerAddress;  
            batchExport.CustomerName=updateCustomerBatchExportDTO.CustomerName;
            batchExport.CustomerPhone=updateCustomerBatchExportDTO.CustomerPhone;
            batchExport.CustomerNote=updateCustomerBatchExportDTO.CustomerNote;
            _context.BatchExports.Update(batchExport);
            await _context.SaveChangesAsync();
            return batchExport;
        }

        public async Task<bool> CanChangeLivestockInBatchExportDetail(string batchExportDetailId)
        {
            BatchExportDetail batchExportDetail = _context.BatchExportDetails.FirstOrDefault(x=>x.Id == batchExportDetailId);
            if (batchExportDetail == null) {
                throw new Exception("Không tim thấy batch export detail");
            }
            if (batchExportDetail.ExpiredInsuranceDate < DateTime.Now)
            {
                return false;
            }
            return true;
        }

       
        public async Task<bool> ConfirmHandoverBatchExportDetail(string livestockId, string UpdatedBy)
        {
            BatchExportDetail batchExportDetail = _context.BatchExportDetails.Include(x => x.Livestock).FirstOrDefault(x => x.LivestockId == livestockId);
            if (batchExportDetail == null)
            {
                throw new Exception("Không tim thấy batch export detail");
            }
            List<BatchExportDetail> batchExportDetails = _context.BatchExportDetails.Where(x => x.BatchExportId == batchExportDetail.BatchExportId).ToList();

            Livestock livestock = batchExportDetail.Livestock;
            livestock.Status = livestock_status.ĐÃ_XUẤT;
            _context.Livestocks.Update(livestock);
            _context.SaveChanges();
            batchExportDetail.HandoverDate = DateTime.Now;
            batchExportDetail.Status = batch_export_status.ĐÃ_BÀN_GIAO;
            batchExportDetail.UpdatedBy = UpdatedBy;
            batchExportDetail.UpdatedAt = DateTime.Now;
            _context.Update(batchExportDetail);
            await _context.SaveChangesAsync();
            if (batchExportDetails.All(x => x.Status == batch_export_status.ĐÃ_BÀN_GIAO))
            {
                BatchExport batchExport = _context.BatchExports.FirstOrDefault(x => x.Id == batchExportDetail.BatchExportId);
                batchExport.Status = batch_export_status.ĐÃ_BÀN_GIAO;
                List<BatchExport> listBatchExports = _context.BatchExports.Where(x => x.ProcurementPackageId == batchExport.ProcurementPackageId).ToList();
                if (listBatchExports.All(x => x.Status == batch_export_status.ĐÃ_BÀN_GIAO))
                {
                    ProcurementPackage procurementPackage = _context.ProcurementPackages.FirstOrDefault(x => x.Id == batchExport.ProcurementPackageId);
                    procurementPackage.Status = procurement_status.HOÀN_THÀNH;
                    _context.Update(procurementPackage);
                    _context.SaveChanges();
                }
                    _context.Update(batchExport);
                _context.SaveChanges();
            }
            return true;
        }

        public async Task<bool> AddLivestockToBatchExportDetailByInspectionCode(BatchExportDetailAddDTOByInspectionCode batchExportDetailAddDTO)
        {
            var batchExport = await _context.BatchExports

   .FirstOrDefaultAsync(x => x.Id == batchExportDetailAddDTO.BatchExportId);

            if (batchExport == null)
            {
                throw new Exception("Không tìm thấy lô xuất");
            }
            Livestock livestock = await _context.Livestocks
           .FirstOrDefaultAsync(o => o.InspectionCode == batchExportDetailAddDTO.InspectionCode&&o.Species.Type==batchExportDetailAddDTO.Specie_Type
               && o.Status != livestock_status.CHẾT
               && o.Status != livestock_status.ĐÃ_XUẤT
                && o.Status != livestock_status.XUẤT_BÁN_THỊT
               ) ??
           throw new Exception("Không tìm thấy vật nuôi hoặc vật nuôi đã chết chờ xuất hoặc đã xuất");

            List<BatchExportDetail> existLivestock = _context.BatchExportDetails.Where(x => x.LivestockId == livestock.Id).ToList();
            if (existLivestock.Any())
            {
                throw new Exception("Vật nuôi đã thuộc 1 lô xuất khác");
            }
            //ProcurementDetail procurementDetail = _context.ProcurementDetails.FirstOrDefault(x => x.ProcurementPackageId == batchExport.ProcurementPackageId && x.SpeciesId == livestock.SpeciesId);
            //int ageInDays = (int)(DateTime.Now.Date - livestock.Dob!.Value.Date).TotalDays;
            //decimal? weight = livestock.WeightEstimate ?? livestock.WeightOrigin;
            //if (ageInDays < procurementDetail.RequiredAgeMin)
            //{
            //    throw new Exception($"Tuổi vật nuôi ({ageInDays} ngày) nhỏ hơn tuổi tối thiểu yêu cầu ({procurementDetail.RequiredAgeMin} ngày).");
            //}
            //if (ageInDays > procurementDetail.RequiredAgeMax)
            //{
            //    throw new Exception($"Tuổi vật nuôi ({ageInDays} ngày) lớn hơn tuổi tối đa cho phép ({procurementDetail.RequiredAgeMax} ngày).");
            //}
            //if (weight < procurementDetail.RequiredWeightMin)
            //{
            //    throw new Exception($"Khối lượng vật nuôi ({weight} kg) nhỏ hơn khối lượng tối thiểu yêu cầu ({procurementDetail.RequiredWeightMin} kg).");
            //}
            //if (weight > procurementDetail.RequiredWeightMax)
            //{
            //    throw new Exception($"Khối lượng vật nuôi ({weight} kg) vượt quá khối lượng tối đa cho phép ({procurementDetail.RequiredWeightMax} kg).");
            //}

            IdentityUser user = _context.Users.FirstOrDefault(x => x.Id == batchExportDetailAddDTO.CreatedBy);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người cập nhật");
            }
            if (batchExport.Remaining == 0)
            {
                throw new Exception("Người này đã nhận tối đa số vật nuôi");
            }
            livestock.Status = livestock_status.CHỜ_XUẤT;
             _context.Livestocks.Update(livestock);
            await _context.SaveChangesAsync();

            BatchExportDetail batchExportDetail = new BatchExportDetail();
            batchExportDetail.Id = SlugId.New();
            batchExportDetail.LivestockId = livestock.Id;
            batchExportDetail.BatchExportId = batchExportDetailAddDTO.BatchExportId;
            batchExportDetail.Status = batch_export_status.CHỜ_BÀN_GIAO;
            batchExportDetail.CreatedAt = DateTime.Now;
            batchExportDetail.UpdatedAt = DateTime.Now;
            batchExportDetail.CreatedBy = batchExportDetailAddDTO.CreatedBy;
            batchExportDetail.UpdatedBy = batchExportDetailAddDTO.CreatedBy;
            batchExportDetail.ExpiredInsuranceDate = batchExportDetailAddDTO.ExpiredInsuranceDate;
            batchExportDetail.ExportDate = DateTime.Now;
            batchExport.Remaining--;
            if (batchExport.Remaining == 0)
            {
                batchExport.Status = batch_export_status.CHỜ_BÀN_GIAO;
              
            }
            ProcurementPackage procurementPackage = _context.ProcurementPackages.FirstOrDefault(x => x.Id == batchExport.ProcurementPackageId);
            procurementPackage.Status = procurement_status.CHỜ_BÀN_GIAO;
            _context.ProcurementPackages.Update(procurementPackage);
            _context.SaveChanges();
            batchExport.UpdatedBy = batchExportDetailAddDTO.CreatedBy;
            batchExport.UpdatedAt = DateTime.Now;
            _context.BatchExports.Update(batchExport);
            await _context.SaveChangesAsync();
            await _context.BatchExportDetails.AddAsync(batchExportDetail);
            await _context.SaveChangesAsync();
            batchExportDetail.BatchExport = null;
            return true;
        }

        public async Task<bool> ConfirmHandoverBatchExportDetailByInspectionCode(string inspectionCode, specie_type specieType, string updatedBy)
        {
            Livestock query = _context.Livestocks.Include(x=>x.Species).FirstOrDefault(x => x.InspectionCode == inspectionCode && x.Species.Type == specieType);
            BatchExportDetail batchExportDetail = _context.BatchExportDetails.Include(x => x.Livestock).FirstOrDefault(x => x.LivestockId == query.Id);
            if (batchExportDetail == null)
            {
                throw new Exception("Không tim thấy batch export detail");
            }
            List<BatchExportDetail> batchExportDetails = _context.BatchExportDetails.Where(x => x.BatchExportId == batchExportDetail.BatchExportId).ToList();

            Livestock livestock = batchExportDetail.Livestock;
            livestock.Status = livestock_status.ĐÃ_XUẤT;
            _context.Livestocks.Update(livestock);
            _context.SaveChanges();
            batchExportDetail.HandoverDate = DateTime.Now;
            batchExportDetail.Status = batch_export_status.ĐÃ_BÀN_GIAO;
            batchExportDetail.UpdatedBy = updatedBy;
            batchExportDetail.UpdatedAt = DateTime.Now;
            _context.Update(batchExportDetail);
            await _context.SaveChangesAsync();
            if (batchExportDetails.All(x => x.Status == batch_export_status.ĐÃ_BÀN_GIAO))
            {
                BatchExport batchExport = _context.BatchExports.FirstOrDefault(x => x.Id == batchExportDetail.BatchExportId);
                batchExport.Status = batch_export_status.ĐÃ_BÀN_GIAO;
                List<BatchExport> listBatchExports = _context.BatchExports.Where(x => x.ProcurementPackageId == batchExport.ProcurementPackageId).ToList();
                if (listBatchExports.All(x => x.Status == batch_export_status.ĐÃ_BÀN_GIAO))
                {
                    ProcurementPackage procurementPackage = _context.ProcurementPackages.FirstOrDefault(x => x.Id == batchExport.ProcurementPackageId);
                    procurementPackage.Status = procurement_status.HOÀN_THÀNH;
                    _context.Update(procurementPackage);
                    _context.SaveChanges();
                }
                _context.Update(batchExport);
                _context.SaveChanges();
            }
            return true;
        }
    }
}
