using Xunit;
namespace PM.Integration.Tests;

[CollectionDefinition("E2ETests")]
public class E2ETestsCollection : ICollectionFixture<IntegrationWebApplicationFactory>
{

}
