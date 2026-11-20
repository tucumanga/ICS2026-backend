using FluentAssertions;
using Xunit;

namespace ECommerce.UnitTests;

public class DummyUnitTests
{
    [Fact]
    public void UnitTestEnvironment_ShouldBeConfiguredAndPassing()
    {
        // Arrange
        var isConfigured = true;

        // Act & Assert
        isConfigured.Should().BeTrue();
    }
}
