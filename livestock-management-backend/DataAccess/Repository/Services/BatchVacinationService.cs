using AutoMapper;
using BusinessObjects;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos;
using BusinessObjects.ConfigModels;
using ClosedXML;
using static BusinessObjects.Constants.LmsConstants;
using Microsoft.AspNetCore.Identity;
using DocumentFormat.OpenXml.Office2010.Excel;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Globalization;

namespace DataAccess.Repository.Services
{
    public class BatchVacinationService : IBatchVacinationRepository
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LmsContext _context;
        public BatchVacinationService(LmsContext context, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<BatchVaccination> CreateBatchVacinationDetail(BatchVacinationCreate batchVacinationCreate)
        {
            var Medicine = _context.Medicines.AsNoTracking().FirstOrDefault(x => x.Id == batchVacinationCreate.VaccineId);
            if (Medicine == null)
            {
                throw new Exception("Medicine không tồn tại");
            }
            BatchVaccination batchVaccination = new BatchVaccination();
            batchVaccination.Id = SlugId.New();
            batchVaccination.ConductedBy = batchVacinationCreate.ConductedBy;
            batchVaccination.DateSchedule = batchVacinationCreate.DateSchedule;
            batchVaccination.Name = batchVacinationCreate.Name;
            batchVaccination.VaccineId = batchVacinationCreate.VaccineId;
            batchVaccination.Description = batchVacinationCreate.Description;
            batchVaccination.Status = batch_vaccination_status.CHỜ_THỰC_HIỆN;
            batchVaccination.CreatedAt = DateTime.Now;
            batchVaccination.UpdatedAt = DateTime.Now;
            batchVaccination.CreatedBy = batchVacinationCreate.CreatedBy;
            batchVaccination.UpdatedBy = batchVacinationCreate.CreatedBy;
            batchVaccination.Type = batchVacinationCreate.Type;
            _context.BatchVaccinations.Add(batchVaccination);
            _context.SaveChanges();
            return batchVaccination;
        }
        public async Task<BatchVaccination> UpdateBatchVacinationAsync(BatchVacinationUpdate batchVacinationUpdate, string batchVaccinationId)
        {
            var Medicine = _context.Medicines.AsNoTracking().FirstOrDefault(x => x.Id == batchVacinationUpdate.VaccineId.Trim());
            if (Medicine == null)
            {
                throw new Exception("Medicine không tồn tại");
            }
            BatchVaccination batchVaccination = _context.BatchVaccinations.FirstOrDefault(x => x.Id == batchVaccinationId.Trim());
            if (batchVaccination == null)
            {
                throw new Exception("batchVacination không tồn tại");
            }
            if (_userManager.Users.FirstOrDefault(x => x.Id == batchVacinationUpdate.ConductedBy) == null)
            {
                throw new Exception("Người thực hiện này không tồn tại");
            }
            batchVaccination.ConductedBy = batchVacinationUpdate.ConductedBy;
            batchVaccination.DateSchedule = batchVacinationUpdate.DateSchedule;
            batchVaccination.Name = batchVacinationUpdate.Name;
            batchVaccination.VaccineId = batchVacinationUpdate.VaccineId;
            batchVaccination.Description = batchVacinationUpdate.Description;
            batchVaccination.UpdatedAt = DateTime.Now;
            if (_userManager.Users.FirstOrDefault(x => x.Id == batchVacinationUpdate.UpdatedBy) == null)
            {
                throw new Exception("Người cập nhật này không tồn tại");
            }
            batchVaccination.UpdatedBy = batchVacinationUpdate.UpdatedBy;
            batchVaccination.Type = batchVacinationUpdate.Type;
            await _context.SaveChangesAsync();
            return batchVaccination;
        }

        public async Task<ListVaccination> GetListVaccinations(ListVaccinationsFliter filter)
        {
            var result = new ListVaccination()
            {
                Total = 0
            };

            var vaccination = await _context.BatchVaccinations
                .Include(v => v.Vaccine)
                    .ThenInclude(x => x.DiseaseMedicines)
                        .ThenInclude(o => o.Disease)
                .ToArrayAsync();
            if (vaccination == null || !vaccination.Any()) return result;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    vaccination = vaccination
                        .Where(v => v.Name.ToUpper().Contains(filter.Keyword.Trim().ToUpper()))
                        .ToArray();
                }
                if (filter.FromDate != null && filter.FromDate != DateTime.MinValue)
                {
                    vaccination = vaccination
                        .Where(v => v.CreatedAt >= filter.FromDate)
                        .ToArray();
                }
                if (filter.ToDate != null && filter.ToDate != DateTime.MinValue)
                {
                    vaccination = vaccination
                       .Where(v => v.CreatedAt <= filter.ToDate)
                       .ToArray();
                }
                if (filter.Status != null)
                {
                    vaccination = vaccination
                        .Where(v => v.Status == filter.Status)
                        .ToArray();
                }
            }
            if (!vaccination.Any()) return result;
            vaccination = vaccination
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();

            result.Items = vaccination
                .Select(v => new VaccinationSummary
                {
                    Id = v.Id,
                    Name = v.Name,
                    MedcicalType = v.Vaccine.Type,
                    Symptom = v.Vaccine?.DiseaseMedicines != null && v.Vaccine.DiseaseMedicines.Any()
                         ? string.Join(", ", v.Vaccine.DiseaseMedicines
                         .Select(dm => dm.Disease.Symptom)
                            .Distinct())
                            : "Không có triệu chứng",
                    DateSchedule = v.DateSchedule,
                    ConductedBy = v.ConductedBy,
                    Status = v.Status,
                    DateConduct = v.DateConduct,
                    CreatedAt = v.CreatedAt
                })
                .OrderByDescending(v => v.CreatedAt)
                .Skip(filter?.Skip ?? 0)
                .Take(filter?.Take ?? 10)
                .ToArray();
            result.Total = vaccination.Length;

