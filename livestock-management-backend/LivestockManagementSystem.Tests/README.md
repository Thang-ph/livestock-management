# Livestock Management System - Unit Tests

## Tổng quan

Project này chứa các unit test và integration test cho hệ thống quản lý chăn nuôi. Các test được thiết kế để đảm bảo chất lượng code và tính ổn định của hệ thống.

## Cấu trúc thư mục

```
LivestockManagementSystem.Tests/
├── Controllers/           # Unit tests cho các controllers
├── Services/             # Unit tests cho các services
├── Integration/          # Integration tests
├── Helpers/             # Helper classes và utilities
│   ├── TestDbContextFactory.cs    # Factory tạo in-memory database
│   └── TestDataSeeder.cs         # Seeding test data
└── README.md
```

## Công nghệ sử dụng

- **xUnit**: Framework testing chính
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library với syntax dễ đọc
- **Entity Framework InMemory**: In-memory database cho testing
- **AutoMapper**: Object mapping
- **Microsoft.AspNetCore.Identity**: Identity framework

## Các loại test

### 1. Unit Tests cho Services

**Vị trí**: `Services/`

Các test này kiểm tra logic business của các service classes:

- `CustomerServiceTests.cs`: Test cho CustomerService
- `DiseaseServiceTests.cs`: Test cho DiseaseService

**Ví dụ**:

```csharp
[Fact]
public async Task AddCustomer_ValidData_ShouldReturnCustomerInfo()
{
    // Arrange
    var addCustomerDto = new AddCustomerDTO { ... };

    // Act
    var result = await _customerService.AddCustomer(addCustomerDto);

    // Assert
    result.Should().NotBeNull();
    result.CustomerName.Should().Be(addCustomerDto.CustomerName);
}
```

### 2. Unit Tests cho Controllers

**Vị trí**: `Controllers/`

Các test này kiểm tra HTTP endpoints và response handling:

- `CustomerControllerTests.cs`: Test cho CustomerController

**Ví dụ**:

```csharp
[Fact]
public async Task GetCustomerInfomation_ValidId_ShouldReturnOkResult()
{
    // Test HTTP response và status codes
}
```

### 3. Integration Tests

**Vị trí**: `Integration/`

Các test này kiểm tra toàn bộ flow từ controller đến database:

- `CustomerIntegrationTests.cs`: Test toàn bộ customer workflow

## Chạy tests

### Chạy tất cả tests

```bash
dotnet test
```

### Chạy tests với coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Chạy tests cụ thể

```bash
# Chạy tests của một class
dotnet test --filter "CustomerServiceTests"

# Chạy một test method cụ thể
dotnet test --filter "AddCustomer_ValidData_ShouldReturnCustomerInfo"
```

### Chạy tests theo category

```bash
# Chỉ chạy unit tests
dotnet test --filter "Category=Unit"

# Chỉ chạy integration tests
dotnet test --filter "Category=Integration"
```

## Viết test mới

### 1. Unit Test cho Service

```csharp
public class YourServiceTests : IDisposable
{
    private readonly LmsContext _context;
    private readonly YourService _service;
    private readonly IMapper _mapper;

    public YourServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        // Setup mapper và dependencies
        _service = new YourService(_context, _mapper);
    }

    [Fact]
    public async Task YourMethod_ValidInput_ShouldReturnExpectedResult()
    {
        // Arrange
        // Act
        // Assert
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

### 2. Unit Test cho Controller

```csharp
public class YourControllerTests
{
    private readonly Mock<IYourRepository> _mockRepository;
    private readonly YourController _controller;

    public YourControllerTests()
    {
        _mockRepository = new Mock<IYourRepository>();
        _controller = new YourController(_mockRepository.Object);
    }

    [Fact]
    public async Task YourAction_ValidInput_ShouldReturnOkResult()
    {
        // Arrange
        _mockRepository.Setup(x => x.Method()).ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.YourAction();

        // Assert
        result.Should().NotBeNull();
        // Verify mock calls
        _mockRepository.Verify(x => x.Method(), Times.Once);
    }
}
```

## Best Practices

### 1. Naming Convention

- Test class: `{ClassUnderTest}Tests`
- Test method: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`

### 2. Test Structure (AAA Pattern)

```csharp
[Fact]
public async Task Method_Scenario_ExpectedResult()
{
    // Arrange - Setup test data và dependencies

    // Act - Thực hiện action cần test

    // Assert - Kiểm tra kết quả
}
```

### 3. Test Data

- Sử dụng `TestDbContextFactory` để tạo in-memory database
- Sử dụng `TestDataSeeder` để seed test data
- Mỗi test nên có data riêng biệt

### 4. Mocking

- Mock external dependencies
- Verify mock calls khi cần thiết
- Sử dụng `It.IsAny<T>()` cho flexible matching

### 5. Assertions

- Sử dụng FluentAssertions cho readable assertions
- Test cả positive và negative scenarios
- Kiểm tra exception messages

## Continuous Integration

Tests sẽ được chạy tự động trong CI/CD pipeline. Đảm bảo:

1. Tất cả tests pass
2. Code coverage đạt mức tối thiểu
3. Không có flaky tests
4. Tests chạy nhanh (< 30 giây)

## Troubleshooting

### Common Issues

1. **Tests fail với database errors**

   - Đảm bảo sử dụng unique database names
   - Dispose context properly

2. **AutoMapper configuration errors**

   - Kiểm tra AutoMapper profile được load đúng
   - Verify mapping configuration

3. **Mock setup không hoạt động**
   - Kiểm tra method signature match exactly
   - Sử dụng `It.IsAny<T>()` nếu cần

### Debug Tests

```bash
# Chạy tests với verbose output
dotnet test --logger "console;verbosity=detailed"

# Debug specific test
dotnet test --filter "TestMethodName" --logger "console;verbosity=detailed"
```

## Đóng góp

Khi thêm feature mới:

1. Viết tests trước (TDD approach)
2. Đảm bảo coverage cho các scenarios chính
3. Update README nếu cần
4. Chạy tất cả tests trước khi commit
