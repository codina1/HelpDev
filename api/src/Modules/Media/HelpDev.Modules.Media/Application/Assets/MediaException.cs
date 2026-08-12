namespace HelpDev.Modules.Media.Application.Assets;

public sealed class MediaException : Exception
{
    public MediaException(string message)
        : this(message, MediaErrorCodes.Validation)
    {
    }

    public MediaException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public MediaException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class MediaErrorCodes
{
    public const string NotFound = "media_asset_not_found";
    public const string Validation = "media_validation_failed";
    public const string UnsupportedType = "media_unsupported_type";
    public const string PayloadTooLarge = "media_payload_too_large";
    public const string StorageFailed = "media_storage_failed";
}