            return result;
        }

        public async Task<VaccinationGeneral> GetVaccinationGeneralInfo(string id)
        {
            var vaccination = await _context.BatchVaccinations
                                        .Include(v => v.Vaccine)
                                            .ThenInclude(x => x.DiseaseMedicines)
                                                .ThenInclude(o => o.Disease)
                                                    .FirstOrDefaultAsync(v => v.Id == id)
                                        ?? throw new Exception("Không tìm thấy lô tiêm");
            var user = _context.Users.FirstOrDefault(x => x.Id == vaccination.ConductedBy);

            var result = new VaccinationGeneral
            {
                Id = vaccination.Id,
                Name = vaccination.Name,
                VaccinationType = vaccination.Type,
                MedicineName = vaccination.Vaccine.Name,
                Symptom = vaccination.Vaccine?.DiseaseMedicines != null && vaccination.Vaccine.DiseaseMedicines.Any()
                         ? string.Join(", ", vaccination.Vaccine.DiseaseMedicines
                         .Select(dm => dm.Disease.Symptom)
                            .Distinct())
                            : "Không có triệu chứng",
                DateSchedule = vaccination.DateSchedule,
                Status = vaccination.Status,
                ConductedBy = user?.UserName ?? "SYS",
                DateConduct = vaccination.DateConduct,
                Note = vaccination.Description
            };
            return result;
        }

        public async Task<ListLivestocksVaccination> GetListLivestockVaccination(string id, ListLivestockVaccination filter)
        {
            var result = new ListLivestocksVaccination()
            {
                Total = 0
            };

            var livestocks = await _context.LivestockVaccinations
                .Include(v => v.Livestock)
                    .ThenInclude(o => o.Species)
                .Include(v => v.BatchVaccination)
                .Where(v => v.BatchVaccinationId == id)
                .GroupBy(v => v.LivestockId)
                .Select(g => g.OrderByDescending(v => v.CreatedAt).First())
                .ToArrayAsync();
            if (livestocks == null || !livestocks.Any()) return result;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    livestocks = livestocks
                        .Where(v => v.Livestock.InspectionCode.ToUpper().Contains(filter.Keyword.Trim().ToUpper()))
                        .ToArray();
                }
                if (filter.FromDate != null && filter.FromDate != DateTime.MinValue)
                {
                    livestocks = livestocks
                        .Where(v => v.CreatedAt >= filter.FromDate)
                        .ToArray();
                }
                if (filter.ToDate != null && filter.ToDate != DateTime.MinValue)
                {
                    livestocks = livestocks
                       .Where(v => v.CreatedAt <= filter.ToDate)
                       .ToArray();
                }
                if (filter.Status != null)
                {
                    livestocks = livestocks
                        .Where(v => v.Livestock.Status == filter.Status)
                        .ToArray();
                }
            }
            if (!livestocks.Any()) return result;
            livestocks = livestocks
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();

            var injectionCounts = _context.LivestockVaccinations
                .Where(lv => lv.BatchVaccinationId == id)
                .GroupBy(lv => lv.LivestockId)
                .Select(g => new { LivestockId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.LivestockId, x => x.Count);
           
            result.Items = livestocks
               .Select(v => new LivestockVaccineInfo
               {
                   Id = v.Id,
                   InspectionCode = v.Livestock.InspectionCode,
                   Species = v.Livestock.Species.Name,
                   Color = v.Livestock.Color,
                   Status = v.Livestock.Status,
                   ConductedBy= _userManager.Users.FirstOrDefault(x => x.Id == v.CreatedBy)?.UserName??"SYS",
                   DateConduct = v.BatchVaccination != null ? v.BatchVaccination.DateConduct : null,
                   CreatedAt = v.CreatedAt
               })
               .OrderByDescending(v => v.CreatedAt)
               .Skip(filter?.Skip ?? 0)
               .Take(filter?.Take ?? 10)
               .ToArray();
            result.Total = livestocks.Length;

            return result;
        }

        public async Task<bool> CancelVaccinationBatch(ChangeVaccinationBatchStatus request)
        {
            var data = await _context.BatchVaccinations.FirstOrDefaultAsync(x => x.Id == request.VaccinationBatchId);
            if (data == null) throw new Exception("Không tìm thấy lô tiêm");
            if (data.Status == batch_vaccination_status.HOÀN_THÀNH) return false;
            data.Status = batch_vaccination_status.ĐÃ_HỦY;
            data.UpdatedAt = DateTime.Now;
            data.UpdatedBy = request.RequestedBy;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteVaccinationBatch(ChangeVaccinationBatchStatus request)
        {
            var data = await _context.BatchVaccinations.FirstOrDefaultAsync(x => x.Id == request.VaccinationBatchId);
            if (data == null) throw new Exception("Không tìm thấy lô tiêm");
            if (data.Status == batch_vaccination_status.ĐÃ_HỦY) return false;
            var latestVaccinationDate = await _context.LivestockVaccinations
                .Where(v => v.BatchVaccinationId == request.VaccinationBatchId)
                .OrderByDescending(v => v.UpdatedAt)
                .FirstOrDefaultAsync();
            if (latestVaccinationDate == null) return false;
            data.DateConduct = latestVaccinationDate.CreatedAt;
            data.Status = batch_vaccination_status.HOÀN_THÀNH;
            data.DateConduct = data.UpdatedAt = DateTime.Now;
            data.UpdatedBy = request.RequestedBy;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LivestockVaccinationInfo> GetLivestockInfo(ScanLivestockQrCode request)
        {
            var livestockById = await _context.Livestocks.AsNoTracking()
                .Include(l => l.Species)
                .FirstOrDefaultAsync(x => x.Id == request.LivestockId);
            var livestockByInspectionCode = await _context.Livestocks.AsNoTracking()
                .Include(l => l.Species)
                .FirstOrDefaultAsync(x => x.InspectionCode == request.InspectionCode && x.Species.Type == request.SpecieType);

            var livestock = livestockById ?? livestockByInspectionCode;

            if (livestock == null)
            {
                throw new Exception("Con vật này không tồn tại");
            }

            var livestockVaccinations = await _context.LivestockVaccinations
                .Where(lv => lv.LivestockId == livestock.Id)
                .Include(lv => lv.BatchVaccination)
                    .ThenInclude(bv => bv.Vaccine)
                        .ThenInclude(v => v.DiseaseMedicines)
                            .ThenInclude(dm => dm.Disease)
                .ToListAsync();
            if (livestockVaccinations == null)
            {
                return null;
            }
            var vaccinationInfos = livestockVaccinations
                .Where(lv => lv.BatchVaccination.Status == batch_vaccination_status.HOÀN_THÀNH)
                .SelectMany(lv => lv.BatchVaccination.Vaccine.DiseaseMedicines.Select(dm => dm.Disease))
                .GroupBy(d => d.Id)
                .Select(g => new VaccinationInfo
                {
                    DiseaseName = g.First().Name,
                    NumberOfVaccination = g.Count()
                })
                .ToList();

            return new LivestockVaccinationInfo
            {
                LivestockId = livestock.Id,
                InspectionCode = livestock.InspectionCode,
                SpecieName = livestock.Species.Name,
                Color = livestock.Color ?? "N/A",
                VaccinationInfos = vaccinationInfos
            };
        }




        public async Task<bool> AddToVaccinationBatch(AddLivestockToVaccinationBatch request)
        {
            return false;
        }

        public async Task<LivestockVaccination> AddLivestockVacinationToVacinationBatch(LivestockVaccinationAdd livestockVaccinationAdd)
        {
            var batchVacination = await _context.BatchVaccinations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == livestockVaccinationAdd.BatchVaccinationId);
            if (batchVacination == null)
            {
                throw new Exception("Lô tiêm vắc xin không tồn tại");
            }
            var livestock = await _context.Livestocks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == livestockVaccinationAdd.LivestockId);
            if (livestock == null)
            {
                throw new Exception("Con vật không tồn tại");
            }
            List<LivestockVaccination> existLivestock = _context.LivestockVaccinations.Where(x => x.LivestockId == livestockVaccinationAdd.LivestockId).ToList();
            if (existLivestock.Any())
            {
                throw new Exception("Vật nuôi đã thuộc 1 lô tiêm khác");
            }
            _context.BatchVaccinations
    .Where(x => x.Id == livestockVaccinationAdd.BatchVaccinationId)
    .ExecuteUpdateAsync(x => x.SetProperty(b => b.Status, batch_vaccination_status.ĐANG_THỰC_HIỆN));
            LivestockVaccination livestockVaccination = new LivestockVaccination();
            livestockVaccination.Id = SlugId.New();
            livestockVaccination.BatchVaccinationId = livestockVaccinationAdd.BatchVaccinationId;
            livestockVaccination.LivestockId = livestockVaccinationAdd.LivestockId;
            livestockVaccination.UpdatedAt = DateTime.Now;
            livestockVaccination.CreatedBy = livestockVaccinationAdd.CreatedBy;
            livestockVaccination.UpdatedBy = livestockVaccinationAdd.CreatedBy;
            livestockVaccination.CreatedAt = DateTime.Now;
            await _context.AddAsync(livestockVaccination);
            await _context.SaveChangesAsync();
            return livestockVaccination;
        }

        public async Task<LivestockVaccineInfoById> GetLivestockVaccinationByID(string livestockId)
        {
            var livestock = await _context.Livestocks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == livestockId);
            if (livestock == null)
            {
                throw new Exception("Không tìm thấy vật nuôi này");
            }
            var livestockVaccinations = await _context.LivestockVaccinations
                .Where(lv => lv.LivestockId == livestockId)
                .Include(lv => lv.Livestock)
                    .ThenInclude(l => l.Species)
                .Include(lv => lv.BatchVaccination)
                    .ThenInclude(bv => bv.Vaccine)
                        .ThenInclude(v => v.DiseaseMedicines)
                            .ThenInclude(dm => dm.Disease)
                .ToListAsync();

            if (!livestockVaccinations.Any())
            {
                return null;
            }


            var injectionsCountByBatch = livestockVaccinations
                .Where(lv => lv.BatchVaccination.Status == batch_vaccination_status.HOÀN_THÀNH)
                .GroupBy(lv => lv.BatchVaccinationId)
                .Select(g => new
                {
                    BatchId = g.Key,
                    Count = g.Count(),
                    Batch = g.First().BatchVaccination
                })
                .ToList();

            var injectionsDetails = injectionsCountByBatch.Select(batch => new LivestockViccineInfoByIdDetail
            {
                Injections_count = batch.Count,
                DiseaseName = batch.Batch.Vaccine?.DiseaseMedicines != null && batch.Batch.Vaccine.DiseaseMedicines.Any()
                    ? string.Join(", ", batch.Batch.Vaccine.DiseaseMedicines.Select(dm => dm.Disease.Name).Distinct())
                    : "Không có bệnh",
                Name = batch.Batch.Name,
                ConductedBy = batch.Batch.ConductedBy,
                Description = batch.Batch.Description
            }).ToList();

            var firstLivestockVaccination = livestockVaccinations.First();

            var livestockVaccineInfo = new LivestockVaccineInfoById
            {
                Id = firstLivestockVaccination.Id,
                InspectionCode = firstLivestockVaccination.Livestock.InspectionCode,
                Species = firstLivestockVaccination.Livestock.Species.Name,
                Color = firstLivestockVaccination.Livestock.Color,
                Status = firstLivestockVaccination.Livestock.Status,
                Injections_count = injectionsDetails,
                DateConduct = firstLivestockVaccination.BatchVaccination?.DateConduct,
                CreatedAt = firstLivestockVaccination.Livestock.CreatedAt
            };

            return livestockVaccineInfo;
        }

        public async Task<List<UserDTO>> GetStaffAndManagerUserAsync(DateTime dateSchedule)
        {

            var busyUserIds = await _context.BatchVaccinations
                .Where(x => x.DateSchedule == dateSchedule)
                .Select(x => x.ConductedBy)
            .ToListAsync();

            var managers = await _userManager.GetUsersInRoleAsync("Quản trại");
            var staffs = await _userManager.GetUsersInRoleAsync("Nhân viên trại");

            var allUsers = managers;

            if (!allUsers.Any())
            {
                return new List<UserDTO>();
            }

            var userProfiles = new List<UserDTO>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userProfiles.Add(new UserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList()
                });
            }

            return userProfiles;
        }

        public async Task<bool> DeleteLiveStockVaccination(string id)
        {
            var livestockVaccinationModels = await _context.LivestockVaccinations.FirstOrDefaultAsync(x => x.Id == id.Trim());
            if (livestockVaccinationModels == null) throw new Exception("Không tìm thấy vật nuôi trong danh sách lô tiêm");
            var checkBatchStatus = await _context.BatchVaccinations.FirstOrDefaultAsync(x => x.Id == livestockVaccinationModels.BatchVaccinationId);
            if (checkBatchStatus.Status == batch_vaccination_status.HOÀN_THÀNH || (checkBatchStatus.Status == batch_vaccination_status.ĐÃ_HỦY))
                throw new Exception("Không thể xóa vật nuôi trong lô xuất đã hoàn thành hoặc đã hủy");
            _context.LivestockVaccinations.Remove(livestockVaccinationModels);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Stream> ExportTemplateVaccinationBatch()
        {
            // Get species list to use for reference in example data
            var speciesList = await _context.Species
                .Select(s => s.Name)
                .ToListAsync();

            // Get medicines list to use for the reference in example data
            var medicinesList = await _context.Medicines
                .Select(m => m.Name)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Nhập liệu");

                // Add header and instructions
                worksheet.Cell("A1").Value = "TEMPLATE NHẬP DỮ LIỆU VẬT NUÔI CHO LÔ TIÊM";
                worksheet.Range("A1:D1").Merge();
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 14;
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell("A3").Value = "Hướng dẫn:";
                worksheet.Cell("A3").Style.Font.Bold = true;
                worksheet.Cell("A4").Value = "- Nhập dữ liệu vào các cột theo mẫu";
                worksheet.Cell("A5").Value = "- Ngày: Ghi đúng định dạng yêu cầu: dd/MM/yyyy (VD: 31/12/2000)";
                worksheet.Cell("A6").Value = "- Loài: Chọn từ danh sách thả xuống";
                worksheet.Cell("A7").Value = "- Tên thuốc: Chọn từ danh sách thả xuống";

                // Create a hidden sheet for the species list
                var hiddenSheetLoai = workbook.Worksheets.Add("DanhSachLoai");
                hiddenSheetLoai.Visibility = XLWorksheetVisibility.VeryHidden;

                for (var i = 0; i < speciesList.Count; i++)
                {
                    hiddenSheetLoai.Cell($"A{i + 1}").Value = speciesList[i];
                }

                var speciesRange = hiddenSheetLoai.Range($"A1:A{speciesList.Count}");
                workbook.NamedRanges.Add("DanhSachLoai", speciesRange);

                // Create a hidden sheet for the medicine list
                var hiddenSheetThuoc = workbook.Worksheets.Add("DanhSachThuoc");
                hiddenSheetThuoc.Visibility = XLWorksheetVisibility.VeryHidden;

                for (var i = 0; i < medicinesList.Count; i++)
                {
                    hiddenSheetThuoc.Cell($"A{i + 1}").Value = medicinesList[i];
                }

                var medicinesRange = hiddenSheetThuoc.Range($"A1:A{medicinesList.Count}");
                workbook.NamedRanges.Add("DanhSachThuoc", medicinesRange);

                // Define the starting row for the data table
                var startRow = 10;

                // Column headers
                worksheet.Cell($"A{startRow}").Value = "Mã kiểm dịch";
                worksheet.Cell($"B{startRow}").Value = "Loài";
                worksheet.Cell($"C{startRow}").Value = "Tên thuốc";
                worksheet.Cell($"D{startRow}").Value = "Ngày ghi nhận";

                var headerRange = worksheet.Range($"A{startRow}:D{startRow}");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                //Set Inspection Code is Text
                var inspectionCodeColumn = worksheet.Range($"A{startRow + 1}:A{startRow + 100}");
                inspectionCodeColumn.Style.NumberFormat.Format = "@";

                // Set drop-down list for "Species" column
                var speciesColumn = worksheet.Range($"B{startRow + 1}:B{startRow + 100}");
                speciesColumn.SetDataValidation().List("=DanhSachLoai", true);

                // Set drop-down list for "Medicine" column
                var medicineColumn = worksheet.Range($"C{startRow + 1}:C{startRow + 100}");
                medicineColumn.SetDataValidation().List("=DanhSachThuoc", true);

                //// Set date format and validation for "Recorded Date" column**
                //var dateColumn = worksheet.Range($"D{startRow + 1}:D{startRow + 100}");
                //dateColumn.Style.DateFormat.Format = "dd/MM/yyyy"; // Định dạng ngày tháng
                //dateColumn.SetDataValidation().Date.Between(DateTime.MinValue, DateTime.MaxValue); // Giới hạn ngày hợp lệ
                //dateColumn.SetDataValidation().ShowInputMessage = true; // Hiển thị thông báo khi nhấp vào ô
                //dateColumn.SetDataValidation().InputTitle = "Nhập ngày";
                //dateColumn.SetDataValidation().InputMessage = "Vui lòng nhập ngày theo định dạng dd/MM/yyyy (VD: 31/12/2000)";

                // Increase column width**
                worksheet.Column("B").Width = 25; // "Species" column
                worksheet.Column("C").Width = 25; // "Medicine Name" column
                worksheet.Column("D").Width = 20; // "Recorded Date" column (increase slightly for date format)

                // Automatically adjust other columns
                worksheet.Columns("A").AdjustToContents();

                // Save the Excel file into a MemoryStrea
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return stream;
            }
        }

        public async Task<bool> ImportListLivestock(string batchVaccinId, string requestedBy, IFormFile file)
        {
            var batchaVaccin = await _context.BatchVaccinations.FindAsync(batchVaccinId) ??
                throw new Exception("Không tìm thấy lô tiêm");

            ////remove existing list livetocks
            //var currentListInports = await _context.BatchImportDetails
            //    .Where(x => x.BatchImportId == batchImportId).ToArrayAsync();
            //if (currentListInports.Any() || currentListInports != null)
            //{
            //    foreach (var detail in currentListInports)
            //    {
            //        if (detail.Status != batch_import_status.ĐÃ_NHẬP)
            //            _context.BatchImportDetails.RemoveRange(detail);
            //    }
            //}

            var listLivestock = new List<LivestockVaccination>();
            var listMedicalHistories = new List<MedicalHistory>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (file == null || file.Length <= 0)
                throw new Exception("Vui lòng tải lên file Excel hợp lệ.");
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    //validate column headers list livestock
                    var requiredColumns = new string[] {
                        "mã kiểm dịch",
                        "loài",
                        "tên thuốc",
                        "ngày ghi nhận",
                    };
                    var dicColumnIndexes = requiredColumns
                        .Select(o => new
                        {
                            o,
                            index = (int)0
                        })
                        .ToDictionary(o => o.o, o => o.index);
                    for (var i = 1; i <= colCount; i++)
                    {
                        var columnHeader = worksheet.Cells[10, i].Value?.ToString();
                        if (string.IsNullOrEmpty(columnHeader))
                            throw new Exception($"Trống tên cột ở ô [10:{i}]");
                        columnHeader = columnHeader.Trim().ToLower();
                        if (!dicColumnIndexes.ContainsKey(columnHeader))
                            throw new Exception($"Tên cột không hợp lệ ở ô [10:{i}]");
                        dicColumnIndexes[columnHeader] = i;
                    }

                    //read value
                    var idxInspectionCode = dicColumnIndexes["mã kiểm dịch"];
                    var idxSpecies = dicColumnIndexes["loài"];
                    var idxMedicineName = dicColumnIndexes["tên thuốc"];
                    var idxCundutedDate = dicColumnIndexes["ngày ghi nhận"];

                    //Count the eaxct row
                    rowCount = 0;
                    for (int row = 11; row <= worksheet.Dimension.Rows; row++)
                    {
                        var inspectionCode = worksheet.Cells[row, idxInspectionCode].Value?.ToString();
                        var species = worksheet.Cells[row, idxSpecies].Value?.ToString();
                        var medicine = worksheet.Cells[row, idxMedicineName].Value?.ToString();
                        var date = worksheet.Cells[row, idxCundutedDate].Value?.ToString();

                        if (string.IsNullOrWhiteSpace(inspectionCode) &&
                            string.IsNullOrWhiteSpace(species) &&
                            string.IsNullOrWhiteSpace(medicine) &&
                            string.IsNullOrWhiteSpace(date))
                        {
                            break;
                        }
                        rowCount++;
                    }

                    for (int row = 11; row < 11 + rowCount; row++)
                    {
                        var inspectionCode = worksheet.Cells[row, idxInspectionCode].Value?.ToString();
                        if (string.IsNullOrWhiteSpace(inspectionCode))
                            throw new Exception($"Trống mã kiểm dịch của vật tiêm ở ô [{row}:{idxInspectionCode}]");

                        var species = worksheet.Cells[row, idxSpecies].Value?.ToString();
                        if (string.IsNullOrEmpty(species))
                            throw new Exception($"Trống loài của vật tiêm ở ô [{row}:{idxSpecies}]");

                        var livestockCheck = await _context.Livestocks
                            .Include(x => x.Species)
                            .FirstOrDefaultAsync(l => l.InspectionCode.ToLower() == inspectionCode.Trim().ToLower()
                                                   && l.Species.Name.ToLower() == species.Trim().ToLower());
                        if (livestockCheck == null)
                            throw new Exception($"Không tìm thấy vật nuôi với mã kiểm dịch '{inspectionCode}' và loài '{species}' trong hệ thống.");

                        var medicine = worksheet.Cells[row, idxMedicineName].Value?.ToString();
                        if (string.IsNullOrEmpty(medicine))
                            throw new Exception($"Trống ngày tiêm của vật tiêm ở ô [{row}:{idxMedicineName}]");

                        var medicineCheck = await _context.Medicines
                                .Include(m => m.DiseaseMedicines)
                                    .ThenInclude(d => d.Disease)
                            .FirstOrDefaultAsync(m => m.Name.ToLower() == medicine.Trim().ToLower());

                        var date = worksheet.Cells[row, idxCundutedDate].Value?.ToString();
                        if (string.IsNullOrEmpty(date))
                            throw new Exception($"Trống ngày tiêm của vật tiêm ở ô [{row}:{idxCundutedDate}]");
                        string cleanedDate = date.Trim().Split(' ')[0];

                        if (!DateTime.TryParseExact(cleanedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime conductedDate))
                            throw new Exception($"Ngày tiêm '{date}' không hợp lệ ở ô [{row}:{idxCundutedDate}]. Định dạng yêu cầu: dd/MM/yyyy");


                        listLivestock.Add(new LivestockVaccination
                        {
                            Id = SlugId.New(),
                            BatchVaccinationId = batchaVaccin.Id,
                            LivestockId = livestockCheck.Id,
                            CreatedAt = DateTime.Now,
                            CreatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy
                        });

                        //listMedicalHistories.Add(new MedicalHistory
                        //{
                        //    Id = SlugId.New(),
                        //    LivestockId = livestockCheck.Id,
                        //    Status = medical_histories_status.ĐANG_ĐIỀU_TRỊ.ToString(),
                        //    MedicineId = medicineCheck.Id, 
                        //    DiseaseId = medicineCheck.MedicalHistories.First().Disease.Id,
                        //    CreatedAt = DateTime.Now,
                        //    CreatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy,
                        //    UpdatedAt = DateTime.Now,
                        //    UpdatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy
                        //});
                    }
                }
            }

            await _context.MedicalHistories.AddRangeAsync(listMedicalHistories);
            await _context.LivestockVaccinations.AddRangeAsync(listLivestock);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ListRequireVaccinationProcurement>> GetListProcurementRequireVaccination(
     string? procurementSearch,
    OrderBy? orderBy,
     DateTime? fromDate,
     DateTime? toDate)
        {
            List<BatchExport> listBatchExports = _context.BatchExports.Where(x => x.Status == batch_export_status.CHỜ_BÀN_GIAO).ToList();
            List<string> procurementIds = listBatchExports.Select(x => x.ProcurementPackageId).Distinct().ToList();
            List<ProcurementPackage> listProcurementPackages = _context.ProcurementPackages
              .Where(x => procurementIds.Contains(x.Id) &&
                  (string.IsNullOrEmpty(procurementSearch) ||
                   x.Name.Contains(procurementSearch) ||
                   x.Code.Contains(procurementSearch)))
              .ToList();
            List<ListRequireVaccinationProcurement> listRequireVaccinationProcurements = new List<ListRequireVaccinationProcurement>();
            foreach (ProcurementPackage procurementPackage in listProcurementPackages)
            {
                BatchExport batchExports = listBatchExports.FirstOrDefault(x => x.ProcurementPackageId == procurementPackage.Id);
                List<BatchExportDetail> listBatchExportDetails = _context.BatchExportDetails.Where(x => x.BatchExportId == batchExports.Id).ToList();
                List<string> livestockIds = listBatchExportDetails.Select(x => x.LivestockId).Distinct().ToList();
                List<Livestock> listLivestocks = _context.Livestocks.Where(x => livestockIds.Contains(x.Id)).ToList();
                List<LivestockVaccination> livestockVaccinations = _context.LivestockVaccinations.Where(x => livestockIds.Contains(x.LivestockId))
                    .Include(x => x.BatchVaccination).ThenInclude(x => x.Vaccine).ThenInclude(x => x.DiseaseMedicines).ToList();
                List<ProcurementDetail> listProcurementDetail = _context.ProcurementDetails.Where(x => x.ProcurementPackageId == procurementPackage.Id).ToList();
                List<string> procurementDetailId = listProcurementDetail.Select(x => x.Id).Distinct().ToList();
                List<VaccinationRequirement> listVaccinationRequirements = _context.VaccinationRequirement.Include(x => x.Disease).Where(x => procurementDetailId.Contains(x.ProcurementDetailId)).ToList();
                List<DiseaseRequire> listDiseaseRequire = new List<DiseaseRequire>();
                var distinctRequirements = listVaccinationRequirements
                .GroupBy(v => v.Disease.Id)
                .Select(g => g.First())
                .ToList();
                foreach (VaccinationRequirement vaccinationRequirement in distinctRequirements)
                {
                    List<SingleVaccination> singleVaccinationList = _context.SingleVaccination.Include(x => x.Medicine).ThenInclude(x => x.DiseaseMedicines).ThenInclude(x => x.Disease)
                        .Where(x => x.Medicine.DiseaseMedicines.FirstOrDefault().Disease.Id == vaccinationRequirement.Disease.Id&& x.CreatedAt > DateTime.Now.AddDays(-21)).ToList();

                    var diseaseVaccinations = livestockVaccinations
.Where(x => x.BatchVaccination.Vaccine.DiseaseMedicines.FirstOrDefault()?.DiseaseId == vaccinationRequirement.Disease.Id);

                    var recentDiseaseVaccinations = diseaseVaccinations
                        .Where(x => x.CreatedAt > DateTime.Now.AddDays(-21));

                    var count = recentDiseaseVaccinations
                        .Select(x => x.LivestockId)
                        .Distinct()
                        .Count();

     //               var count = recentDiseaseVaccinations
     //.Select(x => x.LivestockId)
     //.Concat(singleVaccinationList.Select(x => x.LivestockId))
     //.Distinct()
     //.Count();
                    DiseaseRequire diseaseRequire = new DiseaseRequire
                    {
                        HasDone = count,
                        DiseaseName = vaccinationRequirement.Disease.Name,
                    };
                    listDiseaseRequire.Add(diseaseRequire);
                }
                var expirationDate = procurementPackage.ExpirationDate;
                if (fromDate.HasValue && expirationDate < fromDate.Value)
                    continue;
                if (toDate.HasValue && expirationDate > toDate.Value)
                    continue;
                ListRequireVaccinationProcurement requireVaccinationProcurement = new ListRequireVaccinationProcurement
                {
                    ProcurementId = procurementPackage.Id,
                    ProcurementName = procurementPackage.Name,
                    ProcurementCode = procurementPackage.Code,
                    ExpirationDate = expirationDate,
                    LivestockQuantity = listBatchExportDetails.Count(),
                    diseaseRequires = listDiseaseRequire,
                };

                listRequireVaccinationProcurements.Add(requireVaccinationProcurement);
            }
            switch (orderBy)
            {
                case OrderBy.HẠN_HOÀN_THÀNH_TĂNG_DẦN:
                    listRequireVaccinationProcurements = listRequireVaccinationProcurements
                        .OrderBy(x => x.ExpirationDate)
                        .ToList();
                    break;

                case OrderBy.HẠN_HOÀN_THÀNH_GIẢM_DẦN:
                    listRequireVaccinationProcurements = listRequireVaccinationProcurements
                        .OrderByDescending(x => x.ExpirationDate)
                        .ToList();
                    break;

                case OrderBy.SỐ_LƯỢNG_CÒN_THIẾU_TĂNG_DẦN:
                    listRequireVaccinationProcurements = listRequireVaccinationProcurements
                        .OrderBy(x => x.LivestockQuantity - x.diseaseRequires.Sum(d => d.HasDone))
                        .ToList();
                    break;

                case OrderBy.SỐ_LƯỢNG_CÒN_THIẾU_GIẢM_DẦN:
                    listRequireVaccinationProcurements = listRequireVaccinationProcurements
                        .OrderByDescending(x => x.LivestockQuantity - x.diseaseRequires.Sum(d => d.HasDone))
                        .ToList();
                    break;

                default:
                    break;
            }
            return listRequireVaccinationProcurements;
        }

        public async Task<List<ListSuggestReVaccination>> GetListSuggestReVaccination(
             string? batchSearch,
             string? medicineId,
             string? diseaseId,
             DateTime? fromDate,
             DateTime? toDate)
        {
            var query = _context.BatchVaccinations
                .Where(x => x.DateConduct < DateTime.Now.AddDays(-21))
                .Include(x => x.LivestockVaccinations)
                .Include(x => x.Vaccine)
                    .ThenInclude(v => v.DiseaseMedicines)
                        .ThenInclude(dm => dm.Disease)
                .AsQueryable();
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.DateConduct >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.DateConduct <= toDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(medicineId))
            {
                query = query.Where(x => x.VaccineId == medicineId);
            }

            if (!string.IsNullOrWhiteSpace(diseaseId))
            {
                query = query.Where(x => x.Vaccine.DiseaseMedicines.Any(dm => dm.DiseaseId == diseaseId));
            }

            if (!string.IsNullOrWhiteSpace(batchSearch))
            {
                batchSearch = batchSearch.Trim().ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(batchSearch));
            }
            var batchVaccinations = await query.ToListAsync();
            var listSuggestReVaccinations = batchVaccinations.Select(batch => new ListSuggestReVaccination
            {
                BatchVaccinationId = batch.Id,
                BatchVaccinationName = batch.Name,
                LivestockQuantity = batch.LivestockVaccinations.Count(),
                MedicineName = batch.Vaccine.Name,
                DiseasaName = batch.Vaccine.DiseaseMedicines.FirstOrDefault()?.Disease?.Name,
                LastDate = batch.DateConduct
            }).ToList();

            return listSuggestReVaccinations;
        }


        public async Task<List<ListFutureBatchVaccination>> GetListFutureVaccination(string? search, string? diseaeId, string? conductId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.BatchVaccinations
             .Where(x => x.DateSchedule > DateTime.Now)
             .Include(x => x.LivestockVaccinations)
             .Include(x => x.Vaccine)
                 .ThenInclude(v => v.DiseaseMedicines)
                     .ThenInclude(dm => dm.Disease)
             .AsQueryable();
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.DateConduct >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.DateConduct <= toDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(diseaeId))
            {
                query = query.Where(x => x.Vaccine.DiseaseMedicines.Any(dm => dm.DiseaseId == diseaeId));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search));
            }
            var batchVaccinations = await query.ToListAsync();
            var listFutureBatchVaccinations = batchVaccinations.Select(batch => new ListFutureBatchVaccination
            {
                BatchVaccinationId = batch.Id,
                BatchVaccinationName = batch.Name,
                ConductName = batch.ConductedBy,
                MedicineName = batch.Vaccine.Name,
                SchedulteTime = batch.DateSchedule,
                DiseaseName = batch.Vaccine.DiseaseMedicines.FirstOrDefault()?.Disease?.Name,

            }).ToList();

            return listFutureBatchVaccinations;
        }

        public async Task<RequireVaccinationProcurementDetail> GetProcurementRequireVaccinationDetail(string procurementId)
        {

            var package1 = _context.ProcurementPackages
                            .Include(x => x.ProcurementDetails)
                                .ThenInclude(pd => pd.VaccinationRequirements)
                                    .ThenInclude(vr => vr.Disease)
                            .FirstOrDefault(x => x.Id == procurementId);

            List<Disease> listDiseases = new List<Disease>();
            if (package1 != null)
            {
                listDiseases = package1.ProcurementDetails
                    .SelectMany(pd => pd.VaccinationRequirements ?? new List<VaccinationRequirement>())
                    .Select(vr => vr.Disease)
                    .Where(d => d != null)
                    .Distinct()
                    .ToList();
            }
            List<DiseaseRequireForSpecie> listDiseaseRequiresForSpecie = new List<DiseaseRequireForSpecie>();
            foreach (Disease disease in listDiseases)
            {
                var package2 = _context.ProcurementPackages
                                .Include(x => x.ProcurementDetails)
                                    .ThenInclude(pd => pd.Species)
                                .FirstOrDefault(x => x.Id == procurementId);

                List<Species> listSpecies = new List<Species>();
                if (package2 != null)
                {
                    listSpecies = package2.ProcurementDetails
                        .Select(pd => pd.Species)
                        .Where(s => s != null)
                        .Distinct()
                        .ToList();
                }

                foreach (Species species in listSpecies)
                {
                    var procurementDetail = _context.ProcurementDetails
                                     .Include(x => x.BatchVaccinationProcurement)
                                         .ThenInclude(bvp => bvp.BatchVaccination)
                                             .ThenInclude(bv => bv.Vaccine)
                                                 .ThenInclude(v => v.MedicalHistories)
                                                     .ThenInclude(mh => mh.Disease)
                                     .Include(x => x.Species)
                                     .Include(x=>x.VaccinationRequirements)
                                     .FirstOrDefault(x => x.ProcurementPackageId == procurementId && x.Species.Id == species.Id);


                    if (procurementDetail != null)
                    {
                        
                        var bvpDetail = _context.BatchVaccinationProcurement
                        .Include(x => x.BatchVaccination)
                            .ThenInclude(bv => bv.Vaccine)
                                .ThenInclude(v => v.DiseaseMedicines)
                        .Include(x => x.BatchVaccination)
                            .ThenInclude(bv => bv.LivestockVaccinations)
                        .FirstOrDefault(x =>
                            x.ProcurementDetailId == procurementDetail.Id &&
                            x.BatchVaccination.Vaccine.DiseaseMedicines.Any(dm => dm.DiseaseId == disease.Id)
                        );

                        BatchVaccination batchVaccination = bvpDetail?.BatchVaccination;


                        List<Livestock> listLivestocks = _context.Livestocks
                                                         .Include(x => x.BatchExportDetails)
                                                             .ThenInclude(bed => bed.BatchExport)
                                                                 .ThenInclude(be => be.ProcurementPackage)
                                                         .Where(x => x.Species.Id == species.Id &&
                                                                     x.BatchExportDetails.Any(bed => bed.BatchExport.ProcurementPackage.Id == procurementId))
                                                         .ToList();

                        List<Livestock> listLivestockVaccinated = _context.Livestocks
                                                                .Include(x => x.LivestockVaccinations)
                                                                    .ThenInclude(lv => lv.BatchVaccination)
                                                                        .ThenInclude(bv => bv.Vaccine)
                                                                            .ThenInclude(v => v.DiseaseMedicines)
                                                                .Include(x => x.BatchExportDetails)
                                                                    .ThenInclude(bed => bed.BatchExport)
                                                                        .ThenInclude(be => be.ProcurementPackage)
                                                                .Where(x =>
                                                                    x.LivestockVaccinations.Any(lv =>
                                                                        lv.CreatedAt >= DateTime.Now.AddDays(-21) &&
                                                                        lv.BatchVaccination.Vaccine.DiseaseMedicines.Any(dm => dm.DiseaseId == disease.Id)
                                                                    )
                                                                    && x.Species.Id == species.Id
                                                                    && x.BatchExportDetails.Any(bed => bed.BatchExport.ProcurementPackage.Id == procurementId)
                                                                )
                                                                .ToList();

                        IsCreated isCreated = IsCreated.CHƯA_TẠO;
                        if (batchVaccination != null)
                        {
                            isCreated = IsCreated.ĐÃ_TẠO;
                        }
                        if (listLivestocks.Count() == listLivestockVaccinated.Count())
                        {
                            isCreated = IsCreated.ĐÃ_HOÀN_THÀNH;
                        }
                        DiseaseRequireForSpecie diseaseRequireForSpecie = new DiseaseRequireForSpecie
                        {
                            DiseaseName = disease.Name,
                            MedicineName = batchVaccination?.Vaccine?.Name ?? "N/A",
                            BatchVaccinationId = batchVaccination?.Id ?? "N/A",
                            SpecieName = procurementDetail.Species?.Name ?? "N/A",
                            isCreated = isCreated,
                            HasDone = listLivestockVaccinated.Count(),
                            TotalQuantity= listLivestocks.Count()
                        };
                        if (procurementDetail.VaccinationRequirements.Any(x => x.Disease.Id == disease.Id))
                        {
                            listDiseaseRequiresForSpecie.Add(diseaseRequireForSpecie);
                        }
                    }

                 
                }
            }
            BatchExport batchExports = _context.BatchExports.Include(x=>x.BatchExportDetails).FirstOrDefault(x => x.ProcurementPackageId == procurementId);
            ProcurementPackage procurementPackage = _context.ProcurementPackages.FirstOrDefault(x => x.Id == procurementId);
            if (procurementPackage == null)
            {
                throw new Exception("Không tìm thấy thông tin gói thầu này");
            }
            var requireVaccinationProcurementDetail = new RequireVaccinationProcurementDetail
            {
                ProcurementId = procurementPackage.Id,
                ProcurementName = procurementPackage.Name,
                ProcurementCode = procurementPackage.Code,
                ExpirationDate = procurementPackage.ExpirationDate,
                LivestockQuantity = batchExports?.BatchExportDetails.Count()??0,
                diseaseRequiresForSpecie = listDiseaseRequiresForSpecie,
            };
            return requireVaccinationProcurementDetail;
        }

        public async Task<LivestockRequireVaccinationProcurement> GetLivestockRequirementForProcurement(string livestockId, string procurementId)
        {
            Livestock livestock = _context.Livestocks.Include(x => x.Species).FirstOrDefault(x => x.Id == livestockId);
            if(livestock == null)
            {
                throw new Exception("Không tìm thấy vật nuôi này");
            }
            var filteredBatchVaccinations = _context.BatchVaccinations
                .Include(x => x.Vaccine)
                    .ThenInclude(v => v.DiseaseMedicines)
                        .ThenInclude(dm => dm.Disease)
                .Include(x => x.BatchVaccinationProcurement)
                    .ThenInclude(bvpd => bvpd.ProcurementDetail)
                .Where(x => x.BatchVaccinationProcurement
                    .Any(bvpd => bvpd.ProcurementDetail.ProcurementPackageId == procurementId))
                .ToList();
            var listDisease = filteredBatchVaccinations
                .SelectMany(b => b.Vaccine.DiseaseMedicines)
                .Select(dm => dm.Disease)
                .Where(d => d != null)
                .Distinct()
                .ToList();

            List<Disease> listVaccinatedDisease = _context.LivestockVaccinations
         .Where(x => x.LivestockId == livestock.Id && x.CreatedAt >= DateTime.Now.AddDays(-21))
         .Select(x => x.BatchVaccination.Vaccine.DiseaseMedicines.FirstOrDefault().Disease)
         .ToList();

            List<Disease> listUnvaccinatedDiseases = listDisease
            .Where(d => !listVaccinatedDisease.Any(vd => vd.Id == d.Id))
            .ToList();
            var unvaccinatedDiseaseIds = listUnvaccinatedDiseases.Select(d => d.Id).ToList();

            var listBatchVaccinations = _context.BatchVaccinations
                .Include(x => x.Vaccine)
                    .ThenInclude(v => v.DiseaseMedicines)
                        .ThenInclude(dm => dm.Disease)
                .Include(x => x.BatchVaccinationProcurement)
                    .ThenInclude(bvpd => bvpd.ProcurementDetail)
                .Where(x =>
                    x.BatchVaccinationProcurement.Any(bvpd =>
                        bvpd.ProcurementDetail.ProcurementPackageId == procurementId
                    ) &&
                    x.Vaccine.DiseaseMedicines.Any(dm =>
                        unvaccinatedDiseaseIds.Contains(dm.DiseaseId)
                    )
                )
                .ToList();

            List<LivestockRequireDisease> listLivestockRequireDiseases = new List<LivestockRequireDisease>();
            foreach (BatchVaccination batchVaccination in listBatchVaccinations)
            {
                LivestockRequireDisease livestockRequireDisease = new LivestockRequireDisease
                {
                    DiseaseName = batchVaccination.Vaccine.DiseaseMedicines.FirstOrDefault().Disease.Name,
                    MedicineName = batchVaccination.Vaccine.Name,
                    BatchVaccinationId = batchVaccination.Id
                };
                listLivestockRequireDiseases.Add(livestockRequireDisease);
            }
            LivestockRequireVaccinationProcurement result = new LivestockRequireVaccinationProcurement
            {
                LivestockId = livestock.Id,
                InspectionCode = livestock.InspectionCode,
                SpecieName = livestock.Species.Name,
                Color = livestock.Color,
                livestockRequireDisease = listLivestockRequireDiseases
            };
            return result;
        }
        public async Task<LivestockRequireVaccinationProcurement> GetLivestockRequirementForProcurement(string inspectionCode, specie_type specieType, string procurementId)
        {
            Livestock livestock = _context.Livestocks.Include(x => x.Species).FirstOrDefault(x => x.InspectionCode == inspectionCode && x.Species.Type == specieType);
            if (livestock == null)
            {
                throw new Exception("Không tìm thấy vật nuôi này");
            }
            var filteredBatchVaccinations = _context.BatchVaccinations
                .Include(x => x.Vaccine)
                    .ThenInclude(v => v.DiseaseMedicines)
                        .ThenInclude(dm => dm.Disease)
                .Include(x => x.BatchVaccinationProcurement)
                    .ThenInclude(bvpd => bvpd.ProcurementDetail)
                .Where(x => x.BatchVaccinationProcurement
                    .Any(bvpd => bvpd.ProcurementDetail.ProcurementPackageId == procurementId))
                .ToList();
            var listDisease = filteredBatchVaccinations
                .SelectMany(b => b.Vaccine.DiseaseMedicines)
                .Select(dm => dm.Disease)
                .Where(d => d != null)
                .Distinct()
                .ToList();
            List<Disease> listVaccinatedDisease = _context.LivestockVaccinations
     .Where(x => x.LivestockId == livestock.Id && x.CreatedAt >= DateTime.Now.AddDays(-21))
     .Select(x => x.BatchVaccination.Vaccine.DiseaseMedicines.FirstOrDefault().Disease)
     .ToList();

            List<Disease> listUnvaccinatedDiseases = listDisease
            .Where(d => !listVaccinatedDisease.Any(vd => vd.Id == d.Id))
            .ToList();
            var unvaccinatedDiseaseIds = listUnvaccinatedDiseases.Select(d => d.Id).ToList();

            var listBatchVaccinations = _context.BatchVaccinations
                .Include(x => x.Vaccine)
                    .ThenInclude(v => v.DiseaseMedicines)
                        .ThenInclude(dm => dm.Disease)
                .Include(x => x.BatchVaccinationProcurement)
                    .ThenInclude(bvpd => bvpd.ProcurementDetail)
                .Where(x =>
                    x.BatchVaccinationProcurement.Any(bvpd =>
                        bvpd.ProcurementDetail.ProcurementPackageId == procurementId
                    ) &&
                    x.Vaccine.DiseaseMedicines.Any(dm =>
                        unvaccinatedDiseaseIds.Contains(dm.DiseaseId)
                    )
                )
                .ToList();
            List<LivestockRequireDisease> listLivestockRequireDiseases = new List<LivestockRequireDisease>();
            foreach (BatchVaccination batchVaccination in listBatchVaccinations)
            {
                LivestockRequireDisease livestockRequireDisease = new LivestockRequireDisease
                {
                    DiseaseName = batchVaccination.Vaccine.DiseaseMedicines.FirstOrDefault().Disease.Name,
                    MedicineName = batchVaccination.Vaccine.Name,
                    BatchVaccinationId = batchVaccination.Id
                };
                listLivestockRequireDiseases.Add(livestockRequireDisease);
            }
            LivestockRequireVaccinationProcurement result = new LivestockRequireVaccinationProcurement
            {
                LivestockId = livestock.Id,
                InspectionCode = livestock.InspectionCode,
                SpecieName = livestock.Species.Name,
                Color = livestock.Color,
                livestockRequireDisease = listLivestockRequireDiseases
            };
            return result;
        }

        public async Task<LivestockVaccination> AddLivestockVaccinationToVaccinationBatch(LivestockVaccinationAddByInspectionCode livestockVaccinationAddByInspectionCode)
        {
            var batchVacination = await _context.BatchVaccinations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == livestockVaccinationAddByInspectionCode.BatchVaccinationId);
            if (batchVacination == null)
            {
                throw new Exception("Lô tiêm vắc xin không tồn tại");
            }
            var livestock = await _context.Livestocks.AsNoTracking().FirstOrDefaultAsync(x => x.InspectionCode == livestockVaccinationAddByInspectionCode.InspectionCoded&&x.Species.Type==livestockVaccinationAddByInspectionCode.Specie_Type);
            if (livestock == null)
            {
                throw new Exception("Con vật không tồn tại");
            }
            List<LivestockVaccination> existLivestock = _context.LivestockVaccinations.Include(x=>x.Livestock)
                .ThenInclude(x=>x.Species).Where(x => x.Livestock.InspectionCode == livestockVaccinationAddByInspectionCode.InspectionCoded && x.Livestock.Species.Type == livestockVaccinationAddByInspectionCode.Specie_Type).ToList();
            //if (existLivestock.Any())
            //{
            //    throw new Exception("Vật nuôi đã thuộc 1 lô tiêm khác");
            //}
            _context.BatchVaccinations
    .Where(x => x.Id == livestockVaccinationAddByInspectionCode.BatchVaccinationId)
    .ExecuteUpdateAsync(x => x.SetProperty(b => b.Status, batch_vaccination_status.ĐANG_THỰC_HIỆN));
            LivestockVaccination livestockVaccination = new LivestockVaccination();
            livestockVaccination.Id = SlugId.New();
            livestockVaccination.BatchVaccinationId = livestockVaccinationAddByInspectionCode.BatchVaccinationId;
            livestockVaccination.LivestockId = livestock.Id;
            livestockVaccination.UpdatedAt = DateTime.Now;
            livestockVaccination.CreatedBy = livestockVaccinationAddByInspectionCode.CreatedBy;
            livestockVaccination.UpdatedBy = livestockVaccinationAddByInspectionCode.CreatedBy;
            livestockVaccination.CreatedAt = DateTime.Now;
            await _context.LivestockVaccinations.AddAsync(livestockVaccination);
            await _context.SaveChangesAsync();
            return livestockVaccination;
        }

        public async Task<SingleVaccinationCreate> AddLivestockVaccinationToSingleVaccination(SingleVaccinationCreate singleVaccinationCreate)
        {
            if (!String.IsNullOrEmpty(singleVaccinationCreate.BatchImportId))
            {
                BatchImport batchImport = _context.BatchImports.FirstOrDefault(x => x.Id == singleVaccinationCreate.BatchImportId);
                if (batchImport == null)
                {
                    throw new Exception("Không tìm thấy lô nhập này");
                }
            }
            if (_context.Livestocks.FirstOrDefault(x => x.Id == singleVaccinationCreate.LivestockId)==null){
                throw new Exception("Vật nuôi này không tồn tại");
            }
            if (_context.Medicines.FirstOrDefault(x => x.Id == singleVaccinationCreate.MedicineId) == null)
            {
                throw new Exception("Thuốc này không tồn tại");
            }
            if (_userManager.Users.FirstOrDefault(x => x.Id == singleVaccinationCreate.CreatedBy) == null)
            {
                throw new Exception("Người thực hiện này không tồn tại");
            }
            SingleVaccination singleVaccination = new SingleVaccination
            {
                Id = SlugId.New(),
                BatchImportId = singleVaccinationCreate.BatchImportId,
                CreatedAt = DateTime.Now,
                CreatedBy = singleVaccinationCreate.CreatedBy,
                UpdatedAt = DateTime.Now,
                UpdatedBy = singleVaccinationCreate.CreatedBy,
                LivestockId = singleVaccinationCreate.LivestockId,
                MedicineId = singleVaccinationCreate.MedicineId
            };
           await _context.AddAsync(singleVaccination);
            await _context.SaveChangesAsync();
            return singleVaccinationCreate;
        }

        public async Task<SingleVaccinationCreateByInspection> AddLivestockVaccinationToSingleVaccinationByInspectionCode(SingleVaccinationCreateByInspection singleVaccinationCreate)
        {
            if (!String.IsNullOrEmpty(singleVaccinationCreate.BatchImportId))
            {
                BatchImport batchImport = _context.BatchImports.FirstOrDefault(x => x.Id == singleVaccinationCreate.BatchImportId);
                if (batchImport == null)
                {
                    throw new Exception("Không tìm thấy lô nhập này");
                }
            }
            var livestock = _context.Livestocks.Include(x => x.Species).FirstOrDefault(x => x.InspectionCode == singleVaccinationCreate.InspectionCode && x.Species.Type == singleVaccinationCreate.SpecieType);
            if (livestock != null)
            {
                if (_context.Medicines.FirstOrDefault(x => x.Id == singleVaccinationCreate.MedicineId) == null)
                {
                    throw new Exception("Thuốc này không tồn tại");
                }
                if (_userManager.Users.FirstOrDefault(x => x.Id == singleVaccinationCreate.CreatedBy) == null)
                {
                    throw new Exception("Người thực hiện này không tồn tại");
                }
                SingleVaccination singleVaccination = new SingleVaccination
                {
                    Id = SlugId.New(),
                    BatchImportId = singleVaccinationCreate.BatchImportId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = singleVaccinationCreate.CreatedBy,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = singleVaccinationCreate.CreatedBy,
                    LivestockId = livestock.Id,
                    MedicineId = singleVaccinationCreate.MedicineId
                };
                await _context.SingleVaccination.AddAsync(singleVaccination);
                await _context.SaveChangesAsync();
                return singleVaccinationCreate;

            }
            throw new Exception("Vật nuôi này không tồn tại");

        }
    }
}
