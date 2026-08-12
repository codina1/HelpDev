using System.Text.Json;
using HelpDev.Modules.Toolbox.Domain.Tools;

namespace HelpDev.Modules.Toolbox.Application.Execution;

public sealed record ToolExecutionInput(JsonElement Payload);

public sealed record ToolExecutionOutput(JsonElement Payload, bool IsTruncated = false);

public interface IToolExecutor
{
    ToolType Type { get; }

    Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default);
}

public interface IToolExecutorRegistry
{
    IToolExecutor GetRequired(ToolType type);
}

public sealed class ToolboxException : Exception
{
    public ToolboxException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public ToolboxException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class ToolboxApplicationErrorCodes
{
    public const string CategoryNotFound = Domain.ToolboxErrorCodes.CategoryNotFound;
    public const string CategoryNameRequired = Domain.ToolboxErrorCodes.CategoryNameRequired;
    public const string CategoryNameInvalid = Domain.ToolboxErrorCodes.CategoryNameInvalid;
    public const string CategorySlugRequired = Domain.ToolboxErrorCodes.CategorySlugRequired;
    public const string CategorySlugInvalid = Domain.ToolboxErrorCodes.CategorySlugInvalid;
    public const string CategorySlugDuplicate = Domain.ToolboxErrorCodes.CategorySlugDuplicate;
    public const string CategoryInactive = Domain.ToolboxErrorCodes.CategoryInactive;

    public const string ToolNotFound = Domain.ToolboxErrorCodes.ToolNotFound;
    public const string ToolNameRequired = Domain.ToolboxErrorCodes.ToolNameRequired;
    public const string ToolNameInvalid = Domain.ToolboxErrorCodes.ToolNameInvalid;
    public const string ToolSlugRequired = Domain.ToolboxErrorCodes.ToolSlugRequired;
    public const string ToolSlugInvalid = Domain.ToolboxErrorCodes.ToolSlugInvalid;
    public const string ToolSlugDuplicate = Domain.ToolboxErrorCodes.ToolSlugDuplicate;
    public const string ToolSummaryRequired = Domain.ToolboxErrorCodes.ToolSummaryRequired;
    public const string ToolSummaryInvalid = Domain.ToolboxErrorCodes.ToolSummaryInvalid;
    public const string ToolTypeInvalid = Domain.ToolboxErrorCodes.ToolTypeInvalid;
    public const string ToolSchemaInvalid = Domain.ToolboxErrorCodes.ToolSchemaInvalid;
    public const string ToolDisabled = Domain.ToolboxErrorCodes.ToolDisabled;
    public const string ToolUnpublished = Domain.ToolboxErrorCodes.ToolUnpublished;
    public const string ToolCategoryInvalid = Domain.ToolboxErrorCodes.ToolCategoryInvalid;
    public const string ToolCannotPublish = Domain.ToolboxErrorCodes.ToolCannotPublish;
    public const string ToolRequiresAuthentication = Domain.ToolboxErrorCodes.ToolRequiresAuthentication;

    public const string ExecutionInputInvalid = Domain.ToolboxErrorCodes.ExecutionInputInvalid;
    public const string ExecutionInputTooLarge = Domain.ToolboxErrorCodes.ExecutionInputTooLarge;
    public const string ExecutionOutputTooLarge = Domain.ToolboxErrorCodes.ExecutionOutputTooLarge;
    public const string ExecutionTypeUnsupported = Domain.ToolboxErrorCodes.ExecutionTypeUnsupported;
    public const string ExecutionFailed = Domain.ToolboxErrorCodes.ExecutionFailed;
    public const string JsonInvalid = Domain.ToolboxErrorCodes.JsonInvalid;
    public const string Base64Invalid = Domain.ToolboxErrorCodes.Base64Invalid;
    public const string Utf8Invalid = Domain.ToolboxErrorCodes.Utf8Invalid;
    public const string UuidCountInvalid = Domain.ToolboxErrorCodes.UuidCountInvalid;
    public const string HashAlgorithmInvalid = Domain.ToolboxErrorCodes.HashAlgorithmInvalid;
    public const string TimestampInvalid = Domain.ToolboxErrorCodes.TimestampInvalid;
    public const string RegexPatternInvalid = Domain.ToolboxErrorCodes.RegexPatternInvalid;
    public const string RegexTimeout = Domain.ToolboxErrorCodes.RegexTimeout;
    public const string RegexOptionsInvalid = Domain.ToolboxErrorCodes.RegexOptionsInvalid;

    public const string FavoriteNotFound = Domain.ToolboxErrorCodes.FavoriteNotFound;
    public const string FavoriteInvalid = Domain.ToolboxErrorCodes.FavoriteInvalid;
    public const string FavoriteRequiresAuthentication = Domain.ToolboxErrorCodes.FavoriteRequiresAuthentication;

    public const string HistoryNotFound = Domain.ToolboxErrorCodes.HistoryNotFound;
    public const string HistoryAccessDenied = Domain.ToolboxErrorCodes.HistoryAccessDenied;
    public const string PaginationInvalid = Domain.ToolboxErrorCodes.PaginationInvalid;
}
