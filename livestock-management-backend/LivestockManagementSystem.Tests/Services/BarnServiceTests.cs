using BusinessObjects;
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
    public class BarnServiceTests : IDisposable
    {
        private readonly LmsContext _context;
        private readonly BarnService _barnService;

        public BarnServiceTests()
        {
            // Setup in-memory database
            _context = TestDbContextFactory.CreateInMemoryContext();

            // Create service instance
            _barnService = new BarnService(_context);

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var barn1 = new Barn
            {
                Id = "test-barn-1",
                Name = "Chuồng số 1",
                Address = "123 Đường Test, Quận 1, TP.HCM",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var barn2 = new Barn
            {
                Id = "test-barn-2",
                Name = "Chuồng số 2",
                Address = "456 Đường Test, Quận 2, TP.HCM",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var barn3 = new Barn
            {
                Id = "test-barn-3",
                Name = "Chuồng số 3",
                Address = "789 Đường Test, Quận 3, TP.HCM",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            _context.Barns.AddRange(barn1, barn2, barn3);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetBarnById_ValidId_ShouldReturnBarnInfo()
        {
            // Act
            var result = await _barnService.GetBarnById("test-barn-1");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-barn-1");
            result.Name.Should().Be("Chuồng số 1");
            result.Address.Should().Be("123 Đường Test, Quận 1, TP.HCM");
        }

        [Fact]
        public async Task GetBarnById_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.GetBarnById("invalid-id"));

            exception.Message.Should().Be("Không có trại này trong hệ thống");
        }

        [Fact]
        public async Task GetBarnById_NullId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.GetBarnById(null));

            exception.Message.Should().Be("Không có trại này trong hệ thống");
        }

        [Fact]
        public async Task GetBarnById_EmptyId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.GetBarnById(""));

            exception.Message.Should().Be("Không có trại này trong hệ thống");
        }

        [Fact]
        public async Task GetListBarns_ShouldReturnAllBarns()
        {
            // Act
            var result = await _barnService.GetListBarns();

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(3);
            result.Items.Should().HaveCount(3);
            result.Items.Should().Contain(b => b.Name == "Chuồng số 1");
            result.Items.Should().Contain(b => b.Name == "Chuồng số 2");
            result.Items.Should().Contain(b => b.Name == "Chuồng số 3");
        }

        [Fact]
        public async Task GetListBarns_EmptyDatabase_ShouldReturnEmptyResult()
        {
            // Arrange - Clear all barns
            _context.Barns.RemoveRange(_context.Barns);
            await _context.SaveChangesAsync();

            // Act
            var result = await _barnService.GetListBarns();

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(0);
            result.Items.Should().BeNull();
        }

        [Fact]
        public async Task CreateBarn_ValidData_ShouldCreateBarn()
        {
            // Arrange
            var createDto = new CreateBarnDTO
            {
                Name = "Chuồng mới",
                Address = "999 Đường Mới, Quận 9, TP.HCM",
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _barnService.CreateBarn(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Chuồng mới");
            result.Address.Should().Be("999 Đường Mới, Quận 9, TP.HCM");
            result.Id.Should().NotBeNullOrEmpty();

            // Verify in database
            var createdBarn = await _context.Barns.FindAsync(result.Id);
            createdBarn.Should().NotBeNull();
            createdBarn.CreatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task CreateBarn_DuplicateName_ShouldThrowException()
        {
            // Arrange
            var createDto = new CreateBarnDTO
            {
                Name = "Chuồng số 1", // Duplicate name
                Address = "Địa chỉ khác",
                RequestedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.CreateBarn(createDto));

            exception.Message.Should().Be("Trang trại này đã tồn tại trong hệ thống");
        }

        [Fact]
        public async Task CreateBarn_CaseInsensitiveDuplicateName_ShouldThrowException()
        {
            // Arrange
            var createDto = new CreateBarnDTO
            {
                Name = "CHUỒNG SỐ 1", // Same name but different case
                Address = "Địa chỉ khác",
                RequestedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.CreateBarn(createDto));

            exception.Message.Should().Be("Trang trại này đã tồn tại trong hệ thống");
        }

        [Fact]
        public async Task CreateBarn_WithWhitespace_ShouldTrimAndCreate()
        {
            // Arrange
            var createDto = new CreateBarnDTO
            {
                Name = "  Chuồng có khoảng trắng  ",
                Address = "  Địa chỉ có khoảng trắng  ",
                RequestedBy = "  TEST_USER  "
            };

            // Act
            var result = await _barnService.CreateBarn(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Chuồng có khoảng trắng");
            result.Address.Should().Be("Địa chỉ có khoảng trắng");

            // Verify in database
            var createdBarn = await _context.Barns.FindAsync(result.Id);
            createdBarn.Should().NotBeNull();
            createdBarn.CreatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task CreateBarn_WithNullRequestedBy_ShouldUseDefaultValue()
        {
            // Arrange
            var createDto = new CreateBarnDTO
            {
                Name = "Chuồng null user",
                Address = "Địa chỉ test",
                RequestedBy = null
            };

            // Act
            var result = await _barnService.CreateBarn(createDto);

            // Assert
            result.Should().NotBeNull();

            // Verify in database
            var createdBarn = await _context.Barns.FindAsync(result.Id);
            createdBarn.Should().NotBeNull();
            createdBarn.CreatedBy.Should().Be("SYS");
        }

        [Fact]
        public async Task UpdateBarn_ValidData_ShouldUpdateBarn()
        {
            // Arrange
            var updateDto = new UpdateBarnDTO
            {
                Name = "Chuồng số 1 đã cập nhật",
                Address = "123 Đường Test Cập nhật, Quận 1, TP.HCM",
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _barnService.UpdateBarn("test-barn-1", updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-barn-1");
            result.Name.Should().Be("Chuồng số 1 đã cập nhật");
            result.Address.Should().Be("123 Đường Test Cập nhật, Quận 1, TP.HCM");

            // Verify in database
            var updatedBarn = await _context.Barns.FindAsync("test-barn-1");
            updatedBarn.Should().NotBeNull();
            updatedBarn.UpdatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task UpdateBarn_InvalidId_ShouldThrowException()
        {
            // Arrange
            var updateDto = new UpdateBarnDTO
            {
                Name = "Test Barn",
                Address = "Test Address",
                RequestedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.UpdateBarn("invalid-id", updateDto));

            exception.Message.Should().Be("Trang trại này không tồn tại trong hệ thống");
        }

        [Fact]
        public async Task UpdateBarn_WithWhitespace_ShouldTrimAndUpdate()
        {
            // Arrange
            var updateDto = new UpdateBarnDTO
            {
                Name = "  Tên mới có khoảng trắng  ",
                Address = "  Địa chỉ mới có khoảng trắng  ",
                RequestedBy = "  TEST_USER  "
            };

            // Act
            var result = await _barnService.UpdateBarn("test-barn-1", updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Tên mới có khoảng trắng");
            result.Address.Should().Be("Địa chỉ mới có khoảng trắng");

            // Verify in database
            var updatedBarn = await _context.Barns.FindAsync("test-barn-1");
            updatedBarn.Should().NotBeNull();
            updatedBarn.UpdatedBy.Should().Be("TEST_USER");
        }

        [Fact]
        public async Task UpdateBarn_WithNullRequestedBy_ShouldUseDefaultValue()
        {
            // Arrange
            var updateDto = new UpdateBarnDTO
            {
                Name = "Tên mới",
                Address = "Địa chỉ mới",
                RequestedBy = null
            };

            // Act
            var result = await _barnService.UpdateBarn("test-barn-1", updateDto);

            // Assert
            result.Should().NotBeNull();

            // Verify in database
            var updatedBarn = await _context.Barns.FindAsync("test-barn-1");
            updatedBarn.Should().NotBeNull();
            updatedBarn.UpdatedBy.Should().Be("SYS");
        }

        [Fact]
        public async Task DeleteBarn_ValidId_ShouldDeleteBarn()
        {
            // Act
            var result = await _barnService.DeleteBarn("test-barn-3");

            // Assert
            result.Should().BeTrue();

            // Verify deletion
            var deletedBarn = await _context.Barns.FindAsync("test-barn-3");
            deletedBarn.Should().BeNull();
        }

        [Fact]
        public async Task DeleteBarn_InvalidId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.DeleteBarn("invalid-id"));

            exception.Message.Should().Be("Trang trại này không tồn tại trong hệ thống");
        }

        [Fact]
        public async Task DeleteBarn_BarnInUseByLivestock_ShouldThrowException()
        {
            // Arrange - Add livestock using this barn
            var livestock = new Livestock
            {
                Id = "test-livestock-1",
                InspectionCode = "LV001",
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
                _barnService.DeleteBarn("test-barn-1"));

            exception.Message.Should().Be("Trang trại đang được sử dụng trong hệ thống, không thể xóa.");
        }

        [Fact]
        public async Task DeleteBarn_BarnInUseByBatchImport_ShouldThrowException()
        {
            // Arrange - Add batch import using this barn
            var batchImport = new BatchImport
            {
                Id = "test-batch-import-1",
                Name = "Test Batch Import",
                OriginLocation = "Test Origin",
                BarnId = "test-barn-1",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };
            _context.BatchImports.Add(batchImport);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.DeleteBarn("test-barn-1"));

            exception.Message.Should().Be("Trang trại đang được sử dụng trong hệ thống, không thể xóa.");
        }

        [Fact]
        public async Task DeleteBarn_BarnInUseByBatchExport_ShouldThrowException()
        {
            // Arrange - Add batch export using this barn
            var batchExport = new BatchExport
            {
                Id = "test-batch-export-1",
                CustomerName = "Test Customer",
                ProcurementPackageId = "test-package-1",
                BarnId = "test-barn-1",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };
            _context.BatchExports.Add(batchExport);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _barnService.DeleteBarn("test-barn-1"));

            exception.Message.Should().Be("Trang trại đang được sử dụng trong hệ thống, không thể xóa.");
        }

        [Fact]
        public async Task CreateBarn_WithSpecialCharacters_ShouldSucceed()
        {
            // Arrange
            var createDto = new CreateBarnDTO
            {
                Name = "Chuồng A-1 (Khu vực B)",
                Address = "123/45 Đường Nguyễn Văn A, P.1, Q.Tân Bình, TP.HCM",
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _barnService.CreateBarn(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createDto.Name);
            result.Address.Should().Be(createDto.Address);
        }

        [Fact]
        public async Task GetListBarns_VerifyAllFields_ShouldReturnCompleteInfo()
        {
            // Act
            var result = await _barnService.GetListBarns();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().OnlyContain(b =>
                !string.IsNullOrEmpty(b.Id) &&
                !string.IsNullOrEmpty(b.Name) &&
                !string.IsNullOrEmpty(b.Address));
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}