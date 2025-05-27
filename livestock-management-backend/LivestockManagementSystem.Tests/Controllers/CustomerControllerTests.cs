using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using FluentAssertions;
using LivestockManagementSystemAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LivestockManagementSystem.Tests.Controllers
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<ILogger<CustomerController>> _mockLogger;
        private readonly CustomerController _controller;

        public CustomerControllerTests()
        {
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockLogger = new Mock<ILogger<CustomerController>>();
            _controller = new CustomerController(_mockCustomerRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCustomerInfomation_ValidId_ShouldReturnOkResult()
        {
            // Arrange
            var customerId = "test-customer-1";
            var expectedCustomer = new CustomerInfo
            {
                Id = customerId,
                CustomerName = "Nguyễn Văn A",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com"
            };

            _mockCustomerRepository
                .Setup(x => x.GetCustomerInfo(customerId))
                .ReturnsAsync(expectedCustomer);

            // Act
            var result = await _controller.GetCustomerInfomation(customerId);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            // Verify repository was called
            _mockCustomerRepository.Verify(x => x.GetCustomerInfo(customerId), Times.Once);
        }

        [Fact]
        public async Task GetCustomerInfomation_InvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var customerId = "invalid-id";
            var errorMessage = "Không tìm thấy người dùng trong hệ thống";

            _mockCustomerRepository
                .Setup(x => x.GetCustomerInfo(customerId))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _controller.GetCustomerInfomation(customerId);

            // Assert
            result.Should().NotBeNull();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);

            // Verify repository was called
            _mockCustomerRepository.Verify(x => x.GetCustomerInfo(customerId), Times.Once);
        }

        [Fact]
        public async Task AddCustomer_ValidModel_ShouldReturnCreatedResult()
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

            var expectedResult = new CustomerInfo
            {
                Id = "new-customer-id",
                CustomerName = addCustomerDto.CustomerName,
                Phone = addCustomerDto.Phone,
                Address = addCustomerDto.Address,
                Email = addCustomerDto.Email
            };

            _mockCustomerRepository
                .Setup(x => x.AddCustomer(It.IsAny<AddCustomerDTO>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            var createdResult = result.Result as ObjectResult;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(200);

            // Verify repository was called
            _mockCustomerRepository.Verify(x => x.AddCustomer(It.IsAny<AddCustomerDTO>()), Times.Once);
        }

        [Fact]
        public async Task AddCustomer_InvalidModel_ShouldReturnBadRequest()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "", // Invalid - empty name
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com"
            };

            // Simulate ModelState error
            _controller.ModelState.AddModelError("CustomerName", "Tên không được để trống.");

            // Act
            var result = await _controller.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);

            // Verify repository was NOT called
            _mockCustomerRepository.Verify(x => x.AddCustomer(It.IsAny<AddCustomerDTO>()), Times.Never);
        }

        [Fact]
        public async Task AddCustomer_DuplicateCustomer_ShouldReturnBadRequest()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn A",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com"
            };

            var errorMessage = "Nguời dùng đã tồn tại trong hệ thống";

            _mockCustomerRepository
                .Setup(x => x.AddCustomer(It.IsAny<AddCustomerDTO>()))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _controller.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);

            // Verify repository was called
            _mockCustomerRepository.Verify(x => x.AddCustomer(It.IsAny<AddCustomerDTO>()), Times.Once);
        }

        [Fact]
        public async Task AddCustomer_RepositoryThrowsException_ShouldReturnBadRequest()
        {
            // Arrange
            var addCustomerDto = new AddCustomerDTO
            {
                CustomerName = "Nguyễn Văn Test",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com"
            };

            _mockCustomerRepository
                .Setup(x => x.AddCustomer(It.IsAny<AddCustomerDTO>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.AddCustomer(addCustomerDto);

            // Assert
            result.Should().NotBeNull();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);

            // Verify repository was called
            _mockCustomerRepository.Verify(x => x.AddCustomer(It.IsAny<AddCustomerDTO>()), Times.Once);
        }
    }
}