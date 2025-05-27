using AutoMapper;
using BusinessObjects;
using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Services;
using FluentAssertions;
using LivestockManagementSystem.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystem.Tests.Services
{
    public class SpecieServiceTests : IDisposable
    {
        private readonly LmsContext _context;
        private readonly SpecieService _specieService;
        private readonly IMapper _mapper;

        public SpecieServiceTests()
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
            _specieService = new SpecieService(_context, _mapper);

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var species1 = new Species
            {
                Id = "test-species-1",
                Name = "Lợn Yorkshire",
                Description = "Giống lợn thịt chất lượng cao",
                Type = specie_type.LỢN,
                GrowthRate = 0.8m,
                DressingPercentage = 75.5m,
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
                GrowthRate = 0.6m,
                DressingPercentage = 65.0m,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var species3 = new Species
            {
                Id = "test-species-3",
                Name = "Gà Đông Tảo",
                Description = "Giống gà đặc sản Việt Nam",
                Type = specie_type.GÀ,
                GrowthRate = 1.2m,
                DressingPercentage = 70.0m,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.Species.AddRange(species1, species2, species3);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSpecies()
        {
            // Act
            var result = await _specieService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(s => s.Name == "Lợn Yorkshire");
            result.Should().Contain(s => s.Name == "Bò Wagyu");
            result.Should().Contain(s => s.Name == "Gà Đông Tảo");
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ShouldReturnSpecies()
        {
            // Act
            var result = await _specieService.GetByIdAsync("test-species-1");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-species-1");
            result.Name.Should().Be("Lợn Yorkshire");
            result.Description.Should().Be("Giống lợn thịt chất lượng cao");
            result.Type.Should().Be(specie_type.LỢN);
            result.GrowthRate.Should().Be(0.8m);
            result.DressingPercentage.Should().Be(75.5m);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.GetByIdAsync("invalid-id"));

            exception.Message.Should().Be("Không có loài vật này trong hệ thống");
        }

        [Fact]
        public async Task GetByIdAsync_EmptyId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.GetByIdAsync(""));

            exception.Message.Should().Be("Không có loài vật này trong hệ thống");
        }

        [Fact]
        public async Task GetByIdAsync_NullId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.GetByIdAsync(null));

            exception.Message.Should().Be("Không có loài vật này trong hệ thống");
        }

        [Fact]
        public async Task CreateAsync_ValidData_ShouldCreateSpecies()
        {
            // Arrange
            var createDto = new SpecieCreate
            {
                Name = "Lợn Landrace",
                Description = "Giống lợn nái tốt",
                Type = specie_type.LỢN,
                GrowthRate = 0.7m,
                DressingPercentage = 72.0m,
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _specieService.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createDto.Name);
            result.Description.Should().Be(createDto.Description);
            result.Type.Should().Be(createDto.Type);
            result.GrowthRate.Should().Be(createDto.GrowthRate);
            result.DressingPercentage.Should().Be(createDto.DressingPercentage);
            result.Id.Should().NotBeNullOrEmpty();

            // Verify in database
            var createdSpecies = await _context.Species.FindAsync(result.Id);
            createdSpecies.Should().NotBeNull();
            createdSpecies.CreatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task CreateAsync_DuplicateName_ShouldThrowException()
        {
            // Arrange
            var createDto = new SpecieCreate
            {
                Name = "Lợn Yorkshire", // Duplicate name
                Description = "Giống lợn khác",
                Type = specie_type.LỢN,
                GrowthRate = 0.9m,
                DressingPercentage = 80.0m,
                RequestedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.CreateAsync(createDto));

            exception.Message.Should().Be("Tên loài vật này đã có trong hệ thống");
        }

        [Fact]
        public async Task CreateAsync_SameNameDifferentType_ShouldSucceed()
        {
            // Arrange
            var createDto = new SpecieCreate
            {
                Name = "Yorkshire", // Same name but different type
                Description = "Giống bò Yorkshire",
                Type = specie_type.BÒ,
                GrowthRate = 0.5m,
                DressingPercentage = 60.0m,
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _specieService.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createDto.Name);
            result.Type.Should().Be(specie_type.BÒ);
        }

        [Fact]
        public async Task CreateAsync_WithNullRequestedBy_ShouldUseDefaultValue()
        {
            // Arrange
            var createDto = new SpecieCreate
            {
                Name = "Lợn Duroc",
                Description = "Giống lợn thịt",
                Type = specie_type.LỢN,
                GrowthRate = 0.75m,
                DressingPercentage = 73.0m,
                RequestedBy = null
            };

            // Act
            var result = await _specieService.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();

            // Verify in database
            var createdSpecies = await _context.Species.FindAsync(result.Id);
            createdSpecies.Should().NotBeNull();
            createdSpecies.CreatedBy.Should().Be("SYS");
        }

        [Fact]
        public async Task UpdateAsync_ValidData_ShouldUpdateSpecies()
        {
            // Arrange
            var updateDto = new SpecieUpdate
            {
                Name = "Lợn Yorkshire Cải tiến",
                Description = "Giống lợn thịt chất lượng cao được cải tiến",
                Type = specie_type.LỢN,
                GrowthRate = 0.85m,
                DressingPercentage = 78.0m,
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _specieService.UpdateAsync("test-species-1", updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(updateDto.Name);
            result.Description.Should().Be(updateDto.Description);
            result.GrowthRate.Should().Be(updateDto.GrowthRate);
            result.DressingPercentage.Should().Be(updateDto.DressingPercentage);

            // Verify in database
            var updatedSpecies = await _context.Species.FindAsync("test-species-1");
            updatedSpecies.Should().NotBeNull();
            updatedSpecies.UpdatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task UpdateAsync_InvalidId_ShouldThrowException()
        {
            // Arrange
            var updateDto = new SpecieUpdate
            {
                Name = "Test Species",
                Description = "Test Description",
                Type = specie_type.LỢN,
                GrowthRate = 0.5m,
                DressingPercentage = 70.0m,
                RequestedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.UpdateAsync("invalid-id", updateDto));

            exception.Message.Should().Be("Không có loài vật này trong hệ thống");
        }

        [Fact]
        public async Task UpdateAsync_DuplicateName_ShouldThrowException()
        {
            // Arrange
            var updateDto = new SpecieUpdate
            {
                Name = "Bò Wagyu", // Name already exists for another species
                Description = "Updated description",
                Type = specie_type.BÒ,
                GrowthRate = 0.9m,
                DressingPercentage = 80.0m,
                RequestedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.UpdateAsync("test-species-1", updateDto));

            exception.Message.Should().Be("Tên loài vật đã được sử dụng");
        }

        [Fact]
        public async Task UpdateAsync_SameNameSameSpecies_ShouldSucceed()
        {
            // Arrange
            var updateDto = new SpecieUpdate
            {
                Name = "Lợn Yorkshire", // Same name as current species
                Description = "Updated description",
                Type = specie_type.LỢN,
                GrowthRate = 0.9m,
                DressingPercentage = 80.0m,
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _specieService.UpdateAsync("test-species-1", updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ShouldDeleteSpecies()
        {
            // Act
            var result = await _specieService.DeleteAsync("test-species-3");

            // Assert
            result.Should().BeTrue();

            // Verify deletion
            var deletedSpecies = await _context.Species.FindAsync("test-species-3");
            deletedSpecies.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.DeleteAsync("invalid-id"));

            exception.Message.Should().Be("Không có loài vật này trong hệ thống");
        }

        [Fact]
        public async Task DeleteAsync_SpeciesInUse_ShouldThrowException()
        {
            // Arrange - Add livestock using this species
            var livestock = new Livestock
            {
                Id = "test-livestock-1",
                InspectionCode = "LV001",
                SpeciesId = "test-species-1",
                BarnId = "test-barn-1",
                Status = livestock_status.KHỎE_MẠNH,
                Gender = livestock_gender.ĐỰC,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };
            _context.Livestocks.Add(livestock);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.DeleteAsync("test-species-1"));

            exception.Message.Should().Be("Loài vật này đang được sử dụng trong hệ thống, không thể xóa.");
        }

        [Fact]
        public async Task GetListCanDeleteSpecies_ShouldReturnOnlyDeletableSpecies()
        {
            // Arrange - Add livestock using one species
            var livestock = new Livestock
            {
                Id = "test-livestock-1",
                InspectionCode = "LV001",
                SpeciesId = "test-species-1",
                BarnId = "test-barn-1",
                Status = livestock_status.KHỎE_MẠNH,
                Gender = livestock_gender.ĐỰC,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };
            _context.Livestocks.Add(livestock);
            await _context.SaveChangesAsync();

            // Act
            var result = await _specieService.GetListCanDeleteSpecies();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only species 2 and 3 can be deleted
            result.Should().NotContain(s => s.Id == "test-species-1");
            result.Should().Contain(s => s.Id == "test-species-2");
            result.Should().Contain(s => s.Id == "test-species-3");
        }

        [Fact]
        public async Task GetListSpecieNameByType_ValidType_ShouldReturnSpeciesOfType()
        {
            // Act
            var result = await _specieService.GetListSpecieNameByType(specie_type.LỢN);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Id.Should().Be("test-species-1");
            result.First().Name.Should().Be("Lợn Yorkshire");
        }

        [Fact]
        public async Task GetListSpecieNameByType_NoSpeciesOfType_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _specieService.GetListSpecieNameByType(specie_type.TRÂU));

            exception.Message.Should().Be("Không tồn tại loài vật này");
        }

        [Theory]
        [InlineData(specie_type.BÒ, 1)]
        [InlineData(specie_type.GÀ, 1)]
        [InlineData(specie_type.LỢN, 1)]
        public async Task GetListSpecieNameByType_DifferentTypes_ShouldReturnCorrectCount(specie_type type, int expectedCount)
        {
            // Act
            var result = await _specieService.GetListSpecieNameByType(type);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
            result.All(s => s.Id != null && s.Name != null).Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_WithSpecialCharacters_ShouldSucceed()
        {
            // Arrange
            var createDto = new SpecieCreate
            {
                Name = "Lợn Móng Cái (F1)",
                Description = "Giống lợn lai F1 - chất lượng cao",
                Type = specie_type.LỢN,
                GrowthRate = 0.8m,
                DressingPercentage = 75.0m,
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _specieService.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createDto.Name);
        }

        [Fact]
        public async Task UpdateAsync_WithNullRequestedBy_ShouldUseDefaultValue()
        {
            // Arrange
            var updateDto = new SpecieUpdate
            {
                Name = "Updated Species",
                Description = "Updated Description",
                Type = specie_type.LỢN,
                GrowthRate = 0.9m,
                DressingPercentage = 80.0m,
                RequestedBy = null
            };

            // Act
            var result = await _specieService.UpdateAsync("test-species-1", updateDto);

            // Assert
            result.Should().NotBeNull();

            // Verify in database
            var updatedSpecies = await _context.Species.FindAsync("test-species-1");
            updatedSpecies.Should().NotBeNull();
            updatedSpecies.UpdatedBy.Should().Be("SYS");
        }

        [Theory]
        [InlineData("  test-species-1  ")]
        [InlineData("test-species-1   ")]
        [InlineData("   test-species-1")]
        public async Task GetByIdAsync_WithWhitespace_ShouldTrimAndWork(string idWithWhitespace)
        {
            // Act
            var result = await _specieService.GetByIdAsync(idWithWhitespace);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-species-1");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}