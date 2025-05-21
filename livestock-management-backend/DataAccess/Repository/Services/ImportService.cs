using AutoMapper;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using ClosedXML.Excel;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Xml.Linq;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services;

public class ImportService : IImportRepository
{
    private readonly LmsContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ImportService> _logger;
    private readonly IMapper _mapper;

    public ImportService(LmsContext context, ILogger<ImportService> logger, IMapper mapper,
         UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        _mapper = mapper;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> StatusCancel(BatchImportStatus status)
    {
        var batchImportModels = await _context.BatchImports.FirstOrDefaultAsync(x => x.Id == status.Id);
        if (batchImportModels == null)
            throw new Exception("Không tìm thấy lô nhập");

        if (batchImportModels.Status == batch_import_status.HOÀN_THÀNH)
            throw new Exception("Lô nhập đã hoàn thành, không thể hủy");

        var listBatchImportDetails = await _context.BatchImportDetails
                    .Include(l => l.Livestock)
                    .Where(x => x.BatchImportId == status.Id).ToListAsync();
        if (listBatchImportDetails.Any())
        {
            foreach (var details in listBatchImportDetails)
            {
                if (details.Status == batch_import_status.CHỜ_NHẬP && details.Livestock.Status != livestock_status.CHẾT)
                {
                    details.Status = batch_import_status.ĐÃ_HỦY;
                    details.UpdatedAt = DateTime.Now;
                    details.UpdatedBy = status.RequestedBy;
                }
            }
        }
        batchImportModels.Status = batch_import_status.ĐÃ_HỦY;
        batchImportModels.UpdatedAt = DateTime.Now;
        batchImportModels.UpdatedBy = status.RequestedBy ?? "SYS";

        var checkPinnedBatch = await _context.PinnedBatchImports.FirstOrDefaultAsync(x => x.BatchImportId == status.Id && x.CreatedBy == status.RequestedBy);
        if (checkPinnedBatch != null)
            _context.PinnedBatchImports.Remove(checkPinnedBatch);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> StatusComplete(BatchImportStatus status)
    {
        var batchImportModels = await _context.BatchImports.FirstOrDefaultAsync(x => x.Id == status.Id);
        if (batchImportModels == null)
            throw new Exception("Không tìm thấy lô nhập");

        if (batchImportModels.Status == batch_import_status.ĐÃ_HỦY)
            throw new Exception("Lô nhập đã bị huỷ, không thể hoàn thành");

        var listBatchImportDetails = await _context.BatchImportDetails
                    .Include(l => l.Livestock)
                    .Where(x => x.BatchImportId == status.Id
                    && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null)).ToListAsync();
        if (!listBatchImportDetails.Any())
            throw new Exception("Lô nhập chưa có danh sách thông tin vật nuôi, không thể hoàn thành");

        var count = listBatchImportDetails.Count();
        if (count < batchImportModels.EstimatedQuantity)
            throw new Exception("Lô nhập chưa đủ " + batchImportModels.EstimatedQuantity
                + " vật nuôi, còn thiếu " + (batchImportModels.EstimatedQuantity - count)
                + " vật nuôi, chưa thể hoàn thành ");

        foreach (var details in listBatchImportDetails)
        {
            if (details.Status == batch_import_status.CHỜ_NHẬP && details.Livestock.Status != livestock_status.CHẾT)
            {
                details.Status = batch_import_status.ĐÃ_NHẬP;
                details.ImportedDate = DateTime.Now;
                details.UpdatedAt = DateTime.Now;
                details.UpdatedBy = status.RequestedBy;
            }
        }

        batchImportModels.Status = batch_import_status.HOÀN_THÀNH;
        batchImportModels.CompletionDate = DateTime.Now;
        batchImportModels.UpdatedAt = DateTime.Now;
        batchImportModels.UpdatedBy = status.RequestedBy ?? "SYS";

        var checkPinnedBatch = await _context.PinnedBatchImports.FirstOrDefaultAsync(x => x.BatchImportId == status.Id && x.CreatedBy == status.RequestedBy);
        if(checkPinnedBatch != null)
            _context.PinnedBatchImports.Remove(checkPinnedBatch);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<BatchImport> CreateBatchImport(BatchImportCreate batchImportCreate)
    {
        var barn = await _context.Barns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == batchImportCreate.BarnId.Trim());
        if (barn == null)
            throw new Exception("Barn not exist");

        var nameCheck = await _context.BatchImports.Where(x => x.Name.ToLower().Trim() == batchImportCreate.Name.ToLower().Trim()).ToArrayAsync();
        if (nameCheck.Any())
            throw new Exception("Tên đã được sử dụng");

        var batchImport = new BatchImport();
        batchImport.Id = SlugId.New();
        batchImport.OriginLocation = batchImportCreate.OriginLocation;
        batchImport.BarnId = batchImportCreate.BarnId;
        batchImport.CreatedAt = DateTime.Now;
        batchImport.UpdatedAt = DateTime.Now;
        batchImport.CreatedBy = string.IsNullOrEmpty(batchImportCreate.CreatedBy) ? "SYS" : batchImportCreate.CreatedBy;
        batchImport.UpdatedBy = string.IsNullOrEmpty(batchImportCreate.CreatedBy) ? "SYS" : batchImportCreate.CreatedBy;
        batchImport.EstimatedQuantity = batchImportCreate.EstimatedQuantity;
        batchImport.ExpectedCompletionDate = batchImportCreate.ExpectedCompletionDate;
        batchImport.Name = batchImportCreate.Name;
        batchImport.Status = batch_import_status.CHỜ_NHẬP;

        await _context.BatchImports.AddAsync(batchImport);
        await _context.SaveChangesAsync();

        return batchImport;
    }

    public async Task<BatchImport> UpdateBatchImportAsync(string id, BatchImportUpdate batchImportUpdate)
    {
        var barn = await _context.Barns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == batchImportUpdate.BarnId.Trim());
        if (barn == null)
            throw new Exception("Barn not exist");

        var batchImport = await GetBatchImportById(id);
        if (batchImport == null)
            throw new Exception("Batch Import not exist");

        if (batchImport.Status == batch_import_status.HOÀN_THÀNH || batchImport.Status == batch_import_status.ĐÃ_HỦY)
            throw new Exception("Lô nhập đã hoàn thành hoặc đã bị hủy, không thể chỉnh sửa");

        if (batchImportUpdate.Name != batchImport.Name)
        {
            var nameCheck = await _context.BatchImports
                .Where(x => x.Name.ToLower().Trim() == batchImportUpdate.Name.ToLower().Trim())
                .ToArrayAsync();
            if (nameCheck.Any())
                throw new Exception("Tên lô nhập đã được sử dụng");
        }

        //var livestockInBatch = await _context.BatchImportDetails
        //    .Include(l => l.Livestock)
        //    .Where(x => x.BatchImportId == id
        //            && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null))
        //    .CountAsync();
        //if (int.Parse(batchImportUpdate.EstimatedQuantity.ToString()) < livestockInBatch)
        //    throw new Exception("Hiện đã có " + livestockInBatch + " loài vật trong lô nhập này, không thể sửa ít hơn");

        batchImport.OriginLocation = batchImportUpdate.OriginLocation;
        batchImport.BarnId = batchImportUpdate.BarnId;
        batchImport.UpdatedAt = DateTime.Now;
        batchImport.UpdatedBy = string.IsNullOrEmpty(batchImportUpdate.UpdatedBy) ? "SYS" : batchImportUpdate.UpdatedBy;
        batchImport.EstimatedQuantity = batchImportUpdate.EstimatedQuantity;
        batchImport.ExpectedCompletionDate = batchImportUpdate.ExpectedCompletionDate;
        batchImport.Name = batchImportUpdate.Name;

        //STATUS CHANGE WARNING: kiem tra danh sach co vat nuoi chua de chuyen status thanh dang chon
        //if (batchImportUpdate.EstimatedQuantity == livestockInBatch)
        //{
        //    if (batchImport.Status == batch_import_status.ĐANG_CHỌN)
        //        batchImport.Status = batch_import_status.CHỜ_NHẬP;
        //}

        //if(batchImportUpdate.EstimatedQuantity > livestockInBatch)
        //{
        //    if (batchImport.Status == batch_import_status.ĐANG_NHẬP)
        //        batchImport.Status = batch_import_status.CHỜ_CHỌN;
        //}

        await _context.SaveChangesAsync();

        var batch = _mapper.Map<BatchImport>(batchImport);

        return batch;
    }

