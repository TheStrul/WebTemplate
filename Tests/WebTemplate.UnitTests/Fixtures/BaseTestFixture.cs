namespace WebTemplate.UnitTests.Fixtures
{
    using AutoFixture;

    /// <summary>
    /// Base test fixture with common test utilities
    /// </summary>
    public class BaseTestFixture
    {
        protected Fixture Fixture { get; }

        public BaseTestFixture()
        {
            Fixture = new Fixture();

            // Configure AutoFixture behaviors
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        /// <summary>
        /// Creates a random string of specified length
        /// </summary>
        protected string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());
        }

        /// <summary>
        /// Creates a random email address
        /// </summary>
        protected string RandomEmail() => $"{RandomString(8)}@example.com";

        /// <summary>
        /// Creates a random password that meets complexity requirements
        /// </summary>
        protected string RandomPassword() => $"{RandomString(8)}1!Aa";
    }
}
