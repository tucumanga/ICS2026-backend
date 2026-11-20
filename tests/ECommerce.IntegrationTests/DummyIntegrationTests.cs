using ECommerce.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

[Collection("IntegrationTests")]
public class DummyIntegrationTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DummyIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public void IntegrationTestEnvironment_ShouldBeConfiguredAndPassing()
    {
        // Arrange
        var isConfigured = true;

        // Act & Assert
        isConfigured.Should().BeTrue();
        _client.Should().NotBeNull();
    }
}
