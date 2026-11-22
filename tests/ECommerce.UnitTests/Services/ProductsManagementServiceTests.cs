using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace ECommerce.UnitTests.Services;

public class ProductsManagementServiceTests
{
    private readonly Mock<IRepository> _repositoryMock;
    private readonly Mock<ILogger<ProductsManagementService>> _loggerMock;
    private readonly ProductsManagementService _service;

    public ProductsManagementServiceTests()
    {
        _repositoryMock = new Mock<IRepository>();
        _loggerMock = new Mock<ILogger<ProductsManagementService>>();
        _service = new ProductsManagementService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddProduct_WithValidData_ShouldCreateAndReturnProductResponse()
    {
        // Arrange
        var request = new ProductModel.Request(
            Sku: "SKU-LAPTOP-01",
            InternalCode: "INT-01",
            Name: "Notebook Gamer",
            Description: "Notebook potente",
            CurrentUnitPrice: 1500m,
            StockQuantity: 10
        );

        _repositoryMock
            .Setup(r => r.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync((Product?)null);

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        // Act
        var result = await _service.AddProduct(request);

        // Assert
        result.Should().NotBeNull();
        result.Sku.Should().Be(request.Sku);
        result.Name.Should().Be(request.Name);
        result.CurrentUnitPrice.Should().Be(request.CurrentUnitPrice);
        result.StockQuantity.Should().Be(request.StockQuantity);
        result.IsActive.Should().BeTrue();

        _repositoryMock.Verify(r => r.Add(It.Is<Product>(p => p.Sku == request.Sku && p.Name == request.Name)), Times.Once);
    }

    [Fact]
    public async Task AddProduct_WithEmptySkuOrName_ShouldThrowInvalidDataException()
    {
        // Arrange
        var invalidRequest = new ProductModel.Request(
            Sku: "",
            InternalCode: "INT-01",
            Name: "Notebook",
            Description: "Desc",
            CurrentUnitPrice: 1000m,
            StockQuantity: 5
        );

        // Act
        Func<Task> act = async () => await _service.AddProduct(invalidRequest);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>()
            .WithMessage("El sku y el nombre son requerido");

        _repositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task AddProduct_WithDuplicateSku_ShouldThrowDuplicatedEntityException()
    {
        // Arrange
        var request = new ProductModel.Request(
            Sku: "SKU-DUPLICATE",
            InternalCode: "INT-02",
            Name: "Monitor 4K",
            Description: "Monitor",
            CurrentUnitPrice: 400m,
            StockQuantity: 8
        );

        var existingProduct = new Product("SKU-DUPLICATE", "Monitor Existente", 400m, 8);

        _repositoryMock
            .Setup(r => r.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync(existingProduct);

        // Act
        Func<Task> act = async () => await _service.AddProduct(request);

        // Assert
        await act.Should().ThrowAsync<DuplicatedEntityException>()
            .WithMessage($"Ya existe un producto con el mismo SKU {request.Sku}");

        _repositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ShouldReturnProductResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product("SKU-100", "Teclado Mecanico", 85m, 15, "Teclado RGB", "INT-100");

        _repositoryMock
            .Setup(r => r.GetById<Product>(productId, It.IsAny<string[]>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetProductById(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Sku.Should().Be("SKU-100");
        result.Name.Should().Be("Teclado Mecanico");
        result.CurrentUnitPrice.Should().Be(85m);
        result.StockQuantity.Should().Be(15);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetById<Product>(missingId, It.IsAny<string[]>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _service.GetProductById(missingId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"No se encontró un producto con el ID: {missingId}");
    }
}
