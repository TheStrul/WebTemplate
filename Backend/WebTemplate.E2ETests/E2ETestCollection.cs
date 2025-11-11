using Xunit;

namespace WebTemplate.E2ETests;

/// <summary>
/// Collection definition for E2E tests.
/// E2E tests are run sequentially to avoid conflicts when testing against a shared backend.
/// </summary>
[CollectionDefinition("E2E Tests", DisableParallelization = true)]
public class E2ETestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and configure E2E test execution.
}
