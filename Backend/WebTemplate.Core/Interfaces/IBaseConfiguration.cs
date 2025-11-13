namespace WebTemplate.Core.Configuration
{
    using WebTemplate.Core.Common;

    /// <summary>
    /// Base interface for all configuration objects.
    /// Provides validation capabilities using the Result pattern.
    /// </summary>
    public interface IBaseConfiguration
    {
        /// <summary>
        /// Validates the configuration and returns a Result indicating success or failure.
        /// All validation errors are collected and returned in the Result.
        /// </summary>
        /// <returns>Result containing all validation errors if any, or success if valid</returns>
        Result Validate();
    }
}
