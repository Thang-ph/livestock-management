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
    public class DiseaseServiceTests : IDisposable
    {
        private readonly LmsContext _context;
        private readonly DiseaseService _diseaseService;
        private readonly IMapper _mapper;

        public DiseaseServiceTests()
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
            _diseaseService = new DiseaseService(_context, _mapper);

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var disease1 = new Disease
            {
                Id = "test-disease-1",
                Name = "Bệnh cúm gia cầm",
                Symptom = "Sốt cao, khó thở",
                Description = "Bệnh truyền nhiễm nguy hiểm",
                Type = LmsConstants.disease_type.TRUYỀN_NHIỄM_NGUY_HIỂM,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var disease2 = new Disease
            {
                Id = "test-disease-2",
                Name = "Bệnh tiêu chảy",
                Symptom = "Tiêu chảy, mất nước",
                Description = "Bệnh thường gặp ở gia súc",
                Type = LmsConstants.disease_type.KHÔNG_TRUYỀN_NHIỄM,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.Diseases.AddRange(disease1, disease2);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetListDiseases_WithoutKeyword_ShouldReturnAllDiseases()
        {
            // Act
            var result = await _diseaseService.GetListDiseases(string.Empty);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Items.Should().Contain(d => d.Name == "Bệnh cúm gia cầm");
            result.Items.Should().Contain(d => d.Name == "Bệnh tiêu chảy");
        }

        [Fact]
        public async Task GetListDiseases_WithKeyword_ShouldReturnFilteredDiseases()
        {
            // Act
            var result = await _diseaseService.GetListDiseases("cúm");

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.Should().Contain(d => d.Name == "Bệnh cúm gia cầm");
        }

        [Fact]
        public async Task GetListDiseases_WithNonExistentKeyword_ShouldReturnEmptyResult()
        {
            // Act
            var result = await _diseaseService.GetListDiseases("không tồn tại");

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(0);
            if (result.Items != null)
            {
                result.Items.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task UpdateDisease_ValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            var updateDto = new DiseaseUpdateDTO
            {
                Name = "Bệnh cúm gia cầm cập nhật",
                Symptom = "Sốt cao, khó thở, mệt mỏi",
                Description = "Bệnh truyền nhiễm rất nguy hiểm",
                Type = LmsConstants.disease_type.TRUYỀN_NHIỄM_NGUY_HIỂM,
                requestedBy = "TEST_USER"
            };

            // Act
            var result = await _diseaseService.UpdateDisease("test-disease-1", updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(updateDto.Name);
            result.Symptom.Should().Be(updateDto.Symptom);
            result.Description.Should().Be(updateDto.Description);
            result.Type.Should().Be(updateDto.Type);

            // Verify in database
            var updatedDisease = await _context.Diseases.FindAsync("test-disease-1");
            updatedDisease.Should().NotBeNull();
            updatedDisease.Name.Should().Be(updateDto.Name);
            updatedDisease.UpdatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task UpdateDisease_InvalidId_ShouldThrowException()
        {
            // Arrange
            var updateDto = new DiseaseUpdateDTO
            {
                Name = "Bệnh test",
                Symptom = "Test symptom",
                Description = "Test description",
                Type = LmsConstants.disease_type.KHÔNG_TRUYỀN_NHIỄM,
                requestedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _diseaseService.UpdateDisease("invalid-id", updateDto));

            exception.Message.Should().Be("Không tìm thấy bệnh");
        }

        [Fact]
        public async Task DeleteDisease_ValidId_ShouldDeleteSuccessfully()
        {
            // Act
            var result = await _diseaseService.DeleteDisease("test-disease-1");

            // Assert
            result.Should().BeTrue();

            // Verify deletion
            var deletedDisease = await _context.Diseases.FindAsync("test-disease-1");
            deletedDisease.Should().BeNull();
        }

        [Fact]
        public async Task DeleteDisease_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _diseaseService.DeleteDisease("invalid-id"));

            exception.Message.Should().Be("Không tìm thấy bệnh");
        }

        [Fact]
        public async Task DeleteDisease_DiseaseInUse_ShouldThrowException()
        {
            // Arrange - Add a disease medicine relationship
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
                _diseaseService.DeleteDisease("test-disease-1"));

            exception.Message.Should().Be("Không thể xóa vì bệnh này đang được sử dụng trong hệ thống.");
        }



        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}