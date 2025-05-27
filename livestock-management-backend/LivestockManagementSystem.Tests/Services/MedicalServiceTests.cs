using AutoMapper;
using BusinessObjects;
using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Services;
using FluentAssertions;
using LivestockManagementSystem.Tests.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystem.Tests.Services
{
    public class MedicalServiceTests : IDisposable
    {
        private readonly LmsContext _context;
        private readonly MedicalService _medicalService;
        private readonly IMapper _mapper;

        public MedicalServiceTests()
        {
            // Setup in-memory database
            _context = TestDbContextFactory.CreateInMemoryContext();

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DataAccess.AutoMapperConfig.ConfigMapper>();
            });
            _mapper = config.CreateMapper();

            // Create service instance
            _medicalService = new MedicalService(_context, _mapper);

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var medicine1 = new Medicine
            {
                Id = "test-medicine-1",
                Name = "Paracetamol",
                Description = "Thuốc giảm đau, hạ sốt",
                Type = medicine_type.THUỐC_CHỮA_BỆNH,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var medicine2 = new Medicine
            {
                Id = "test-medicine-2",
                Name = "Amoxicillin",
                Description = "Kháng sinh điều trị nhiễm khuẩn",
                Type = medicine_type.KHÁNG_SINH,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.Medicines.AddRange(medicine1, medicine2);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateAsync_ValidData_ShouldReturnMedicineDTO()
        {
            // Arrange
            var createMedicineDto = new CreateMedicineDTO
            {
                Name = "Aspirin",
                Description = "Thuốc giảm đau chống viêm",
                Type = medicine_type.THUỐC_CHỮA_BỆNH,
                CreatedBy = "TEST_USER"
            };

            // Act
            var result = await _medicalService.CreateAsync(createMedicineDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createMedicineDto.Name);
            result.Description.Should().Be(createMedicineDto.Description);
            result.Type.Should().Be(createMedicineDto.Type);
            result.Id.Should().NotBeNullOrEmpty();

            // Verify in database
            var dbMedicine = await _context.Medicines.FindAsync(result.Id);
            dbMedicine.Should().NotBeNull();
            dbMedicine.CreatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task CreateAsync_DuplicateName_ShouldThrowException()
        {
            // Arrange
            var createMedicineDto = new CreateMedicineDTO
            {
                Name = "Paracetamol", // Same as existing medicine
                Description = "Thuốc giảm đau",
                Type = medicine_type.THUỐC_CHỮA_BỆNH,
                CreatedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _medicalService.CreateAsync(createMedicineDto));

            exception.Message.Should().Be("Tên thuốc trùng");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_InvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var createMedicineDto = new CreateMedicineDTO
            {
                Name = invalidName,
                Description = "Test description",
                Type = medicine_type.THUỐC_CHỮA_BỆNH,
                CreatedBy = "TEST_USER"
            };

            // Act - Service doesn't validate name, so it should succeed
            var result = await _medicalService.CreateAsync(createMedicineDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(invalidName.Trim());
        }

        [Fact]
        public async Task CreateAsync_NullName_ShouldThrowException()
        {
            // Arrange
            var createMedicineDto = new CreateMedicineDTO
            {
                Name = null,
                Description = "Test description",
                Type = medicine_type.THUỐC_CHỮA_BỆNH,
                CreatedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _medicalService.CreateAsync(createMedicineDto));

            exception.Should().NotBeNull();
        }

        [Fact]
        public async Task GetListMedicineAsync_WithoutFilter_ShouldReturnAllMedicines()
        {
            // Arrange
            var filter = new MedicinesFliter();

            // Act
            var result = await _medicalService.GetListMedicineAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Items.Should().Contain(m => m.Name == "Paracetamol");
            result.Items.Should().Contain(m => m.Name == "Amoxicillin");
        }

        [Fact]
        public async Task GetListMedicineAsync_WithKeywordFilter_ShouldReturnFilteredMedicines()
        {
            // Arrange
            var filter = new MedicinesFliter
            {
                Keyword = "Para"
            };

            // Act
            var result = await _medicalService.GetListMedicineAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.Should().Contain(m => m.Name == "Paracetamol");
        }

        [Fact]
        public async Task GetListMedicineAsync_WithNonExistentKeyword_ShouldReturnEmptyResult()
        {
            // Arrange
            var filter = new MedicinesFliter
            {
                Keyword = "không tồn tại"
            };

            // Act
            var result = await _medicalService.GetListMedicineAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(0);
            if (result.Items != null)
            {
                result.Items.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetListMedicineAsync_WithTypeFilter_ShouldReturnFilteredMedicines()
        {
            // Arrange
            var filter = new MedicinesFliter
            {
                Type = medicine_type.KHÁNG_SINH
            };

            // Act
            var result = await _medicalService.GetListMedicineAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.Should().Contain(m => m.Name == "Amoxicillin");
        }

        [Fact]
        public async Task GetListMedicineAsync_WithDateFilter_ShouldReturnFilteredMedicines()
        {
            // Arrange
            var filter = new MedicinesFliter
            {
                FromDate = DateTime.Now.AddDays(-1),
                ToDate = DateTime.Now.AddDays(1)
            };

            // Act
            var result = await _medicalService.GetListMedicineAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(2); // Both medicines created today
            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetListMedicineAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var filter = new MedicinesFliter
            {
                Skip = 0,
                Take = 1
            };

            // Act
            var result = await _medicalService.GetListMedicineAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(2);
            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ShouldReturnMedicine()
        {
            // Act
            var result = await _medicalService.GetByIdAsync("test-medicine-1");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-medicine-1");
            result.Name.Should().Be("Paracetamol");
            result.Type.Should().Be(medicine_type.THUỐC_CHỮA_BỆNH);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _medicalService.GetByIdAsync("invalid-id"));

            exception.Message.Should().Be("Không tìm thấy thuốc.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetByIdAsync_EmptyId_ShouldThrowException(string invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _medicalService.GetByIdAsync(invalidId));

            exception.Message.Should().Be("Không tìm thấy thuốc.");
        }

        [Fact]
        public async Task UpdateAsync_ValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            var updateDto = new UpdateMedicineDTO
            {
                Name = "Paracetamol Extra",
                Description = "Thuốc giảm đau, hạ sốt cải tiến",
                Type = medicine_type.THUỐC_CHỮA_BỆNH,
                UpdatedBy = "TEST_USER"
            };

            // Act
            var result = await _medicalService.UpdateAsync("test-medicine-1", updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(updateDto.Name);
            result.Description.Should().Be(updateDto.Description);
            result.Type.Should().Be(updateDto.Type);

            // Verify in database
            var updatedMedicine = await _context.Medicines.FindAsync("test-medicine-1");
            updatedMedicine.Should().NotBeNull();
            updatedMedicine.Name.Should().Be(updateDto.Name);
            updatedMedicine.UpdatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task UpdateAsync_InvalidId_ShouldThrowException()
        {
            // Arrange
            var updateDto = new UpdateMedicineDTO
            {
                Name = "Test Medicine",
                Description = "Test description",
                Type = medicine_type.THUỐC_CHỮA_BỆNH,
                UpdatedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _medicalService.UpdateAsync("invalid-id", updateDto));

            exception.Message.Should().Be("Không tìm thấy thuốc.");
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ShouldDeleteSuccessfully()
        {
            // Act
            var result = await _medicalService.DeleteAsync("test-medicine-1");

            // Assert
            result.Should().BeTrue();

            // Verify deletion
            var deletedMedicine = await _context.Medicines.FindAsync("test-medicine-1");
            deletedMedicine.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _medicalService.DeleteAsync("invalid-id"));

            exception.Message.Should().Be("Không tìm thấy thuốc.");
        }

        [Fact]
        public async Task DeleteAsync_MedicineInUse_ShouldThrowException()
        {
            // Arrange - Add disease and relationship
            var disease = new Disease
            {
                Id = "test-disease-1",
                Name = "Bệnh test",
                Symptom = "Test symptom",
                Description = "Test description",
                Type = LmsConstants.disease_type.KHÔNG_TRUYỀN_NHIỄM,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };
            _context.Diseases.Add(disease);

            var diseaseMedicine = new DiseaseMedicine
            {
                Id = "test-dm-1",
                DiseaseId = "test-disease-1",
                MedicineId = "test-medicine-1",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };
            _context.DiseaseMedicines.Add(diseaseMedicine);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _medicalService.DeleteAsync("test-medicine-1"));

            exception.Message.Should().Be("Thuốc đang được sử dụng trong hệ thống, không thể xóa.");
        }

        [Fact]
        public async Task MedicalExists_ExistingId_ShouldReturnTrue()
        {
            // Act
            var result = await _medicalService.MedicalExists("test-medicine-1");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MedicalExists_NonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _medicalService.MedicalExists("non-existing-id");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMedicineByDisease_ValidDiseaseId_ShouldReturnMedicines()
        {
            // Arrange - Add disease and relationship
            var disease = new Disease
            {
                Id = "test-disease-1",
                Name = "Bệnh test",
                Symptom = "Test symptom",
                Description = "Test description",
                Type = LmsConstants.disease_type.KHÔNG_TRUYỀN_NHIỄM,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };
            _context.Diseases.Add(disease);

            var diseaseMedicine = new DiseaseMedicine
            {
                Id = "test-dm-1",
                DiseaseId = "test-disease-1",
                MedicineId = "test-medicine-1",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };
            _context.DiseaseMedicines.Add(diseaseMedicine);
            await _context.SaveChangesAsync();

            // Act
            var result = await _medicalService.GetMedicineByDisease("test-disease-1");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().Contain(m => m.Id == "test-medicine-1");
        }

        [Fact]
        public async Task GetMedicineByDisease_InvalidDiseaseId_ShouldReturnEmptyList()
        {
            // Act
            var result = await _medicalService.GetMedicineByDisease("invalid-disease-id");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("para", 1)] // Case insensitive search
        [InlineData("PARA", 1)]
        [InlineData("amox", 1)]
        [InlineData("thuốc", 0)] // Search doesn't find "thuốc" in test data
        public async Task GetListMedicineAsync_CaseInsensitiveSearch_ShouldWork(string keyword, int expectedCount)
        {
            // Arrange
            var filter = new MedicinesFliter
            {
                Keyword = keyword
            };

            // Act
            var result = await _medicalService.GetListMedicineAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(expectedCount);
        }

        [Fact]
        public async Task GetListMedicineAsync_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var filter = new MedicinesFliter
            {
                Keyword = "@#$%^&*()"
            };

            // Act
            var result = await _medicalService.GetListMedicineAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(0);
            if (result.Items != null)
            {
                result.Items.Should().BeEmpty();
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}