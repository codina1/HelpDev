namespace HelpDev.SharedKernel.Results;

/// <summary>
/// Common application/domain error catalog. Additive factories only.
/// </summary>
public static class CommonErrors
{
    public static Error NotFound(string entityName, object? id = null) =>
        new(
            "common.not_found",
            id is null
                ? $"{entityName} was not found."
                : $"{entityName} '{id}' was not found.");

    public static Error Validation(string message) =>
        new("common.validation", message);

    public static Error Validation(string code, string message) =>
        new(code, message);

    public static Error Conflict(string message) =>
        new("common.conflict", message);

    public static Error Unauthorized(string message = "Unauthorized.") =>
        new("common.unauthorized", message);

    public static Error Forbidden(string message = "Forbidden.") =>
        new("common.forbidden", message);

    public static Error Unexpected(string message = "An unexpected error occurred.") =>
        new("common.unexpected", message);
}
