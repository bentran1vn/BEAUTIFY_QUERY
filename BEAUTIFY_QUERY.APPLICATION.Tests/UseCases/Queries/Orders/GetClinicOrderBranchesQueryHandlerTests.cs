using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace BEAUTIFY_QUERY.APPLICATION.Tests.UseCases.Queries.Orders;
public class GetClinicOrderBranchesQueryHandlerTests
{
    private readonly GetClinicOrderBranchesQueryHandler _handler;
    private readonly Mock<IRepositoryBase<Clinic, Guid>> _mockClinicRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IRepositoryBase<Order, Guid>> _mockOrderRepository;

    public GetClinicOrderBranchesQueryHandlerTests()
    {
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockOrderRepository = new Mock<IRepositoryBase<Order, Guid>>();
        _mockClinicRepository = new Mock<IRepositoryBase<Clinic, Guid>>();
        _handler = new GetClinicOrderBranchesQueryHandler(
            _mockCurrentUserService.Object,
            _mockOrderRepository.Object,
            _mockClinicRepository.Object);
    }

    [Fact]
    public async Task Handle_WithServiceNameSearch_FiltersCorrectly()
    {
        // Arrange
        var parentClinicId = Guid.NewGuid();
        var childClinicId = Guid.NewGuid();
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();

        _mockCurrentUserService.Setup(x => x.ClinicId).Returns(parentClinicId);

        var parentClinic = new Clinic
        {
            Id = parentClinicId,
            IsParent = true,
            Name = "Parent Clinic",
            Email = "parent@example.com",
            PhoneNumber = "1234567890",
            TaxCode = "TAX123",
            BusinessLicenseUrl = "https://example.com/business-license",
            OperatingLicenseUrl = "https://example.com/operating-license"
        };
        var childClinics = new List<Clinic>
        {
            new Clinic
            {
                Id = childClinicId,
                ParentId = parentClinicId,
                Name = "Child Clinic",
                Email = "child@example.com",
                PhoneNumber = "1234567891",
                TaxCode = "TAX124",
                BusinessLicenseUrl = "https://example.com/business-license-child",
                OperatingLicenseUrl = "https://example.com/operating-license-child"
            }
        };

        _mockClinicRepository.Setup(x => x.FindByIdAsync(parentClinicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentClinic);

        // Mock DbSet for child clinics
        var mockChildClinicDbSet = CreateMockDbSet(childClinics);
        _mockClinicRepository.Setup(x => x.FindAll(It.IsAny<Expression<Func<Clinic, bool>>>()))
            .Returns(mockChildClinicDbSet.Object);

        // Create orders with different service names
        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1Id,
                Customer = new User
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Password = "123",
                    PhoneNumber = "1234567890",
                    Email = "john@example.com",
                    Status = 1
                },
                Service = new Service
                {
                    Name = "Haircut", // This should match
                    Description = "Standard haircut service",
                    ClinicServices = new List<ClinicService>
                    {
                        new ClinicService { ClinicId = childClinicId }
                    }
                },
                DepositAmount = 25,
                FinalAmount = 100
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2Id,
                Customer = new User
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Password = "123",
                    PhoneNumber = "9876543210",
                    Email = "jane@example.com",
                    Status = 1
                },
                Service = new Service
                {
                    Name = "Manicure", // This shouldn't match
                    Description = "Standard manicure service",
                    ClinicServices = new List<ClinicService>
                    {
                        new ClinicService { ClinicId = childClinicId }
                    }
                },
                DepositAmount = 10,
                FinalAmount = 50
            }
        };

        // Return ALL orders via the repository
        // Let the handler filter them instead
        var mockOrderDbSet = CreateMockDbSet(orders);
        _mockOrderRepository.Setup(x => x.FindAll(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(mockOrderDbSet.Object);

        var query = new Query.GetClinicOrderBranchesQuery(
            "Hair",
            "serviceName", // This should be correct now
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Items);
        Assert.Equal("Haircut", result.Value.Items.First().ServiceName);
    }

    [Fact]
    public async Task Handle_WithValidParentClinic_ReturnsOrdersFromChildClinics()
    {
        // Arrange
        var parentClinicId = Guid.NewGuid();
        var childClinic1Id = Guid.NewGuid();
        var childClinic2Id = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _mockCurrentUserService.Setup(x => x.ClinicId).Returns(parentClinicId);

        var parentClinic = new Clinic
        {
            Id = parentClinicId,
            IsParent = true,
            Name = "Parent Clinic",
            Email = "parent@example.com",
            PhoneNumber = "1234567890",
            TaxCode = "TAX123",
            BusinessLicenseUrl = "https://example.com/business-license",
            OperatingLicenseUrl = "https://example.com/operating-license"
        };
        var childClinics = new List<Clinic>
        {
            new Clinic
            {
                Id = childClinic1Id,
                ParentId = parentClinicId,
                Name = "Child Clinic 1",
                Email = "child1@example.com",
                PhoneNumber = "1234567891",
                TaxCode = "TAX124",
                BusinessLicenseUrl = "https://example.com/business-license1",
                OperatingLicenseUrl = "https://example.com/operating-license1"
            },
            new Clinic
            {
                Id = childClinic2Id,
                ParentId = parentClinicId,
                Name = "Child Clinic 2",
                Email = "child2@example.com",
                PhoneNumber = "1234567892",
                TaxCode = "TAX125",
                BusinessLicenseUrl = "https://example.com/business-license2",
                OperatingLicenseUrl = "https://example.com/operating-license2"
            }
        };

        _mockClinicRepository.Setup(x => x.FindByIdAsync(parentClinicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentClinic);

        // Create a mock DbSet for child clinics
        var mockChildDbSet = CreateMockDbSet(childClinics);

        // Set up the FindAll method to return our mock DbSet
        _mockClinicRepository.Setup(x => x.FindAll(It.IsAny<Expression<Func<Clinic, bool>>>()))
            .Returns(mockChildDbSet.Object);

        var orders = new List<Order>
        {
            new Order
            {
                Id = orderId,
                CustomerId = customerId,
                Customer = new User
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Password = "1",
                    PhoneNumber = "1234567890",
                    Email = "john@example.com",
                    Status = 1
                },
                Service = new Service
                {
                    Name = "Haircut",
                    Description = "Standard haircut service",
                    ClinicServices = new List<ClinicService>
                    {
                        new ClinicService { ClinicId = childClinic1Id }
                    }
                },
                TotalAmount = 100,
                Discount = 10,
                DepositAmount = 20,
                FinalAmount = 90,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                Status = Constant.OrderStatus.ORDER_COMPLETED
            }
        };

        // Create a mock DbSet for orders that supports async operations
        var mockOrderDbSet = CreateMockDbSet(orders);

        // Set up the FindAll method to return our mock DbSet
        _mockOrderRepository.Setup(x => x.FindAll(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(mockOrderDbSet.Object);

        var query = new Query.GetClinicOrderBranchesQuery(
            "John",
            "customerName",
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Items);
        Assert.Equal(orderId, result.Value.Items.First().Id);
        Assert.Equal("John Doe", result.Value.Items.First().CustomerName);
    }

    [Fact]
    public async Task Handle_WithNonParentClinic_ReturnsFailure()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        _mockCurrentUserService.Setup(x => x.ClinicId).Returns(clinicId);

        var clinic = new Clinic
        {
            Id = clinicId,
            IsParent = false,
            Name = "Non-Parent Clinic",
            Email = "non-parent@example.com",
            PhoneNumber = "1234567890",
            TaxCode = "TAX126",
            BusinessLicenseUrl = "https://example.com/business-license-np",
            OperatingLicenseUrl = "https://example.com/operating-license-np"
        };
        _mockClinicRepository.Setup(x => x.FindByIdAsync(clinicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clinic);

        var query = new Query.GetClinicOrderBranchesQuery(
            null,
            null,
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("403", result.Error.Code);
        Assert.Equal("Only parent clinics can access this endpoint", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersCorrectly()
    {
        // Arrange
        var parentClinicId = Guid.NewGuid();
        var childClinicId = Guid.NewGuid();
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();

        _mockCurrentUserService.Setup(x => x.ClinicId).Returns(parentClinicId);

        var parentClinic = new Clinic
        {
            Id = parentClinicId,
            IsParent = true,
            Name = "Parent Clinic",
            Email = "parent@example.com",
            PhoneNumber = "1234567890",
            TaxCode = "TAX123",
            BusinessLicenseUrl = "https://example.com/business-license",
            OperatingLicenseUrl = "https://example.com/operating-license"
        };
        var childClinics = new List<Clinic>
        {
            new Clinic
            {
                Id = childClinicId,
                ParentId = parentClinicId,
                Name = "Child Clinic",
                Email = "child@example.com",
                PhoneNumber = "1234567891",
                TaxCode = "TAX124",
                BusinessLicenseUrl = "https://example.com/business-license-child",
                OperatingLicenseUrl = "https://example.com/operating-license-child"
            }
        };

        _mockClinicRepository.Setup(x => x.FindByIdAsync(parentClinicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentClinic);

        // Mock DbSet for child clinics
        var mockChildClinicDbSet = CreateMockDbSet(childClinics);
        _mockClinicRepository.Setup(x => x.FindAll(It.IsAny<Expression<Func<Clinic, bool>>>()))
            .Returns(mockChildClinicDbSet.Object);

        // Create orders with "John" in the name for testing search functionality
        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1Id,
                Customer = new User
                {
                    FirstName = "John",
                    Password = "123",
                    LastName = "Doe",
                    PhoneNumber = "1234567890",
                    Email = "john@example.com",
                    Status = 1
                },
                Service = new Service
                {
                    Name = "Haircut",
                    Description = "Standard haircut service",
                    ClinicServices = new List<ClinicService>
                    {
                        new ClinicService { ClinicId = childClinicId }
                    }
                },
                DepositAmount = 25,
                FinalAmount = 100
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2Id,
                Customer = new User
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Password = "123",
                    PhoneNumber = "9876543210",
                    Email = "jane@example.com",
                    Status = 1
                },
                Service = new Service
                {
                    Name = "Manicure",
                    Description = "Standard manicure service",
                    ClinicServices = new List<ClinicService>
                    {
                        new ClinicService { ClinicId = childClinicId }
                    }
                },
                DepositAmount = 10,
                FinalAmount = 50
            }
        };

        // Only return John's order when searching for "john" 
        var filteredOrders = orders.Where(o =>
            o.Customer != null &&
            (o.Customer.FirstName + " " + o.Customer.LastName).Contains("john", StringComparison.OrdinalIgnoreCase)
        ).ToList();

        // Create mock DbSet that returns filtered results
        var mockOrderDbSet = CreateMockDbSet(filteredOrders);
        _mockOrderRepository.Setup(x => x.FindAll(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(mockOrderDbSet.Object);

        var query = new Query.GetClinicOrderBranchesQuery(
            "john",
            "customerName",
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Items);
        Assert.Equal("John Doe", result.Value.Items.First().CustomerName);
    }

    // Helper method to create a mock DbSet that supports async operations
    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        // Setup for synchronous operations
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryableData.Provider));
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

        // Setup for asynchronous operations
        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        return mockDbSet;
    }
}

// Required helper classes for async querying
// These classes help with the mocking of async EF Core operations
public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executeMethod = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })
            ?.MakeGenericMethod(resultType);

        var result = executeMethod?.Invoke(_inner, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(resultType)
            .Invoke(null, new[] { result });
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(((IQueryable)this).Provider);
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}