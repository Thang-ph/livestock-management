using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using ClosedXML.Excel;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using QRCoder;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services
{
    public class LivestockService : ILivestockRepository
    {
        private readonly LmsContext _context;
        private readonly ICloudinaryRepository _cloudinaryService;

        public LivestockService(LmsContext context, ICloudinaryRepository cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public byte[] GenerateQRCode(string text)
        {
            byte[] QRCode = null;
            if (!string.IsNullOrEmpty(text))
            {
                QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
                QRCodeData data = qRCodeGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode bitmap = new BitmapByteQRCode(data);
                QRCode = bitmap.GetGraphic(20);
            }
            return QRCode;
        }

        public async Task<byte[]> ExportListNoCodeLivestockExcel()
        {
            var listLivestock = await _context.Livestocks
                .Where(x => string.IsNullOrEmpty(x.InspectionCode))
                .ToListAsync();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Livestock QR");

                ws.Cell(1, 1).Value = "STT";
                ws.Cell(1, 2).Value = "QR Code";
                var headerRange = ws.Range("A1:B1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int qrSize = 300;
                ws.Column(2).Width = qrSize * 0.75;

                int row = 2;
                int stt = 1;
                foreach (var item in listLivestock)
                {
                    ws.Cell(row, 1).Value = stt;

                    byte[] qrBytes = GenerateQRCode(urlDeploy + item.Id.ToString());

                    if (qrBytes != null)
                    {
                        using (MemoryStream ms = new MemoryStream(qrBytes))
                        {
                            using (Image img = Image.FromStream(ms))
                            {
                                using (MemoryStream imgStream = new MemoryStream())
                                {
                                    img.Save(imgStream, ImageFormat.Png);
                                    imgStream.Seek(0, SeekOrigin.Begin);

                                    var qrPic = ws.AddPicture(imgStream)
                                                  .MoveTo(ws.Cell(row, 2))
                                                  .WithSize(qrSize, qrSize);

                                    ws.Row(row).Height = qrSize * 0.75;
                                }
                            }
                        }
                    }

                    row++;
                    stt++;
                }

                ws.Columns(1, 2).AdjustToContents();

                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }

        public async Task<ListLivestocks> GetListLivestocks(ListLivestocksFilter filter)
        {
            var result = new ListLivestocks()
            {
                Total = 0
            };

            var livestocks = await _context.Livestocks
                .Include(o => o.Species)
                .Where(o => !string.IsNullOrEmpty(o.InspectionCode))
                .ToArrayAsync();
            if (livestocks == null || !livestocks.Any())
                return result;

            var livestockDiseases = await _context.MedicalHistories
                .Include(o => o.Disease)
                .Where(o => o.Status == medical_history_status.ĐANG_ĐIỀU_TRỊ)
                .ToArrayAsync();


            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    livestocks = livestocks
                        .Where(o => o.InspectionCode.ToUpper().Contains(filter.Keyword.Trim().ToUpper()))
                        .ToArray();
                }
                if (filter.MinWeight != null)
                {
                    livestocks = livestocks
                        .Where(o => o.WeightEstimate >= filter.MinWeight)
                        .ToArray();
                }
                if (filter.MaxWeight != null)
                {
                    livestocks = livestocks
                        .Where(o => o.WeightEstimate <= filter.MaxWeight)
                        .ToArray();
                }
                if (filter.SpeciesIds != null && filter.SpeciesIds.Any())
                {
                    livestocks = livestocks
                        .Where(o => filter.SpeciesIds.Contains(o.SpeciesId))
                        .ToArray();
                }
                if (filter.Statuses != null && filter.Statuses.Any())
                {
                    livestocks = livestocks
                        .Where(o => filter.Statuses.Contains(o.Status))
                        .ToArray();
                }

                livestocks = livestocks
                    .OrderBy(o => o.InspectionCode)
                    .Skip(filter.Skip)
                    .Take(filter.Take)
                    .ToArray();
            }

            result.Items = livestocks
                .Select(o => new LivestockSummary
                {
                    Id = o.Id,
                    InspectionCode = o.InspectionCode,
                    Status = o.Status,
                    Color = o.Color,
                    Gender = o.Gender,
                    Origin = o.Origin,
                    Species = o.Species.Name,
                    Weight = o.WeightEstimate
                })
                .OrderBy(o => o.InspectionCode)
                .ToArray();
            result.Total = livestocks.Length;

            return result;
        }

        public async Task<LivestockGeneralInfo> GetLivestockGeneralInfo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("Livestock id is missing");
            }

            var livestock = await _context.Livestocks
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();
            if (livestock == null)
            {
                throw new Exception("Livestock not found");
            }
            LivestockGeneralInfo result = new LivestockGeneralInfo();
            result.BarnId = livestock.BarnId;
            result.Gender = livestock.Gender;
            result.WeightExport = livestock.WeightExport;
            result.InspectionCode = livestock.InspectionCode;
            result.Status = livestock.Status;
            result.Dob = livestock.Dob;
            result.Color = livestock.Color;
            result.Origin = livestock.Origin;
            result.SpeciesId = livestock.SpeciesId;
            result.WeightOrigin = livestock.WeightOrigin;
            result.WeightExport = livestock.WeightExport;
            result.WeightEstimate = livestock.WeightEstimate;
            result.BarnId = livestock.BarnId;
            return result;
        }

        public async Task<LivestockSicknessHistory> GetLivestockSicknessHistory(string id)
        {


            var livestock = await _context.Livestocks
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            if (livestock == null)
            {
                throw new Exception("Không tìm thấy vật nuôi");
            }

            var listHistory = await _context.MedicalHistories.Include(x => x.Medicine).Include(x => x.Disease)
                .Where(x => x.LivestockId == id)
                .ToListAsync();

            var sicknessDetails = listHistory.Select(x => new LivestockSicknessDetail
            {
                Symptom = x.Symptom,
                Disease = x.Disease.Name,
                dateOfRecord = x.CreatedAt,
                MedicineName = x.Medicine.Name,
                Status = x.Status
            }).ToList();

            return new LivestockSicknessHistory
            {
                LivestockId = livestock.Id,
                InspectionCode = livestock.InspectionCode,
                disease = sicknessDetails.Any() ? sicknessDetails : null
            };
        }

        public async Task<LivestockSummary> GetLivestockSummaryInfo(string id)
        {
            var livestock = await _context.Livestocks.FindAsync(id) ??
                throw new Exception("Không tìm thấy vật nuôi");
            var specie = await _context.Species.FindAsync(livestock.SpeciesId) ??
                throw new Exception("Không tìm thấy mã vật nuôi");

            var result = new LivestockSummary
            {
                Id = id,
                InspectionCode = livestock.InspectionCode ?? "N/A",
                Color = livestock.Color,
                Gender = livestock.Gender,
                Origin = livestock.Origin,
                Species = specie.Name,
                Weight = livestock.WeightEstimate,
                Status = livestock.Status
            };

            return result;
        }

        public async Task<LivestockVaccinationHistory> GetLivestockVaccinationHistory(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Livestock ID is required", nameof(id));
            }

            var livestock = await _context.Livestocks
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            if (livestock == null)
            {
                throw new Exception("Livestock not found");
            }

            var vaccinationHistory = await _context.LivestockVaccinations.Include(x => x.BatchVaccination).ThenInclude(x => x.Vaccine)
                .Where(v => v.LivestockId == id)
                .Select(v => new LivestockVaccinationDetail
                {
                    createdAt = v.CreatedAt,
                    vaccine = v.BatchVaccination.Vaccine.Name,
                    description = v.BatchVaccination.Description
                })
                .ToListAsync();

            var result = new LivestockVaccinationHistory
            {
                LivestockId = id,
                InspectionCode = livestock.InspectionCode,
                vaccineHistory = vaccinationHistory
            };

            return result;
        }

        public async Task<LivestockSummary> GetLiveStockIdByInspectionCodeAndType(LivestockIdFindDTO model)
        {
            var livestock = await _context.Livestocks
                .Include(s => s.Species)
                .FirstOrDefaultAsync(x => x.InspectionCode == model.InspectionCode
                && x.Species.Type == model.SpecieType);
            if (livestock == null)
                throw new Exception("Không tìm thấy id cho loài " + model.SpecieType);

            var result = new LivestockSummary
            {
                Id = livestock.Id,
                InspectionCode = livestock.InspectionCode,
                Species = livestock.SpeciesId,
                Weight = livestock.WeightOrigin,
                Gender = livestock.Gender,
                Color = livestock.Color,
                Origin = livestock.Origin,
                Status = livestock.Status
            };
            return result;
        }

        public async Task<LivestockGeneralInfo> GetLivestockGeneralInfo(string inspectionCode, specie_type specieType)
        {
            if (string.IsNullOrEmpty(inspectionCode))
            {
                throw new Exception("Inspection code id is missing");
            }

            var livestock = await _context.Livestocks.Include(x => x.Species)
                .Where(o => o.InspectionCode == inspectionCode && o.Species.Type == specieType)
                .FirstOrDefaultAsync();
            if (livestock == null)
            {
                throw new Exception("Livestock not found");
            }
            LivestockGeneralInfo result = new LivestockGeneralInfo();
            result.BarnId = livestock.BarnId;
            result.Gender = livestock.Gender;
            result.WeightExport = livestock.WeightExport;
            result.InspectionCode = livestock.InspectionCode;
            result.Status = livestock.Status;
            result.Dob = livestock.Dob;
            result.Color = livestock.Color;
            result.Origin = livestock.Origin;
            result.Id = livestock.Id;
            result.SpeciesId = livestock.SpeciesId;
            result.WeightOrigin = livestock.WeightOrigin;
            result.WeightExport = livestock.WeightExport;
            result.WeightEstimate = livestock.WeightEstimate;
            result.BarnId = livestock.BarnId;
            return result;
        }

        public async Task<DashboardLivestock> GetDashboarLivestock()
        {
            var result = new DashboardLivestock()
            {
                DiseaseRatioSummary = new DiseaseRatioSummary
                {
                    Items = new List<DiseaseRatio>
                    {
                        new DiseaseRatio
                        {
                            DiseaseId = "1",
                            DiseaseName = "Lở mồm long móng",
                            Quantity = 1,
                            Ratio = 0.1M,
                            Severity = severity.HIGH
                        },
                        new DiseaseRatio
                        {
                            DiseaseId = "1",
                            DiseaseName = "Đau mắt",
                            Quantity = 24,
                            Ratio = 0.24M,
                            Severity = severity.HIGH
                        },
                        new DiseaseRatio
                        {
                            DiseaseId = "1",
                            DiseaseName = "Bệnh khác",
                            Quantity = 0,
                            Ratio = 0M,
                            Severity = severity.LOW
                        }
                    },
                    Total = 25,
                    TotalRatio = 0.25M
                },
                TotalDisease = 6,
                VaccinationRatioSummary = new VaccinationRatioSummary
                {
                    Items = new List<VaccinationRatio>
                    {
                        new VaccinationRatio
                        {
                            DiseaseId = "1",
                            DiseaseName = "Tẩy kí sing trùng",
                            Ratio = 0.6M,
                            Severity = severity.HIGH
                        },
                        new VaccinationRatio
                        {
                            DiseaseId = "1",
                            DiseaseName = "Lở mồm long móng",
                            Ratio = 0.7M,
                            Severity = severity.MEDIUM
                        },
                        new VaccinationRatio
                        {
                            DiseaseId = "1",
                            DiseaseName = "Tụ huyết trùng",
                            Ratio = 1M,
                            Severity = severity.LOW
                        },
                        new VaccinationRatio
                        {
                            DiseaseId = "1",
                            DiseaseName = "Viêm da nổi cục",
                            Ratio = 1M,
                            Severity = severity.LOW
                        }
                    },
                    Total = 4
                },
                TotalLivestockMissingInformation = 10,
                InspectionCodeQuantitySummary = new InspectionCodeQuantitySummary
                {
                    Items = new List<InspectionCodeQuantityBySpecie>
                    {
                        new InspectionCodeQuantityBySpecie
                        {
                            Specie_Type = specie_type.TRÂU,
                            TotalQuantity = 500,
                            RemainingQuantity = 40,
                            Severity = severity.HIGH,
                        },
                        new InspectionCodeQuantityBySpecie
                        {
                            Specie_Type = specie_type.BÒ,
                            TotalQuantity = 1000,
                            RemainingQuantity = 100,
                            Severity = severity.MEDIUM,
                        },
                        new InspectionCodeQuantityBySpecie
                        {
                            Specie_Type = specie_type.DÊ,
                            TotalQuantity = 100,
                            RemainingQuantity = 90,
                            Severity = severity.LOW,
                        },
                    },
                    Total = 3,
                },
                SpecieRatioSummary = new SpecieRatioSummary
                {
                    Items = new List<SpecieRatio>
                    {
                        new SpecieRatio
                        {
                            SpecieId = "1",
                            SpecieName = "Bò lai Sind cái",
                            Quantity = 25,
                            Ratio = 0.25M
                        },
                        new SpecieRatio
                        {
                            SpecieId = "1",
                            SpecieName = "Bò lai Sind đực",
                            Quantity = 10,
                            Ratio = 0.1M
                        },
                        new SpecieRatio
                        {
                            SpecieId = "1",
                            SpecieName = "Bò BBB cái",
                            Quantity = 25,
                            Ratio = 0.25M
                        },
                        new SpecieRatio
                        {
                            SpecieId = "1",
                            SpecieName = "Bò BBB đực",
                            Quantity = 10,
                            Ratio = 0.1M
                        },
                        new SpecieRatio
                        {
                            SpecieId = "1",
                            SpecieName = "Trâu cái",
                            Quantity = 25,
                            Ratio = 0.25M
                        },new SpecieRatio
                        {
                            SpecieId = "1",
                            SpecieName = "Trâu đực",
                            Quantity = 5,
                            Ratio = 0.05M
                        },
                    },
                    Total = 100
                },
                WeightRatioSummary = new WeightRatioSummary
                {
                    Items = new List<WeightRatioBySpecie>
                    {
                        new WeightRatioBySpecie
                        {
                            SpecieId = "1",
                            SpecieName = "Bò lai Sind cái",
                            TotalQuantity = 25,
                            WeightRatios = new List<WeightRatio>
                            {
                                new WeightRatio
                                {
                                    WeightRange = "<90 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "90 - 130 kg",
                                    Quantity = 0,
                                    Ratio = 0M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "130 - 160 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "160 - 190 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "190 - 250 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = ">250 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                            }
                        },
                        new WeightRatioBySpecie
                        {
                            SpecieId = "1",
                            SpecieName = "Bò BBB cái",
                            TotalQuantity = 25,
                            WeightRatios = new List<WeightRatio>
                            {
                                new WeightRatio
                                {
                                    WeightRange = "<90 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "90 - 130 kg",
                                    Quantity = 0,
                                    Ratio = 0M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "130 - 160 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "160 - 190 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "190 - 250 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = ">250 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                            }
                        },
                        new WeightRatioBySpecie
                        {
                            SpecieId = "1",
                            SpecieName = "Trâu cái",
                            TotalQuantity = 25,
                            WeightRatios = new List<WeightRatio>
                            {
                                new WeightRatio
                                {
                                    WeightRange = "<90 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "90 - 130 kg",
                                    Quantity = 0,
                                    Ratio = 0M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "130 - 160 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "160 - 190 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = "190 - 250 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                                new WeightRatio
                                {
                                    WeightRange = ">250 kg",
                                    Quantity = 5,
                                    Ratio = 0.2M
                                },
                            }
                        },
                    },
                    Total = 100,
                }
            };

            return result;
        }

        public async Task<string> GetDiseaseReport()
        {
            var medicalHistories = await _context.MedicalHistories
                .Where(o => o.Status == medical_history_status.CHỜ_KHÁM
                    || o.Status == medical_history_status.ĐANG_ĐIỀU_TRỊ
                    || o.Status == medical_history_status.TÁI_PHÁT)
                .ToArrayAsync();
            var species = await _context.Species.ToArrayAsync();
            //var 


            //Export to file
            var nowTime = DateTime.Now;
            var stringDate = $"Ngày {nowTime.Day}, tháng {nowTime.Month}, năm {nowTime.Year}";
            var fileName = $"Báo cáo dịch bệnh_{nowTime.ToString("yyyyMMddhhmmss")}.xlsx";
            var diseaseIds = medicalHistories
                .GroupBy(o => o.DiseaseId)
                .Select(o => o.Key)
                .ToArray();
            var diseaseNames = await _context.Diseases
                .Where(o => diseaseIds.Contains(o.Id))
                .Select(o => o.Name)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new MemoryStream());
            var worksheet = package.Workbook.Worksheets.Add($"Báo cáo dịch bệnh");

            var richTextRow1 = worksheet.Cells["A1"].RichText;
            var richTextRow2 = worksheet.Cells["A2"].RichText;
            var boldSegmentRow1 = richTextRow1.Add(OrganizationName);
            boldSegmentRow1.Bold = true;
            var boldSegmentRow2 = richTextRow2.Add(stringDate);
            boldSegmentRow2.Bold = true;

            var columns = new string[] { "Giống/Bệnh" };
            var tmpCols = columns.ToList();
            tmpCols.AddRange(diseaseNames);
            columns = tmpCols.ToArray();
            var data = new DataTable();
            data.Columns.AddRange(columns.Select(o => new DataColumn(o)).ToArray());
            worksheet.Cells["A3"].LoadFromDataTable(data, true, TableStyles.Light1);
            await package.SaveAsync();
            var stream = package.Stream;
            stream.Position = 0;
            var url = await _cloudinaryService.UploadFileStreamAsync(CloudFolderFileReportsName, fileName, stream);
            return url;
        }

        public async Task<string> GetWeightBySpecieReport()
        {
            return
                @"https://www.google.com/url?sa=i&url=https%3A%2F%2Fcharacter-stats-and-profiles.fandom.com%2Fwiki%2FTung_Tung_Tung_Sahur_%2528Italian_Brainrot%2C_Canon%2FEvanTheProNoob%2529&psig=AOvVaw2PISjrk2prM3siorJ4uF5l&ust=1748010304176000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCKiByPujt40DFQAAAAAdAAAAABAU";
        }

        public async Task<ListLivestockSummary> ListLivestockSummary()
        {
            var acceptedStatuses = new List<livestock_status>
            {
                livestock_status.CHỜ_ĐỊNH_DANH,
                livestock_status.KHỎE_MẠNH,
                livestock_status.ỐM,
                livestock_status.CHỜ_XUẤT
            };
            var allLivestocks = await _context.Livestocks
                .Where(o => acceptedStatuses.Contains(o.Status))
                .ToArrayAsync();
            var totalQuantity = allLivestocks.Length;
            var quantityByStatus = allLivestocks
                .GroupBy(o => o.Status)
                .Select(o => new LivestockQuantityByStatus
                {
                    Quantitiy = o.Count(),
                    Ratio = o.Count() / totalQuantity * 100,
                    Status = o.Key
                })
                .ToArray();
            var summaryByStatus = new SummaryByStatus
            {
                Items = quantityByStatus,
                Total = quantityByStatus.Sum(o => o.Quantitiy),
                TotalRatio = quantityByStatus.Sum(o => o.Ratio)
            };
            var result = new ListLivestockSummary
            {
                TotalLivestockQuantity = totalQuantity,
                SummaryByStatus = summaryByStatus
            };

            return result;
        }

        public async Task<string> GetListLivestocksReport()
        {
            return
                @"https://www.google.com/url?sa=i&url=https%3A%2F%2Fcharacter-stats-and-profiles.fandom.com%2Fwiki%2FTung_Tung_Tung_Sahur_%2528Italian_Brainrot%2C_Canon%2FEvanTheProNoob%2529&psig=AOvVaw2PISjrk2prM3siorJ4uF5l&ust=1748010304176000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCKiByPujt40DFQAAAAAdAAAAABAU";
        }

        public async Task<string> GetRecordLivestockStatusTemplate()
        {
            var nowTime = DateTime.Now;
            var stringDate = $"Ngày {nowTime.Day}, tháng {nowTime.Month}, năm {nowTime.Year}";
            var fileName = $"Mẫu theo dõi tình trạng vật nuôi_{nowTime.ToString("yyyyMMddhhmmss")}.xlsx";

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new MemoryStream());
            var worksheet = package.Workbook.Worksheets.Add($"Theo dõi tình trạng vật nuôi");

            var richTextRow1 = worksheet.Cells["A1"].RichText;
            var richTextRow2 = worksheet.Cells["A2"].RichText;
            var boldSegmentRow1 = richTextRow1.Add(OrganizationName);
            boldSegmentRow1.Bold = true;
            var boldSegmentRow2 = richTextRow2.Add(stringDate);
            boldSegmentRow2.Bold = true;

            var columns = new string[] { "Mã kiểm dịch", "Giống", "Trạng thái", "Biểu hiện", "Chẩn đoán", "Thuốc" };
            var data = new DataTable();
            data.Columns.AddRange(columns.Select(o => new DataColumn(o)).ToArray());
            worksheet.Cells["A3"].LoadFromDataTable(data, true, TableStyles.Light1);
            await package.SaveAsync();
            var stream = package.Stream;
            stream.Position = 0;
            var url = await _cloudinaryService.UploadFileStreamAsync(CloudFolderFileTemplateName, fileName, stream);
            return url;
        }

        public async Task ImportRecordLivestockStatusFile(string requestedBy, IFormFile file)
        {
            return;
        }

        public async Task<int> GetTotalEmptyRecords()
        {
            var result = await _context.Livestocks
                .Where(o => string.IsNullOrEmpty(o.InspectionCode)
                    && string.IsNullOrEmpty(o.SpeciesId)
                    && o.Status == livestock_status.TRỐNG
                )
                .CountAsync();

            return result;
        }

        public async Task<string> GetEmptyQrCodesFile()
        {
            return
                @"https://www.google.com/url?sa=i&url=https%3A%2F%2Fcharacter-stats-and-profiles.fandom.com%2Fwiki%2FTung_Tung_Tung_Sahur_%2528Italian_Brainrot%2C_Canon%2FEvanTheProNoob%2529&psig=AOvVaw2PISjrk2prM3siorJ4uF5l&ust=1748010304176000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCKiByPujt40DFQAAAAAdAAAAABAU";
        }

        public async Task<bool> CreateEmptyLivestockRecords(string requestedBy, int quantity)
        {
            if (quantity < 0)
                throw new Exception("Số lượng mã QR cần tạo phải là số tự nhiên lớn hơn 0");

            var barn = await _context.Barns.FirstOrDefaultAsync();

            var newEmptyRecords = new List<Livestock>();
            for (int i = 0; i < quantity; i++)
            {
                newEmptyRecords.Add(new Livestock
                {
                    Id = SlugId.New(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = requestedBy,
                    UpdatedBy = requestedBy,
                    Status = livestock_status.TRỐNG,
                    BarnId = barn?.Id ?? string.Empty
                });
            }
            await _context.Livestocks.AddRangeAsync(newEmptyRecords);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetRecordLivestockStatInformationTemplate()
        {
            var nowTime = DateTime.Now;
            var stringDate = $"Ngày {nowTime.Day}, tháng {nowTime.Month}, năm {nowTime.Year}";
            var fileName = $"Mẫu thông tin vật nuôi_{nowTime.ToString("yyyyMMddhhmmss")}.xlsx";

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new MemoryStream());
            var worksheet = package.Workbook.Worksheets.Add($"Thông tin vật nuôi");

            var richTextRow1 = worksheet.Cells["A1"].RichText;
            var richTextRow2 = worksheet.Cells["A2"].RichText;
            var boldSegmentRow1 = richTextRow1.Add(OrganizationName);
            boldSegmentRow1.Bold = true;
            var boldSegmentRow2 = richTextRow2.Add(stringDate);
            boldSegmentRow2.Bold = true;

            var columns = new string[] { "Mã kiểm dịch", "Giống", "Màu lông", "Trọng lượng (kg)", "Ngày sinh" };
            var data = new DataTable();
            data.Columns.AddRange(columns.Select(o => new DataColumn(o)).ToArray());
            worksheet.Cells["A3"].LoadFromDataTable(data, true, TableStyles.Light1);
            await package.SaveAsync();
            var stream = package.Stream;
            stream.Position = 0;
            var url = await _cloudinaryService.UploadFileStreamAsync(CloudFolderFileTemplateName, fileName, stream);
            return url;
        }

        public async Task ImportRecordLivestockInformationFile(string requestedBy, IFormFile file)
        {
            return;
        }

        public async Task ChangeLivestockStatus(string requestedBy, string[] livestockIds, livestock_status status)
        {
            if (livestockIds == null || !livestockIds.Any())
                return;
            var livestocks = await _context.Livestocks
                .Where(o => livestockIds.Contains(o.Id)
                    && ((status == livestock_status.CHẾT && o.Status != livestock_status.CHẾT) || (status == livestock_status.KHỎE_MẠNH && o.Status == livestock_status.ỐM))
                )
                .ToArrayAsync();
            if (!livestocks.Any())
                return;
            foreach (var livestock in livestocks)
            {
                livestock.Status = status;
                livestock.UpdatedAt = DateTime.Now;
                livestock.UpdatedBy = requestedBy;
            }
            await _context.SaveChangesAsync();
            return;
        }

        public async Task<LivestockDetails> GetLivestockDetails(GetLivestockDetailsRequest request)
        {
            // Validate input parameters
            if (request == null ||
                (string.IsNullOrEmpty(request.LivestockId) &&
                 (string.IsNullOrEmpty(request.InspectionCode) || request.SpecieType == null)))
            {
                throw new ArgumentException("Hãy quét mã QR hoặc điền đầy đủ thông tin");
            }

            // Find livestock by two methods:
            // Method 1: By LivestockId
            // Method 2: By both InspectionCode AND SpecieType
            var livestock = await _context.Livestocks
                .Include(l => l.Species)
                .Include(l => l.Barn)
                .Include(l => l.MedicalHistories)
                    .ThenInclude(mh => mh.Disease)
                .Include(l => l.MedicalHistories)
                    .ThenInclude(mh => mh.Medicine)
                .Include(l => l.LivestockVaccinations)
                    .ThenInclude(lv => lv.BatchVaccination)
                        .ThenInclude(bv => bv.Vaccine)
                            .ThenInclude(v => v.DiseaseMedicines)
                                .ThenInclude(dm => dm.Disease)
                .Include(l => l.BatchImportDetails)
                .Include(l => l.BatchExportDetails)
                .Where(l => (!string.IsNullOrEmpty(request.LivestockId) && l.Id == request.LivestockId) ||
                           (!string.IsNullOrEmpty(request.InspectionCode) && request.SpecieType != null &&
                            l.InspectionCode == request.InspectionCode && l.Species.Type == request.SpecieType))
                .FirstOrDefaultAsync();

            if (livestock == null)
            {
                throw new Exception("Không tìm thấy vật nuôi");
            }

            // Get import information
            var importDetail = livestock.BatchImportDetails?.OrderBy(bid => bid.CreatedAt).FirstOrDefault();

            // Get export information  
            var exportDetail = livestock.BatchExportDetails?.OrderByDescending(bed => bed.CreatedAt).FirstOrDefault();

            // Get vaccinated diseases (diseases that have been vaccinated against)
            var vaccinatedDiseases = livestock.LivestockVaccinations?
                .Where(lv => lv.BatchVaccination?.Vaccine?.DiseaseMedicines != null)
                .SelectMany(lv => lv.BatchVaccination.Vaccine.DiseaseMedicines)
                .GroupBy(dm => dm.Disease.Id)
                .Select(g => new LivestockVaccinatedDisease
                {
                    DiseaseId = g.Key,
                    DiseaseName = g.First().Disease.Name,
                    LastVaccinatedAt = livestock.LivestockVaccinations
                        .Where(lv => lv.BatchVaccination.Vaccine.DiseaseMedicines.Any(dm => dm.DiseaseId == g.Key))
                        .Max(lv => lv.CreatedAt)
                })
                .ToList() ?? new List<LivestockVaccinatedDisease>();

            // Get current diseases (diseases currently being treated)
            var currentDiseases = livestock.MedicalHistories?
                .Where(mh => mh.Status == medical_history_status.ĐANG_ĐIỀU_TRỊ)
                .GroupBy(mh => mh.DiseaseId)
                .Select(g => new LivestockCurrentDisease
                {
                    DiseaseId = g.Key,
                    DiseaseName = g.First().Disease.Name,
                    Status = g.First().Status,
                    StartDate = g.Min(mh => mh.CreatedAt),
                    EndDate = g.FirstOrDefault(mh => mh.Status == medical_history_status.ĐÃ_KHỎI)?.UpdatedAt
                })
                .ToList() ?? new List<LivestockCurrentDisease>();

            var result = new LivestockDetails
            {
                LivestockId = livestock.Id,
                InspectionCode = livestock.InspectionCode ?? "N/A",
                SpecieId = livestock.SpeciesId ?? "N/A",
                SpecieType = livestock.Species?.Type,
                SpecieName = livestock.Species?.Name ?? "N/A",
                LivestockStatus = livestock.Status,
                Color = livestock.Color,
                Weight = livestock.WeightEstimate,
                Origin = livestock.Origin,
                BarnId = livestock.BarnId,
                BarnName = livestock.Barn?.Name ?? "N/A",
                ImportDate = importDetail?.ImportedDate,
                ImportWeight = importDetail?.WeightImport ?? livestock.WeightOrigin,
                ExportDate = exportDetail?.ExportDate,
                ExportWeight = exportDetail?.WeightExport ?? livestock.WeightExport,
                LastUpdatedAt = livestock.UpdatedAt,
                LastUpdatedBy = livestock.UpdatedBy,
                LivestockVaccinatedDiseases = vaccinatedDiseases,
                LivestockCurrentDiseases = currentDiseases
            };

            return result;
        }

        public async Task UpdateLivestockDetails(UpdateLivestockDetailsRequest request)
        {
            return;
        }

        public async Task RecordLivestockDiseases(RecordLivstockDiseases request)
        {
            return;
        }
    }
}
