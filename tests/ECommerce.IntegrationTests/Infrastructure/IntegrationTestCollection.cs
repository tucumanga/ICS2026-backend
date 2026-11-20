using Xunit;

namespace ECommerce.IntegrationTests.Infrastructure;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // Esta clase no contiene código ejecutable. Su propósito es definir la colección
    // de pruebas xUnit para compartir la instancia de CustomWebApplicationFactory
    // y prevenir problemas de concurrencia en la base de datos en memoria.
}
