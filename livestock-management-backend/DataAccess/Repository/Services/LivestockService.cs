using QRCoder;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using ClosedXML.Excel;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services
{
    public class LivestockService : ILivestockRepository
    {
        private readonly LmsContext _context;
        private readonly ICloudinaryRepository _cloudinaryService;
        private readonly IDiseaseRepository _diseaseService;

        public LivestockService(LmsContext context, ICloudinaryRepository cloudinaryService, IDiseaseRepository diseaseService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _diseaseService = diseaseService;
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
                .Where(o => o.Status == medical_history_status.CHỜ_KHÁM
                    ||o.Status == medical_history_status.ĐANG_ĐIỀU_TRỊ
                    || o.Status == medical_history_status.TÁI_PHÁT)
                .ToArrayAsync();

            var now = DateTime.Now;
            //var dicMedicineDisease = await _context.DiseaseMedicines
            //   .ToDictionaryAsync(o => o.MedicineId, o => o.DiseaseId);
            var twentyOneDaysAgo = now.AddDays(-21);

            var singleVaccinatedLivestockIds = await _context.SingleVaccination
                .Where(o => o.CreatedAt >= twentyOneDaysAgo)
                .Select(o => new
                {
                    LivestockId = o.LivestockId,
                    MedicineId = o.MedicineId,
                    DiseaseId = _context.DiseaseMedicines
                        .Where(x => x.MedicineId == o.MedicineId)
                        .Select(x => x.DiseaseId)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Ngày giới hạn

            // Bước 1: Tạo Dictionary ánh xạ MedicineId → DiseaseId
            var dicMedicineDisease = await _context.DiseaseMedicines
                .GroupBy(x => x.MedicineId)
                .Select(g => new { MedicineId = g.Key, DiseaseId = g.First().DiseaseId })
                .ToDictionaryAsync(x => x.MedicineId, x => x.DiseaseId);

            // Bước 2: Lấy dữ liệu từ database, chưa xử lý TotalDays và truy vấn lồng
            var vaccinationData = await _context.LivestockVaccinations
                .Include(o => o.BatchVaccination)
                .Where(o => o.BatchVaccination.Status == batch_vaccination_status.HOÀN_THÀNH
                    && o.BatchVaccination.DateConduct != null)
                .ToListAsync();

            // Bước 3: Xử lý trong bộ nhớ (in-memory)
            var livestockVaccination = vaccinationData
                .Where(o => o.BatchVaccination.DateConduct >= twentyOneDaysAgo)
                .Select(o => new
                {
                    LivestockId = o.LivestockId,
                    MedicineId = o.BatchVaccination.VaccineId,
                    DiseaseId = dicMedicineDisease.TryGetValue(o.BatchVaccination.VaccineId, out var diseaseId)
                        ? diseaseId
                        : "" // hoặc null, tùy yêu cầu
                })
                .ToList();


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
                if (filter.DiseaseIds != null && filter.DiseaseIds.Any())
                {
                    var livestockIds = livestockDiseases
                        .Where(o => filter.DiseaseIds.Contains(o.DiseaseId))
                        .GroupBy(o => o.LivestockId)
                        .Select(o => o.Key) 
                        .ToArray();
                    livestocks = livestocks
                        .Where(o => livestockIds.Contains(o.Id))
                        .ToArray();
                }
                if (filter.DiseaseVaccinatedIds != null && filter.DiseaseVaccinatedIds.Any())
                {
                    var singleVaccinatedIds = singleVaccinatedLivestockIds
                        .Where(o => filter.DiseaseVaccinatedIds.Contains(o.DiseaseId))
                        .GroupBy(o => o.LivestockId)
                        .Select(o => o.Key)
                        .ToArray();
                    var vaccinatedIds = livestockVaccination
                        .Where(o => filter.DiseaseVaccinatedIds.Contains(o.DiseaseId))
                        .GroupBy(o => o.LivestockId)
                        .Select(o => o.Key)
                        .ToArray();
                    var tmp = singleVaccinatedIds.ToList();
                    tmp.AddRange(vaccinatedIds.ToList());
                    singleVaccinatedIds = tmp.Distinct().ToArray();
                    livestocks = livestocks
                        .Where(o => tmp.Contains(o.Id))
                        .ToArray();
                }
                if (filter.IsMissingInformation ?? false)
                {
                    livestocks = livestocks
                        .Where(o => string.IsNullOrEmpty(o.Color)
                            || o.WeightOrigin == null
                            || o.WeightOrigin == 0
                            || o.WeightEstimate == null
                            || o.WeightOrigin == 0
                            || o.Dob == null)
                        .ToArray();
                } 

                livestocks = livestocks
                    .OrderBy(o => o.InspectionCode)
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
                .OrderBy(o => o.Species)
                .ThenBy(o => o.InspectionCode)
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
            var diseaseRatioSummary = await _diseaseService.GetDiseaseRatios();
            var vaccinationRatioSummary = await _diseaseService.GetVaccinatedRatios();
            var inspectionCodeQuantitySummary = new InspectionCodeQuantitySummary
            {
                Total = 0
            };
            var specieRatioSummary = await GetSpecieRatios();
            var weightRatioSummary = await GetWeightRatios();
            var totalDisease = await _diseaseService.GetCurrentDiseaseQuantity();
            var totalLivestockMissingInfor = await GetLivestockMissingInforQuantity();
            var result = new DashboardLivestock()
            {
                DiseaseRatioSummary = diseaseRatioSummary,
                TotalDisease = totalDisease,
                VaccinationRatioSummary = vaccinationRatioSummary,
                TotalLivestockMissingInformation = totalLivestockMissingInfor,
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
                SpecieRatioSummary = specieRatioSummary,
                WeightRatioSummary = weightRatioSummary
            };
            return result;
        }

        public async Task<SpecieRatioSummary> GetSpecieRatios()
        {
            var result = new SpecieRatioSummary
            {
                Total = 0,
                Items = new List<SpecieRatio>()
            };
            var livestocks = await _context.Livestocks
                .Where(o => !string.IsNullOrEmpty(o.SpeciesId)
                    && (o.Status == livestock_status.CHỜ_ĐỊNH_DANH
                    || o.Status == livestock_status.KHỎE_MẠNH
                    || o.Status == livestock_status.ỐM)
                )
                .ToArrayAsync();
            if (!livestocks.Any())
                return result;
            var totalQuantity = livestocks.Length;
            var specieIds = livestocks
                .GroupBy(o => o.SpeciesId)
                .Select(o => o.Key)
                .ToArray();
            var dicDiseases = await _context.Diseases
                .Where(o => specieIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.Name);
            var resultItems = livestocks
                .GroupBy(o => o.SpeciesId)
                .Select(o => new SpecieRatio
                {
                    SpecieId = o.Key,
                    SpecieName = dicDiseases.ContainsKey(o.Key) ? dicDiseases[o.Key] : "N/A",
                    Quantity = o.Count(),
                    Ratio = o.Count() / totalQuantity * 100
                })
                .ToArray();
            result.Items = resultItems;
            result.Total = resultItems.Length;
            return result;
        }

        public async Task<WeightRatioSummary> GetWeightRatios()
        {
            var result = new WeightRatioSummary
            {
                Total = 0,
                Items = new List<WeightRatioBySpecie>()
            };
            var livestocks = await _context.Livestocks
                .Where(o => o.Status == livestock_status.CHỜ_ĐỊNH_DANH
                    || o.Status == livestock_status.KHỎE_MẠNH
                    || o.Status == livestock_status.ỐM)
                .ToArrayAsync();
            if (!livestocks.Any())
                return result;
            var totalQuantity = livestocks.Length;
            var resultItems = new List<WeightRatioBySpecie>();
            var specieIds = livestocks
                .GroupBy(o => o.SpeciesId)
                .Select(o => o.Key)
                .ToArray();
            var dicSpecies = await _context.Species
                .Where(o => specieIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.Name);
            foreach (var specideId in specieIds)
            {
                var livestocksBySpecie = livestocks
                    .Where(o => o.SpeciesId == specideId)
                    .ToArray();
                if (!livestocksBySpecie.Any())
                    continue;
                var quantityBySpecie = livestocksBySpecie.Length;
                var summaryBySpecie = new WeightRatioBySpecie
                {
                    SpecieId = specideId ?? "N/A",
                    SpecieName = dicSpecies.ContainsKey(specideId ?? string.Empty) ? dicSpecies[specideId] : "N/A",
                    TotalQuantity = quantityBySpecie,
                };
                var listWeights = new List<WeightRatio>();
                var livestockNoWeights = livestocksBySpecie
                    .Where(o => o.WeightEstimate == null || o.WeightEstimate == 0)
                    .ToArray();
                listWeights.Add(new WeightRatio
                {
                    Quantity = livestockNoWeights.Length,
                    Ratio = livestockNoWeights.Length / quantityBySpecie * 100,
                    WeightRange = "N/A"
                });
                var livestock90 = livestocksBySpecie
                    .Where(o => o.WeightEstimate != null
                        && o.WeightEstimate > 0
                        && o.WeightEstimate < 90)
                    .ToArray();
                listWeights.Add(new WeightRatio
                {
                    Quantity = livestock90.Length,
                    Ratio = livestock90.Length / quantityBySpecie * 100,
                    WeightRange = "<90 kg"
                });
                var livestock130 = livestocksBySpecie
                    .Where(o => o.WeightEstimate != null
                        && o.WeightEstimate > 90
                        && o.WeightEstimate > 130)
                    .ToArray();
                listWeights.Add(new WeightRatio
                {
                    Quantity = livestock130.Length,
                    Ratio = livestock130.Length / quantityBySpecie * 100,
                    WeightRange = "90-130 kg"
                });
                var livestock160 = livestocksBySpecie
                    .Where(o => o.WeightEstimate != null
                        && o.WeightEstimate > 130
                        && o.WeightEstimate < 160)
                    .ToArray();
                listWeights.Add(new WeightRatio
                {
                    Quantity = livestock160.Length,
                    Ratio = livestock160.Length / quantityBySpecie * 100,
                    WeightRange = "130-160 kg"
                });
                var livestock190 = livestocksBySpecie
                   .Where(o => o.WeightEstimate != null
                       && o.WeightEstimate > 160
                       && o.WeightEstimate > 190)
                   .ToArray();
                listWeights.Add(new WeightRatio
                {
                    Quantity = livestock190.Length,
                    Ratio = livestock190.Length / quantityBySpecie * 100,
                    WeightRange = "160-190 kg"
                });
                var livestock250 = livestocksBySpecie
                   .Where(o => o.WeightEstimate != null
                       && o.WeightEstimate > 190
                       && o.WeightEstimate > 250)
                   .ToArray();
                listWeights.Add(new WeightRatio
                {
                    Quantity = livestock250.Length,
                    Ratio = livestock250.Length / quantityBySpecie * 100,
                    WeightRange = "190-250 kg"
                });
                var livestockMax = livestocksBySpecie
                   .Where(o => o.WeightEstimate != null
                       && o.WeightEstimate > 250)
                   .ToArray();
                listWeights.Add(new WeightRatio
                {
                    Quantity = livestockMax.Length,
                    Ratio = livestockMax.Length / quantityBySpecie * 100,
                    WeightRange = ">250 kg"
                });
                summaryBySpecie.WeightRatios = listWeights;
                resultItems.Add(summaryBySpecie);
            }
            result.Items = resultItems;
            result.Total = resultItems.Count();
            return result;    
        }

        public async Task<int> GetLivestockMissingInforQuantity()
        {
            var livestocks = await _context.Livestocks
                .Where(o => (o.Status == livestock_status.CHỜ_ĐỊNH_DANH
                    || o.Status == livestock_status.KHỎE_MẠNH
                    || o.Status == livestock_status.ỐM)
                    && (string.IsNullOrEmpty(o.Color)
                    || (o.Dob == null || o.Dob == DateTime.MinValue)
                    || string.IsNullOrEmpty(o.Origin)
                    || (o.WeightOrigin == null || o.WeightOrigin == 0)
                    || (o.WeightEstimate == null || o.WeightEstimate == 0))
                )
                .ToArrayAsync();
            return livestocks.Length;
        }

        public async Task<string> GetDiseaseReport()
        {
            var medicalHistories = await _context.MedicalHistories
                .Where(o => o.Status == medical_history_status.CHỜ_KHÁM
                    || o.Status == medical_history_status.ĐANG_ĐIỀU_TRỊ
                    || o.Status == medical_history_status.TÁI_PHÁT)
                .ToArrayAsync();
            if (!medicalHistories.Any())
                throw new Exception("Không có vật nuôi nào đang bị bệnh");
            var totalQuantity = medicalHistories.Length; 
            var diseaseIds = medicalHistories
                .GroupBy(o => o.DiseaseId)
                .Select(o => o.Key)
                .ToArray();
            var diseases = await _context.Diseases
                .Where(o => diseaseIds.Contains(o.Id))
                .ToListAsync();
            var dicDiseases = diseases
                .ToDictionary(o => o.Id, o => o.Name);
            var diseaseNames = diseases
                .Select(o => o.Name)
                .OrderBy(o => o)
                .ToList();
            var livestockIds = medicalHistories
                .GroupBy(o => o.LivestockId)
                .Select(o => o.Key)
                .ToArray();
            var livestocks = await _context.Livestocks
                .Where(o => livestockIds.Contains(o.Id))
                .ToArrayAsync();
            var specieIds = livestocks
                .GroupBy(o => o.SpeciesId)
                .Select(o => o.Key)
                .ToArray();
            var species = await _context.Species
                .Where(o => specieIds.Contains(o.Id))
                .ToArrayAsync();
            var diseasesBySpecie = species
                .Select(o => new DiseaseBySpecie
                {
                    SpecieId = o.Id,
                    SpecieName = o.Name
                })
                .ToArray();
            foreach (var item in diseasesBySpecie)
            {
                var livestockIdsBySpecie = livestocks
                    .Where(o => o.SpeciesId == item.SpecieId)
                    .Select(o => o.Id)
                    .Distinct()
                    .ToArray();
                item.DiseaseQuantities = medicalHistories
                    .Where(o => livestockIdsBySpecie.Contains(o.LivestockId))
                    .GroupBy(o => o.DiseaseId)
                    .Select(o => new DiseaseQuantity
                    {
                        DiseaseId = o.Key,
                        DiseaseName = dicDiseases.ContainsKey(o.Key) ? dicDiseases[o.Key] : "N/A",
                        Quantity = o.Count(),
                        Ratio = o.Count() / totalQuantity
                    })
                    .ToArray();
            }

            //Export to file
            var nowTime = DateTime.Now;
            var stringDate = $"Ngày {nowTime.Day}, tháng {nowTime.Month}, năm {nowTime.Year}";
            var fileName = $"Báo cáo dịch bệnh_{nowTime.ToString("yyyyMMddhhmmss")}.xlsx";

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
            tmpCols.Add("Tổng");
            columns = tmpCols.ToArray();
            var data = new DataTable();
            data.Columns.AddRange(columns.Select(o => new DataColumn(o)).ToArray());

            diseasesBySpecie = diseasesBySpecie
                .OrderBy(o => o.SpecieName)
                .ToArray();
            foreach (var item in diseasesBySpecie)
            {
                var row = data.NewRow();
                foreach (var column in columns)
                {
                    if (column == "Giống/Bệnh")
                    {
                        row[column] = item.SpecieName;
                        continue;
                    }
                    if (column == "Tổng")
                    {
                        row[column] = item.DiseaseQuantities.Sum(o => o.Quantity);
                        continue;
                    }
                    row[column] = item.DiseaseQuantities.FirstOrDefault(o => o.DiseaseName == column)?.Quantity ?? 0;
                }
                data.Rows.Add(row);
            }
            var groupByDisease = diseasesBySpecie
                    .SelectMany(o => o.DiseaseQuantities)
                    .GroupBy(o => o.DiseaseName)
                    .Select(o => new
                    {
                        o.Key,
                        total = o.Sum(x => x.Quantity)
                    })
                    .ToArray();
            var totalRow = data.NewRow();
            foreach (var column in columns)
            {
                if (column == "Giống/Bệnh")
                {
                    totalRow[column] = "Tổng";
                    continue;
                }
                if (column == "Tổng")
                {
                    totalRow[column] = totalQuantity;
                    continue;
                }
                totalRow[column] = groupByDisease.FirstOrDefault(o => o.Key == column)?.total ?? 0;
            }
            data.Rows.Add(totalRow);

            worksheet.Cells["A3"].LoadFromDataTable(data, true, TableStyles.Light1);
            await package.SaveAsync();
            var stream = package.Stream;
            stream.Position = 0;
            var url = await _cloudinaryService.UploadFileStreamAsync(CloudFolderFileReportsName, fileName, stream);
            return url;
        }

        public async Task<string> GetWeightBySpecieReport()
        {
            var weightRatiosBySpecie = await GetWeightRatios();
            if (weightRatiosBySpecie.Total == 0)
                throw new Exception("Không tìm thấy vật nuôi");
            var totalQuantity = weightRatiosBySpecie.Items
                .Sum(o => o.TotalQuantity);

            //Export to file
            var nowTime = DateTime.Now;
            var stringDate = $"Ngày {nowTime.Day}, tháng {nowTime.Month}, năm {nowTime.Year}";
            var fileName = $"Phân bổ vật nuôi theo trọng lượng_{nowTime.ToString("yyyyMMddhhmmss")}.xlsx";

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new MemoryStream());
            var worksheet = package.Workbook.Worksheets.Add($"Phân bổ trọng lượng");

            var richTextRow1 = worksheet.Cells["A1"].RichText;
            var richTextRow2 = worksheet.Cells["A2"].RichText;
            var boldSegmentRow1 = richTextRow1.Add(OrganizationName);
            boldSegmentRow1.Bold = true;
            var boldSegmentRow2 = richTextRow2.Add(stringDate);
            boldSegmentRow2.Bold = true;

            var columns = new string[] { "Giống/Trọng lượng", "N/A", "<90 kg", "90-130 kg", "130-160 kg", "160-190 kg", "190-250 kg", ">250 kg", "Tổng" };
            var data = new DataTable();
            data.Columns.AddRange(columns.Select(o => new DataColumn(o)).ToArray());

            var ratios = weightRatiosBySpecie.Items
                .OrderBy(o => o.SpecieName)
                .ToArray();
            foreach (var item in ratios)
            {
                var row = data.NewRow();
                foreach (var column in columns)
                {
                    if (column == "Giống/Trọng lượng")
                    {
                        row[column] = item.SpecieName;
                        continue;
                    }
                    if (column == "Tổng")
                    {
                        row[column] = item.WeightRatios.Sum(o => o.Quantity);
                        continue;
                    }
                    row[column] = item.WeightRatios.FirstOrDefault(o => o.WeightRange == column)?.Quantity ?? 0;
                }
                data.Rows.Add(row);
            }
            var groupByWeight = weightRatiosBySpecie.Items
                   .SelectMany(o => o.WeightRatios)
                   .GroupBy(o => o.WeightRange)
                   .Select(o => new
                   {
                       o.Key,
                       total = o.Sum(x => x.Quantity)
                   })
                   .ToArray();
            var totalRow = data.NewRow();
            foreach (var column in columns)
            {
                if (column == "Giống/Trọng lượng")
                {
                    totalRow[column] = "Tổng";
                    continue;
                }
                if (column == "Tổng")
                {
                    totalRow[column] = totalQuantity;
                    continue;
                }
                totalRow[column] = groupByWeight.FirstOrDefault(o => o.Key == column)?.total ?? 0;
            }
            data.Rows.Add(totalRow);

            worksheet.Cells["A3"].LoadFromDataTable(data, true, TableStyles.Light1);
            await package.SaveAsync();
            var stream = package.Stream;
            stream.Position = 0;
            var url = await _cloudinaryService.UploadFileStreamAsync(CloudFolderFileReportsName, fileName, stream);
            return url;
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
            throw new Exception("Tính năng hiện tại đang phát triển.");
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
            throw new Exception("Tính năng hiện tại đang phát triển.");
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
            var emptyRecordIds = await _context.Livestocks
                .Where(o => string.IsNullOrEmpty(o.InspectionCode)
                    && o.Status == livestock_status.TRỐNG)
                .Select(o => o.Id)
                .Distinct()
                .ToArrayAsync();
            if (!emptyRecordIds.Any())
                throw new Exception("Hiện không có mã QR trống nào. Vui lòng tạo thêm.");

            var nowTime = DateTime.Now;
            var fileName = $"QR_codes_{nowTime:yyyyMMddHHmmss}.zip";

            // Generate QR codes and insert them into Excel
            var zipStream = new MemoryStream();

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var generator = new QRCodeGenerator();
                int index = 1;

                foreach (var id in emptyRecordIds)
                {
                    var uri = urlDeploy + id;
                    var qrData = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
                    var pngQr = new PngByteQRCode(qrData);
                    byte[] qrBytes = pngQr.GetGraphic(20);

                    var entry = archive.CreateEntry(SanitizeFileName($"QR_{index}.png"));
                    using var entryStream = entry.Open();
                    entryStream.Write(qrBytes, 0, qrBytes.Length);

                    index++;
                }
            }
            zipStream.Position = 0;

            var url = await _cloudinaryService.UploadFileStreamAsync(CloudFolderFileQrCodesName, fileName, zipStream);
            return url;
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
            throw new Exception("Tính năng hiện tại đang phát triển.");
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

            // Get export information Order
            var exportDetail = livestock.BatchExportDetails?.OrderByDescending(b => b.CreatedAt).FirstOrDefault();

            // Get export order details
            var exportOrder = await _context.OrderDetails
                .FirstOrDefaultAsync(x => x.LivestockId == request.LivestockId);

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
                ExportDate = exportDetail?.ExportDate ?? exportOrder?.ExportedDate,
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
            if (request == null || (string.IsNullOrEmpty(request.LivestockId) && string.IsNullOrEmpty(request.InspectionCode) && string.IsNullOrEmpty(request.SpecieId)))
                throw new Exception("Hãy quét mã QR hoặc điền đầy đủ thông tin");
            var livestock = await _context.Livestocks
                .FirstOrDefaultAsync(o => (!string.IsNullOrEmpty(request.LivestockId) && o.Id == request.LivestockId) ||
                           (!string.IsNullOrEmpty(request.InspectionCode) && !string.IsNullOrEmpty(request.SpecieId) &&
                            o.InspectionCode == request.InspectionCode && o.SpeciesId == request.SpecieId));
            if (livestock == null)
                throw new Exception("Không tìm thấy vật nuôi phù hợp");
            if (request.Weight != null && request.Weight < 0)
                throw new Exception("Trọng lượng phải là số lớn hơn 0");
            if (request.WeightOrigin != null && request.WeightOrigin < 0)
                throw new Exception("Trọng lượng nhập phải là số lớn hơn 0");
            if (livestock == null)
                throw new Exception("Không tìm được vật nuôi");
            if (!string.IsNullOrEmpty(request.Color))
                livestock.Color = request.Color;
            if (!string.IsNullOrEmpty(request.Origin))
                livestock.Origin = request.Origin;
            if (request.WeightOrigin != null)
                livestock.WeightOrigin = request.WeightOrigin;
            if (request.Weight != null)
                livestock.WeightEstimate = request.Weight;
            livestock.UpdatedAt = DateTime.Now;
            livestock.UpdatedBy = request.RequestedBy;
            return;
        }

        public async Task RecordLivestockDiseases(RecordLivstockDiseases request)
        {
            if (request == null || (string.IsNullOrEmpty(request.LivestockId) && string.IsNullOrEmpty(request.InspectionCode) && string.IsNullOrEmpty(request.SpecieId)))
                throw new Exception("Hãy quét mã QR hoặc điền đầy đủ thông tin");
            if (request.DiseaseIds == null || !request.DiseaseIds.Any())
                throw new Exception("Hãy chọn loại bệnh muốn ghi nhận");

            var livestock = await _context.Livestocks
                .FirstOrDefaultAsync(o => (!string.IsNullOrEmpty(request.LivestockId) && o.Id == request.LivestockId) ||
                           (!string.IsNullOrEmpty(request.InspectionCode) && !string.IsNullOrEmpty(request.SpecieId) &&
                            o.InspectionCode == request.InspectionCode && o.SpeciesId == request.SpecieId));
            if (livestock == null)
                throw new Exception("Không tìm thấy vật nuôi phù hợp");
            var diseaseMedicines = await _context.DiseaseMedicines
                .Where(o => request.DiseaseIds.Contains(o.DiseaseId))
                .ToDictionaryAsync(o => o.DiseaseId, o => o.MedicineId);

            var newMedicalHistories = request.DiseaseIds
                .Select(o => new MedicalHistory
                {
                    Id = SlugId.New(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = request.RequestedBy,
                    UpdatedBy = request.RequestedBy,
                    LivestockId = livestock.Id,
                    DiseaseId = o,
                    MedicineId = request.MedicineIds?.FirstOrDefault(x => x == diseaseMedicines[o]) ?? null,
                    Symptom = request.Symptoms,
                    Status = medical_history_status.CHỜ_KHÁM,
                    RecoverDate = null,
                })
                .ToArray();
            await _context.MedicalHistories.AddRangeAsync(newMedicalHistories);
            await _context.SaveChangesAsync();
            return;
        }

        private string SanitizeFileName(string input)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                input = input.Replace(c, '_');
            return input;
        }
    }
}
