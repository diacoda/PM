namespace PM.SharedKernel
{
    /// <summary>
    /// Defines the category or nature of an <see cref="Error"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ErrorType"/> enumeration helps classify different error scenarios
    /// so that they can be handled or displayed appropriately within the application.
    /// </remarks>
    public enum ErrorType
    {
        /// <summary>
        /// Represents a general failure or unexpected error.
        /// </summary>
        Failure = 0,

        /// <summary>
        /// Represents a validation error, typically caused by invalid input or business rule violations.
        /// </summary>
        Validation = 1,

        /// <summary>
        /// Represents a problem that occurred during processing, such as an exception or internal issue.
        /// </summary>
        Problem = 2,

        /// <summary>
        /// Represents a case where a requested resource could not be found.
        /// </summary>
        NotFound = 3,

        /// <summary>
        /// Represents a conflict, such as a duplicate resource or concurrency issue.
        /// </summary>
        Conflict = 4
    }
}
