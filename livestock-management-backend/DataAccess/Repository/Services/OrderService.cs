using AutoMapper;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services
{
    public class OrderService : IOrderRepository
    {
        private readonly LmsContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICloudinaryRepository _cloudinaryService;

        public OrderService(LmsContext context, IMapper mapper, ICloudinaryRepository cloudinaryService)
        {
            _mapper = mapper;
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<LivestockBatchImportInfo> AddLivestockToOrder(string orderId, AddLivestockToOrderDTO model)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán");

            if (order.Status == order_status.HOÀN_THÀNH || order.Status == order_status.ĐÃ_HỦY)
                throw new Exception("Đơn đã hoàn thành hoặc bị hủy, không thể chọn ");

            //Check Order reached the limit of livestocks
            var livestockQuantity = await _context.OrderRequirements
                .Where(o => o.OrderId == orderId)
                .SumAsync(x => (int?)x.Quantity);
            var livestockInOrder = await _context.OrderDetails
                .Where(x => x.OrderId == orderId)
                .CountAsync();
            if (livestockQuantity == livestockInOrder)
                throw new Exception("Đơn đã đủ không thể thêm");

            var livestock = await _context.Livestocks
                .Include(s => s.Species)
                .FirstOrDefaultAsync(x => x.Id == model.LivestockId);
            if (livestock == null)
                throw new Exception("Không tìm thấy thông tin vật nuôi");

            //Check Livestock in our barn or not
            //var checkLivestockHasImported = await _context.BatchImportDetails
            //    .FirstOrDefaultAsync(x => x.LivestockId == model.LivestockId
            //    && x.ImportedDate != null);
            //if (checkLivestockHasImported == null)
            //    throw new Exception("Loài vật chưa được nhập về trại không thể thêm");

            //Check duplicated livestock
            var oderDetailsCheck = await _context.OrderDetails
                .FirstOrDefaultAsync(x => x.LivestockId == model.LivestockId
                && x.OrderId == orderId);
            if (oderDetailsCheck != null)
                throw new Exception("Loài vật đã tồn tại trong đơn bán này");

            //Check livestock is dead
            if (livestock.Status == livestock_status.CHẾT)
                throw new Exception("Loài vật đã chết không thể chọn");

            //Check livestock in other procurement
            var checkBatchExportDetails = await _context.BatchExportDetails
                .Include(b => b.BatchExport)
                    .ThenInclude(p => p.ProcurementPackage)
                .FirstOrDefaultAsync(x => x.LivestockId == model.LivestockId
                && x.ExportDate != null
                && x.BatchExport.ProcurementPackage.Status != procurement_status.ĐÃ_HỦY);
            if (checkBatchExportDetails != null)
                throw new Exception("Loài vật đã tồn tại trong gói thầu khác");

            //Check livestock in other order
            var checkOrder = await _context.OrderDetails
                .Include(o => o.Order)
                .FirstOrDefaultAsync(x => x.LivestockId == model.LivestockId
                && x.OrderId != orderId
                && x.ExportedDate != null
                && x.Order.Status != order_status.ĐÃ_HỦY);
            if (checkOrder != null)
                throw new Exception("Loài vật đã tồn tại trong đơn khác");

            // Lấy danh sách yêu cầu của đơn hàng
            var requirements = await _context.OrderRequirements
                .Where(r => r.OrderId == orderId)
                .ToListAsync();

            OrderRequirement matchedReq = null;

            foreach (var req in requirements)
            {
                // So khớp SpecieId
                if (livestock.Species.Id != req.SpecieId) continue;

                // Kiểm tra WeightFrom/WeightTo nếu có
                if (req.WeightFrom.HasValue && livestock.WeightOrigin < req.WeightFrom) continue;
                if (req.WeightTo.HasValue && livestock.WeightOrigin > req.WeightTo) continue;

                // Đếm số con đã chọn cho yêu cầu này
                var countSelected = await _context.OrderDetails
                    .CountAsync(d =>
                        d.OrderId == orderId
                        && d.Livestock.Species.Id == req.SpecieId
                        && (!req.WeightFrom.HasValue || d.Livestock.WeightOrigin >= req.WeightFrom)
                        && (!req.WeightTo.HasValue || d.Livestock.WeightOrigin <= req.WeightTo)
                    );

                if (countSelected >= req.Quantity) continue;

                matchedReq = req;
                break;
            }

            if (matchedReq == null)
                throw new Exception("Không có yêu cầu nào phù hợp hoặc đã đủ số lượng");

            var checkOrderDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId)
                .ToArrayAsync();
            if (!checkOrderDetails.Any())
            {
                order.Status = order_status.ĐANG_CHUẨN_BỊ;
                order.StartPrepareAt = DateTime.Now;
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = model.RequestedBy ?? "SYS";
            }

            var orderDetails = new OrderDetail();

            orderDetails.Id = SlugId.New();

            orderDetails.OrderId = orderId;
            orderDetails.LivestockId = model.LivestockId;
            orderDetails.ExportedDate = null;

            orderDetails.CreatedAt = DateTime.Now;
            orderDetails.UpdatedAt = DateTime.Now;
            orderDetails.UpdatedBy = model.RequestedBy ?? "SYS";
            orderDetails.CreatedBy = model.RequestedBy ?? "SYS";

            await _context.OrderDetails.AddAsync(orderDetails);

            var livestockChooseInOrder = await _context.OrderDetails
                .Where(x => x.OrderId == orderId
                && x.ExportedDate == null)
                .CountAsync();

            if (livestockChooseInOrder + 1 == livestockQuantity)
            {
                order.Status = order_status.CHỜ_BÀN_GIAO;
                order.AwaitDeliverAt = DateTime.Now;
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = model.RequestedBy ?? "SYS";
            }

            await _context.SaveChangesAsync();

            return new LivestockBatchImportInfo
            {
                Id = model.LivestockId,
                InspectionCode = livestock.InspectionCode,
                SpecieType = livestock.Species.Type,
                SpecieName = livestock.Species.Name,
                LiveStockStatus = livestock.Status,
                Gender = livestock.Gender,
                Color = livestock.Color,
                Weight = livestock.WeightOrigin,
                Dob = livestock.Dob
            };
        }

        public async Task<bool> CancelOrder(string orderId, string? requestedBy)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán");

            if (order.Status == order_status.HOÀN_THÀNH || order.Status == order_status.ĐANG_BÀN_GIAO)
                throw new Exception("Không được hủy, đơn đang trong quá trình bàn giao hoặc đã hoàn thành");

            order.Status = order_status.ĐÃ_HỦY;
            order.CancelledAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;
            order.Type = order_type.KHÔNG_YÊU_CẦU;
            order.CancelledAt = DateTime.Now;
            order.UpdatedBy = requestedBy ?? "SYS";

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CompletelOrder(string orderId, string? requestedBy)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán");

            if (order.Status == order_status.ĐÃ_HỦY)
                throw new Exception("Không được hoàn thành, đơn đã bị hủy");

            var livestockQuantity = await _context.OrderRequirements
                .Where(o => o.OrderId == orderId)
                .SumAsync(x => (int?)x.Quantity);
            var listOrderDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId)
                .CountAsync();
            if (livestockQuantity > listOrderDetails)
                throw new Exception("Chưa đủ số lượng vật nuôi, không thể hoàn thành");

            var livestockInOrder = await _context.OrderDetails
                .Include(l => l.Livestock)
                .Where(x => x.OrderId == orderId
                && x.ExportedDate == null)
                .ToArrayAsync();
            foreach (var item in livestockInOrder)
            {
                item.Livestock.Status = livestock_status.ĐÃ_XUẤT;
                item.ExportedDate = DateTime.Now;
                item.UpdatedAt = DateTime.Now;
                item.UpdatedBy = requestedBy ?? "SYS";
            }

            order.Status = order_status.HOÀN_THÀNH;
            order.CompletedAt = DateTime.Now;
            order.Type = order_type.KHÔNG_YÊU_CẦU;
            order.UpdatedAt = DateTime.Now;
            order.UpdatedBy = requestedBy ?? "SYS";

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmExport(string orderId, string? requestedBy)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán");

            if (order.Status == order_status.HOÀN_THÀNH || order.Status == order_status.ĐÃ_HỦY)
                throw new Exception("Đơn đã hoàn thành hoặc đã hủy, không còn xác nhận xuất");

            //Check Order Havent Choose
            var checkDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId)
                .ToArrayAsync();
            if (!checkDetails.Any())
                throw new Exception("Đơn bán chưa có vật nuôi không thể xác nhận");

            //Check Order have at least 1 Exportable Livestock
            var orderDetails = await _context.OrderDetails
                    .Include(l => l.Livestock)
                .Where(x => x.OrderId == orderId
                && x.ExportedDate == null)
                .ToArrayAsync();
            if (orderDetails == null)
                throw new Exception("Chưa có loài vật nào trong danh sách để xuất bán");

            //Update ExportedDate for each Livestock
            foreach (var item in orderDetails)
            {
                //Update livestock
                item.Livestock.Status = livestock_status.ĐÃ_XUẤT;
                item.Livestock.UpdatedAt = DateTime.Now;
                item.Livestock.UpdatedBy = requestedBy ?? "SYS";

                //Update orderDetails
                item.ExportedDate = DateTime.Now;
                item.UpdatedAt = DateTime.Now;
                item.UpdatedBy = requestedBy ?? "SYS";
            }

            //Update Order Status
            order.Status = order_status.ĐANG_BÀN_GIAO;
            order.StartDeliverAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;
            order.UpdatedBy = requestedBy ?? "SYS";

            await _context.SaveChangesAsync();

            //Check Total Exported 
            var livestockQuantity = await _context.OrderRequirements
               .Where(o => o.OrderId == orderId)
               .SumAsync(x => (int?)x.Quantity);
            var listOrderDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId
                && x.ExportedDate != null)
                .CountAsync();
            if (listOrderDetails == livestockQuantity)
            {
                order.Status = order_status.HOÀN_THÀNH;
                order.CompletedAt = DateTime.Now;
                order.Type = order_type.KHÔNG_YÊU_CẦU;
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = requestedBy ?? "SYS";
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GetReportedFile(string orderId)
        {
            var order = await _context.Orders
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(x => x.Id == orderId) ??
             throw new Exception("Không tìm thấy gói thầu");

            var orderRequirement = await _context.OrderRequirements
                    .Include(s => s.Species)
                .Where(x => x.OrderId == orderId)
                .ToArrayAsync();
            if (orderRequirement == null)
                throw new Exception("Không tìm thấy yêu cầu theo đơn này");

            var oderDetails = await _context.OrderDetails
                .Include(l => l.Livestock)
                    .ThenInclude(s => s.Species)
                .Where(x => x.OrderId == orderId)
                .ToArrayAsync();
            if (oderDetails == null || !oderDetails.Any())
                throw new Exception("Không tìm thấy thông tin vật nuôi trong đơn này");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new MemoryStream());
            var worksheet = package.Workbook.Worksheets.Add($"{order.Code}");

            var richTextRow1 = worksheet.Cells["A1"].RichText;
            var richTextRow2 = worksheet.Cells["A2"].RichText;
            var richTextRow3 = worksheet.Cells["A3"].RichText;
            var richTextRow4 = worksheet.Cells["A4"].RichText;

            var boldSegmentRow1 = richTextRow1.Add("Hợp tác xã dịch vụ tổng hợp và SXNN Lúa Vàng");
            boldSegmentRow1.Bold = false;
            //richTextRow1.Add($"{order.Code}");
            var boldSegmentRow2 = richTextRow2.Add("Hồng Giang, Đức Giang, Yên Dũng, Bắc Giang");
            boldSegmentRow2.Bold = false;
            //richTextRow2.Add($"{order.Customer.Fullname}");
            var boldSegmentRow3 = richTextRow3.Add("BẢNG KÊ XUẤT HÀNG");
            boldSegmentRow3.Bold = true;
            boldSegmentRow3.Size = 16;
            var boldSegmentRow4 = richTextRow4.Add("Tên khách hàng: ");
            boldSegmentRow4.Bold = true;
            richTextRow4.Add($"{order.Customer.Fullname}");

            var columns = new string[] { "STT", "Loài vật nuôi", "MTT", "Trọng lượng nhập", "Trọng lượng xuất", "Ngày nhập", "Ngày xuất", "Thời gian nuôi" };
            var data = new DataTable();

            ////InsertData
            data.Columns.AddRange(columns.Select(o => new DataColumn(o)).ToArray());

            int stt = 1;
            foreach (var item in oderDetails)
            {
                var livestock = item.Livestock;
                var batchImportDetail = await _context.BatchImportDetails
                    .Where(x => x.LivestockId == livestock.Id && x.ImportedDate != null)
                    .OrderBy(x => x.ImportedDate)
                    .FirstOrDefaultAsync();

                var importedDate = batchImportDetail?.ImportedDate;
                var exportedDate = item.ExportedDate;
                int? days = null;
                if (importedDate.HasValue && exportedDate.HasValue)
                    days = (exportedDate.Value.Date - importedDate.Value.Date).Days;

                data.Rows.Add(
                    stt++,
                    livestock.Species?.Name ?? "",
                    livestock.InspectionCode ?? "",
                    livestock.WeightOrigin?.ToString("0.##") ?? "",
                    livestock.WeightExport?.ToString("0.##") ?? livestock.WeightOrigin.ToString(),
                    importedDate?.ToString("dd/MM/yyyy") ?? "",
                    exportedDate?.ToString("dd/MM/yyyy") ?? "",
                    days?.ToString() ?? ""
                );
            }

            // Load data into worksheet
            worksheet.Cells["A6"].LoadFromDataTable(data, true, TableStyles.Light1);

            // Optional: Format columns (e.g., for text, numbers, dates)
            worksheet.Column(3).Style.Numberformat.Format = "@"; // MTT as text
            worksheet.Column(4).Style.Numberformat.Format = "#,##0.##"; // Trọng lượng nhập
            worksheet.Column(5).Style.Numberformat.Format = "#,##0.##"; // Trọng lượng xuất
            worksheet.Column(6).Style.Numberformat.Format = "dd/MM/yyyy"; // Ngày nhập
            worksheet.Column(7).Style.Numberformat.Format = "dd/MM/yyyy"; // Ngày xuất
            worksheet.Column(8).Style.Numberformat.Format = "0"; // Thời gian nuôi

            // Optionally, auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            //Finish Insert

            using var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0; // Quan trọng: reset vị trí về đầu stream

            var fileUrl = await _cloudinaryService.UploadFileStreamAsync(CloudFolderFileReportsName, "Thong_tin_don_ban_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx", stream);
            return fileUrl;
        }

        public async Task<string> GetTemplateToChooseLivestock(string orderId)
        {
            var order = await _context.Orders
      .Include(c => c.Customer)
      .FirstOrDefaultAsync(x => x.Id == orderId)
      ?? throw new Exception("Không tìm thấy gói thầu");

            var orderRequirement = await _context.OrderRequirements
                .Include(s => s.Species)
                .Where(x => x.OrderId == orderId)
                .FirstOrDefaultAsync();
            if (orderRequirement == null)
                throw new Exception("Không tìm thấy yêu cầu theo đơn này");

            // Get species list for dropdown
            var speciesList = await _context.Species
                .Select(s => s.Name)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new MemoryStream());
            var worksheet = package.Workbook.Worksheets.Add($"{order.Code}");

            // Info rows
            var richTextRow1 = worksheet.Cells["A1"].RichText;
            var richTextRow2 = worksheet.Cells["A2"].RichText;
            var richTextRow3 = worksheet.Cells["A3"].RichText;

            var boldSegmentRow1 = richTextRow1.Add("MÃ ĐƠN BÁN: ");
            boldSegmentRow1.Bold = true;
            richTextRow1.Add($"{order.Code}");
            var boldSegmentRow2 = richTextRow2.Add("KHÁCH HÀNG: ");
            boldSegmentRow2.Bold = true;
            richTextRow2.Add($"{order.Customer.Fullname}");
            var boldSegmentRow3 = richTextRow3.Add("SỐ ĐIỆN THOẠI: ");
            boldSegmentRow3.Bold = true;
            richTextRow3.Add($"{order.Customer.Phone}");

            worksheet.Cells["A5"].Value = $"DANH SÁCH LỰA CHỌN NGHIỆM THU ĐÁNH GIÁ CHẤT LƯỢNG ";
            worksheet.Cells["A5"].Style.Font.Bold = true;

            // Table headers
            worksheet.Cells["A6"].Value = "Mã kiểm dịch";
            worksheet.Cells["B6"].Value = "Loài";

            // Highlight header row
            using (var range = worksheet.Cells["A6:B6"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            // Format "Mã kiểm dịch" as text to preserve leading zeros
            worksheet.Column(1).Style.Numberformat.Format = "@";
            worksheet.Column(2).Style.Numberformat.Format = "@";

            // Add species list to a hidden sheet for dropdown source
            var speciesSheet = package.Workbook.Worksheets.Add("SpeciesList");
            for (int i = 0; i < speciesList.Count; i++)
            {
                speciesSheet.Cells[i + 1, 1].Value = speciesList[i];
            }
            speciesSheet.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;

            // Set data validation (dropdown) for "Loài" column (B7:B100)
            var validation = worksheet.DataValidations.AddListValidation("B7:B100");
            validation.Formula.ExcelFormula = $"=SpeciesList!$A$1:$A${speciesList.Count}";
            validation.ShowErrorMessage = true;
            validation.ErrorTitle = "Giá trị không hợp lệ";
            validation.Error = "Vui lòng chọn loài từ danh sách.";

            // Set data validation for "Mã kiểm dịch" to only allow numbers (but format is text)
            var codeValidation = worksheet.DataValidations.AddCustomValidation("A7:A100");
            codeValidation.Formula.ExcelFormula = "ISNUMBER(VALUE(A7))";
            codeValidation.ShowErrorMessage = true;
            codeValidation.ErrorTitle = "Chỉ nhập số";
            codeValidation.Error = "Chỉ được nhập số vào cột Mã kiểm dịch.";

            using var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;

            var fileUrl = await _cloudinaryService.UploadFileStreamAsync(
                CloudFolderFileTemplateName,
                "Mau_chon_vat_nuoi_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx",
                stream
            );
            return fileUrl;
        }

        public async Task ImportListChosedLivestock(string orderId, string requestedBy, IFormFile file)
        {
            var order = await _context.Orders.FindAsync(orderId) ??
                throw new Exception("Không tìm thấy đơn hàng");

            var listLivestocks = new List<OrderDetail>();

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
                    var orderCodeStr = worksheet.Cells["A1"].Value?.ToString();
                    if (string.IsNullOrEmpty(orderCodeStr))
                        throw new Exception("Mã đơn bán không hợp lệ");
                    var code = orderCodeStr.Trim().Split(":").Last();
                    if (string.IsNullOrEmpty(code))
                        throw new Exception("Mã gói thầu không hợp lệ");
                    if (order.Code.ToUpper() != code.Trim().ToUpper())
                        throw new Exception($"Không tìm thấy mã đơn bán {code}");

                    //validate column headers list customers
                    var requiredColumns = new string[] {
                        "mã kiểm dịch",
                        "loài",
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
                    var idxInspectionCode = dicColumnIndexes["mã kiểm dịch"];
                    var idxSpecie = dicColumnIndexes["loài"];
                    for (int row = 7; row <= rowCount; row++)
                    {
                        var inspectionCode = worksheet.Cells[row, idxInspectionCode].Value?.ToString();
                        if (string.IsNullOrEmpty(inspectionCode))
                            throw new Exception($"Trống mã kiểm dịch ở ô [{row}:{idxInspectionCode}]");

                        var species = worksheet.Cells[row, idxSpecie].Value?.ToString();

                        var livestockCheck = await _context.Livestocks
                            .Include(x => x.Species)
                            .FirstOrDefaultAsync(l => l.InspectionCode.ToLower() == inspectionCode.Trim().ToLower()
                                                   && l.Species.Name.ToLower() == species.Trim().ToLower());
                        if (livestockCheck == null)
                            throw new Exception($"Không tìm thấy vật nuôi với mã kiểm dịch '{inspectionCode}' và loài '{species}' trong hệ thống.");

                        // Check duplicate livestock in order
                        var existingLivestock = await _context.OrderDetails
                            .FirstOrDefaultAsync(x => x.LivestockId == livestockCheck.Id && x.OrderId == orderId);
                        if (existingLivestock != null)
                            throw new Exception($"Vật nuôi với mã kiểm dịch '{inspectionCode}' đã tồn tại trong đơn bán này.");
                        
                        listLivestocks.Add(new OrderDetail
                        {
                            Id = SlugId.New(),
                            CreatedAt = DateTime.Now,
                            CreatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = string.IsNullOrEmpty(requestedBy) ? "SYS" : requestedBy,
                            LivestockId = livestockCheck.Id,
                            OrderId = orderId,
                            ExportedDate = null
                        });
                    }
                }

            }

            await _context.OrderDetails.AddRangeAsync(listLivestocks);
            await _context.SaveChangesAsync();

            return;
        }

        public async Task<bool> RemoveRequestChoose(string orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán lẻ");

            order.Type = order_type.KHÔNG_YÊU_CẦU;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RequestChoose(string orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán lẻ");

            if (order.Status == order_status.ĐÃ_HỦY || order.Status == order_status.HOÀN_THÀNH)
                throw new Exception("Đơn đã hoàn thành hoặc bị hủy, không thể chọn loài vật");

            if (order.Type == order_type.YÊU_CẦU_CHỌN)
                throw new Exception("Đơn đang yêu cầu chọn không thể yêu cầu tiếp");

            if (order.Type == order_type.YÊU_CẦU_XUẤT)
                throw new Exception("Đơn đang yêu cầu xuất không thể yêu cầu tiếp");

            var livestockQuantity = await _context.OrderRequirements
              .Where(o => o.OrderId == orderId)
              .SumAsync(x => (int?)x.Quantity);
            var listOrderDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId)
                .CountAsync();
            if (livestockQuantity == listOrderDetails)
                throw new Exception("Đơn đã chọn đủ theo yêu cầu");

            order.Type = order_type.YÊU_CẦU_CHỌN;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RequestExport(string orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán lẻ");

            if (order.Status == order_status.ĐÃ_HỦY || order.Status == order_status.HOÀN_THÀNH)
                throw new Exception("Đơn đã hoàn thành hoặc bị hủy, không thể xuất bán");

            if (order.Type == order_type.YÊU_CẦU_XUẤT)
                throw new Exception("Đơn đang yêu cầu xuất không thể yêu cầu tiếp");

            var orderDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId)
                .ToArrayAsync();
            if (!orderDetails.Any())
                throw new Exception("Đơn lẻ chưa có danh sách vật nuôi, không thể đề xuất xuất bán");

            var livestockQuantity = await _context.OrderRequirements
               .Where(o => o.OrderId == orderId)
               .SumAsync(x => (int?)x.Quantity);
            var listOrderDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId
                && x.ExportedDate != null)
                .CountAsync();
            if (livestockQuantity == listOrderDetails)
                throw new Exception("Đơn đã xuất đủ theo yêu cầu");

            order.Type = order_type.YÊU_CẦU_XUẤT;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteLivestockFromOrder(string orderDetailsId)
        {
            var orderDetails = await _context.OrderDetails
                .FirstOrDefaultAsync(x => x.Id == orderDetailsId);
            if (orderDetails == null)
                throw new Exception("Không tìm thấy thông tin vật nuôi");

            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderDetails.OrderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán");

            if (order.Status == order_status.ĐÃ_HỦY || order.Status == order_status.HOÀN_THÀNH)
                throw new Exception("Đơn đã hoàn thành hoặc bị hủy, không thể xóa");

            if (order.Status == order_status.ĐANG_BÀN_GIAO && orderDetails.ExportedDate != null)
                throw new Exception("Không thể xóa loài vật đã được xuất");

            _context.OrderDetails.Remove(orderDetails);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ListOrderLivestocks> GetListLivestockInOrder(string orderId, ListLivestockFilter filter)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn bán");

            var orderDetails = await _context.OrderDetails
                .Include(l => l.Livestock)
                    .ThenInclude(s => s.Species)
                .Where(x => x.OrderId == orderId)
                .ToArrayAsync();

            var result = new ListOrderLivestocks
            {
                Total = 0
            };

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    orderDetails = orderDetails
                        .Where(v => v.Livestock.InspectionCode.ToUpper().Contains(filter.Keyword.Trim().ToUpper()))
                        .ToArray();
                }
            }
            if (!orderDetails.Any())
                return result;

            orderDetails = orderDetails
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();

            result.Items = orderDetails
                .Select(v => new OrderLivestockInfo
                {
                    Id = v.Id,
                    LivestockId = v.LivestockId,
                    InspectionCode = v.Livestock.InspectionCode,
                    SpecieId = v.Livestock.Species.Id,
                    SpecieName = v.Livestock.Species.Name,
                    Weight = v.Livestock.WeightOrigin,
                    Status = v.Livestock.Status,
                    CreatedAt = v.CreatedAt,
                    ExportedDate = v.ExportedDate
                })
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();
            result.Total = orderDetails.Length;
            return result;
        }

        public async Task<ListOrders> GetListOrderToChoose()
        {
            var data = await _context.Orders
                .Include(c => c.Customer)
                .Where(x => x.Type == order_type.YÊU_CẦU_CHỌN)
                .ToArrayAsync();

            var result = new ListOrders()
            {
                Total = 0
            };

            if (!data.Any())
                return result;

            data = data
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();

            result.Items = data
                .Select(v => new OrderSummary
                {
                    Id = v.Id,
                    Code = v.Code,
                    CustomerName = v.Customer.Fullname,
                    PhoneNumber = v.Customer.Phone,
                    Total = _context.OrderRequirements
                        .Where(o => o.OrderId == v.Id)
                        .Sum(x => (int?)x.Quantity) ?? 0,

                    Received = _context.OrderDetails
                        .Where(x => x.OrderId == v.Id).Count(),
                    Type = v.Type,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();
            result.Total = data.Length;
            return result;
        }

        public async Task<ListOrderExport> GetListOrderToExport()
        {
            var data = await _context.Orders
                 .Include(c => c.Customer)
                 .Where(x => x.Type == order_type.YÊU_CẦU_XUẤT)
                 .ToArrayAsync();

            var result = new ListOrderExport()
            {
                Total = 0
            };

            if (!data.Any())
                return result;

            data = data
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();

            result.Items = data
                .Select(v => new OrderExport
                {
                    Id = v.Id,
                    Code = v.Code,
                    CustomerName = v.Customer.Fullname,
                    PhoneNumber = v.Customer.Phone,
                    Total = _context.OrderDetails
                        .Where(x => x.OrderId == v.Id).Count(),

                    Received = _context.OrderDetails
                        .Where(x => x.OrderId == v.Id
                        && x.ExportedDate != null).Count(),

                    ExportCount = _context.OrderDetails
                        .Where(x => x.OrderId == v.Id
                        && x.ExportedDate == null).Count(),
                    Type = v.Type,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();
            result.Total = data.Length;
            return result;
        }

        public async Task<OrderDetailsDTO> CreateOrder(CreateOrderDTO createModel)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Fullname.ToLower().Trim() == createModel.CustomerName.ToLower().Trim()
                && x.Phone == createModel.Phone);

            foreach (var item in createModel.Details)
            {
                if (item == null)
                    throw new Exception("Yêu cầu khách hàng trống!");

                if (item.WeightFrom != null && item.WeightTo != null && item.WeightFrom > item.WeightTo)
                    throw new Exception("Trọng lượng yêu cầu chưa đúng định dạng");
            }

            var orderModel = new Order();

            orderModel.Id = SlugId.New();

            if (customer != null)
                orderModel.CustomerId = customer.Id;

            if (customer == null)
            {
                var customerModel = new Customer();
                customerModel.Id = SlugId.New();
                customerModel.Fullname = createModel.CustomerName;
                customerModel.Phone = createModel.Phone;
                customerModel.Address = createModel.Addrress ?? null;
                customerModel.Email = createModel.Email ?? null;
                customerModel.CreatedAt = DateTime.Now;
                customerModel.UpdatedAt = DateTime.Now;
                customerModel.CreatedBy = createModel.RequestedBy ?? "SYS";
                customerModel.UpdatedBy = createModel.RequestedBy ?? "SYS";

                orderModel.CustomerId = customerModel.Id;

                await _context.Customers.AddAsync(customerModel);
            }

            orderModel.Code = await GenerateOrderCode();
            orderModel.Status = order_status.MỚI;
            orderModel.CreatedAt = DateTime.Now;
            orderModel.UpdatedAt = DateTime.Now;
            orderModel.CreatedBy = createModel.RequestedBy ?? "SYS";
            orderModel.UpdatedBy = createModel.RequestedBy ?? "SYS";

            await _context.Orders.AddAsync(orderModel);

            var specieIds = createModel.Details.Select(d => d.SpecieId).Distinct().ToList();
            var existingSpecies = await _context.Species
                .Where(s => specieIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            foreach (var detail in createModel.Details)
            {
                if (!existingSpecies.ContainsKey(detail.SpecieId))
                    throw new Exception($"Không tìm thấy loài vật với ID: {detail.SpecieId}");

                var newOrderRequirement = new OrderRequirement
                {
                    Id = SlugId.New(),

                    OrderId = orderModel.Id,
                    SpecieId = detail.SpecieId,
                    WeightFrom = detail.WeightFrom ?? null,
                    WeightTo = detail.WeightTo ?? null,
                    Quantity = detail.Total,
                    Description = detail.Description,

                    CreatedAt = DateTime.Now,
                    CreatedBy = createModel.RequestedBy ?? "SYS",
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = createModel.RequestedBy ?? "SYS",
                };

                await _context.OrderRequirements.AddAsync(newOrderRequirement);

                if (detail.VaccintionRequirement != null)
                {
                    foreach (var vaccinationRequire in detail.VaccintionRequirement)
                    {
                        var vaccinationRequireCreate = new VaccinationRequirement
                        {
                            Id = SlugId.New(),

                            DiseaseId = vaccinationRequire.DiseaseId,
                            OrderRequirementId = newOrderRequirement.Id,
                            InsuranceDuration = vaccinationRequire.InsuranceDuration ?? 21,

                            CreatedAt = DateTime.Now,
                            CreatedBy = createModel.RequestedBy ?? "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = createModel.RequestedBy ?? "SYS",
                        };
                        await _context.VaccinationRequirement.AddAsync(vaccinationRequireCreate);
                    }
                }
            }

            await _context.SaveChangesAsync();

            var result = new OrderDetailsDTO
            {
                Id = orderModel.Id,
                CustomerId = orderModel.CustomerId,
                CustomerName = customer?.Fullname ?? createModel.CustomerName,
                PhoneNumber = customer?.Phone ?? createModel.Phone,
                Address = customer?.Address ?? createModel.Addrress,
                Email = customer?.Email ?? createModel.Email,
                Code = orderModel.Code,
                Total = await _context.OrderRequirements
        .Where(o => o.OrderId == orderModel.Id)
        .SumAsync(x => (int?)x.Quantity) ?? 0,

                Imported = await _context.OrderDetails
        .Where(x => x.OrderId == orderModel.Id && x.ExportedDate != null)
        .CountAsync(),

                Status = orderModel.Status,
                Details = await _context.OrderRequirements
        .Where(x => x.OrderId == orderModel.Id)
        .Select(req => new OrderRequirementdetails
        {
            OrderRequirementId = req.Id,
            SpecieId = req.SpecieId,
            SpecieName = _context.Species
                .Where(s => s.Id == req.SpecieId)
                .Select(s => s.Name)
                .FirstOrDefault(),

            WeightFrom = req.WeightFrom,
            WeightTo = req.WeightTo,
            Total = req.Quantity,
            Description = req.Description,
            VaccintionRequirement = _context.VaccinationRequirement
                .Where(v => v.OrderRequirementId == req.Id)
                .Select(v => new VaccinationRequireOrderDetail
                {
                    VaccinationRequirementId = v.Id,
                    DiseaseId = v.DiseaseId,
                    DiseaseName = _context.Diseases
                        .Where(d => d.Id == v.DiseaseId)
                        .Select(d => d.Name)
                        .FirstOrDefault(),

                    InsuranceDuration = v.InsuranceDuration
                }).ToList()
        }).ToListAsync()
            };
            return result;
        }

        public async Task<ListOrders> GetListOrder(ListOrderFilter filter)
        {
            var query = _context.Orders
                .Include(c => c.Customer)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    string keyword = filter.Keyword.Trim().ToUpper();
                    query = query.Where(v => v.Customer.Fullname.ToUpper().Contains(keyword));
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

            int total = await query.CountAsync();

            var orders = await query
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new OrderSummary
                {
                    Id = v.Id,
                    Code = v.Code,
                    CustomerName = v.Customer.Fullname,
                    PhoneNumber = v.Customer.Phone,
                    Total = _context.OrderRequirements
                        .Where(o => o.OrderId == v.Id)
                        .Sum(x => (int?)x.Quantity) ?? 0,

                    Received = _context.OrderDetails
                        .Where(x => x.OrderId == v.Id
                        && x.ExportedDate != null).Count(),
                    Type = v.Type,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();

            return new ListOrders
            {
                Total = total,
                Items = orders
            };
        }

        public async Task<OrderDetailsDTO> GetOrderDetailsById(string orderId)
        {
            var orderModel = await _context.Orders
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(x => x.Id == orderId);
            if (orderModel == null)
                throw new Exception("Không tìm thấy thông tin đơn");

            var result = new OrderDetailsDTO
            {
                Id = orderModel.Id,
                CustomerId = orderModel.CustomerId,
                CustomerName = orderModel.Customer.Fullname,
                PhoneNumber = orderModel.Customer.Phone,
                Address = orderModel.Customer.Address,
                Email = orderModel.Customer.Email,
                Code = orderModel.Code,
                Total = await _context.OrderRequirements
        .Where(o => o.OrderId == orderModel.Id)
        .SumAsync(x => (int?)x.Quantity) ?? 0,

                Imported = await _context.OrderDetails
        .Where(x => x.OrderId == orderModel.Id && x.ExportedDate != null)
        .CountAsync(),

                Status = orderModel.Status,
                Details = await _context.OrderRequirements
        .Where(x => x.OrderId == orderModel.Id)
        .Select(req => new OrderRequirementdetails
        {
            OrderRequirementId = req.Id,
            SpecieId = req.SpecieId,
            SpecieName = _context.Species
                .Where(s => s.Id == req.SpecieId)
                .Select(s => s.Name)
                .FirstOrDefault(),

            WeightFrom = req.WeightFrom,
            WeightTo = req.WeightTo,
            Total = req.Quantity,
            Description = req.Description,
            VaccintionRequirement = _context.VaccinationRequirement
                .Where(v => v.OrderRequirementId == req.Id)
                .Select(v => new VaccinationRequireOrderDetail
                {
                    VaccinationRequirementId = v.Id,
                    DiseaseId = v.DiseaseId,
                    DiseaseName = _context.Diseases
                        .Where(d => d.Id == v.DiseaseId)
                        .Select(d => d.Name)
                        .FirstOrDefault(),

                    InsuranceDuration = v.InsuranceDuration
                }).ToList()
        }).ToListAsync()
            };
            return result;
        }

        public async Task<OrderDetailsDTO> UpdateOrder(string orserId, UpdateOrderDTO updateOrder)
        {
            var orderModel = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orserId);
            if (orderModel == null)
                throw new Exception("Không tìm thấy đơn bán");

            foreach (var item in updateOrder.Details)
            {
                if (item == null)
                    throw new Exception("Yêu cầu khách hàng trống!");

                if (item.WeightFrom != null && item.WeightTo != null && item.WeightFrom > item.WeightTo)
                    throw new Exception("Trọng lượng yêu cầu chưa đúng định dạng");
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == orderModel.CustomerId);
            if (customer == null)
                throw new Exception("Không tìm thấy thông tin khách hàng");

            //Update Customer
            customer.Fullname = updateOrder.CustomerName;
            customer.Phone = updateOrder.Phone;
            if (updateOrder.Addrress != null)
                customer.Address = updateOrder.Addrress;

            if (updateOrder.Email != null)
                customer.Email = updateOrder.Email;

            customer.UpdatedAt = DateTime.Now;
            customer.UpdatedBy = updateOrder.RequestedBy ?? "SYS";

            //Update Order
            orderModel.UpdatedAt = DateTime.Now;
            orderModel.UpdatedBy = updateOrder.RequestedBy ?? "SYS";

            //Update OrderRequirement
            foreach (var detail in updateOrder.Details)
            {
                var orderRequirement = await _context.OrderRequirements.FirstOrDefaultAsync(x => x.Id == detail.Id);
                if (orderRequirement == null)
                    throw new Exception("Không tìm thấy thông tin yêu cầu");

                orderRequirement.SpecieId = detail.SpecieId;
                orderRequirement.WeightFrom = detail.WeightFrom;
                orderRequirement.WeightTo = detail.WeightTo;
                orderRequirement.Quantity = detail.Total;
                orderRequirement.Description = detail.Description;
                orderRequirement.UpdatedAt = DateTime.Now;
                orderRequirement.UpdatedBy = updateOrder.RequestedBy ?? "SYS";

                // Xóa các bản ghi VaccinationRequirement cũ nếu không có yêu cầu mới
                var existingVaccines = await _context.VaccinationRequirement
                    .Where(v => v.OrderRequirementId == orderRequirement.Id)
                    .ToArrayAsync();

                if (detail.VaccintionRequirement == null || !detail.VaccintionRequirement.Any())
                {
                    _context.VaccinationRequirement.RemoveRange(existingVaccines);
                }
                else
                {
                    _context.VaccinationRequirement.RemoveRange(existingVaccines);

                    foreach (var insurance in detail.VaccintionRequirement)
                    {
                        var vaccinationRequirement = new VaccinationRequirement
                        {
                            Id = SlugId.New(),
                            OrderRequirementId = orderRequirement.Id,
                            DiseaseId = insurance.DiseaseId,
                            InsuranceDuration = insurance.InsuranceDuration ?? 21,
                            CreatedBy = updateOrder.RequestedBy ?? "SYS",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = updateOrder.RequestedBy ?? "SYS"
                        };

                        await _context.VaccinationRequirement.AddAsync(vaccinationRequirement);
                    }
                }
            }

            await _context.SaveChangesAsync();

            var result = new OrderDetailsDTO
            {
                Id = orderModel.Id,
                CustomerId = orderModel.CustomerId,
                CustomerName = orderModel.Customer.Fullname,
                PhoneNumber = orderModel.Customer.Phone,
                Address = orderModel.Customer.Address,
                Email = orderModel.Customer.Email,
                Code = orderModel.Code,
                Total = await _context.OrderRequirements
        .Where(o => o.OrderId == orderModel.Id)
        .SumAsync(x => (int?)x.Quantity) ?? 0,

                Imported = await _context.OrderDetails
        .Where(x => x.OrderId == orderModel.Id && x.ExportedDate != null)
        .CountAsync(),

                Status = orderModel.Status,
                Details = await _context.OrderRequirements
        .Where(x => x.OrderId == orderModel.Id)
        .Select(req => new OrderRequirementdetails
        {
            OrderRequirementId = req.Id,
            SpecieId = req.SpecieId,
            SpecieName = _context.Species
                .Where(s => s.Id == req.SpecieId)
                .Select(s => s.Name)
                .FirstOrDefault(),

            WeightFrom = req.WeightFrom,
            WeightTo = req.WeightTo,
            Total = req.Quantity,
            Description = req.Description,
            VaccintionRequirement = _context.VaccinationRequirement
                .Where(v => v.OrderRequirementId == req.Id)
                .Select(v => new VaccinationRequireOrderDetail
                {
                    VaccinationRequirementId = v.Id,
                    DiseaseId = v.DiseaseId,
                    DiseaseName = _context.Diseases
                        .Where(d => d.Id == v.DiseaseId)
                        .Select(d => d.Name)
                        .FirstOrDefault(),

                    InsuranceDuration = v.InsuranceDuration
                }).ToList()
        }).ToListAsync()
            };
            return result;
        }

        private async Task<string> GenerateOrderCode()
        {
            var lastOrder = await _context.Orders
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastOrder != null && !string.IsNullOrEmpty(lastOrder.Code) && lastOrder.Code.StartsWith("OD-"))
            {
                var numberPart = lastOrder.Code.Substring(3); // Bỏ "OD-"
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            return $"OD-{nextNumber:D2}";
        }

    }
}
