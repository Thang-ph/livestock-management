using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Data;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services
{
    public class ProcurementService : IProcurementRepository
    {
        private readonly LmsContext _context;

        public ProcurementService(LmsContext context)
        {
            _context = context;
        }

        public async Task<ListProcurements> GetListProcurements(ListProcurementsFilter filter)
        {
            var result = new ListProcurements()
            {
                Total = 0
            };

            var procurements = await _context.ProcurementPackages.Include(x=>x.BatchExports).ThenInclude(x=>x.BatchExportDetails)
                .ToArrayAsync();
            if (procurements == null || !procurements.Any())
                return result;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    procurements = procurements
                        .Where(o => o.Code.ToUpper().Contains(filter.Keyword.Trim().ToUpper())
                            || o.Name.ToUpper().Contains(filter.Keyword.Trim().ToUpper()))
                        .ToArray();
                }
                if (filter.FromDate != null && filter.FromDate != DateTime.MinValue)
                {
                    procurements = procurements
                       .Where(o => o.CreatedAt >= filter.FromDate)
                       .ToArray();
                }
                if (filter.ToDate != null && filter.ToDate != DateTime.MinValue)
                {
                    procurements = procurements
                       .Where(o => o.CreatedAt <= filter.ToDate)
                       .ToArray();
                }
                if (filter.Status != null)
                {
                    procurements = procurements
                        .Where(o => o.Status == filter.Status)
                        .ToArray();
                }
            }
            if (!procurements.Any())
                return result;
            procurements = procurements
                .OrderByDescending(o => o.CreatedAt)
                .ToArray();

            result.Items = procurements
    .Select(o =>
    {
        var batch = o.BatchExports?
            .FirstOrDefault(b => b.Id==o.BatchExports.FirstOrDefault().Id); // chọn batch complete

        return new ProcurementSummary
        {
            Id = o.Id,
            Code = o.Code,
            Name = o.Name,
            SuccessDate = o.SuccessDate,
            CompletionDate = o.CompletionDate,
            ExpirationDate = o.ExpirationDate,
            CreatedAt = o.CreatedAt,
            Status = o.Status,
            TotalRequired = batch?.Total ?? 0,
            TotalExported = (batch != null) ? (batch.Total - batch.Remaining) : 0,
            Handoverinformation = new HandoverInformation
            {
                totalSelected = batch?.BatchExportDetails?.Count(x => x.Status == batch_export_status.CHỜ_BÀN_GIAO) ?? 0,
                totalCount = batch?.Total ?? 0,
                completeCount = (batch?.Total ?? 0) - (batch?.Remaining ?? 0)
            }
        };
    })
    .OrderByDescending(o => o.CreatedAt)
    .Skip(filter?.Skip ?? 0)
    .Take(filter?.Take ?? 10)
    .ToArray();


            result.Total = procurements.Length;

            return result;
        }

        public async Task<ProcurementSummary> CreateProcurementPackage(CreateProcurementPackageRequest request)
        {
            if (request == null)
                throw new Exception("Thông tin gói thầu trống");
            var isCodeDuplicated = await _context.ProcurementPackages
                .AnyAsync(o => o.Code.ToUpper() == request.Code.Trim().ToUpper());
            if (isCodeDuplicated)
                throw new Exception("Mã hợp đồng đã tồn tại");
            foreach (var detail in request.Details)
            {
                if (detail == null)
                    throw new Exception("Thông tin yêu cầu kỹ thuật trống");
                if (detail.RequiredAgeMin != null
                    && detail.RequiredAgeMax != null
                    && detail.RequiredAgeMin > detail.RequiredAgeMax)
                    throw new Exception("Tuổi yêu cầu bé nhất không được lớn hơn tuổi yêu cầu lớn nhất");
                if (detail.RequiredWeightMin != null
                    && detail.RequiredWeightMax != null
                    && detail.RequiredWeightMin > detail.RequiredWeightMax)
                    throw new Exception("Trọng lượng yêu cầu bé nhất không được lớn hơn trọng lượng yêu cầu lớn nhất");
            }

            var newProcurement = new ProcurementPackage
            {
                Id = SlugId.New(),
                CreatedAt = DateTime.Now,
                CreatedBy = request.RequestedBy ?? "SYS",
                UpdatedAt = DateTime.Now,
                UpdatedBy = request.RequestedBy ?? "SYS",
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                Owner = request.Owner.Trim(),
                ExpiredDuration = request.ExpiredDuration,
                SuccessDate = null,
                ExpirationDate = request.ExpiredDuration == null ?
                    null : DateTime.Now.AddDays(request.ExpiredDuration ?? 0),
                Status = procurement_status.ĐANG_ĐẤU_THẦU,
                Description = request.Description,
            };

            await _context.ProcurementPackages.AddAsync(newProcurement);

            foreach (var detail in request.Details)
            {
                var newProcurementDetail = new ProcurementDetail
                {
                    Id = SlugId.New(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = request.RequestedBy ?? "SYS",
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = request.RequestedBy ?? "SYS",
                    ProcurementPackageId = newProcurement.Id,
                    SpeciesId = detail.SpeciesId,
                    RequiredAgeMin = detail.RequiredAgeMin,
                    RequiredAgeMax = detail.RequiredAgeMax,
                    RequiredWeightMin = detail.RequiredWeightMin,
                    RequiredWeightMax = detail.RequiredWeightMax,
                    RequiredInsurance = detail.RequiredInsuranceDuration,
                    RequiredQuantity = detail.RequiredQuantity,
                    Description = detail.Description
                };
                await _context.ProcurementDetails.AddAsync(newProcurementDetail);

                foreach (var vaccinationRequire in detail.vaccinationRequireProcurementDetailCreates)
                {
                    var vaccinationRequireCreate = new VaccinationRequirement
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = request.RequestedBy ?? "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = request.RequestedBy ?? "SYS",
                        DiseaseId = vaccinationRequire.DiseaseId,
                        ProcurementDetailId= newProcurementDetail.Id,
                        InsuranceDuration= vaccinationRequire.InsuranceDuration,
                    };
                    await _context.VaccinationRequirement.AddAsync(vaccinationRequireCreate);
                }
            }

            await _context.SaveChangesAsync();

            var result = new ProcurementSummary
            {
                Id = newProcurement.Id,
                Code = newProcurement.Code,
                Name = newProcurement.Name,
                CreatedAt = newProcurement.CreatedAt,
                Status = newProcurement.Status
            };

            return result;
        }

        public async Task<ProcurementGeneralInfo> GetProcurementGeneralInfo(string id)
        {
            var procurement = await _context.ProcurementPackages.FindAsync(id) ??
                throw new Exception("Không tìm thấy gói thầu");
           var user = await _context.Users.FindAsync(procurement.CreatedBy);
         

            var procurementDetails = await _context.ProcurementDetails
                .Where(o => o.ProcurementPackageId == id)
                .ToArrayAsync();
           
            var species = await _context.Species
                .Where(o => procurementDetails.Select(x => x.SpeciesId).Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.Name);
            var batchExport =_context.BatchExports.FirstOrDefault(x=>x.ProcurementPackageId==id);
            var result = new ProcurementGeneralInfo
            {
                Id = procurement.Id,
                Code = procurement.Code,
                Name = procurement.Name,
                CreatedAt = procurement.CreatedAt,
                Status = procurement.Status,
                Owner = procurement.Owner,
                ExpiredDuration = procurement.ExpiredDuration,
                ExpirationDate = procurement.ExpirationDate,
                Description = procurement.Description,
                TotalExported=(batchExport?.Total-batchExport?.Remaining)??0,
                TotalRequired=batchExport?.Total??0,
                TotalSelected = batchExport?.BatchExportDetails?
                   .Count(x => x.Status == batch_export_status.CHỜ_BÀN_GIAO) ?? 0,
                CreatedBy = user?.UserName ??"SYS",
                Details = procurementDetails
                    .OrderBy(o => o.CreatedAt)
                    .Select(o => new ProcurementDetails
                    {
                        Id = o.Id,
                        ProcurementName = procurement.Name,
                        SpeciesId = o.SpeciesId,
                        SpeciesName = species[o.SpeciesId],
                        RequiredAgeMin = o.RequiredAgeMin,
                        RequiredAgeMax = o.RequiredAgeMax,
                        RequiredWeightMin = o.RequiredWeightMin,
                        RequiredWeightMax = o.RequiredWeightMax,
                        RequiredQuantity = o.RequiredQuantity,
                        RequiredInsurance = o.RequiredInsurance,
                        Description = o.Description,
                                            vaccinationRequire = _context.VaccinationRequirement.Include(x=>x.Disease)
                        .Where(x => x.ProcurementDetailId == o.Id)
                        .Select(x => new VaccinationRequireProcurementDetail
                        {
                            DiseaseName = x.Disease.Name,
                            InsuranceDuration = x.InsuranceDuration
                        })
                        .ToList()

                    })
                    .ToArray()
            };

            return result;
        }

        public async Task<ProcurementSummary> UpdateProcurementPackage(UpdateProcurementPackageRequest request)
        {
            var procurementPackage = await _context.ProcurementPackages.FindAsync(request.Id) ??
                throw new Exception("Không tìm thấy gói thầu");
            var isCodeDuplicated = await _context.ProcurementPackages
                .AnyAsync(o => o.Id != request.Id
                    && o.Code.ToUpper() == request.Code.Trim().ToUpper());
            if (isCodeDuplicated)
                throw new Exception("Mã hợp đồng đã tồn tại");
            var procurementDetails = await _context.ProcurementDetails
                .Where(o => request.Details.Select(x => x.Id).Contains(o.Id))
                .ToArrayAsync();
            if (procurementDetails == null || !procurementDetails.Any())
                throw new Exception("Không tìm thấy yêu cầu kỹ thuật của gói thầu");
            foreach (var detail in request.Details)
            {
                if (detail == null)
                    throw new Exception("Thông tin yêu cầu kỹ thuật trống");
                if (detail.RequiredAgeMin != null
                    && detail.RequiredAgeMax != null
                    && detail.RequiredAgeMin > detail.RequiredAgeMax)
                    throw new Exception("Tuổi yêu cầu bé nhất không được lớn hơn tuổi yêu cầu lớn nhất");
                if (detail.RequiredWeightMin != null
                    && detail.RequiredWeightMax != null
                    && detail.RequiredWeightMin > detail.RequiredWeightMax)
                    throw new Exception("Trọng lượng yêu cầu bé nhất không được lớn hơn trọng lượng yêu cầu lớn nhất");
            }

            procurementPackage.Code = request.Code;
            procurementPackage.Name = request.Name;
            procurementPackage.Owner = request.Owner;
            procurementPackage.ExpiredDuration = request.ExpiredDuration;
            procurementPackage.ExpirationDate = (procurementPackage.SuccessDate != null && request.ExpiredDuration != null) ?
                procurementPackage.SuccessDate.Value.AddDays(request.ExpiredDuration ?? 0) : null;
            procurementPackage.Description = request.Description;
            procurementPackage.UpdatedAt = DateTime.Now;
            procurementPackage.UpdatedBy = request.RequestedBy ?? "SYS";

            foreach (var detail in request.Details)
            {
                var procurementDetail = procurementDetails.FirstOrDefault(o => o.Id == detail.Id) ??
                    throw new Exception("Không tìm thấy yêu cầu kỹ thuật của gói thầu");
                procurementDetail.SpeciesId = detail.SpeciesId;
                procurementDetail.RequiredAgeMin = detail.RequiredAgeMin;
                procurementDetail.RequiredAgeMax = detail.RequiredAgeMax;
                procurementDetail.RequiredWeightMin = detail.RequiredWeightMin;
                procurementDetail.RequiredWeightMax = detail.RequiredWeightMax;
                procurementDetail.RequiredQuantity = detail.RequiredQuantity;
                procurementDetail.RequiredInsurance = detail.RequiredInsurance;
                procurementDetail.Description = detail.Description;
                procurementDetail.UpdatedAt = DateTime.Now;
                procurementDetail.UpdatedBy = request.RequestedBy ?? "SYS";
                foreach (var vaccinationRequire in detail.vaccinationRequireProcurementDetailUpdates)
                {
                    var vaccinationRequireCreate = new VaccinationRequirement
                    {
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = request.RequestedBy ?? "SYS",
                        DiseaseId = vaccinationRequire.DiseaseId,
                        InsuranceDuration = vaccinationRequire.InsuranceDuration,
                    };
                    await _context.VaccinationRequirement.AddAsync(vaccinationRequireCreate);
                }
            }

            await _context.SaveChangesAsync();

            var result = await _context.ProcurementPackages
                .Where(o => o.Id == request.Id)
                .Select(o => new ProcurementSummary
                {
                    Id = o.Id,
                    Code = o.Code,
                    Name = o.Name,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                })
                .FirstOrDefaultAsync() ??
                throw new Exception("Lỗi lưu thay đổi: Không tìm thấy gói thầu");

            return result;
        }

        public async Task<Stream> GetTemplateListCustomers(string id)
        {
            var procurementPackage = await _context.ProcurementPackages.FindAsync(id) ??
                throw new Exception("Không tìm thấy gói thầu");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new MemoryStream());
            var worksheet = package.Workbook.Worksheets.Add($"{procurementPackage.Code}");

            var richTextRow1 = worksheet.Cells["A1"].RichText;
            var richTextRow2 = worksheet.Cells["A2"].RichText;
            var richTextRow3 = worksheet.Cells["A3"].RichText;
            var richTextRow4 = worksheet.Cells["A5"].RichText;

            var boldSegmentRow1 = richTextRow1.Add("MÃ GÓI THẦU: ");
            boldSegmentRow1.Bold = true;
            richTextRow1.Add($"{procurementPackage.Code}");
            var boldSegmentRow2 = richTextRow2.Add("TÊN GÓI THẦU: ");
            boldSegmentRow2.Bold = true;
            richTextRow2.Add($"{procurementPackage.Name}");
            var boldSegmentRow3 = richTextRow3.Add("BÊN MỜI THẦU: ");
            boldSegmentRow3.Bold = true;
            richTextRow3.Add($"{procurementPackage.Owner}");

            worksheet.Cells["A5"].Value = $"DANH SÁCH LỰA CHỌN NGHIỆM THU ĐÁNH GIÁ CHẤT LƯỢNG ";
            worksheet.Cells["A5"].Style.Font.Bold = true;

            var columns = new string[] { "Tên", "Địa chỉ", "Số điện thoại", "Ghi chú", "Số lượng" };
            var data = new DataTable();
            data.Columns.AddRange(columns.Select(o => new DataColumn(o)).ToArray());

            worksheet.Cells["A6"].LoadFromDataTable(data, true, TableStyles.Light1);
            worksheet.Column(3).Style.Numberformat.Format = "@";
            worksheet.Column(5).Style.Numberformat.Format = "@";

            await package.SaveAsync();
            return package.Stream;
        }

        public async Task ImportListCustomers(string procurementId, string requestedBy, IFormFile file)
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

            var listCustomers = new List<BatchExport>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (file == null || file.Length <= 0)
                throw new Exception("Không tìm thấy file");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    //validate procurement package info
                    var procurementCodeStr = worksheet.Cells["A1"].Value?.ToString();
                    if (string.IsNullOrEmpty(procurementCodeStr))
                        throw new Exception("Mã gói thầu không hợp lệ");
                    var code = procurementCodeStr.Trim().Split(":").Last();
                    if (string.IsNullOrEmpty(code))
                        throw new Exception("Mã gói thầu không hợp lệ");
                    if (procurementPackage.Code.ToUpper() != code.Trim().ToUpper())
                        throw new Exception($"Không tìm thấy mã gói thầu {code}");

                    //validate column headers list customers
                    var requiredColumns = new string[] {
                        "tên",
                        "địa chỉ",
                        "số điện thoại",
                        "ghi chú",
                        "số lượng",
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
                        var columnHeader = worksheet.Cells[6, i].Value?.ToString();
                        if (string.IsNullOrEmpty(columnHeader))
                            throw new Exception($"Trống tên cột ở ô [6:{i}]");
                        columnHeader = columnHeader.Trim().ToLower();
                        if (!dicColumnIndexes.ContainsKey(columnHeader))
                            throw new Exception($"Tên cột không hợp lệ ở ô [6:{i}]");
                        dicColumnIndexes[columnHeader] = i;
                    }

                    //read value
                    var idxName = dicColumnIndexes["tên"];
                    var idxAddress = dicColumnIndexes["địa chỉ"];
                    var idxPhone = dicColumnIndexes["số điện thoại"];
                    var idxNote = dicColumnIndexes["ghi chú"];
                    var idxQuantity = dicColumnIndexes["số lượng"];
                    for (int row = 7; row <= rowCount; row++)
                    {
                        var customerName = worksheet.Cells[row, idxName].Value?.ToString();
                        if (string.IsNullOrEmpty(customerName))
                            throw new Exception($"Trống tên khách hàng ở ô [{row}:{idxName}]");

                        var customerAddress = worksheet.Cells[row, idxAddress].Value?.ToString();
                        var customerPhone = worksheet.Cells[row, idxPhone].Value?.ToString();
                        var customerNote = worksheet.Cells[row, idxNote].Value?.ToString();
                        if (string.IsNullOrEmpty(customerAddress)
                            && string.IsNullOrEmpty(customerPhone)
                            && string.IsNullOrEmpty(customerNote))
                            throw new Exception($"Ít nhất 1 trong 3 mục địa chỉ, số điện thoại, ghi chú không trống ở dòng [{row}]");

                        int quantity = -1;
                        var strQuantity = worksheet.Cells[row, idxQuantity].Value?.ToString();
                        if (string.IsNullOrEmpty(strQuantity))
                            throw new Exception($"Trống số lượng ở ô [{row}:{idxQuantity}]");
                        var isInteger = Int32.TryParse(strQuantity.Trim(), out quantity);
                        if (!isInteger)
                            throw new Exception($"Số lượng không hợp lệ ở ô [{row}:{idxQuantity}]");
                        if (quantity < 0)
                            throw new Exception($"Số lượng không được bé hơn 0 ở ô [{row}:{idxQuantity}]");

                        listCustomers.Add(new BatchExport
                        {
                            Id = SlugId.New(),
                            CreatedAt = DateTime.Now,
                            CreatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy,
                            BarnId = null,
                            ProcurementPackageId = procurementPackage.Id,
                            Status = batch_export_status.CHỜ_BÀN_GIAO,
                            CustomerName = customerName.Trim(),
                            CustomerAddress = string.IsNullOrEmpty(customerAddress) ?
                                null : customerAddress.Trim(),
                            CustomerPhone = string.IsNullOrEmpty(customerPhone) ?
                                null : customerPhone.Trim(),
                            CustomerNote = string.IsNullOrEmpty(customerNote) ?
                                null : customerNote.Trim(),
                            Total = quantity,
                            Remaining = quantity
                        });
                    }
                }

            }

            await _context.BatchExports.AddRangeAsync(listCustomers);
            await _context.SaveChangesAsync();

            return;
        }

        public IQueryable<Livestock> GetSuggestLivestock(string procurementID)
        {
            var procurementGeneralInfo = GetProcurementGeneralInfo(procurementID).Result; 
            var details = procurementGeneralInfo.Details.FirstOrDefault();

            var query = _context.Livestocks
                 .Include(x => x.Species)
                 .Include(x => x.Barn)
                 .AsQueryable();

            if (details != null)
            {
                if (details.RequiredAgeMin > 0)
                {
                    var minDob = DateTime.UtcNow.AddMonths(-details.RequiredAgeMin??0);
                    query = query.Where(x => x.Dob <= minDob);
                }
                //if (details.RequiredWeightMin > 0)
                //{
                //    query = query.Where(x => x.WeightOrigin >= details.RequiredWeightMin);
                //}
                //if (details.RequiredWeightMax > 0)
                //{
                //    query = query.Where(x => x.WeightOrigin <= details.RequiredWeightMax);
                //}
                if (!string.IsNullOrEmpty(details.SpeciesId))
                {
                    query = query.Where(x => x.SpeciesId == details.SpeciesId);
                }
            }

            return query.Take(details.RequiredQuantity??0);
        }

        public async Task<CreateExportDetail> CreateExportDetail(CreateExportDetail request)
        {
            var batchExport = await _context.BatchExports.FindAsync(request.BatchExportId) ??
                throw new Exception("Không tìm thấy thông tin bàn giao");
            if (batchExport.Status == batch_export_status.ĐÃ_BÀN_GIAO)
                throw new Exception("Gói thầu đã bàn giao hoặc đã hủy không thể chọn thêm vật nuôi");
            if (batchExport.Remaining <= 0)
                throw new Exception("Đã nhận đủ số vật nuôi không thể chọn thêm");
            var livestock = await _context.Livestocks
                .FirstOrDefaultAsync(o => o.Id == request.LivestockId
                    && o.Status != livestock_status.CHẾT
                    && o.Status != livestock_status.ĐÃ_XUẤT) ??
                throw new Exception("Không tìm thấy vật nuôi hoặc vật nuôi đã bán hoặc chết");
            var anyExistingExportDetail = await _context.BatchExportDetails
                .Where(o => o.LivestockId == request.LivestockId)
                .AnyAsync();
            if (anyExistingExportDetail)
                throw new Exception("Vật nuôi thuộc gói thầu khác không thể chọn");

            var newExportDetail = new BatchExportDetail
            {
                Id = SlugId.New(),
                CreatedAt = DateTime.Now,
                CreatedBy = request.RequestedBy ?? "SYS",
                UpdatedAt = DateTime.Now,
                UpdatedBy = request.RequestedBy ?? "SYS",
                HandoverDate = DateTime.Now,
                BatchExportId = request.BatchExportId,
                LivestockId = request.LivestockId,
                Status = batch_export_status.CHỜ_BÀN_GIAO,
                ExpiredInsuranceDate = null,
                ExportDate = null,
                PriceExport = null,
                WeightExport = livestock.WeightExport,
            };

            await _context.BatchExportDetails.AddAsync(newExportDetail);

            batchExport.Remaining = batchExport.Remaining - 1;
            batchExport.UpdatedAt = DateTime.Now;
            batchExport.UpdatedBy = request.RequestedBy ?? "SYS";

            await _context.SaveChangesAsync();

            return request;
        }

        public async Task<UpdateExportDetail> UpdateExportDetail(UpdateExportDetail request)
        {
            var batchExport = await _context.BatchExports.FindAsync(request.BatchExportId) ??
                throw new Exception("Không tìm thấy thông tin bàn giao");
            if (batchExport.Status == batch_export_status.ĐÃ_BÀN_GIAO)
                throw new Exception("Gói thầu đã bàn giao hoặc đã hủy không thể thay đổi vật nuôi");

            var existingExportDetails = await _context.BatchExportDetails
                .Where(o => o.BatchExportId == batchExport.Id)
                .ToArrayAsync();
            if (existingExportDetails == null || !existingExportDetails.Any())
                throw new Exception("Không tìm thấy chi tiết bàn giao để thay đổi");
            var currentExportDetail = existingExportDetails.FirstOrDefault(o => o.Id == request.BatchExportDetailId) ??
                throw new Exception("Không tìm thấy chi tiết bàn giao để thay đổi");

            var livestock = await _context.Livestocks
                .FirstOrDefaultAsync(o => o.Id == request.LivestockId
                    && o.Status != livestock_status.CHẾT
                    && o.Status != livestock_status.ĐÃ_XUẤT) ??
                throw new Exception("Không tìm thấy vật nuôi hoặc vật nuôi đã bán hoặc chết");

            var anyExistingExportDetail = await _context.BatchExportDetails
                .Where(o => o.Id != request.BatchExportDetailId
                    && o.LivestockId == request.LivestockId)
                .AnyAsync();
            if (anyExistingExportDetail)
                throw new Exception("Vật nuôi thuộc gói thầu khác không thể chọn");

            var isLivestockInTheSameBatch = existingExportDetails
                .Any(o => o.Id != currentExportDetail.Id
                    && o.LivestockId == request.LivestockId);
            if (isLivestockInTheSameBatch)
                throw new Exception("Vật nuôi đã được chọn trong cùng danh sách bàn giao không thể thay đổi");

            currentExportDetail.LivestockId = request.LivestockId;
            currentExportDetail.HandoverDate = DateTime.Now;
            currentExportDetail.Status = batch_export_status.CHỜ_BÀN_GIAO;
            currentExportDetail.PriceExport = null;
            currentExportDetail.WeightExport = livestock.WeightExport;
            currentExportDetail.ExportDate = null;
            currentExportDetail.ExpiredInsuranceDate = null;
            currentExportDetail.UpdatedAt = DateTime.Now;
            currentExportDetail.UpdatedBy = request.RequestedBy;

            await _context.SaveChangesAsync();

            return request;
        }

        public async Task<ListExportDetails> GetListExportDetails(string id)
        {
            var batchExport = await _context.BatchExports.FindAsync(id) ??
               throw new Exception("Không tìm thấy thông tin bàn giao");

            var result = new ListExportDetails()
            {
                BatchExportId = batchExport.Id,
                CustomerName = batchExport.CustomerName,
                CustomerAddress = batchExport.CustomerAddress,
                CustomerPhone = batchExport.CustomerPhone,
                CustomerNote = batchExport.CustomerNote,
                TotalLivestocks = batchExport.Total,
                Remaining = batchExport.Remaining,
                Received = batchExport.Total - batchExport.Remaining,
                Status = batchExport.Status,
                Total = 0
            };

            var exportDetails = await _context.BatchExportDetails
                .Where(o => o.BatchExportId == id)
                .ToArrayAsync();
            if (exportDetails == null || !exportDetails.Any()) 
                return result;

            var livestocks = await _context.Livestocks
                .Where(o => exportDetails.Select(x => x.LivestockId).Contains(o.Id))
                .ToArrayAsync();
            if (livestocks == null || !exportDetails.Any())
                throw new Exception("Không tìm thấy thông tin vật nuôi");

            var resultExportDetails = exportDetails
                .Select(o => new ExportDetail
                {
                    BatchExportDetailId = o.Id,
                    LivestockId = o.LivestockId,
                    InspectionCode = livestocks.FirstOrDefault(x => x.Id == o.LivestockId)?.InspectionCode,
                    HandoverDate = o.HandoverDate,
                    WeightExport = o.WeightExport,
                    ExportDate = o.ExportDate,  
                    ExpiredInsuranceDate = o.ExpiredInsuranceDate,
                    Status = o.Status
                })
                .ToArray();

            result.Items = resultExportDetails;

            return result;  
        }

        public async Task<DataTable> GetEmpData(string procurementID)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Livestock";
            dt.Columns.Add("Inspection Code", typeof(string));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("Date of birth", typeof(string));
            dt.Columns.Add("Gender", typeof(string));
            dt.Columns.Add("Origin", typeof(string));
            dt.Columns.Add("Specie", typeof(string));
            dt.Columns.Add("Weight Origin", typeof(string));
            dt.Columns.Add("Barn", typeof(string));
            dt.Columns.Add("Created At", typeof(string));
            dt.Columns.Add("Updated At", typeof(string));
            dt.Columns.Add("Created By", typeof(string));
            dt.Columns.Add("Updated By", typeof(string));
            dt.Columns.Add("Color", typeof(string));
            dt.Columns.Add("Weight Export", typeof(string));

            var query = GetSuggestLivestock(procurementID);
            var _list = await query.ToListAsync();

            if (_list.Any())
            {
                _list.ForEach(x =>
                {
                    dt.Rows.Add(
                        x.InspectionCode, x.Status, x.Dob, x.Gender, x.Origin,
                        x.Species.Name, x.WeightOrigin, x.Barn.Name, x.CreatedAt,
                        x.UpdatedAt, x.CreatedBy, x.UpdatedBy, x.Color, x.WeightExport
                    );
                });
            }


            return dt;
        }

        public async Task<bool> CompleteProcurementPackage(ProcurementStatus status)
        {
            var result = false;
            var procurementPackagesModels = await _context.ProcurementPackages.FirstOrDefaultAsync(x => x.Id == status.Id);
            if (procurementPackagesModels == null) throw new Exception("Không tìm thấy gói thầu");
            var listCustomer = await _context.BatchExports.Where(x => x.ProcurementPackageId == status.Id).ToListAsync();
            if (listCustomer.Any())
                foreach (var details in listCustomer)
                        details.Status = batch_export_status.ĐÃ_BÀN_GIAO;
            procurementPackagesModels.Status = procurement_status.HOÀN_THÀNH;
            procurementPackagesModels.UpdatedAt = DateTime.Now;
            procurementPackagesModels.UpdatedBy = status.RequestedBy;
            await _context.SaveChangesAsync();
            result = true;
            return result;
        }

        public async Task<bool> CancelProcurementPackage(ProcurementStatus status)
        {
            var result = false;
            var procurementPackagesModels = await _context.ProcurementPackages.FirstOrDefaultAsync(x => x.Id == status.Id);
            if (procurementPackagesModels == null) throw new Exception("Không tìm thấy gói thầu");
            procurementPackagesModels.Status = procurement_status.ĐÃ_HỦY;
            procurementPackagesModels.UpdatedAt = DateTime.Now;
            procurementPackagesModels.UpdatedBy = status.RequestedBy;
            await _context.SaveChangesAsync();
            result = true;
            return result;
        }
    }
}
