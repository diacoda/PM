namespace PM.SharedKernel
{
    /// <summary>
    /// Represents a standardized error object used across the application.
    /// </summary>
    /// <remarks>
    /// The <see cref="Error"/> record encapsulates a unique error <see cref="Code"/>, 
    /// a human-readable <see cref="Description"/>, and an <see cref="ErrorType"/> 
    /// that categorizes the nature of the error (e.g., failure, not found, conflict).
    /// </remarks>
    public record Error
    {
        /// <summary>
        /// Represents an empty or non-error state.
        /// </summary>
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

        /// <summary>
        /// Represents an error indicating that a null value was provided where it was not expected.
        /// </summary>
        public static readonly Error NullValue = new(
            "General.Null",
            "Null value was provided",
            ErrorType.Failure);

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> record.
        /// </summary>
        /// <param name="code">A unique string identifying the type or source of the error.</param>
        /// <param name="description">A human-readable description of the error.</param>
        /// <param name="type">The <see cref="ErrorType"/> indicating the error category.</param>
        public Error(string code, string description, ErrorType type)
        {
            Code = code;
            Description = description;
            Type = type;
        }

        /// <summary>
        /// Gets the unique error code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the error description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the type or category of the error.
        /// </summary>
        public ErrorType Type { get; }

        /// <summary>
        /// Creates an error that represents a general failure.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Failure"/>.</returns>
        public static Error Failure(string code, string description) =>
            new(code, description, ErrorType.Failure);

        /// <summary>
        /// Creates an error that represents a resource not found.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.NotFound"/>.</returns>
        public static Error NotFound(string code, string description) =>
            new(code, description, ErrorType.NotFound);

        /// <summary>
        /// Creates an error that represents a generic problem or unexpected issue.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Problem"/>.</returns>
        public static Error Problem(string code, string description) =>
            new(code, description, ErrorType.Problem);

        /// <summary>
        /// Creates an error that represents a conflict (e.g., duplicate resource, concurrency issue).
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Conflict"/>.</returns>
        public static Error Conflict(string code, string description) =>
            new(code, description, ErrorType.Conflict);
    }
}
