using System.Linq.Expressions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Orders;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BEAUTIFY_QUERY.APPLICATION.Tests.UseCases.Queries.Orders;
public class GetOrdersByCustomerIdQueryHandlerTests
{
    private readonly GetOrdersByCustomerIdQueryHandler _handler;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IRepositoryBase<Order, Guid>> _mockOrderRepository;

    public GetOrdersByCustomerIdQueryHandlerTests()
    {
        _mockOrderRepository = new Mock<IRepositoryBase<Order, Guid>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new GetOrdersByCustomerIdQueryHandler(
            _mockOrderRepository.Object,
            _mockCurrentUserService.Object);

        // Patch PagedResult.CreateAsync
        SetupMockPagedResult();
    }

    private void SetupMockPagedResult()
    {
        // Fix: Add the cancellationToken parameter to the callback
        _mockOrderRepository.Setup(r => r.FindAll(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns((Expression<Func<Order, bool>> predicate) =>
            {
                // Create a mock DbSet with our MockDbSet helper
                var allOrders = GetSampleOrders();
                // Filter the orders based on the predicate
                var filteredOrders = allOrders.AsQueryable().Where(predicate).ToList();
                return CreateMockDbSet(filteredOrders).Object;
            });
    }

    private List<Order> GetSampleOrders()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        return new List<Order>
        {
            new Order
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                CustomerId = userId,
                Customer = new User
                {
                    Id = userId,
                    FirstName = "John",
                    LastName = "Doe",
                    Password = "123",
                    PhoneNumber = "1234567890",
                    Email = "john@example.com",
                    Status = 1
                },
                Service = new Service
                {
                    Name = "Haircut",
                    Description = "Standard haircut service"
                },
                TotalAmount = 100,
                Discount = 10,
                DepositAmount = 20,
                FinalAmount = 90,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                CreatedOnUtc = DateTime.UtcNow
            },
            new Order
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                CustomerId = userId,
                Customer = new User
                {
                    Id = userId,
                    FirstName = "John",
                    LastName = "Doe",
                    Password = "123",
                    PhoneNumber = "1234567890",
                    Email = "john@example.com",
                    Status = 1
                },
                Service = new Service
                {
                    Name = "Manicure",
                    Description = "Standard manicure service"
                },
                TotalAmount = 50,
                Discount = 5,
                DepositAmount = 10,
                FinalAmount = 45,
                OrderDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                CreatedOnUtc = DateTime.UtcNow.AddDays(-1)
            }
        };
    }

    [Fact]
    public async Task Handle_ReturnsCustomerOrders_WhenCustomerHasOrders()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        var query = new Query.GetOrdersByCustomerId(
            null,
            null,
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal("John Doe", result.Value.Items.First().CustomerName);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenCustomerHasNoOrders()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000002"); // Different user ID
        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        var query = new Query.GetOrdersByCustomerId(
            null,
            null,
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_SearchesByDateRange_WhenSearchTermContainsValidDates()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        // Create a date range that includes today but not yesterday
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);

        var query = new Query.GetOrdersByCustomerId(
            $"{today:yyyy-MM-dd} to {today:yyyy-MM-dd}",
            null,
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.TotalCount); // Only one order from today
        Assert.Equal("Haircut", result.Value.Items.First().ServiceName);
    }

    [Fact]
    public async Task Handle_SearchesByPriceRange_WhenSearchTermContainsValidPrices()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        var query = new Query.GetOrdersByCustomerId(
            "80 to 100", // Price range that includes only the $90 final amount
            null,
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(90, result.Value.Items.First().FinalAmount);
    }

    [Fact]
    public async Task Handle_SortsByTotalAmount_WhenSortColumnIsTotalAmount()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        var query = new Query.GetOrdersByCustomerId(
            null,
            "total amount",
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);
        // Check if sorted by total amount ascending
        Assert.Equal(50, result.Value.Items.First().TotalAmount);
        Assert.Equal(100, result.Value.Items.Last().TotalAmount);
    }

    [Fact]
    public async Task Handle_SortsByDiscount_WhenSortColumnIsDiscount()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        var query = new Query.GetOrdersByCustomerId(
            null,
            "discount",
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);
        // Check if sorted by discount ascending
        Assert.Equal(5, result.Value.Items.First().Discount);
        Assert.Equal(10, result.Value.Items.Last().Discount);
    }

    [Fact]
    public async Task Handle_SortsByFinalAmount_WhenSortColumnIsFinalAmount()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        var query = new Query.GetOrdersByCustomerId(
            null,
            "final amount",
            SortOrder.Ascending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);
        // Check if sorted by final amount ascending
        Assert.Equal(45, result.Value.Items.First().FinalAmount);
        Assert.Equal(90, result.Value.Items.Last().FinalAmount);
    }

    [Fact]
    public async Task Handle_SortsDescending_WhenSortOrderIsDescending()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        var query = new Query.GetOrdersByCustomerId(
            null,
            "total amount",
            SortOrder.Descending,
            0,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);
        // Check if sorted by total amount descending
        Assert.Equal(100, result.Value.Items.First().TotalAmount);
        Assert.Equal(50, result.Value.Items.Last().TotalAmount);
    }

    // Fixed implementation of the mock DbSet
    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        // Setup for synchronous operations - simpler approach that won't cause recursion
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

        // Setup for Include() method - return the same mock to support chaining
        mockDbSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockDbSet.Object);

        return mockDbSet;
    }
}