using Xunit;

namespace WebTemplate.ApiTests
{
    // xUnit collection definition to share TestWebAppFactory across integration tests
    [CollectionDefinition("Integration Tests")]
    public class IntegrationTestsCollection : ICollectionFixture<TestWebAppFactory>
    {
        // No code needed; serves as a marker for the collection fixture
    }
}
