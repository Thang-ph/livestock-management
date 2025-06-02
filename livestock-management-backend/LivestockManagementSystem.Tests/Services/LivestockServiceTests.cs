using AutoMapper;
using BusinessObjects;
using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DataAccess.Repository.Services;
using FluentAssertions;
using LivestockManagementSystem.Tests.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystem.Tests.Services
{
    public class LivestockServiceTests : IDisposable
    {
        private readonly LmsContext _context;
        private readonly LivestockService _livestockService;
        private readonly Mock<ICloudinaryRepository> _mockCloudinaryService;

        public LivestockServiceTests()
        {
            // Setup in-memory database
            _context = TestDbContextFactory.CreateInMemoryContext();

            // Setup mock cloudinary service
            _mockCloudinaryService = new Mock<ICloudinaryRepository>();

            // Create service instance
            _livestockService = new LivestockService(_context, _mockCloudinaryService.Object);

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            // Add species
            var species1 = new Species
            {
                Id = "test-species-1",
                Name = "Lợn Yorkshire",
                Description = "Giống lợn thịt",
                Type = specie_type.LỢN,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var species2 = new Species
            {
                Id = "test-species-2",
                Name = "Bò Wagyu",
                Description = "Giống bò thịt cao cấp",
                Type = specie_type.BÒ,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.Species.AddRange(species1, species2);

            // Add barn
            var barn = new Barn
            {
                Id = "test-barn-1",
                Name = "Chuồng số 1",
                Address = "Địa chỉ chuồng thử nghiệm",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.Barns.Add(barn);

            // Add livestock
            var livestock1 = new Livestock
            {
                Id = "test-livestock-1",
                InspectionCode = "LV001",
                Status = livestock_status.KHỎE_MẠNH,
                Gender = livestock_gender.ĐỰC,
                Color = "Trắng",
                Origin = "Việt Nam",
                SpeciesId = "test-species-1",
                BarnId = "test-barn-1",
                WeightOrigin = 50.5m,
                WeightEstimate = 55.0m,
                Dob = DateTime.Now.AddMonths(-6),
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var livestock2 = new Livestock
            {
                Id = "test-livestock-2",
                InspectionCode = "LV002",
                Status = livestock_status.ỐM,
                Gender = livestock_gender.CÁI,
                Color = "Đen",
                Origin = "Thái Lan",
                SpeciesId = "test-species-2",
                BarnId = "test-barn-1",
                WeightOrigin = 200.0m,
                WeightEstimate = 220.0m,
                Dob = DateTime.Now.AddMonths(-12),
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var livestock3 = new Livestock
            {
                Id = "test-livestock-3",
                InspectionCode = "", // Empty inspection code for testing
                Status = livestock_status.KHỎE_MẠNH,
                Gender = livestock_gender.ĐỰC,
                SpeciesId = "test-species-1",
                BarnId = "test-barn-1",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.Livestocks.AddRange(livestock1, livestock2, livestock3);

            // Add disease for medical history
            var disease = new Disease
            {
                Id = "test-disease-1",
                Name = "Bệnh cúm",
                Symptom = "Sốt, ho",
                Description = "Bệnh cúm gia súc",
                Type = disease_type.TRUYỀN_NHIỄM_NGUY_HIỂM,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.Diseases.Add(disease);

            // Add medical history
            var medicalHistory = new MedicalHistory
            {
                Id = "test-medical-1",
                LivestockId = "test-livestock-2",
                DiseaseId = "test-disease-1",
                Status = medical_history_status.ĐANG_ĐIỀU_TRỊ,
                Symptom = "Sốt cao, mệt mỏi",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.MedicalHistories.Add(medicalHistory);

            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetListLivestocks_WithoutFilter_ShouldReturnAllLivestocksWithInspectionCode()
        {
            // Arrange
            var filter = new ListLivestocksFilter();

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(2); // Only livestock with inspection codes
            result.Items.Should().HaveCount(2);
            result.Items.Should().Contain(l => l.InspectionCode == "LV001");
            result.Items.Should().Contain(l => l.InspectionCode == "LV002");
        }

        [Fact]
        public async Task GetListLivestocks_WithKeywordFilter_ShouldReturnFilteredLivestock()
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                Keyword = "LV001"
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.First().InspectionCode.Should().Be("LV001");
        }

        [Fact]
        public async Task GetListLivestocks_WithWeightFilter_ShouldReturnFilteredLivestock()
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                MinWeight = 100m,
                MaxWeight = 300m
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.First().InspectionCode.Should().Be("LV002");
            result.Items.First().Weight.Should().Be(220.0m);
        }

        [Fact]
        public async Task GetListLivestocks_WithSpeciesFilter_ShouldReturnFilteredLivestock()
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                SpeciesIds = new[] { "test-species-2" }
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.First().InspectionCode.Should().Be("LV002");
            result.Items.First().Species.Should().Be("Bò Wagyu");
        }

        [Fact]
        public async Task GetListLivestocks_WithStatusFilter_ShouldReturnFilteredLivestock()
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                Statuses = new[] { livestock_status.ỐM }
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.First().InspectionCode.Should().Be("LV002");
            result.Items.First().Status.Should().Be(livestock_status.ỐM);
        }

        [Fact]
        public async Task GetListLivestocks_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                Skip = 0,
                Take = 1
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetLivestockGeneralInfo_ValidId_ShouldReturnLivestockInfo()
        {
            // Act
            var result = await _livestockService.GetLivestockGeneralInfo("test-livestock-1");

            // Assert
            result.Should().NotBeNull();
            result.InspectionCode.Should().Be("LV001");
            result.Status.Should().Be(livestock_status.KHỎE_MẠNH);
            result.Gender.Should().Be(livestock_gender.ĐỰC);
            result.Color.Should().Be("Trắng");
            result.Origin.Should().Be("Việt Nam");
            result.WeightOrigin.Should().Be(50.5m);
            result.WeightEstimate.Should().Be(55.0m);
            result.BarnId.Should().Be("test-barn-1");
        }

        [Fact]
        public async Task GetLivestockGeneralInfo_EmptyId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _livestockService.GetLivestockGeneralInfo(""));

            exception.Message.Should().Be("Livestock id is missing");
        }

        [Fact]
        public async Task GetLivestockGeneralInfo_NullId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _livestockService.GetLivestockGeneralInfo(null));

            exception.Message.Should().Be("Livestock id is missing");
        }

        [Fact]
        public async Task GetLivestockGeneralInfo_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _livestockService.GetLivestockGeneralInfo("invalid-id"));

            exception.Message.Should().Be("Livestock not found");
        }

        [Fact]
        public async Task GetLivestockSummaryInfo_ValidId_ShouldReturnSummary()
        {
            // Act
            var result = await _livestockService.GetLivestockSummaryInfo("test-livestock-1");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-livestock-1");
            result.InspectionCode.Should().Be("LV001");
            result.Species.Should().Be("Lợn Yorkshire");
            result.Weight.Should().Be(55.0m);
            result.Gender.Should().Be(livestock_gender.ĐỰC);
            result.Color.Should().Be("Trắng");
            result.Origin.Should().Be("Việt Nam");
            result.Status.Should().Be(livestock_status.KHỎE_MẠNH);
        }

        [Fact]
        public async Task GetLivestockSummaryInfo_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _livestockService.GetLivestockSummaryInfo("invalid-id"));

            exception.Message.Should().Be("Không tìm thấy vật nuôi");
        }

        [Fact]
        public async Task GetLivestockVaccinationHistory_ValidId_ShouldReturnHistory()
        {
            // Act
            var result = await _livestockService.GetLivestockVaccinationHistory("test-livestock-1");

            // Assert
            result.Should().NotBeNull();
            result.LivestockId.Should().Be("test-livestock-1");
            result.InspectionCode.Should().Be("LV001");
            result.vaccineHistory.Should().NotBeNull();
            result.vaccineHistory.Should().BeEmpty(); // No vaccination history in test data
        }

        [Fact]
        public async Task GetLivestockVaccinationHistory_EmptyId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _livestockService.GetLivestockVaccinationHistory(""));

            exception.Message.Should().Contain("Livestock ID is required");
        }

        [Fact]
        public async Task GetLivestockVaccinationHistory_NullId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _livestockService.GetLivestockVaccinationHistory(null));

            exception.Message.Should().Contain("Livestock ID is required");
        }

        [Fact]
        public async Task GetLivestockVaccinationHistory_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _livestockService.GetLivestockVaccinationHistory("invalid-id"));

            exception.Message.Should().Be("Livestock not found");
        }

        [Fact]
        public async Task GetLivestockGeneralInfo_ByInspectionCodeAndType_ValidData_ShouldReturnInfo()
        {
            // Act
            var result = await _livestockService.GetLivestockGeneralInfo("LV001", specie_type.LỢN);

            // Assert
            result.Should().NotBeNull();
            result.InspectionCode.Should().Be("LV001");
            result.Id.Should().Be("test-livestock-1");
            result.Status.Should().Be(livestock_status.KHỎE_MẠNH);
        }

        [Fact]
        public async Task GetLivestockGeneralInfo_ByInspectionCodeAndType_InvalidCode_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _livestockService.GetLivestockGeneralInfo("INVALID", specie_type.LỢN));

            exception.Message.Should().Be("Livestock not found");
        }

        [Fact]
        public async Task GetLivestockGeneralInfo_ByInspectionCodeAndType_WrongSpecieType_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _livestockService.GetLivestockGeneralInfo("LV001", specie_type.BÒ));

            exception.Message.Should().Be("Livestock not found");
        }

        [Fact]
        public async Task GetLiveStockIdByInspectionCodeAndType_ValidData_ShouldReturnSummary()
        {
            // Arrange
            var model = new LivestockIdFindDTO
            {
                InspectionCode = "LV002",
                SpecieType = specie_type.BÒ
            };

            // Act
            var result = await _livestockService.GetLiveStockIdByInspectionCodeAndType(model);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-livestock-2");
            result.InspectionCode.Should().Be("LV002");
            result.Species.Should().Be("test-species-2");
        }

        [Fact]
        public async Task GetLiveStockIdByInspectionCodeAndType_InvalidData_ShouldThrowException()
        {
            // Arrange
            var model = new LivestockIdFindDTO
            {
                InspectionCode = "INVALID",
                SpecieType = specie_type.LỢN
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _livestockService.GetLiveStockIdByInspectionCodeAndType(model));

            exception.Message.Should().Be("Không tìm thấy id cho loài LỢN");
        }

        [Fact]
        public async Task ExportListNoCodeLivestockExcel_ShouldReturnExcelData()
        {
            // Act
            var result = await _livestockService.ExportListNoCodeLivestockExcel();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GenerateQRCode_ValidText_ShouldReturnQRCodeBytes()
        {
            // Arrange
            var text = "LV001";

            // Act
            var result = _livestockService.GenerateQRCode(text);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GenerateQRCode_EmptyText_ShouldReturnNull()
        {
            // Act
            var result = _livestockService.GenerateQRCode("");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GenerateQRCode_NullText_ShouldReturnNull()
        {
            // Act
            var result = _livestockService.GenerateQRCode(null);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("LV")]
        [InlineData("lv001")]
        [InlineData("001")]
        public async Task GetListLivestocks_CaseInsensitiveKeywordSearch_ShouldWork(string keyword)
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                Keyword = keyword
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetListLivestocks_WithSpecialCharactersKeyword_ShouldReturnEmptyResult()
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                Keyword = "@#$%^&*()"
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(0);
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetListLivestocks_WithMultipleFilters_ShouldReturnCorrectResults()
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                Keyword = "LV",
                MinWeight = 50m,
                MaxWeight = 60m,
                SpeciesIds = new[] { "test-species-1" },
                Statuses = new[] { livestock_status.KHỎE_MẠNH }
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.First().InspectionCode.Should().Be("LV001");
        }

        [Fact]
        public async Task GetListLivestocks_WithNoMatchingFilters_ShouldReturnEmptyResult()
        {
            // Arrange
            var filter = new ListLivestocksFilter
            {
                MinWeight = 1000m, // Very high weight that no livestock has
                MaxWeight = 2000m
            };

            // Act
            var result = await _livestockService.GetListLivestocks(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(0);
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetListLivestocks_WithNullFilter_ShouldReturnAllLivestock()
        {
            // Act
            var result = await _livestockService.GetListLivestocks(filter: null);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(2); // Only livestock with inspection codes
            result.Items.Should().HaveCount(2);
        }

       

        [Fact]
        public async Task GetLivestockGeneralInfo_LivestockWithAllFields_ShouldReturnCompleteInfo()
        {
            // Act
            var result = await _livestockService.GetLivestockGeneralInfo("test-livestock-1");

            // Assert
            result.Should().NotBeNull();
            result.InspectionCode.Should().Be("LV001");
            result.Status.Should().Be(livestock_status.KHỎE_MẠNH);
            result.Gender.Should().Be(livestock_gender.ĐỰC);
            result.Color.Should().Be("Trắng");
            result.Origin.Should().Be("Việt Nam");
            result.SpeciesId.Should().Be("test-species-1");
            result.WeightOrigin.Should().Be(50.5m);
            result.WeightEstimate.Should().Be(55.0m);
            result.BarnId.Should().Be("test-barn-1");
            result.Dob.Should().NotBeNull();
        }

        [Fact]
        public async Task GetLivestockSummaryInfo_LivestockWithNullInspectionCode_ShouldReturnNA()
        {
            // Act
            var result = await _livestockService.GetLivestockSummaryInfo("test-livestock-3");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-livestock-3");
            result.InspectionCode.Should().Be("");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}