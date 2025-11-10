using Xunit;
namespace PM.Integration.Tests;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationWebApplicationFactory>
{
    // Marker class for xUnit integration test collection
}
