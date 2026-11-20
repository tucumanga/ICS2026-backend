using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerce.UnitTests.Services;

public class OrdersManagementServiceTests
{
    private readonly Mock<IRepository> _repositoryMock;
    private readonly Mock<ILogger<OrdersManagementService>> _loggerMock;
    private readonly OrdersManagementService _service;

    public OrdersManagementServiceTests()
    {
        _repositoryMock = new Mock<IRepository>();
        _loggerMock = new Mock<ILogger<OrdersManagementService>>();
        _service = new OrdersManagementService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyItems_ShouldThrowInvalidOrderDataException()
    {
        // Arrange
        var request = new OrderRequest(
            CustomerId: Guid.NewGuid(),
            ShippingAddress: "Av. Siempre Viva 123",
            BillingAddress: "Av. Siempre Viva 123",
            Notes: null,
            Items: new List<OrderItemRequest>()
        );

        // Act
        Func<Task> act = async () => await _service.CreateOrder(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOrderDataException>()
            .WithMessage("La orden debe tener al menos un ítem.");

        _repositoryMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WhenCustomerDoesNotExist_ShouldThrowCustomerNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new OrderRequest(
            CustomerId: customerId,
            ShippingAddress: "Calle Falsa 123",
            BillingAddress: "Calle Falsa 123",
            Notes: null,
            Items: new List<OrderItemRequest>
            {
                new OrderItemRequest(Guid.NewGuid(), 1)
            }
        );

        _repositoryMock
            .Setup(r => r.GetById<Customer>(customerId, It.IsAny<string[]>()))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () => await _service.CreateOrder(request);

        // Assert
        await act.Should().ThrowAsync<CustomerNotFoundException>();
        _repositoryMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WhenStockIsInsufficient_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var customer = new Customer { Email = "cliente@test.com", Name = "Cliente Test" };
        var product = new Product("SKU-01", "Auriculares", 50m, 2, "Bluetooth", "INT-01") { Id = productId };

        var request = new OrderRequest(
            CustomerId: customerId,
            ShippingAddress: "Calle 1",
            BillingAddress: "Calle 1",
            Notes: null,
            Items: new List<OrderItemRequest>
            {
                new OrderItemRequest(productId, 5) // Pide 5 unidades habiendo solo 2 en stock
            }
        );

        _repositoryMock
            .Setup(r => r.GetById<Customer>(customerId, It.IsAny<string[]>()))
            .ReturnsAsync(customer);

        _repositoryMock
            .Setup(r => r.GetById<Product>(productId, It.IsAny<string[]>()))
            .ReturnsAsync(product);

        // Act
        Func<Task> act = async () => await _service.CreateOrder(request);

        // Assert
        await act.Should().ThrowAsync<InsufficientStockException>();
        _repositoryMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WithValidDataAndStock_ShouldDeductStockAndCreateOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var customer = new Customer { Email = "carlos@test.com", Name = "Carlos Gomez" };
        var product = new Product("SKU-MOUSE", "Mouse Gamer", 30m, 10, "Optico", "INT-03") { Id = productId };

        var request = new OrderRequest(
            CustomerId: customerId,
            ShippingAddress: "Av. Corrientes 500",
            BillingAddress: "Av. Corrientes 500",
            Notes: "Entregar por la tarde",
            Items: new List<OrderItemRequest>
            {
                new OrderItemRequest(productId, 3)
            }
        );

        _repositoryMock
            .Setup(r => r.GetById<Customer>(customerId, It.IsAny<string[]>()))
            .ReturnsAsync(customer);

        _repositoryMock
            .Setup(r => r.GetById<Product>(productId, It.IsAny<string[]>()))
            .ReturnsAsync(product);

        _repositoryMock
            .Setup(r => r.Update(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        // Act
        var result = await _service.CreateOrder(request);

        // Assert
        result.Should().NotBeNull();
        result.ClientName.Should().Be("Carlos Gomez");
        result.TotalAmount.Should().Be(90m); // 3 * 30m
        result.Items.Should().HaveCount(1);
        result.Items.First().Subtotal.Should().Be(90m);

        product.StockQuantity.Should().Be(7); // 10 - 3 = 7
        _repositoryMock.Verify(r => r.Update(It.Is<Product>(p => p.StockQuantity == 7)), Times.Once);
        _repositoryMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
    }
}