    public async Task<ImportBatchDetails> GetImportBatchDetails(string id)
    {
        _logger.LogInformation($"[{nameof(ImportService)}] Starting to retrieve import batch details with id: {id}");

        if (string.IsNullOrEmpty(id))
        {
            _logger.LogError($"[{nameof(ImportService)}] Error: Import batch ID cannot be empty");
            throw new ArgumentException("Import batch ID cannot be empty", nameof(id));
        }

        try
        {
            var batchImport = await _context.BatchImports
                .Include(x => x.Barn)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (batchImport == null)
            {
                _logger.LogError($"[{nameof(ImportService)}] Error: Import batch with id: {id} not found");
                throw new Exception($"Import batch with id: {id} not found");
            }

            // Lấy thông tin người tạo từ bảng Users
            var createdBy = await _context.Users
                .Where(u => u.Id == batchImport.CreatedBy)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();

            // Đếm số lượng vật nuôi chờ nhập và đã nhập trong lô
            var importedQuantity = await _context.BatchImportDetails
                .Where(x => x.BatchImportId == id
                && x.ImportedDate != null)
                .CountAsync();

            var result = new ImportBatchDetails
            {
                Id = batchImport.Id,
                Name = batchImport.Name,
                EstimatedQuantity = batchImport.EstimatedQuantity,
                ImportedQuantity = importedQuantity,
                ExpectedCompletionDate = batchImport.ExpectedCompletionDate,
                CompletionDate = batchImport.CompletionDate,
                Status = batchImport.Status,
                OriginLocation = batchImport.OriginLocation,
                ImportToBarn = batchImport.Barn?.Name ?? "N/A",
                CreatedBy = createdBy ?? "SYS",
                CreatedAt = batchImport.CreatedAt
            };

            // Lấy danh sách vật nuôi trong lô nhập và user IDs để hiển thị tên
            var importDetailsQuery = _context.BatchImportDetails
                .Where(d => d.BatchImportId == id);

            // Lấy danh sách user IDs để tìm tên
            var userIds = await importDetailsQuery
                .Select(d => d.CreatedBy)
                .Distinct()
                .ToListAsync();

            // Lấy thông tin người dùng
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var livestocks = await importDetailsQuery
                .Join(_context.Livestocks,
                    detail => detail.LivestockId,
                    livestock => livestock.Id,
                    (detail, livestock) => new { detail, livestock })
                .Join(_context.Species,
                    joined => joined.livestock.SpeciesId,
                    species => species.Id,
                    (joined, species) => new
                    {
                        detail = joined.detail,
                        livestock = joined.livestock,
                        species = species
                    })
                .ToListAsync();

            var importedLivestocksInfo = livestocks.Select(joined =>
            {
                string createdByUserName = "SYS";
                if (users.ContainsKey(joined.detail.CreatedBy))
                {
                    createdByUserName = users[joined.detail.CreatedBy];
                }

                return new ImportedLivestockInfo
                {
                    Id = joined.detail.Id,
                    LivestockId = joined.livestock.Id,
                    InspectionCode = joined.livestock.InspectionCode ?? "N/A",
                    SpecieId = joined.livestock.SpeciesId,
                    SpecieName = joined.species.Name,
                    SpecieType = joined.species.Type,
                    CreatedAt = joined.detail.CreatedAt,
                    ImportedDate = joined.detail.ImportedDate,
                    WeightImport = joined.detail.WeightImport ?? joined.livestock.WeightOrigin,
                    Status = joined.livestock.Status,
                    ImportedBy = createdByUserName
                };
            }).ToList();

            result.ListImportedLivestocks = new ListImportedLivestocks
            {
                Items = importedLivestocksInfo,
                Total = importedLivestocksInfo.Count,
            };

            _logger.LogInformation(
                $"[{nameof(ImportService)}] Successfully retrieved import batch details. Batch: {batchImport.Name}, Livestock count: {importedLivestocksInfo.Count}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{nameof(ImportService)}] Error retrieving import batch details: {ex.Message}");
            throw;
        }
    }

