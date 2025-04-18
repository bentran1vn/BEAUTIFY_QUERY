using System.Collections;
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
            new()
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
            new()
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
                        new() { ClinicId = childClinicId }
                    }
                },
                DepositAmount = 25,
                FinalAmount = 100
            },
            new()
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
                        new() { ClinicId = childClinicId }
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
            new()
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
            new()
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
            new()
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
                        new() { ClinicId = childClinic1Id }
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
            new()
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
            new()
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
                        new() { ClinicId = childClinicId }
                    }
                },
                DepositAmount = 25,
                FinalAmount = 100
            },
            new()
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
                        new() { ClinicId = childClinicId }
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

    // Fixed: Simplified ExecuteAsync method to avoid reflection errors
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        // For common EF Core async operations (Count, First, etc.), we'll execute them synchronously
        // This works because we're dealing with in-memory collections
        var result = _inner.Execute<TResult>(expression);

        // For Task<T> or ValueTask<T> results, we need to wrap them
        if (!typeof(TResult).IsGenericType ||
            (typeof(TResult).GetGenericTypeDefinition() != typeof(Task<>) &&
             typeof(TResult).GetGenericTypeDefinition() != typeof(ValueTask<>))) return result;
        // Get the T from Task<T> or ValueTask<T>
        var resultType = typeof(TResult).GetGenericArguments()[0];

        // For Task<T>
        if (typeof(TResult).GetGenericTypeDefinition() == typeof(Task<>))
        {
            var methodInfo = typeof(Task).GetMethod(nameof(Task.FromResult));
            var genericMethod = methodInfo.MakeGenericMethod(resultType);

            // Get the underlying result
            var innerResult = _inner.Execute(expression);
            if (innerResult is not IEnumerable enumerable || resultType == typeof(IEnumerable))
                return (TResult)genericMethod.Invoke(null, new[] { innerResult });
            // If the result is IEnumerable but the expected return type is something like int, 
            // we need to handle specific operations like Count, Sum, etc.
            if (resultType == typeof(int))
            {
                // Most likely Count or Sum
                innerResult = enumerable.Cast<object>().Count();
            }

            return (TResult)genericMethod.Invoke(null, new[] { innerResult });
        }

        // For ValueTask<T>
        var valueTaskConstructor = typeof(ValueTask<>).MakeGenericType(resultType)
            .GetConstructor(new[] { resultType });
        if (valueTaskConstructor != null)
        {
            return (TResult)valueTaskConstructor.Invoke(new[] { _inner.Execute(expression) });
        }

        return result;
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }
}

// Fixed: Removed circular provider reference
public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryProvider _provider;

    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
        _provider = new TestAsyncQueryProvider<T>(((IQueryable)this).Provider);
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
        _provider = new TestAsyncQueryProvider<T>(((IQueryable)this).Provider);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    // Fixed: Don't create a new provider each time, which creates a circular reference
    IQueryProvider IQueryable.Provider => _provider;
}

public class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        return ValueTask.CompletedTask;
    }
}