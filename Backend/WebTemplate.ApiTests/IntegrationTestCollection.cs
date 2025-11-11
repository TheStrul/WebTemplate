namespace WebTemplate.ApiTests
{
    using Xunit;

    [CollectionDefinition("Integration Tests")]
    public class IntegrationTestCollection : ICollectionFixture<TestWebAppFactory>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
