using Xunit;
namespace PM.Integration.Tests;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // Marker class for xUnit integration test collection
}
