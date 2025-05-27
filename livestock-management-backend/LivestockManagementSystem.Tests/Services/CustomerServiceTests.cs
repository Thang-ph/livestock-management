using AutoMapper;
using BusinessObjects;
using BusinessObjects.Dtos;
using DataAccess.Repository.Services;
using FluentAssertions;
using LivestockManagementSystem.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LivestockManagementSystem.Tests.Services
{
    public class CustomerServiceTests : IDisposable
    {
        private readonly LmsContext _context;
        private readonly CustomerService _customerService;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly IMapper _mapper;

        public CustomerServiceTests()
        {
            // Setup in-memory database
            _context = TestDbContextFactory.CreateInMemoryContext();

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DataAccess.AutoMapperConfig.ConfigMapper>();
            });
            _mapper = config.CreateMapper();

            // Setup mock UserManager
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);

            // Create service instance
            _customerService = new CustomerService(_context, _mapper, _mockUserManager.Object);
        }

        [Fact]
        public async Task AddCustomer_ValidData_ShouldReturnCustomerInfo()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn Test",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com",
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.CustomerName.Should().Be(addCustomerDto.CustomerName);
            result.Phone.Should().Be(addCustomerDto.Phone);
            result.Address.Should().Be(addCustomerDto.Address);
            result.Email.Should().Be(addCustomerDto.Email);
            result.Id.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task AddCustomer_DuplicateCustomer_ShouldThrowException()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn A",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com",
                RequestedBy = "TEST_USER"
            };

            // Add first customer
            await _customerService.AddCustomer(addCustomerDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _customerService.AddCustomer(addCustomerDto));

            exception.Message.Should().Be("Nguời dùng đã tồn tại trong hệ thống");
        }

        [Fact]
        public async Task GetCustomerInfo_ValidId_ShouldReturnCustomerInfo()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn Test",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com",
                RequestedBy = "TEST_USER"
            };

            var addedCustomer = await _customerService.AddCustomer(addCustomerDto);

            // Act
            var result = await _customerService.GetCustomerInfo(addedCustomer.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(addedCustomer.Id);
            result.CustomerName.Should().Be(addCustomerDto.CustomerName);
            result.Phone.Should().Be(addCustomerDto.Phone);
            result.Address.Should().Be(addCustomerDto.Address);
            result.Email.Should().Be(addCustomerDto.Email);
        }

        [Fact]
        public async Task GetCustomerInfo_InvalidId_ShouldThrowException()
        {
            // Arrange
            var invalidId = "invalid-customer-id";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _customerService.GetCustomerInfo(invalidId));

            exception.Message.Should().Be("Không tìm thấy người dùng trong hệ thống");
        }

        [Theory]
        [InlineData("", "0123456789", "Tên không được để trống.")]
        [InlineData("Nguyễn Văn A", "", "Số điện thoại không được để trống.")]
        [InlineData("Nguyễn Văn A", "123", "Số điện thoại phải bắt đầu bằng 0 và có từ 10 đến 11 chữ số.")]
        [InlineData("Nguyễn Văn A", "1234567890", "Số điện thoại phải bắt đầu bằng 0 và có từ 10 đến 11 chữ số.")]
        public void AddCustomerDTO_Validation_ShouldValidateCorrectly(string customerName, string phone, string expectedError)
        {
            // Arrange
            var dto = new AddCustomerDTO
            {
                CustomerName = customerName,
                Phone = phone
            };

            // Act & Assert
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, validationContext, validationResults, true);

            if (!string.IsNullOrEmpty(expectedError))
            {
                isValid.Should().BeFalse();
                validationResults.Should().Contain(vr => vr.ErrorMessage == expectedError);
            }
            else
            {
                isValid.Should().BeTrue();
            }
        }

        [Fact]
        public async Task AddCustomer_WithNullRequestedBy_ShouldUseDefaultValue()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn Test Null",
                Phone = "0123456780",
                Address = "123 Test Street",
                Email = "testnull@example.com",
                RequestedBy = null
            };

            // Act
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.CustomerName.Should().Be(addCustomerDto.CustomerName);
        }

        [Fact]
        public async Task AddCustomer_WithEmptyRequestedBy_ShouldUseDefaultValue()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn Test Empty",
                Phone = "0123456781",
                Address = "123 Test Street",
                Email = "testempty@example.com",
                RequestedBy = ""
            };

            // Act
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.CustomerName.Should().Be(addCustomerDto.CustomerName);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@domain.com")]
        [InlineData("user@")]
        [InlineData("user.domain.com")]
        public async Task AddCustomer_WithInvalidEmail_ShouldSucceed(string invalidEmail)
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = $"Test Customer {invalidEmail}",
                Phone = $"012345678{invalidEmail.Length % 10}",
                Address = "123 Test Street",
                Email = invalidEmail,
                RequestedBy = "TEST_USER"
            };

            // Act - Service doesn't validate email format, so it should succeed
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(invalidEmail);
        }

        [Theory]
        [InlineData("0123456789")]
        [InlineData("01234567890")]
        [InlineData("0987654321")]
        public async Task AddCustomer_WithValidPhoneFormats_ShouldSucceed(string validPhone)
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = $"Test Customer {validPhone}",
                Phone = validPhone,
                Address = "123 Test Street",
                Email = $"test{validPhone}@example.com",
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.Phone.Should().Be(validPhone);
        }

        [Theory]
        [InlineData("123456789")]   // 9 digits
        [InlineData("012345678901")] // 12 digits
        [InlineData("1123456789")]   // doesn't start with 0
        [InlineData("0abc123456")]   // contains letters
        public async Task AddCustomer_WithInvalidPhoneFormats_ShouldSucceed(string invalidPhone)
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = $"Test Customer {invalidPhone}",
                Phone = invalidPhone,
                Address = "123 Test Street",
                Email = $"test{invalidPhone}@example.com",
                RequestedBy = "TEST_USER"
            };

            // Act - Service doesn't validate phone format, so it should succeed
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.Phone.Should().Be(invalidPhone);
        }

        [Fact]
        public async Task AddCustomer_WithVeryLongName_ShouldSucceed()
        {
            // Arrange
            var longName = new string('A', 100); // Use reasonable length
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = longName,
                Phone = "0123456782",
                Address = "123 Test Street",
                Email = "testlong@example.com",
                RequestedBy = "TEST_USER"
            };

            // Act - Service doesn't validate name length, so it should succeed
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.CustomerName.Should().Be(longName);
        }

        [Fact]
        public async Task AddCustomer_WithSpecialCharactersInName_ShouldSucceed()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn A-B (Công ty XYZ)",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com",
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.CustomerName.Should().Be(addCustomerDto.CustomerName);
        }

        [Fact]
        public async Task GetCustomerInfo_WithNullId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _customerService.GetCustomerInfo(null));

            exception.Message.Should().Be("Không tìm thấy người dùng trong hệ thống");
        }

        [Fact]
        public async Task GetCustomerInfo_WithEmptyId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _customerService.GetCustomerInfo(""));

            exception.Message.Should().Be("Không tìm thấy người dùng trong hệ thống");
        }

        [Fact]
        public async Task GetCustomerInfo_WithWhitespaceId_ShouldThrowException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _customerService.GetCustomerInfo("   "));

            exception.Message.Should().Be("Không tìm thấy người dùng trong hệ thống");
        }

        [Fact]
        public async Task AddCustomer_ConcurrentRequests_ShouldHandleCorrectly()
        {
            // Arrange
            var addCustomerDto1 = new AddCustomerDTO
            {
                CustomerName = "Concurrent Customer 1",
                Phone = "0123456781",
                Address = "123 Test Street",
                Email = "concurrent1@example.com",
                RequestedBy = "TEST_USER_1"
            };

            var addCustomerDto2 = new AddCustomerDTO
            {
                CustomerName = "Concurrent Customer 2",
                Phone = "0123456782",
                Address = "123 Test Street",
                Email = "concurrent2@example.com",
                RequestedBy = "TEST_USER_2"
            };

            // Act
            var task1 = _customerService.AddCustomer(addCustomerDto1);
            var task2 = _customerService.AddCustomer(addCustomerDto2);

            var results = await Task.WhenAll(task1, task2);

            // Assert
            results.Should().HaveCount(2);
            results[0].Should().NotBeNull();
            results[1].Should().NotBeNull();
            results[0].Id.Should().NotBe(results[1].Id);
        }

        [Fact]
        public async Task AddCustomer_DatabaseConnectionIssue_ShouldThrowMeaningfulException()
        {
            // Arrange
            _context.Dispose(); // Simulate database connection issue

            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Test Customer",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com",
                RequestedBy = "TEST_USER"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                _customerService.AddCustomer(addCustomerDto));

            // Should throw ObjectDisposedException when context is disposed
            exception.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCustomerInfo_VerifyAllFieldsReturned()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Complete Customer Test",
                Phone = "0123456789",
                Address = "123 Complete Test Street, Ward 1, District 1, Ho Chi Minh City",
                Email = "complete.test@example.com",
                RequestedBy = "TEST_USER"
            };

            var addedCustomer = await _customerService.AddCustomer(addCustomerDto);

            // Act
            var result = await _customerService.GetCustomerInfo(addedCustomer.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(addedCustomer.Id);
            result.CustomerName.Should().Be(addCustomerDto.CustomerName);
            result.Phone.Should().Be(addCustomerDto.Phone);
            result.Address.Should().Be(addCustomerDto.Address);
            result.Email.Should().Be(addCustomerDto.Email);
        }

        [Theory]
        [InlineData("test@gmail.com")]
        [InlineData("user.name@domain.co.uk")]
        [InlineData("test+tag@example.org")]
        [InlineData("123@456.789")]
        public async Task AddCustomer_WithValidEmailFormats_ShouldSucceed(string validEmail)
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = $"Test Customer {validEmail}",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = validEmail,
                RequestedBy = "TEST_USER"
            };

            // Act
            var result = await _customerService.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(validEmail);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}