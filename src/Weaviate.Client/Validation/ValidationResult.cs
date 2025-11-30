namespace Weaviate.Client.Validation;

/// <summary>
/// Represents the result of validating a C# type against a Weaviate collection schema.
/// Contains errors (critical issues) and warnings (informational issues).
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates whether validation passed (no errors).
    /// Warnings do not affect validity.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Critical validation errors that prevent type compatibility.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; init; } = Array.Empty<ValidationError>();

    /// <summary>
    /// Non-critical validation warnings (validation still passes).
    /// </summary>
    public IReadOnlyList<ValidationWarning> Warnings { get; init; } =
        Array.Empty<ValidationWarning>();

    /// <summary>
    /// Throws TypeValidationException if validation failed (has errors).
    /// </summary>
    /// <exception cref="TypeValidationException">Thrown when IsValid is false</exception>
    public void ThrowIfInvalid()
    {
        if (!IsValid)
            throw new TypeValidationException(this);
    }

    /// <summary>
    /// Returns a formatted error message combining all errors and warnings.
    /// </summary>
    public string GetDetailedMessage()
    {
        var lines = new List<string>();

        if (Errors.Count > 0)
        {
            lines.Add($"Validation failed with {Errors.Count} error(s):");
            foreach (var error in Errors)
            {
                lines.Add($"  [ERROR] {error.PropertyName}: {error.Message}");
                if (
                    !string.IsNullOrEmpty(error.ExpectedType)
                    && !string.IsNullOrEmpty(error.ActualType)
                )
                {
                    lines.Add($"     Expected: {error.ExpectedType}, Got: {error.ActualType}");
                }
            }
        }

        if (Warnings.Count > 0)
        {
            if (lines.Count > 0)
                lines.Add("");
            lines.Add($"{Warnings.Count} warning(s):");
            foreach (var warning in Warnings)
            {
                lines.Add($"  [WARNING]  {warning.PropertyName}: {warning.Message}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Creates a successful validation result with no errors or warnings.
    /// </summary>
    public static ValidationResult Success() => new ValidationResult();

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    public static ValidationResult Failed(params ValidationError[] errors) =>
        new ValidationResult { Errors = errors };

    /// <summary>
    /// Creates a validation result with errors and warnings.
    /// </summary>
    public static ValidationResult WithIssues(
        IEnumerable<ValidationError> errors,
        IEnumerable<ValidationWarning> warnings
    ) => new ValidationResult { Errors = errors.ToList(), Warnings = warnings.ToList() };
}

/// <summary>
/// Represents a critical validation error that prevents type compatibility.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// The C# property name that has the validation error.
    /// </summary>
    public required string PropertyName { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// The type of validation error.
    /// </summary>
    public required ValidationErrorType ErrorType { get; init; }

    /// <summary>
    /// The expected Weaviate data type from the schema (e.g., "text", "int", "date[]").
    /// </summary>
    public string? ExpectedType { get; init; }

    /// <summary>
    /// The actual C# type detected (e.g., "System.String", "System.Int32").
    /// </summary>
    public string? ActualType { get; init; }

    /// <summary>
    /// Additional context information for debugging.
    /// </summary>
    public string? AdditionalInfo { get; init; }
}

/// <summary>
/// Categorizes the type of validation error.
/// </summary>
public enum ValidationErrorType
{
    /// <summary>
    /// Property type doesn't match schema type (e.g., C# string but schema has int).
    /// </summary>
    TypeMismatch,

    /// <summary>
    /// Array vs non-array mismatch (e.g., C# string but schema has string[]).
    /// </summary>
    ArrayMismatch,

    /// <summary>
    /// Schema defines a property that doesn't exist in the C# type.
    /// May indicate a required property is missing.
    /// </summary>
    RequiredPropertyMissing,

    /// <summary>
    /// C# type uses a type that has no registered converter.
    /// </summary>
    UnsupportedType,

    /// <summary>
    /// Nested object structure doesn't match schema.
    /// </summary>
    NestedObjectMismatch,
}

/// <summary>
/// Represents a non-critical validation warning.
/// Warnings don't prevent validation from passing.
/// </summary>
public class ValidationWarning
{
    /// <summary>
    /// The C# property name that has the validation warning.
    /// </summary>
    public required string PropertyName { get; init; }

    /// <summary>
    /// Human-readable warning message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// The type of validation warning.
    /// </summary>
    public required ValidationWarningType WarningType { get; init; }

    /// <summary>
    /// Additional context information.
    /// </summary>
    public string? AdditionalInfo { get; init; }
}

/// <summary>
/// Categorizes the type of validation warning.
/// </summary>
public enum ValidationWarningType
{
    /// <summary>
    /// C# type has a property not in schema (will be ignored during serialization).
    /// </summary>
    ExtraProperty,

    /// <summary>
    /// Nullable vs non-nullable difference between C# type and schema.
    /// </summary>
    NullabilityDifference,

    /// <summary>
    /// Type difference that might still be compatible (e.g., C# long for Weaviate int).
    /// </summary>
    CompatibleTypeDifference,
}

/// <summary>
/// Exception thrown when type validation fails.
/// Contains the full validation result with detailed error information.
/// </summary>
public class TypeValidationException : Exception
{
    /// <summary>
    /// The validation result containing all errors and warnings.
    /// </summary>
    public ValidationResult ValidationResult { get; }

    /// <summary>
    /// Creates a new TypeValidationException from a validation result.
    /// </summary>
    public TypeValidationException(ValidationResult result)
        : base(result.GetDetailedMessage())
    {
        ValidationResult = result;
    }

    /// <summary>
    /// Creates a new TypeValidationException with a custom message.
    /// </summary>
    public TypeValidationException(string message, ValidationResult result)
        : base(message)
    {
        ValidationResult = result;
    }
}
