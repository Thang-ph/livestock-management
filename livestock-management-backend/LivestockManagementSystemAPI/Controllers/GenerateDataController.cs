using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/generate")]
    [ApiController]
    [AllowAnonymous]
    public class GenerateController : BaseAPIController
    {
        private readonly LmsContext _context;

        public GenerateController(LmsContext context)
        {
            _context = context;
        }

        [HttpPut("procurement")]
        public async Task<ActionResult<bool>> GenerateProcurements([FromForm] int number)
        {
            try
            {
                if (number == 0 || number < 0)
                    throw new Exception("Number must larger than 0");

                // Ensure the database is created
                _context.Database.EnsureCreated();

                // Check if data already exists
                if (_context.ProcurementDetails.Any())
                {
                    var currentProcurementDetails = await _context.ProcurementDetails.ToListAsync();
                    _context.ProcurementDetails.RemoveRange(currentProcurementDetails);
                    await _context.SaveChangesAsync();
                }

                if (_context.ProcurementPackages.Any())
                {
                    var currentProcurements = await _context.ProcurementPackages.ToListAsync();
                    _context.ProcurementPackages.RemoveRange(currentProcurements);
                    await _context.SaveChangesAsync();
                }

                var listSpecies = await _context.Species
                    .ToArrayAsync();
                if (listSpecies == null || !listSpecies.Any())
                    throw new Exception("No species found");

                Random random = new Random();
                for (int i = 1; i <= number; i++)
                {
                    var package = new ProcurementPackage
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        Name = $"Procurement Package Number {i}",
                        Code = $"MDD-{i}",
                        Owner = $"Owner Number {i}",
                        ExpiredDuration = 30,
                        SuccessDate = null,
                        ExpirationDate = DateTime.Now.AddDays(30),
                        Status = procurement_status.ĐANG_ĐẤU_THẦU,
                        Description = "This is a sample procurement package."
                    };
                    await _context.ProcurementPackages.AddAsync(package);

                    int randomIndex = random.Next(listSpecies.Length);
                    var packageDetails = new ProcurementDetail
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        ProcurementPackageId = package.Id,
                        SpeciesId = listSpecies[randomIndex].Id,
                        RequiredAgeMin = 365,
                        RequiredAgeMax = random.Next(365, 400),
                        RequiredWeightMin = random.Next(180, 190),
                        RequiredWeightMax = random.Next(200, 210),
                        RequiredInsurance = 21,
                        RequiredQuantity = random.Next(50, 100)
                    };
                    await _context.ProcurementDetails.AddAsync(packageDetails);


                }
                await _context.SaveChangesAsync();
                return GetSuccess(true);

            }
            catch (Exception ex)
            {
                return GetError(ex.Message);
            }
        }

        [HttpPut("species")]
        public async Task<ActionResult<bool>> GenerateSpecies()
        {
            try
            {
                // Ensure the database is created
                _context.Database.EnsureCreated();

                // Check if data already exists
                if (_context.Species.Any())
                {
                    var currentSpecies = await _context.Species.ToListAsync();
                    _context.Species.RemoveRange(currentSpecies);
                    await _context.SaveChangesAsync();
                }

                var listSpecies = new List<Species>() {
                    new Species
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        Name = "Bò lai Sind",
                        DressingPercentage = (decimal)49.6,
                        GrowthRate = 1800,
                        Description = "Màu lông đỏ, vàng sẫm hoặc đỏ cánh gián",
                    },
                    new Species
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        Name = "Bò Sind",
                        DressingPercentage = (decimal)49,
                        GrowthRate = 1800,
                        Description = "màu lông đỏ cánh gián, có mảng tối ở cổ, u vai",
                    },
                    new Species
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        Name = "Bò Brahman",
                        DressingPercentage = (decimal)53.5,
                        GrowthRate = 1800,
                        Description = "màu lông đỏ hoặc trắng",
                    },
                    new Species
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        Name = "Bò Sahiwal",
                        DressingPercentage = (decimal)53,
                        GrowthRate = 1800,
                        Description = "màu da từ vàng sẫm đến nâu đỏ, mõm và lông mi có màu sáng",
                    },
                    new Species
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        Name = "Bò Angus",
                        DressingPercentage = (decimal)66,
                        GrowthRate = 1800,
                        Description = "lông màu đen hoặc đỏ",
                    },
                };

                await _context.Species.AddRangeAsync(listSpecies);
                await _context.SaveChangesAsync();
                return GetSuccess(true);

            }
            catch (Exception ex)
            {
                return GetError(ex.Message);
            }
        }

        [HttpPut("barn")]
        public async Task<ActionResult<bool>> GenerateBarns()
        {
            try
            {
                // Ensure the database is created
                _context.Database.EnsureCreated();

                // Check if data already exists
                if (_context.Barns.Any())
                {
                    var currentBarns = await _context.Barns.ToListAsync();
                    _context.Barns.RemoveRange(currentBarns);
                    await _context.SaveChangesAsync();
                }

                var listBarns = new List<Barn>()
                {
                    new Barn
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        Name = "Trang trại Hợp tác xã Lúa Vàng cơ sở Bắc Giang",
                        Address = "Huyện Yên Dũng, Tỉnh Bắc Giang"
                    },
                    new Barn
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        Name = "Trang trại Hợp tác xã Lúa Vàng cơ sở Nghệ An",
                        Address = "Tỉnh Nghệ An"
                    }
                };

                await _context.Barns.AddRangeAsync(listBarns);

                await _context.SaveChangesAsync();
                return GetSuccess(true);

            }
            catch (Exception ex)
            {
                return GetError(ex.Message);
            }
        }

        [HttpPut("livestocks")]
        public async Task<ActionResult<bool>> GenerateLivestocks([FromForm] int number)
        {
            try
            {
                if (number == 0 || number < 0)
                    throw new Exception("Number must larger than 0");

                // Ensure the database is created
                _context.Database.EnsureCreated();

                // Check if data already exists
                if (_context.Livestocks.Any())
                {
                    var currentLicestocks = await _context.Livestocks.ToListAsync();
                    _context.Livestocks.RemoveRange(currentLicestocks);
                    await _context.SaveChangesAsync();
                }

                var listGenders = new livestock_gender[]
                {
                    livestock_gender.ĐỰC,
                    livestock_gender.CÁI
                };
                var listColors = new string[]
                {
                    "Đen", "Đỏ", "Trắng", "Vàng sẫm", "Nâu đỏ", "Vàng cánh gián", "Đỏ cánh gián"
                };
                var listSpecies = await _context.Species
                    .ToArrayAsync();
                if (listSpecies == null || !listSpecies.Any())
                    throw new Exception("No species found");
                var listBarns = await _context.Barns
                    .ToArrayAsync();
                if (listBarns == null || !listBarns.Any())
                    throw new Exception("No barns found");

                Random random = new Random();
                for (int i = 1; i <= number; i++)
                {
                    int randomGender = random.Next(listGenders.Length);
                    int randomColor = random.Next(listColors.Length);
                    int randomSpecies = random.Next(listSpecies.Length);
                    int randomBarn = random.Next(listBarns.Length);
                    decimal randomWeightOrigin = (decimal)random.NextDouble() * (30 - 10) + 10;
                    var livestock = new Livestock
                    {
                        Id = SlugId.New(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = "SYS",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "SYS",
                        InspectionCode = i.ToString("D6"),
                        Dob = DateTime.Now.AddDays(-random.Next(20, 30)),
                        Gender = listGenders[randomGender],
                        Color = listColors[randomColor],
                        Origin = "Việt Nam",
                        SpeciesId = listSpecies[randomSpecies].Id,
                        WeightOrigin = randomWeightOrigin,
                        WeightExport = null,
                        WeightEstimate = randomWeightOrigin + 15,
                        BarnId = listBarns[randomBarn].Id,
                        Status = livestock_status.KHỎE_MẠNH
                    };
                    await _context.Livestocks.AddAsync(livestock);

                }
                await _context.SaveChangesAsync();
                return GetSuccess(true);

            }
            catch (Exception ex)
            {
                return GetError(ex.Message);
            }
        }

        [HttpPut("disease-sql")]
        public async Task<ActionResult<bool>> GenerateDiseaseSQLQuery()
        {
            try
            {
                // Ensure the database is created
                _context.Database.EnsureCreated();


            string sqlQuery = $@"
                 INSERT INTO [dbo].[Disease] (Id, Name, Symptom, Description, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) VALUES
                 ('{SlugId.New()}', N'Lở mồm long móng', N'Sốt, mụn nước ở miệng, lưỡi, chân và vú, chảy nước dãi nhiều.', N'Bệnh truyền nhiễm do virus gây ra, ảnh hưởng đến gia súc có móng chẵn.', '{DateTime.UtcNow}', '{DateTime.UtcNow}', N'SYS', N'SYS'),
                 ('{SlugId.New()}', N'Tụ huyết trùng', N'Sốt cao, khó thở, sưng cổ, tiết nhiều nước mũi có lẫn máu.', N'Bệnh do vi khuẩn Pasteurella multocida gây ra, có thể gây nhiễm trùng huyết.', '{DateTime.UtcNow}', '{DateTime.UtcNow}', N'SYS', N'SYS'),
                 ('{SlugId.New()}', N'Viêm da nổi cục', N'Nổi cục trên da, sốt, chán ăn, giảm sản lượng sữa.', N'Bệnh do virus lây qua côn trùng và tiếp xúc trực tiếp.', '{DateTime.UtcNow}', '{DateTime.UtcNow}', N'SYS', N'SYS'),
                 ('{SlugId.New()}', N'Sán lá gan', N'Gầy yếu, vàng da, tiêu chảy hoặc táo bón, chậm lớn.', N'Bệnh do sán Fasciola spp. gây tổn thương gan.', '{DateTime.UtcNow}', '{DateTime.UtcNow}', N'SYS', N'SYS'),
                 ('{SlugId.New()}', N'Tiêu chảy do E. coli', N'Tiêu chảy kéo dài, mất nước, suy nhược, biếng ăn.', N'Bệnh do vi khuẩn E. coli gây tiêu chảy nghiêm trọng.', '{DateTime.UtcNow}', '{DateTime.UtcNow}', N'SYS', N'SYS');
                 ";

                // Xóa ký tự xuống dòng
                sqlQuery = sqlQuery.Replace("\r\n", " ").Replace("\n", " ");

                return GetSuccess(sqlQuery);
            }
            catch (Exception ex)
            {
                return GetError(ex.Message);
            }
        }

        [HttpPut("medical")]
        public async Task<ActionResult<bool>> GenerateMedical()
        {
            try
            {
                // Ensure the database is created
                _context.Database.EnsureCreated();
                var random = new Random();
                var listTypes = new medicine_type[]
                {
                     medicine_type.KHÁNG_SINH,
                     medicine_type.THUỐC_CHỮA_BỆNH,
                        medicine_type.VACCINE
                };

                var listMedical = new List<Medicine>()
                    {
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Thuốc kháng virus FMD",
                            Description = "Thuốc điều trị hỗ trợ bệnh lở mồm long móng, giúp giảm triệu chứng và tăng đề kháng.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Vắc xin LMLM",
                            Description = "Vắc xin phòng bệnh lở mồm long móng, giúp tạo miễn dịch bảo vệ đàn gia súc.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Kháng sinh Tylosin",
                            Description = "Thuốc kháng sinh điều trị bệnh tụ huyết trùng do vi khuẩn Pasteurella multocida.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Vắc xin tụ huyết trùng",
                            Description = "Vắc xin giúp phòng ngừa bệnh tụ huyết trùng ở trâu bò.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Thuốc Ivermectin",
                            Description = "Thuốc trị viêm da nổi cục do ký sinh trùng và côn trùng lây nhiễm.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Vắc xin viêm da nổi cục",
                            Description = "Vắc xin giúp phòng ngừa bệnh viêm da nổi cục cho trâu bò.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Thuốc Albendazole",
                            Description = "Thuốc điều trị sán lá gan, giúp loại bỏ ký sinh trùng trong gan của trâu bò.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Thuốc Oxyclozanide",
                            Description = "Thuốc diệt sán lá gan, bảo vệ gan và hệ tiêu hóa của gia súc.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Kháng sinh Enrofloxacin",
                            Description = "Thuốc kháng sinh điều trị tiêu chảy do vi khuẩn E. coli gây ra.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        },
                        new Medicine
                        {
                            Id = SlugId.New(),
                            Name = "Điện giải Glucose",
                            Description = "Dung dịch bổ sung điện giải giúp trâu bò hồi phục khi mắc tiêu chảy.",
                            Type = listTypes[random.Next(listTypes.Length)],
                            CreatedAt = DateTime.Now,
                            CreatedBy = "SYS",
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = "SYS",
                        }
                    };
                await _context.Medicines.AddRangeAsync(listMedical);

                await _context.SaveChangesAsync();
                return GetSuccess(listMedical);
            }
            catch (Exception ex)
            {
                return GetError(ex.Message);
            }
        }

        [HttpPut("diseaseMedicine-sql")]
        public async Task<ActionResult<bool>> GenerateMedicineSQLQuery()
        {
            try
            {
                // Ensure the database is created
                _context.Database.EnsureCreated();


                string sqlQuery = "INSERT INTO [dbo].[DiseaseMedicine] (Id, DiseaseId, MedicineId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) VALUES\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Lở mồm long móng'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Thuốc kháng virus FMD'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Lở mồm long móng'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin LMLM'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Tụ huyết trùng'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Kháng sinh Tylosin'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Tụ huyết trùng'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin tụ huyết trùng'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Viêm da nổi cục'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Thuốc Ivermectin'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Viêm da nổi cục'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin viêm da nổi cục'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Sán lá gan'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Thuốc Albendazole'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Sán lá gan'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Thuốc Oxyclozanide'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Tiêu chảy do E. coli'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Kháng sinh Enrofloxacin'), GETDATE(), GETDATE(), N'SYS', N'SYS'),\r\n(NEWID(), (SELECT Id FROM [dbo].[Disease] WHERE Name = N'Tiêu chảy do E. coli'), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Điện giải Glucose'), GETDATE(), GETDATE(), N'SYS', N'SYS');\r\n";

                // Xóa ký tự xuống dòng
                sqlQuery = sqlQuery.Replace("\r\n", " ").Replace("\n", " ");

                return GetSuccess(sqlQuery);
            }
            catch (Exception ex)
            {
                return GetError(ex.Message);
            }
        }

        [HttpPut("batch-vaccination-sql")]
        public async Task<ActionResult<bool>> GenerateBatchVaccinationSQLQuery([FromForm] int number)
        {
            try
            {
                // Ensure the database is created
                _context.Database.EnsureCreated();
                string sqlQuery = "INSERT INTO [dbo].[BatchVaccinations] (Id, DateSchedule, DateConduct, VaccineId, Description, Status, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, ConductedBy, Type, Name) VALUES\r\n(NEWID(), DATEADD(DAY, -10, GETDATE()), GETDATE(), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin LMLM'), N'Lô tiêm phòng bệnh Lở mồm long móng', N'ĐANG_THỰC_HIỆN', GETDATE(), GETDATE(), N'SYS', N'SYS', N'Kim Văn Dự', N'TIÊM_VACCINE', N'Lô tiêm LMLM 2025'),\r\n(NEWID(), DATEADD(DAY, -15, GETDATE()), NULL, (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin tụ huyết trùng'), N'Lô tiêm phòng bệnh tụ huyết trùng', N'ĐÃ_HỦY', GETDATE(), GETDATE(), N'SYS', N'SYS', N'N/A', N'TIÊM_VACCINE', N'Lô tiêm Tụ huyết trùng 2025'),\r\n(NEWID(), DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -2, GETDATE()), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin viêm da nổi cục'), N'Lô tiêm phòng viêm da nổi cục', N'HOÀN_THÀNH', GETDATE(), GETDATE(), N'SYS', N'SYS', N'Nguyễn Trung Điện', N'TIÊM_VACCINE', N'Lô tiêm Viêm da nổi cục 2025'),\r\n(NEWID(), DATEADD(DAY, 5, GETDATE()), NULL, (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin LMLM'), N'Lô tiêm phòng bệnh Lở mồm long móng sắp tới', N'CHỜ_THỰC_HIỆN', GETDATE(), GETDATE(), N'SYS', N'SYS', N'N/A', N'TIÊM_VACCINE', N'Lô tiêm LMLM 2025 - Đợt 2'),\r\n(NEWID(), DATEADD(DAY, -3, GETDATE()), GETDATE(), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin tụ huyết trùng'), N'Lô tiêm chữa bệnh tụ huyết trùng', N'ĐANG_THỰC_HIỆN', GETDATE(), GETDATE(), N'SYS', N'SYS', N'Kim Văn Dự', N'TIÊM_CHỮA_BỆNH', N'Lô chữa trị tụ huyết trùng 2025'),\r\n(NEWID(), DATEADD(DAY, 10, GETDATE()), NULL, (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Vắc xin viêm da nổi cục'), N'Lô tiêm phòng bệnh viêm da nổi cục đợt tới', N'CHỜ_THỰC_HIỆN', GETDATE(), GETDATE(), N'SYS', N'SYS', N'N/A', N'TIÊM_VACCINE', N'Lô tiêm Viêm da nổi cục 2025 - Đợt 2'),\r\n(NEWID(), DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -15, GETDATE()), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Thuốc Ivermectin'), N'Lô tiêm chữa bệnh viêm da nổi cục', N'HOÀN_THÀNH', GETDATE(), GETDATE(), N'SYS', N'SYS', N'Nguyễn Trung Điện', N'TIÊM_CHỮA_BỆNH', N'Lô chữa trị viêm da nổi cục 2025'),\r\n(NEWID(), DATEADD(DAY, -25, GETDATE()), NULL, (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Thuốc Albendazole'), N'Lô tiêm chữa bệnh sán lá gan', N'ĐÃ_HỦY', GETDATE(), GETDATE(), N'SYS', N'SYS', N'N/A', N'TIÊM_CHỮA_BỆNH', N'Lô chữa trị sán lá gan 2025'),\r\n(NEWID(), DATEADD(DAY, -5, GETDATE()), GETDATE(), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Kháng sinh Enrofloxacin'), N'Lô chữa bệnh tiêu chảy do E. coli', N'ĐANG_THỰC_HIỆN', GETDATE(), GETDATE(), N'SYS', N'SYS', N'Kim Văn Dự', N'TIÊM_CHỮA_BỆNH', N'Lô chữa trị tiêu chảy E. coli 2025'),\r\n(NEWID(), DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, -25, GETDATE()), (SELECT Id FROM [dbo].[Medicines] WHERE Name = N'Điện giải Glucose'), N'Lô hỗ trợ điều trị tiêu chảy E. coli', N'HOÀN_THÀNH', GETDATE(), GETDATE(), N'SYS', N'SYS', N'Nguyễn Trung Điện', N'TIÊM_CHỮA_BỆNH', N'Lô hỗ trợ tiêu chảy E. coli 2025');\r\n";
                // Xóa ký tự xuống dòng
                sqlQuery = sqlQuery.Replace("\r\n", " ").Replace("\n", " ");
                return GetSuccess(sqlQuery);

            }
            catch (Exception ex)
            {
                return GetError(ex.Message);
            }
        }
    }
}
