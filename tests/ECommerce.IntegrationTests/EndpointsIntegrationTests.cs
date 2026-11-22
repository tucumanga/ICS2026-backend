using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

[Collection("IntegrationTests")]
public class EndpointsIntegrationTests
{
    private readonly HttpClient _client;

    public EndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_HealthCheck_ShouldReturnOkWithHealthyStatus()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/healthcheck");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task Get_Products_AnonymousAccess_ShouldReturnSuccessStatusCode()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Get_ProductById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_Product_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var newProduct = new
        {
            Sku = "UNAUTH-SKU",
            InternalCode = "UNAUTH-INT",
            Name = "Producto Sin Auth",
            Description = "Descripcion",
            CurrentUnitPrice = 100m,
            StockQuantity = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_AdminProducts_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/products/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Orders_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_OrderById_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            Username = "usuario_inexistente",
            Password = "PasswordIncorrecto123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Register_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange (Identity requiere contraseñas de al menos 8 caracteres)
        var registerRequest = new
        {
            Username = "usuario_invalido",
            Email = "invalido@test.com",
            Password = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