    public async Task<ListImportBatches> GetListImportBatches(ListImportBatchesFilter filter)
    {
        _logger.LogInformation($"[{nameof(ImportService)}] Starting to retrieve list of import batches");

        try
        {

            // Tạo query cơ bản
            var query = from bi in _context.BatchImports
                        join bid in _context.BatchImportDetails
                            on bi.Id equals bid.BatchImportId into importDetails
                        from id in importDetails.DefaultIfEmpty()
                        group new { bi, id } by new
                        {
                            bi.Id,
                            bi.Name,
                            bi.EstimatedQuantity,
                            bi.ExpectedCompletionDate,
                            bi.CompletionDate,
                            bi.Status,
                            bi.CreatedAt,
                            bi.CreatedBy
                        } into g
                        select new
                        {
                            g.Key.Id,
                            g.Key.Name,
                            g.Key.EstimatedQuantity,
                            g.Key.ExpectedCompletionDate,
                            g.Key.CompletionDate,
                            g.Key.Status,
                            g.Key.CreatedAt,
                            g.Key.CreatedBy,
                            ImportedCount = g.Count(x => x.id != null && x.id.ImportedDate != null)
                        };

            // Áp dụng các bộ lọc
            if (filter.FromDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= filter.FromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var endDate = filter.ToDate.Value.AddDays(1);
                query = query.Where(x => x.CreatedAt < endDate);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var keyword = filter.Keyword.ToLower().Trim();
                query = query.Where(x => x.Name.ToLower().Contains(keyword));
            }

            // Lấy tổng số lượng cho phân trang
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang
            var skip = filter.Skip;
            var take = filter.Take > 0 ? filter.Take : 10;

            var resultsRaw = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            // Truy xuất thông tin người dùng và ánh xạ kết quả
            var userIds = resultsRaw.Select(x => x.CreatedBy).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var results = resultsRaw.Select(x =>
            {
                if (!users.TryGetValue(x.CreatedBy, out var userName) && x.CreatedBy != "SYS")
                {
                    throw new Exception($"Không tìm thấy người tạo batch có ID: {x.CreatedBy}");
                }

                return new ImportBatchSummary
                {
                    Id = x.Id,
                    Name = x.Name,
                    EstimatedQuantity = x.EstimatedQuantity,
                    ImportedQuantity = x.ImportedCount,
                    ExpectedCompletionDate = x.ExpectedCompletionDate,
                    CompletionDate = x.CompletionDate,
                    Status = x.Status,
                    CreatedBy = userName ?? "SYS",
                    CreatedAt = x.CreatedAt
                };
            }).ToList();

            var response = new ListImportBatches
            {
                Items = results,
                Total = totalCount
            };

            _logger.LogInformation(
                $"[{nameof(ImportService)}] Successfully retrieved {results.Count} import batches out of {totalCount} total");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{nameof(ImportService)}] Error retrieving list of import batches: {ex.Message}");
            throw;
        }
    }


    public async Task<BatchImport> GetBatchImportById(string id)
    {
        var batchImport = await _context.BatchImports.FirstOrDefaultAsync(x => x.Id == id.Trim());
        if (batchImport == null)
            return null;

        return batchImport;
    }


    public async Task<bool> DeleteLivestockFromBatchImport(string importDetailId)
    {
        var importBatchDetails = await _context.BatchImportDetails.FirstOrDefaultAsync(x => x.Id == importDetailId);
        if (importBatchDetails == null)
            throw new Exception("Không tìm thấy thông tin loài vật");

        var batchImport = await _context.BatchImports.Where(x => x.Id == importBatchDetails.BatchImportId).FirstOrDefaultAsync();
        if (batchImport == null)
            throw new Exception("Không tìm thấy lô nhập của loài vật này");

        if (batchImport.Status == batch_import_status.HOÀN_THÀNH || batchImport.Status == batch_import_status.ĐÃ_HỦY)
            throw new Exception("Lô nhập này đã hoàn thành hoặc đã hủy không tồn tại");

        if (importBatchDetails.Status == batch_import_status.ĐÃ_NHẬP)
            throw new Exception("Loài vật đã được nhập không thể xóa");

        _context.BatchImportDetails.Remove(importBatchDetails);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<LivestockBatchImportInfo> GetLivestockInfoInBatchImport(string id)
    {
        var batchImportDetails = await _context.BatchImportDetails.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new Exception("Không tìm thấy chi tiết lô nhập");

        var livestock = await _context.Livestocks.FirstOrDefaultAsync(x => x.Id == batchImportDetails.LivestockId) ??
            throw new Exception("Không tìm thấy vật nuôi");

        var specie = await _context.Species.FindAsync(livestock.SpeciesId) ??
            throw new Exception("Không tìm thấy loài vật nuôi");

        var barn = await _context.Barns.FindAsync(livestock.BarnId) ??
            throw new Exception("Không tìm thấy trang trại của vật nuôi");

        var result = new LivestockBatchImportInfo
        {
            Id = id,
            InspectionCode = livestock.InspectionCode ?? "N/A",
            SpecieType = specie.Type,
            SpecieName = specie.Name,
            LiveStockStatus = livestock.Status,
            Gender = livestock.Gender,
            Color = livestock.Color,
            Weight = livestock.WeightEstimate,
            Dob = livestock.Dob,
            ImportStatus = batchImportDetails.Status,
            CreatedAt = batchImportDetails.CreatedAt,
            ImportedDate = batchImportDetails.ImportedDate,
        };
        return result;
    }

    public async Task<bool> SetLivestockDead(string bathcImportDetailsId, string requestedBy)
    {
        var batchimportDetails = await _context.BatchImportDetails.FirstOrDefaultAsync(x => x.Id == bathcImportDetailsId) ??
            throw new Exception("Không tìm thấy thông tin loài vật");

        var batchImport = await _context.BatchImports.FirstOrDefaultAsync(x => x.Id == batchimportDetails.BatchImportId) ??
            throw new Exception("Không tìm thấy lô nhập");

        if (batchImport.Status == batch_import_status.HOÀN_THÀNH || batchImport.Status == batch_import_status.ĐÃ_HỦY)
            throw new Exception("Lô nhập đã thành công hoặc bị hủy, không được phép xóa");

        var livestock = await _context.Livestocks.FirstOrDefaultAsync(x => x.Id == batchimportDetails.LivestockId) ??
            throw new Exception("Không tìm thấy vật nuôi");

        //Update Livestock
        if (batchimportDetails.ImportedDate != null)
            livestock.Status = livestock_status.XUẤT_BÁN_THỊT;

        else
            livestock.Status = livestock_status.CHẾT;

        livestock.UpdatedAt = DateTime.Now;
        livestock.UpdatedBy = requestedBy ?? "SYS";

        //Update BatchImport Details
        if (batchimportDetails.Status != batch_import_status.ĐÃ_NHẬP)
        {
            batchimportDetails.Status = batch_import_status.ĐÃ_HỦY;
        }
        batchimportDetails.UpdatedAt = DateTime.Now;
        batchimportDetails.UpdatedBy = requestedBy ?? "SYS";

        await _context.SaveChangesAsync();

        //STATUS CHANGE WARNING: Cap nhat lai trang thai batchImport status neu thieu
        //var count = await _context.BatchImportDetails
        //        .Include(l => l.Livestock)
        //        .Where(x => x.BatchImportId == batchImport.Id
        //         && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null))
        //        .CountAsync();
        //if (count < batchImport.EstimatedQuantity)
        //    batchImport.Status = batch_import_status.CHỜ_CHỌN;

        //await _context.SaveChangesAsync();

        return true;
    }

    public async Task<LivestockBatchImportInfo> AddLivestockToDetails(string batchImportId, AddImportLivestockDTO livestockAddModel)
    {
        //Check BatchImport
        var batchImport = await _context.BatchImports.FirstOrDefaultAsync(x => x.Id == batchImportId);
        if (batchImport == null)
            throw new Exception("Không tìm thấy lô nhập");

        if (batchImport.Status == batch_import_status.HOÀN_THÀNH || batchImport.Status == batch_import_status.ĐÃ_HỦY)
            throw new Exception("Không thể thêm khi đã hoàn thành hoặc bị hủy");

        var count = await _context.BatchImportDetails
                        .Include(l => l.Livestock)
                        .Where(x => x.BatchImportId == batchImportId
                         && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null))
                        .CountAsync();
        //if (count == batchImport.EstimatedQuantity)
        //    throw new Exception("Lô nhập đã đủ với số lượng dự kiến không thể thêm mới");

        //Check Livestock
        var livestock = await _context.Livestocks
                        .Include(s => s.Species)
                        .FirstOrDefaultAsync(x => x.Id == livestockAddModel.Id);
        if (livestock == null)
            throw new Exception("ID trên mã thẻ tai không tồn tại trong hệ thống.");

        var specie = await _context.Species.FirstOrDefaultAsync(x => x.Id == livestockAddModel.SpecieId);
        if (specie == null)
            throw new Exception("Không tìm thấy loài vật");

        var liveStockDuplicateCheck = await _context.BatchImportDetails.FirstOrDefaultAsync(x => x.BatchImportId == batchImportId && x.LivestockId == livestock.Id);
        if (liveStockDuplicateCheck != null)
            throw new Exception("Loài vật đã tồn tại trong danh sách");

        var liveStockCheck = await _context.BatchImportDetails.FirstOrDefaultAsync(x => x.BatchImportId != batchImportId && x.LivestockId == livestock.Id);
        if (liveStockCheck != null)
            throw new Exception("Loài vật đã tồn tại trong lô nhập khác");

        if (livestockAddModel.Dob != null && livestockAddModel.Dob > DateTime.Now)
            throw new Exception("Ngày sinh không thể quá hôm nay");

        //STATUS CHANGE WARNING: kiem tra danh sach co vat nuoi chua de chuyen status thanh dang chon
        var listBatchImportDetails = await _context.BatchImportDetails
            .Include(l => l.Livestock)
            .Where(x => x.BatchImportId == batchImportId
            && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null)).ToArrayAsync();
        //if (!listBatchImportDetails.Any())
        //    batchImport.Status = batch_import_status.ĐANG_CHỌN;

        //Add livestock info
        livestock.Id = livestockAddModel.Id;
        livestock.InspectionCode = livestockAddModel.InspectionCode;
        livestock.SpeciesId = specie.Id;
        livestock.Status = livestockAddModel.LivestockStatus ?? livestock_status.KHỎE_MẠNH;
        livestock.Gender = livestockAddModel.Gender ?? livestock_gender.ĐỰC;
        livestock.Color = livestockAddModel.Color ?? "Nâu";
        livestock.WeightOrigin = livestockAddModel.Weight ?? 100;
        livestock.Dob = livestockAddModel.Dob ?? null;
        livestock.UpdatedAt = DateTime.Now;
        livestock.UpdatedBy = livestockAddModel.RequestedBy ?? "SYS";

        //Add batchimport details
        var batchImportDetails = new BatchImportDetail();
        batchImportDetails.Id = SlugId.New();
        batchImportDetails.BatchImportId = batchImportId;
        batchImportDetails.LivestockId = livestock.Id;
        batchImportDetails.Status = batch_import_status.CHỜ_NHẬP;
        batchImportDetails.CreatedAt = DateTime.Now;
        batchImportDetails.CreatedBy = livestockAddModel.RequestedBy ?? "SYS";
        batchImportDetails.UpdatedAt = batchImport.CreatedAt;
        batchImportDetails.UpdatedBy = batchImport.CreatedBy;

        //Add ToSearchHistory
        var searchHistory = new SearchHistory();
        searchHistory.Id = SlugId.New();
        searchHistory.LivestockId = livestock.Id;
        searchHistory.CreatedAt = DateTime.Now;
        searchHistory.UpdatedAt = DateTime.Now;
        searchHistory.CreatedBy = livestockAddModel.RequestedBy ?? "SYS";
        searchHistory.UpdatedBy = livestockAddModel.RequestedBy ?? "SYS";

        //Remove if searchHistory have more than 20
        var histories = await _context.SearchHistories
            .Where(x => x.CreatedBy == livestockAddModel.RequestedBy)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        if (histories.Count >= 20)
        {
            var toRemove = histories.Skip(19);
            _context.SearchHistories.RemoveRange(toRemove);
        }

        //Add SingleVaccination if have
        if (livestockAddModel.MedicineId != null)
        {
            var medicine = await _context.Medicines.FirstOrDefaultAsync(x => x.Id == livestockAddModel.MedicineId);
            if (medicine == null)
                throw new Exception("Không tìm thấy thuốc này");

            var singleVaccination = new SingleVaccination();
            singleVaccination.Id = SlugId.New();
            singleVaccination.BatchImportId = batchImport.Id;
            singleVaccination.LivestockId = livestock.Id;
            singleVaccination.MedicineId = medicine.Id;
            singleVaccination.CreatedAt = DateTime.Now;
            singleVaccination.UpdatedAt = DateTime.Now;
            singleVaccination.CreatedBy = livestockAddModel.RequestedBy ?? "SYS";
            singleVaccination.UpdatedBy = livestockAddModel.RequestedBy ?? "SYS";

            await _context.SingleVaccination.AddAsync(singleVaccination);
        }

        //STATUS CHANGE WARNING: If last livestock is added
        //if (count + 1 == batchImport.EstimatedQuantity)
        //{
        //    batchImport.Status = batch_import_status.CHỜ_NHẬP;

            var pinnedBatchImport = await _context.PinnedBatchImports
                .FirstOrDefaultAsync(x => x.BatchImportId == batchImportId
                && x.CreatedBy == livestockAddModel.RequestedBy);
            if(pinnedBatchImport != null)
            _context.PinnedBatchImports.Remove(pinnedBatchImport);

        //Save data
        await _context.SearchHistories.AddAsync(searchHistory);
        await _context.BatchImportDetails.AddAsync(batchImportDetails);
        await _context.SaveChangesAsync();


        return new LivestockBatchImportInfo
        {
            Id = livestock.Id,
            InspectionCode = livestock.InspectionCode ?? "N/A",
            SpecieType = livestock.Species.Type,
            SpecieName = livestock.Species.Name,
            LiveStockStatus = livestock.Status,
            Gender = livestock.Gender,
            Color = livestock.Color,
            Weight = livestock.WeightOrigin,
            Dob = livestock.Dob,
            ImportStatus = batchImportDetails.Status,
            CreatedAt = batchImportDetails.CreatedAt,
            ImportedDate = batchImportDetails.ImportedDate,
        };
    }

    public async Task<LivestockBatchImportInfo> UpdateLivestockInDetails(string id, UpdateImportLivestockDTO livestockUpdateModel)
    {
        var batchImportDetails = await _context.BatchImportDetails.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new Exception("Không tìm thấy chi tiết lô nhập");

        var livestock = await _context.Livestocks
            .Include(s => s.Species)
            .FirstOrDefaultAsync(x => x.Id == batchImportDetails.LivestockId) ??
            throw new Exception("Không tìm thấy vật nuôi");

        var specie = await _context.Species.FindAsync(livestockUpdateModel.SpecieId) ??
            throw new Exception("Không tìm thấy loài vật nuôi");

        if (livestockUpdateModel.Dob != null && livestockUpdateModel.Dob > DateTime.Now)
            throw new Exception("Ngày sinh không thể quá hôm nay");

        //UpdateLivestock
        livestock.SpeciesId = livestockUpdateModel.SpecieId ?? livestock.SpeciesId;
        livestock.Gender = livestockUpdateModel.Gender ?? livestock.Gender;
        livestock.Color = livestockUpdateModel.Color ?? livestock.Color;
        livestock.WeightOrigin = livestockUpdateModel.Weight ?? livestock.WeightOrigin;
        livestock.Dob = livestockUpdateModel.Dob ?? livestock.Dob;
        livestock.UpdatedAt = DateTime.Now;
        livestock.UpdatedBy = livestockUpdateModel.RequestedBy ?? "SYS";

        //UpdateImportDetails
        batchImportDetails.UpdatedAt = DateTime.Now;
        batchImportDetails.UpdatedBy = livestockUpdateModel.RequestedBy ?? "SYS";

        await _context.SaveChangesAsync();

        return new LivestockBatchImportInfo
        {
            Id = livestock.Id,
            InspectionCode = livestock.InspectionCode ?? "N/A",
            SpecieType = livestock.Species.Type,
            SpecieName = livestock.Species.Name,
            LiveStockStatus = livestock.Status,
            Gender = livestock.Gender,
            Color = livestock.Color,
            Weight = livestock.WeightOrigin,
            Dob = livestock.Dob,
            ImportStatus = batchImportDetails.Status,
            CreatedAt = batchImportDetails.CreatedAt,
            ImportedDate = batchImportDetails.ImportedDate,
        };
    }

    public async Task<PinnedBatchImport> AddToPinImportBatch(string batchImportId, string requestedBy)
    {
        var pinBatch = await _context.PinnedBatchImports.FirstOrDefaultAsync(x => x.CreatedBy == requestedBy && x.BatchImportId == batchImportId);
        if (pinBatch != null)
            throw new Exception("Lô nhập này đã tồn tại sẵn trong mục ghim");

        var result = new PinnedBatchImport();
        result.Id = SlugId.New();
        result.BatchImportId = batchImportId;
        result.CreatedBy = requestedBy;
        result.CreatedAt = DateTime.Now;
        result.UpdatedBy = requestedBy;
        result.UpdatedAt = DateTime.Now;

        await _context.PinnedBatchImports.AddAsync(result);
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<ListPinnedImportBatches> GetListPinnedBatcImport(string userId)
    {
        var result = new ListPinnedImportBatches()
        {
            Total = 0
        };

        var pinBatch = await _context.PinnedBatchImports
            .Include(i => i.BatchImport)
            .Where(x => x.CreatedBy == userId).ToArrayAsync();

        result.Items = pinBatch
            .Select(v => new PinnedBatchImportDTO
            {
                Id = v.Id,
                BatchImportId = v.BatchImportId,
                BatchImportName = v.BatchImport.Name,
                BatchImportCompletedDate = v.BatchImport.ExpectedCompletionDate,
                CreatedAt = v.CreatedAt
            })
            .OrderByDescending(v => v.CreatedAt)
            .ToArray();
        result.Total = pinBatch.Length;
        return result;
    }

    public async Task<bool> RemoveFromPinImportBatch(string pinnedImportId, string requestedBy)
    {
        var pinnedCheck = await _context.PinnedBatchImports.FirstOrDefaultAsync(x => x.Id == pinnedImportId);
        if (pinnedCheck == null)
            throw new Exception("Không tìm thấy lô nhập này trong mục ghim");

        if (pinnedCheck.CreatedBy != requestedBy)
            throw new Exception("Chỉ người ghim lô nhập này mới được gỡ");

        _context.PinnedBatchImports.Remove(pinnedCheck);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ListOverDueImportBatches> GetListOverDueBatchImport()
    {
        var result = new ListOverDueImportBatches()
        {
            Total = 0,
        };

        var batchImport = await _context.BatchImports
            .Where(x => x.ExpectedCompletionDate < DateTime.Now
            && x.ExpectedCompletionDate != null
            && x.CompletionDate == null
            && x.Status != batch_import_status.HOÀN_THÀNH
            || x.Status != batch_import_status.ĐÃ_HỦY
            && !_context.PinnedBatchImports.Any(p => p.BatchImportId == x.Id))
            .ToArrayAsync();

        result.Items = batchImport
            .Select(v => new OverDueBatchImportDTO
            {
                BatchImportId = v.Id,
                BatchImportName = v.Name,
                DayOver = v.ExpectedCompletionDate.HasValue
                ? "Quá " + Math.Abs(ComputeDaysLeft(v.ExpectedCompletionDate.Value)).ToString() + " ngày"
                : "Không có ngày",
                BatchImportCompletedDate = v.ExpectedCompletionDate,
                CreatedAt = v.CreatedAt
            })
            .OrderByDescending(v => v.DayOver)
            .ToArray();
        result.Total = batchImport.Length;
        return result;
    }

    public async Task<ListMissingImportBatches> GetListMissingBatchImport()
    {
        var result = new ListMissingImportBatches()
        {
            Total = 0,
        };

        var batchImports = await _context.BatchImports
            .Where(x => x.Status == batch_import_status.CHỜ_NHẬP)
            .ToListAsync();

        // Duyệt từng batch và tạo danh sách DTO bất đồng bộ
        var tasks = batchImports.Select(async v =>
        {
            var count = await _context.BatchImportDetails
                .Include(l => l.Livestock)
                .Where(x => x.BatchImportId == v.Id
                 && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null)
                 && !_context.PinnedBatchImports.Any(p => p.BatchImportId == x.Id))
                .CountAsync();

            return new MissingBatchImportDTO
            {
                BatchImportId = v.Id,
                BatchImportName = v.Name,
                TotalMissing = "Thiếu " + (v.EstimatedQuantity - count) + " con",
                BatchImportCompletedDate = v.ExpectedCompletionDate,
                CreatedAt = v.CreatedAt
            };
        });

        var dtoList = await Task.WhenAll(tasks);

        result.Items = dtoList
            .OrderByDescending(v => v.TotalMissing)
            .ToArray();

        result.Total = dtoList.Length;
        return result;
    }

    public async Task<ListNearDueImportBatches> GetListNearDueBatchImport(int num)
    {
        var result = new ListNearDueImportBatches()
        {
            Total = 0,
        };

        var batchImport = await _context.BatchImports
            .Where(x => x.ExpectedCompletionDate > DateTime.Now
            && x.ExpectedCompletionDate <= DateTime.Now.AddDays(num)
            && x.ExpectedCompletionDate != null
            && x.CompletionDate == null
            && x.Status != batch_import_status.HOÀN_THÀNH
            && x.Status != batch_import_status.ĐÃ_HỦY
            && !_context.PinnedBatchImports.Any(p => p.BatchImportId == x.Id))
            .ToArrayAsync();

        result.Items = batchImport
            .Select(v => new NearDueBatchImportDTO
            {
                BatchImportId = v.Id,
                BatchImportName = v.Name,
                // Nếu null thì hiển thị "Không có ngày", không null thì tính daysLeft
                Dayleft = v.ExpectedCompletionDate.HasValue
                ? (ComputeDaysLeft(v.ExpectedCompletionDate.Value) <= 0
                    ? "Hôm nay"
                    : "Còn " + ComputeDaysLeft(v.ExpectedCompletionDate.Value).ToString()) + " ngày"
                : "Không có ngày",
                BatchImportCompletedDate = v.ExpectedCompletionDate,
                CreatedAt = v.CreatedAt
            })
            .OrderByDescending(v => v.Dayleft)
            .ToArray();
        result.Total = batchImport.Length;
        return result;
    }

    public async Task<ListUpcomingImportBatches> GetListUpcomingBatchImport(int num)
    {
        var result = new ListUpcomingImportBatches()
        {
            Total = 0,
        };

        var batchImport = await _context.BatchImports
            .Where(x => x.ExpectedCompletionDate > DateTime.Now
            && x.ExpectedCompletionDate <= DateTime.Now.AddDays(num)
            && x.ExpectedCompletionDate != null
            && x.CompletionDate == null
            && x.Status != batch_import_status.HOÀN_THÀNH
            && x.Status != batch_import_status.ĐÃ_HỦY
            && !_context.PinnedBatchImports.Any(p => p.BatchImportId == x.Id))
            .ToArrayAsync();

        //GetListNearDueBatchImport(num);

        result.Items = batchImport
            .Select(v => new UpcomingBatchImportDTO
            {
                BatchImportId = v.Id,
                BatchImportName = v.Name,
                Dayleft = "1",
                BatchImportCompletedDate = v.ExpectedCompletionDate,
                CreatedAt = v.CreatedAt
            })
            .OrderByDescending(v => v.Dayleft)
            .ToArray();
        result.Total = batchImport.Length;
        return result;
    }

    private static int ComputeDaysLeft(DateTime expectedCompletion)
    {
        var s = expectedCompletion.ToString("yyyy-MM-dd");
        var y = int.Parse(s.Substring(0, 4));
        var m = int.Parse(s.Substring(5, 2));
        var d = int.Parse(s.Substring(8, 2));

        var expectedDateOnly = new DateTime(y, m, d);
        var todayOnly = DateTime.Now.Date;
        return (expectedDateOnly - todayOnly).Days;
    }

    //public async Task<BatchImportScanDTO> GetBatchImportScanDetails(string batchImportId)
    //{
    //    var batchImport = await _context.BatchImports.FirstOrDefaultAsync(x => x.Id == batchImportId);
    //    if (batchImport == null)
    //        throw new Exception("Không tìm thấy lô nhập");

    //    var importedQuantity = await _context.BatchImportDetails
    //        .Include(l => l.Livestock)
    //        .Where(x => x.BatchImportId == batchImportId
    //            && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null))
    //        .CountAsync();

    //    var batchImportView = new BatchImportScanDTO
    //    {
    //        BatchImportId = batchImportId,
    //        BatchImportName = batchImport.Name,
    //        Taken = importedQuantity,
    //        Total = batchImport.EstimatedQuantity,
    //    };

    //    return batchImportView;
    //}

    public async Task<ListSearchHistory> GetListSearchHistory(string userId)
    {
        var result = new ListSearchHistory
        {
            Total = 0
        };

        // 1. Lấy toàn bộ search history (kèm livestock + species)
        var listSearchHistories = await _context.SearchHistories
            .Include(l => l.Livestock)
                .ThenInclude(s => s.Species)
            .Where(x => x.CreatedBy == userId)
            .ToArrayAsync();

        // 2. Lấy danh sách livestockId
        var livestockIds = listSearchHistories
            .Select(x => x.LivestockId)
            .Distinct()
            .ToList();

        // 3. Truy vấn SingleVaccination và tạo Dictionary
        var livestockMedicineMap = await _context.SingleVaccination
            .Where(sv => livestockIds.Contains(sv.LivestockId))
            .GroupBy(sv => sv.LivestockId)
            .Select(g => new
            {
                LivestockId = g.Key,
                MedicineId = g.FirstOrDefault().MedicineId // hoặc LastOrDefault() nếu bạn muốn thuốc mới nhất
            })
            .ToDictionaryAsync(x => x.LivestockId, x => x.MedicineId);

        // 4. Ánh xạ dữ liệu
        result.Items = listSearchHistories
            .Select(v => new SearchHistoryBatchImport
            {
                CreatedAt = v.CreatedAt,
                InspectionCode = v.Livestock.InspectionCode,
                SpecieName = v.Livestock.Species.Name,
                MedicineId = livestockMedicineMap.TryGetValue(v.LivestockId, out var medId) ? medId : null,
                Gender = v.Livestock.Gender,
                Color = v.Livestock.Color,
                Weight = v.Livestock.WeightOrigin,
                Dob = v.Livestock.Dob,
            })
            .OrderByDescending(v => v.CreatedAt)
            .ToArray();

        result.Total = listSearchHistories.Length;
        return result;
    }

    public async Task<bool> ConfirmImportedToBarn(string livestockId, string requestedBy)
    {
        var livestock = await _context.Livestocks.FirstOrDefaultAsync(x => x.Id == livestockId);
        if (livestock == null)
            throw new Exception("Không tìm thấy vật nuôi");

        if (livestock.Status == livestock_status.CHẾT)
            throw new Exception("Loài vật đã chết trước khi nhập, không thể nhập");

        var batchImportDetail = await _context.BatchImportDetails
            .Include(i => i.BatchImport)
            .FirstOrDefaultAsync(x => x.LivestockId == livestockId);
        if (batchImportDetail == null)
            throw new Exception("Loài vật này không tồn tại trong lô nhập nào");

        if (batchImportDetail.ImportedDate != null)
            throw new Exception("Loài vật đã được nhập");

        //Update batchImport Details
        batchImportDetail.ImportedDate = DateTime.Now;
        batchImportDetail.Status = batch_import_status.ĐÃ_NHẬP;
        batchImportDetail.UpdatedBy = requestedBy ?? "SYS";
        batchImportDetail.UpdatedAt = DateTime.Now;

        //Update BatchImport
        batchImportDetail.BatchImport.Status = batch_import_status.ĐANG_NHẬP;
        batchImportDetail.BatchImport.UpdatedAt = DateTime.Now;

        var countImported = await _context.BatchImportDetails
            .Where(x => x.BatchImportId == batchImportDetail.BatchImportId
            && x.ImportedDate != null)
            .CountAsync();

        var totalLivestockInBatch = await _context.BatchImportDetails
            .Include(l => l.Livestock)
            .Where(x => x.BatchImportId == batchImportDetail.BatchImportId
                && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null))
            .CountAsync();

        //STATUS CHANGE WARNING: Update status when imported all  
        if (countImported + 1 == totalLivestockInBatch)
        {
            batchImportDetail.BatchImport.Status = batch_import_status.HOÀN_THÀNH;
            batchImportDetail.BatchImport.CompletionDate = DateTime.Now;
            batchImportDetail.BatchImport.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LivestockBatchImportScanInfo> GetLivestockScanInfo(string id)
    {
        var livestock = await _context.Livestocks.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new Exception("Không tìm thấy vật nuôi");

        var batchImportDetails = await _context.BatchImportDetails
                .Include(l => l.Livestock)
                .Include(i => i.BatchImport)
                .FirstOrDefaultAsync(x => x.Livestock.Id == livestock.Id) ??
            throw new Exception("Không tìm thấy chi tiết lô nhập");

        var specie = await _context.Species.FindAsync(livestock.SpeciesId) ??
            throw new Exception("Không tìm thấy loài vật nuôi");

        var barn = await _context.Barns.FindAsync(livestock.BarnId) ??
            throw new Exception("Không tìm thấy trang trại của vật nuôi");

        //So luong loai vat dc chon
        var totalLivestockInBatch = await _context.BatchImportDetails
                .Include(l => l.Livestock)
                .Where(x => x.BatchImportId == batchImportDetails.BatchImportId
                 && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null))
                .CountAsync();

        //So luong loai vat da xac nhan
        var count = await _context.BatchImportDetails
            .Include(l => l.Livestock)
            .Where(x => x.BatchImportId == batchImportDetails.BatchImportId
            && x.ImportedDate != null)
            .CountAsync();

        var result = new LivestockBatchImportScanInfo
        {
            LivestockId = livestock.Id,
            BatchImportName = batchImportDetails.BatchImport.Name,
            InspectionCode = livestock.InspectionCode ?? "N/A",
            SpecieName = specie.Name,
            Gender = livestock.Gender,
            Color = livestock.Color,
            Weight = livestock.WeightEstimate,
            Dob = livestock.Dob,
            CreatedAt = batchImportDetails.CreatedAt,
            Total = totalLivestockInBatch,
            Imported = count,
        };
        return result;
    }
    public async Task<LivestockBatchImportScanInfo> ConfrimReplaceDeadLiveStock(string batchImportId, AddImportLivestockDTO livestockAddModel)
    {
        //Check BatchImport
        var batchImport = await _context.BatchImports.FirstOrDefaultAsync(x => x.Id == batchImportId);
        if (batchImport == null)
            throw new Exception("Không tìm thấy lô nhập");

        if (batchImport.Status == batch_import_status.HOÀN_THÀNH || batchImport.Status == batch_import_status.ĐÃ_HỦY)
            throw new Exception("Không thể thêm khi đã hoàn thành hoặc bị hủy");

        var totalLivestockInBatch = await _context.BatchImportDetails
                        .Include(l => l.Livestock)
                        .Where(x => x.BatchImportId == batchImportId
                         && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null))
                        .CountAsync();
        //if (count == batchImport.EstimatedQuantity)
        //    throw new Exception("Lô nhập đã xác nhận nhập đủ với số lượng dự kiến");

        //Check xem co loai vat chet va chua nhap trong lo chua
        var batchImportCheck = await _context.BatchImportDetails
            .Include(l => l.Livestock)
            .Where(x => x.BatchImportId == batchImportId
            && x.Livestock.Status == livestock_status.CHẾT
            && x.ImportedDate == null)
            .ToArrayAsync();
        if (!batchImportCheck.Any())
            throw new Exception("Lô này không có loài vật nào cần thay thế");

        //Check Livestock
        var livestock = await _context.Livestocks
                        .Include(s => s.Species)
                        .FirstOrDefaultAsync(x => x.Id == livestockAddModel.Id);
        if (livestock == null)
            throw new Exception("ID trên mã thẻ tai không tồn tại trong hệ thống.");

        var specie = await _context.Species.FirstOrDefaultAsync(x => x.Id == livestockAddModel.SpecieId);
        if (specie == null)
            throw new Exception("Không tìm thấy loài vật");

        var liveStockDuplicateCheck = await _context.BatchImportDetails
            .FirstOrDefaultAsync(x => x.BatchImportId == batchImportId
            && x.LivestockId == livestock.Id);
        if (liveStockDuplicateCheck != null)
            throw new Exception("Loài vật đã tồn tại trong danh sách");

        var liveStockCheck = await _context.BatchImportDetails
            .FirstOrDefaultAsync(x => x.BatchImportId != batchImportId
            && x.LivestockId == livestock.Id);
        if (liveStockCheck != null)
            throw new Exception("Loài vật đã tồn tại trong lô nhập khác");

        if (livestockAddModel.Dob != null && livestockAddModel.Dob > DateTime.Now)
            throw new Exception("Ngày sinh không thể quá hôm nay");

        var countImported = await _context.BatchImportDetails
            .Where(x => x.BatchImportId == batchImportId
            && x.ImportedDate != null)
            .CountAsync();


        //Add livestock info
        livestock.Id = livestockAddModel.Id;
        livestock.InspectionCode = livestockAddModel.InspectionCode;
        livestock.SpeciesId = specie.Id;
        livestock.Status = livestockAddModel.LivestockStatus ?? livestock_status.KHỎE_MẠNH;
        livestock.Gender = livestockAddModel.Gender ?? livestock_gender.ĐỰC;
        livestock.Color = livestockAddModel.Color ?? "Nâu";
        livestock.WeightOrigin = livestockAddModel.Weight ?? 100;
        livestock.Dob = livestockAddModel.Dob ?? null;
        livestock.UpdatedAt = DateTime.Now;
        livestock.UpdatedBy = livestockAddModel.RequestedBy ?? "SYS";

        //Add batchimport details
        var batchImportDetails = new BatchImportDetail();
        batchImportDetails.Id = SlugId.New();
        batchImportDetails.BatchImportId = batchImportId;
        batchImportDetails.LivestockId = livestock.Id;
        batchImportDetails.Status = batch_import_status.ĐÃ_NHẬP;
        batchImportDetails.ImportedDate = DateTime.Now;
        batchImportDetails.CreatedAt = DateTime.Now;
        batchImportDetails.CreatedBy = livestockAddModel.RequestedBy ?? "SYS";
        batchImportDetails.UpdatedAt = batchImport.CreatedAt;
        batchImportDetails.UpdatedBy = batchImport.CreatedBy;

        //Add ToSearchHistory
        var searchHistory = new SearchHistory();
        searchHistory.Id = SlugId.New();
        searchHistory.LivestockId = livestock.Id;
        searchHistory.CreatedAt = DateTime.Now;
        searchHistory.UpdatedAt = DateTime.Now;
        searchHistory.CreatedBy = livestockAddModel.RequestedBy ?? "SYS";
        searchHistory.UpdatedBy = livestockAddModel.RequestedBy ?? "SYS";

        //Remove if searchHistory have more than 20
        var histories = await _context.SearchHistories
            .Where(x => x.CreatedBy == livestockAddModel.RequestedBy)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        if (histories.Count >= 20)
        {
            var toRemove = histories.Skip(19);
            _context.SearchHistories.RemoveRange(toRemove);
        }

        //STATUS CHANGE WARNING: If last livestock is confirm
        if (countImported + 1 == totalLivestockInBatch)
        {
            batchImport.Status = batch_import_status.HOÀN_THÀNH;
            batchImport.CompletionDate = DateTime.Now;
            batchImport.UpdatedAt = DateTime.Now;
        }

        //Save data
        await _context.SearchHistories.AddAsync(searchHistory);
        await _context.BatchImportDetails.AddAsync(batchImportDetails);
        await _context.SaveChangesAsync();

        var imported = await _context.BatchImportDetails
            .Include(l => l.Livestock)
            .Where(x => x.BatchImportId == batchImportDetails.BatchImportId
            && x.ImportedDate != null)
            .CountAsync();

        return new LivestockBatchImportScanInfo
        {
            LivestockId = livestock.Id,
            BatchImportName = batchImport.Name,
            InspectionCode = livestock.InspectionCode ?? "N/A",
            SpecieName = specie.Name,
            Gender = livestock.Gender,
            Color = livestock.Color,
            Weight = livestock.WeightOrigin,
            Dob = livestock.Dob,
            CreatedAt = batchImportDetails.CreatedAt,
            Total = totalLivestockInBatch,
            Imported = imported
        };
    }

    public async Task<bool> SetForSaleLivestock(string livestockId, string requestedBy)
    {
        var livestock = await _context.Livestocks.FirstOrDefaultAsync(x => x.Id == livestockId);
        if (livestock == null)
            throw new Exception("Không tìm thấy vật nuôi");

        if (livestock.Status == livestock_status.CHẾT)
            throw new Exception("Loài vật đã chết trước khi nhập, không thể nhập");

        var batchImportDetail = await _context.BatchImportDetails
            .Include(i => i.BatchImport)
            .FirstOrDefaultAsync(x => x.LivestockId == livestockId);
        if (batchImportDetail == null)
            throw new Exception("Loài vật này không tồn tại trong lô nhập nào");

        if (batchImportDetail.ImportedDate != null)
            throw new Exception("Loài vật đã được nhập");

        //Update livestock
        livestock.Status = livestock_status.XUẤT_BÁN_THỊT;
        livestock.UpdatedAt = DateTime.Now;
        livestock.UpdatedBy = requestedBy;

        //Update batchImport Details
        batchImportDetail.ImportedDate = DateTime.Now;
        batchImportDetail.Status = batch_import_status.ĐÃ_NHẬP;
        batchImportDetail.UpdatedBy = requestedBy ?? "SYS";
        batchImportDetail.UpdatedAt = DateTime.Now;

        //Update BatchImport
        batchImportDetail.BatchImport.Status = batch_import_status.ĐANG_NHẬP;
        batchImportDetail.BatchImport.UpdatedAt = DateTime.Now;

        //STATUS CHANGE WARNING: Update status when imported all 
        var countImported = await _context.BatchImportDetails
            .Where(x => x.BatchImportId == batchImportDetail.BatchImportId
            && x.ImportedDate != null)
            .CountAsync();

        var totalLivestockInBatch = await _context.BatchImportDetails
                .Include(l => l.Livestock)
                .Where(x => x.BatchImportId == batchImportDetail.BatchImportId
                 && (x.Livestock.Status != livestock_status.CHẾT || x.ImportedDate != null))
                .CountAsync();

        if (countImported + 1 == totalLivestockInBatch)
        {
            batchImportDetail.BatchImport.Status = batch_import_status.HOÀN_THÀNH;
            batchImportDetail.BatchImport.CompletionDate = DateTime.Now;
            batchImportDetail.BatchImport.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ListImportBatchesForAdmin> GetListBatchImportForAdmin(ListImportBatchesFilter filter)
    {
        var result = new ListImportBatchesForAdmin
        {
            Total = 0
        };

        // Bước 1: Tạo truy vấn IQueryable
        var query = _context.BatchImports.AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToUpper();
                query = query.Where(v => v.Name.ToUpper().Contains(keyword));
            }

            if (filter.FromDate != null && filter.FromDate != DateTime.MinValue)
            {
                query = query.Where(v => v.CreatedAt >= filter.FromDate);
            }

            if (filter.ToDate != null && filter.ToDate != DateTime.MinValue)
            {
                query = query.Where(v => v.CreatedAt <= filter.ToDate);
            }

            if (filter.Status != null)
            {
                query = query.Where(v => v.Status == filter.Status);
            }
        }

        result.Total = await query.CountAsync();

        // Bước 2: Lấy dữ liệu phân trang và map sang DTO trong 1 truy vấn duy nhất
        var pagedItems = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip(filter?.Skip ?? 0)
            .Take(filter?.Take ?? 10)
            .Select(v => new ImportSum
            {
                Id = v.Id,
                Name = v.Name,
                EstimatedQuantity = v.EstimatedQuantity,
                ExpectedCompletionDate = v.ExpectedCompletionDate,
                Status = v.Status,
                CompletionDate = v.CompletionDate,
                CreatedAt = v.CreatedAt,

                // Lấy count ngay trong select
                ImportedQuantity = _context.BatchImportDetails
                    .Where(x => x.BatchImportId == v.Id && x.ImportedDate != null)
                    .Count(),

                // Lấy PinnedId nếu có
                PinnedId = _context.PinnedBatchImports
                    .Where(x => x.BatchImportId == v.Id)
                    .Select(x => x.Id)
                    .FirstOrDefault()
            })
            .ToListAsync();

        result.Items = pagedItems.ToArray();
        return result;
    }
}
