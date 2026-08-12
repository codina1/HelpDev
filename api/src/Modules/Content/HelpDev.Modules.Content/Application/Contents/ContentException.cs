namespace HelpDev.Modules.Content.Application.Contents;

public sealed class ContentException : Exception
{
    public ContentException(string message)
        : this(message, ContentErrorCodes.Validation)
    {
    }

    public ContentException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public ContentException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class ContentErrorCodes
{
    public const string NotFound = "content_not_found";
    public const string SlugDuplicate = "content_slug_duplicate";
    public const string OperationInvalid = "content_invalid_operation";
    public const string Validation = "content_validation_failed";
}
