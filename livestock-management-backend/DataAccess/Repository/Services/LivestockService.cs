using BusinessObjects;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using ClosedXML.Excel;
using DataAccess.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;
using static BusinessObjects.Constants.LmsConstants;
using ClosedXML;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace DataAccess.Repository.Services
{
    public class LivestockService : ILivestockRepository
    {
        private readonly LmsContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public LivestockService(LmsContext context, CloudinaryService cloudinaryService)
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
                ws.Column(2).Width = qrSize *0.75;

                int row = 2;
                int stt = 1;
                foreach (var item in listLivestock)
                {
                    ws.Cell(row, 1).Value = stt;   

                    byte[] qrBytes = GenerateQRCode(urlDeploy+item.Id.ToString());

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

            var listHistory = await _context.MedicalHistories.Include(x=>x.Medicine).Include(x=>x.Disease)
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

            var vaccinationHistory = await _context.LivestockVaccinations.Include(x=>x.BatchVaccination).ThenInclude(x=>x.Vaccine)
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
                throw new Exception("Không tìm thấy id cho loài "+ model.SpecieType);

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

            var livestock = await _context.Livestocks.Include(x=>x.Species)
                .Where(o => o.InspectionCode == inspectionCode && o.Species.Type== specieType)
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
            return 
                @"https://www.google.com/url?sa=i&url=https%3A%2F%2Fcharacter-stats-and-profiles.fandom.com%2Fwiki%2FTung_Tung_Tung_Sahur_%2528Italian_Brainrot%2C_Canon%2FEvanTheProNoob%2529&psig=AOvVaw2PISjrk2prM3siorJ4uF5l&ust=1748010304176000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCKiByPujt40DFQAAAAAdAAAAABAU";
        }

        public async Task<string> GetWeightBySpecieReport()
        {
            return
                @"https://www.google.com/url?sa=i&url=https%3A%2F%2Fcharacter-stats-and-profiles.fandom.com%2Fwiki%2FTung_Tung_Tung_Sahur_%2528Italian_Brainrot%2C_Canon%2FEvanTheProNoob%2529&psig=AOvVaw2PISjrk2prM3siorJ4uF5l&ust=1748010304176000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCKiByPujt40DFQAAAAAdAAAAABAU";
        }
    }
}
