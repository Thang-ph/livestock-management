using AutoMapper;
using BusinessObjects;
using BusinessObjects.Dtos;
using DataAccess.Repository.Services;
using FluentAssertions;
using LivestockManagementSystem.Tests.Helpers;
using LivestockManagementSystemAPI.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LivestockManagementSystem.Tests.Integration
{
    public class CustomerIntegrationTests : IDisposable
    {
        private readonly LmsContext _context;
        private readonly CustomerService _customerService;
        private readonly CustomerController _controller;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<ILogger<CustomerController>> _mockLogger;
        private readonly IMapper _mapper;

        public CustomerIntegrationTests()
        {
            // Setup in-memory database
            _context = TestDbContextFactory.CreateInMemoryContext();

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DataAccess.AutoMapperConfig.ConfigMapper>();
            });
            _mapper = config.CreateMapper();

            // Setup mock dependencies
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<CustomerController>>();

            // Create service and controller
            _customerService = new CustomerService(_context, _mapper, _mockUserManager.Object);
            _controller = new CustomerController(_customerService, _mockLogger.Object);
        }

        [Fact]
        public async Task FullCustomerFlow_AddAndRetrieve_ShouldWorkCorrectly()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn Integration Test",
                Phone = "0123456789",
                Address = "123 Integration Test Street",
                Email = "integration@test.com",
                RequestedBy = "INTEGRATION_TEST"
            };

            // Act 1: Add customer through controller
            var addResult = await _controller.AddCustomer(addCustomerDto);

            // Assert 1: Customer was added successfully
            addResult.Should().NotBeNull();
            var createdResult = addResult.Result as ObjectResult;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(200);

            // Extract CustomerInfo from BaseResponse
            var baseResponse = createdResult.Value as LivestockManagementSystemAPI.Controllers.BaseResponse;
            baseResponse.Should().NotBeNull();
            var createdCustomer = baseResponse.Data as CustomerInfo;
            createdCustomer.Should().NotBeNull();
            createdCustomer.CustomerName.Should().Be(addCustomerDto.CustomerName);
            createdCustomer.Phone.Should().Be(addCustomerDto.Phone);
            createdCustomer.Id.Should().NotBeNullOrEmpty();

            // Act 2: Retrieve customer through controller
            var getResult = await _controller.GetCustomerInfomation(createdCustomer.Id);

            // Assert 2: Customer was retrieved successfully
            getResult.Should().NotBeNull();
            var okResult = getResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            // Extract CustomerInfo from BaseResponse
            var getBaseResponse = okResult.Value as LivestockManagementSystemAPI.Controllers.BaseResponse;
            getBaseResponse.Should().NotBeNull();
            var retrievedCustomer = getBaseResponse.Data as CustomerInfo;
            retrievedCustomer.Should().NotBeNull();
            retrievedCustomer.Id.Should().Be(createdCustomer.Id);
            retrievedCustomer.CustomerName.Should().Be(addCustomerDto.CustomerName);
            retrievedCustomer.Phone.Should().Be(addCustomerDto.Phone);
            retrievedCustomer.Address.Should().Be(addCustomerDto.Address);
            retrievedCustomer.Email.Should().Be(addCustomerDto.Email);

            // Act 3: Verify data exists in database
            var dbCustomer = await _context.Customers.FindAsync(createdCustomer.Id);

            // Assert 3: Data persisted correctly in database
            dbCustomer.Should().NotBeNull();
            dbCustomer.Fullname.Should().Be(addCustomerDto.CustomerName);
            dbCustomer.Phone.Should().Be(addCustomerDto.Phone);
            dbCustomer.Address.Should().Be(addCustomerDto.Address);
            dbCustomer.Email.Should().Be(addCustomerDto.Email);
            dbCustomer.CreatedBy.Should().Be("INTEGRATION_TEST");
        }

        [Fact]
        public async Task AddDuplicateCustomer_ShouldReturnError()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn Duplicate",
                Phone = "0987654321",
                Address = "123 Duplicate Street",
                Email = "duplicate@test.com",
                RequestedBy = "TEST"
            };

            // Act 1: Add customer first time
            var firstAddResult = await _controller.AddCustomer(addCustomerDto);

            // Assert 1: First add should succeed
            firstAddResult.Should().NotBeNull();
            var firstCreatedResult = firstAddResult.Result as ObjectResult;
            firstCreatedResult.Should().NotBeNull();
            firstCreatedResult.StatusCode.Should().Be(200);

            // Act 2: Try to add same customer again
            var secondAddResult = await _controller.AddCustomer(addCustomerDto);

            // Assert 2: Second add should fail
            secondAddResult.Should().NotBeNull();
            var badRequestResult = secondAddResult.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GetNonExistentCustomer_ShouldReturnError()
        {
            // Arrange
            var nonExistentId = "non-existent-customer-id";

            // Act
            var result = await _controller.GetCustomerInfomation(nonExistentId);

            // Assert
            result.Should().NotBeNull();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task AddCustomerWithInvalidData_ShouldReturnValidationError()
        {
            // Arrange
            var invalidCustomerDto = new AddCustomerDTO
            {
                CustomerName = "", // Invalid - empty name
                Phone = "123", // Invalid - wrong format
                Address = "Test Address",
                Email = "invalid-email" // Invalid email format
            };

            // Simulate ModelState validation
            _controller.ModelState.AddModelError("CustomerName", "Tên không được để trống.");
            _controller.ModelState.AddModelError("Phone", "Số điện thoại phải bắt đầu bằng 0 và có từ 10 đến 11 chữ số.");

            // Act
            var result = await _controller.AddCustomer(invalidCustomerDto);

            // Assert
            result.Should().NotBeNull();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}