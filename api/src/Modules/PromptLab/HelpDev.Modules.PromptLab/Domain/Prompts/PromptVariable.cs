using System.Globalization;
using System.Text.RegularExpressions;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Prompts;

public sealed class PromptVariable : Entity<Guid>
{
    private static readonly Regex NamePattern = new(
        "^[A-Za-z][A-Za-z0-9_]*$",
        RegexOptions.CultureInvariant);

    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "system",
        "assistant",
        "user",
        "developer",
        "prompt",
        "template",
        "secrets",
        "apikey",
        "accesstoken",
    };

    private readonly List<string> _allowedValues = [];

    private PromptVariable()
    {
    }

    private PromptVariable(Guid id)
        : base(id)
    {
    }

    public Guid PromptVersionId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName => Name.ToLowerInvariant();

    public string Label { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public PromptVariableType Type { get; private set; }

    public bool IsRequired { get; private set; }

    public string? DefaultValue { get; private set; }

    public int? MinLength { get; private set; }

    public int? MaxLength { get; private set; }

    public decimal? MinValue { get; private set; }

    public decimal? MaxValue { get; private set; }

    public string? ValidationPattern { get; private set; }

    public int DisplayOrder { get; private set; }

    public IReadOnlyList<string> AllowedValues => _allowedValues.AsReadOnly();

    public static PromptVariable Create(
        Guid id,
        Guid promptVersionId,
        string name,
        string label,
        string? description,
        PromptVariableType type,
        bool isRequired,
        string? defaultValue,
        int? minLength,
        int? maxLength,
        decimal? minValue,
        decimal? maxValue,
        string? validationPattern,
        IReadOnlyList<string>? allowedValues,
        int displayOrder)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Variable id must not be empty.", PromptLabErrorCodes.VariableNameInvalid);
        }

        if (promptVersionId == Guid.Empty)
        {
            throw new DomainException("Prompt version id is required.", PromptLabErrorCodes.PromptVersionInvalid);
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException("Variable type is invalid.", PromptLabErrorCodes.VariableTypeInvalid);
        }

        if (displayOrder < 0)
        {
            throw new DomainException("Display order must be >= 0.", PromptLabErrorCodes.VariableConstraintsInvalid);
        }

        var normalizedName = NormalizeName(name);
        var normalizedLabel = NormalizeRequired(
            label,
            PromptLabLimits.MaxVariableLabelLength,
            PromptLabErrorCodes.VariableConstraintsInvalid,
            PromptLabErrorCodes.VariableConstraintsInvalid);
        var normalizedDescription = NormalizeOptional(
            description,
            PromptLabLimits.MaxVariableDescriptionLength,
            PromptLabErrorCodes.VariableConstraintsInvalid);

        ValidateConstraintsForType(type, minLength, maxLength, minValue, maxValue, validationPattern, allowedValues);

        var normalizedOptions = NormalizeAllowedValues(type, allowedValues);
        var normalizedPattern = NormalizeValidationPattern(type, validationPattern);
        var normalizedDefault = NormalizeDefaultValue(
            type,
            defaultValue,
            minLength,
            maxLength,
            minValue,
            maxValue,
            normalizedPattern,
            normalizedOptions);

        var variable = new PromptVariable(id)
        {
            PromptVersionId = promptVersionId,
            Name = normalizedName,
            Label = normalizedLabel,
            Description = normalizedDescription,
            Type = type,
            IsRequired = isRequired,
            DefaultValue = normalizedDefault,
            MinLength = minLength,
            MaxLength = maxLength,
            MinValue = minValue,
            MaxValue = maxValue,
            ValidationPattern = normalizedPattern,
            DisplayOrder = displayOrder,
        };

        if (normalizedOptions.Count > 0)
        {
            variable._allowedValues.AddRange(normalizedOptions);
        }

        return variable;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Variable name is required.", PromptLabErrorCodes.VariableNameRequired);
        }

        var trimmed = name.Trim();
        if (trimmed.Length > PromptLabLimits.MaxVariableNameLength || !NamePattern.IsMatch(trimmed))
        {
            throw new DomainException("Variable name is invalid.", PromptLabErrorCodes.VariableNameInvalid);
        }

        if (ReservedNames.Contains(trimmed))
        {
            throw new DomainException("Variable name is reserved.", PromptLabErrorCodes.VariableNameReserved);
        }

        return trimmed;
    }

    private static void ValidateConstraintsForType(
        PromptVariableType type,
        int? minLength,
        int? maxLength,
        decimal? minValue,
        decimal? maxValue,
        string? validationPattern,
        IReadOnlyList<string>? allowedValues)
    {
        switch (type)
        {
            case PromptVariableType.Text:
            case PromptVariableType.MultilineText:
                if (minValue is not null || maxValue is not null)
                {
                    throw new DomainException(
                        "Numeric constraints are not allowed for text variables.",
                        PromptLabErrorCodes.VariableConstraintsInvalid);
                }

                if (allowedValues is { Count: > 0 })
                {
                    throw new DomainException(
                        "Allowed values are not valid for this variable type.",
                        PromptLabErrorCodes.VariableOptionsInvalid);
                }

                ValidateLengthConstraints(minLength, maxLength);
                break;

            case PromptVariableType.Integer:
            case PromptVariableType.Decimal:
                if (minLength is not null || maxLength is not null)
                {
                    throw new DomainException(
                        "Length constraints are not allowed for numeric variables.",
                        PromptLabErrorCodes.VariableConstraintsInvalid);
                }

                if (!string.IsNullOrWhiteSpace(validationPattern))
                {
                    throw new DomainException(
                        "Validation pattern is not allowed for numeric variables.",
                        PromptLabErrorCodes.VariablePatternInvalid);
                }

                if (allowedValues is { Count: > 0 })
                {
                    throw new DomainException(
                        "Allowed values are not valid for this variable type.",
                        PromptLabErrorCodes.VariableOptionsInvalid);
                }

                if (minValue is not null && maxValue is not null && minValue > maxValue)
                {
                    throw new DomainException(
                        "Min value must be less than or equal to max value.",
                        PromptLabErrorCodes.VariableConstraintsInvalid);
                }

                break;

            case PromptVariableType.Boolean:
                if (minLength is not null || maxLength is not null || minValue is not null || maxValue is not null)
                {
                    throw new DomainException(
                        "Constraints are not allowed for boolean variables.",
                        PromptLabErrorCodes.VariableConstraintsInvalid);
                }

                if (!string.IsNullOrWhiteSpace(validationPattern))
                {
                    throw new DomainException(
                        "Validation pattern is not allowed for boolean variables.",
                        PromptLabErrorCodes.VariablePatternInvalid);
                }

                if (allowedValues is { Count: > 0 })
                {
                    throw new DomainException(
                        "Allowed values are not valid for this variable type.",
                        PromptLabErrorCodes.VariableOptionsInvalid);
                }

                break;

            case PromptVariableType.Select:
                if (minLength is not null || maxLength is not null || minValue is not null || maxValue is not null)
                {
                    throw new DomainException(
                        "Constraints are not allowed for select variables.",
                        PromptLabErrorCodes.VariableConstraintsInvalid);
                }

                if (!string.IsNullOrWhiteSpace(validationPattern))
                {
                    throw new DomainException(
                        "Validation pattern is not allowed for select variables.",
                        PromptLabErrorCodes.VariablePatternInvalid);
                }

                if (allowedValues is null || allowedValues.Count == 0)
                {
                    throw new DomainException(
                        "Select variables require at least one option.",
                        PromptLabErrorCodes.VariableOptionsInvalid);
                }

                break;

            default:
                throw new DomainException("Variable type is invalid.", PromptLabErrorCodes.VariableTypeInvalid);
        }
    }

    private static void ValidateLengthConstraints(int? minLength, int? maxLength)
    {
        if (minLength is < 0)
        {
            throw new DomainException("Min length must be >= 0.", PromptLabErrorCodes.VariableConstraintsInvalid);
        }

        if (maxLength is < 0)
        {
            throw new DomainException("Max length must be >= 0.", PromptLabErrorCodes.VariableConstraintsInvalid);
        }

        if (maxLength is > PromptLabLimits.MaxVariableValueLength)
        {
            throw new DomainException(
                "Max length exceeds absolute variable value limit.",
                PromptLabErrorCodes.VariableConstraintsInvalid);
        }

        if (minLength is not null && maxLength is not null && minLength > maxLength)
        {
            throw new DomainException(
                "Min length must be less than or equal to max length.",
                PromptLabErrorCodes.VariableConstraintsInvalid);
        }
    }

    private static IReadOnlyList<string> NormalizeAllowedValues(
        PromptVariableType type,
        IReadOnlyList<string>? allowedValues)
    {
        if (type != PromptVariableType.Select)
        {
            return Array.Empty<string>();
        }

        if (allowedValues is null || allowedValues.Count == 0)
        {
            throw new DomainException(
                "Select variables require at least one option.",
                PromptLabErrorCodes.VariableOptionsInvalid);
        }

        if (allowedValues.Count > PromptLabLimits.MaxSelectOptions)
        {
            throw new DomainException(
                "Too many select options.",
                PromptLabErrorCodes.VariableOptionsInvalid);
        }

        var normalized = new List<string>(allowedValues.Count);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var option in allowedValues)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                throw new DomainException("Select option is invalid.", PromptLabErrorCodes.VariableOptionsInvalid);
            }

            var trimmed = option.Trim();
            if (trimmed.Length > PromptLabLimits.MaxSelectOptionLength)
            {
                throw new DomainException("Select option is too long.", PromptLabErrorCodes.VariableOptionsInvalid);
            }

            if (!seen.Add(trimmed))
            {
                throw new DomainException("Select options must be unique.", PromptLabErrorCodes.VariableOptionsInvalid);
            }

            normalized.Add(trimmed);
        }

        return normalized;
    }

    private static string? NormalizeValidationPattern(PromptVariableType type, string? validationPattern)
    {
        if (validationPattern is null)
        {
            return null;
        }

        var trimmed = validationPattern.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (type is not PromptVariableType.Text and not PromptVariableType.MultilineText)
        {
            throw new DomainException(
                "Validation pattern is only allowed for text variables.",
                PromptLabErrorCodes.VariablePatternInvalid);
        }

        if (trimmed.Length > PromptLabLimits.MaxValidationPatternLength)
        {
            throw new DomainException("Validation pattern is too long.", PromptLabErrorCodes.VariablePatternInvalid);
        }

        try
        {
            _ = new Regex(
                trimmed,
                RegexOptions.CultureInvariant | RegexOptions.None,
                TimeSpan.FromMilliseconds(PromptLabLimits.ValidationRegexTimeoutMs));
        }
        catch (ArgumentException)
        {
            throw new DomainException("Validation pattern is invalid.", PromptLabErrorCodes.VariablePatternInvalid);
        }
        catch (RegexMatchTimeoutException)
        {
            throw new DomainException("Validation pattern is invalid.", PromptLabErrorCodes.VariablePatternInvalid);
        }

        return trimmed;
    }

    private static string? NormalizeDefaultValue(
        PromptVariableType type,
        string? defaultValue,
        int? minLength,
        int? maxLength,
        decimal? minValue,
        decimal? maxValue,
        string? validationPattern,
        IReadOnlyList<string> allowedValues)
    {
        if (defaultValue is null)
        {
            return null;
        }

        var trimmed = defaultValue.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > PromptLabLimits.MaxVariableValueLength)
        {
            throw new DomainException("Default value is too long.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        return type switch
        {
            PromptVariableType.Text or PromptVariableType.MultilineText =>
                NormalizeTextDefault(trimmed, minLength, maxLength, validationPattern),
            PromptVariableType.Integer => NormalizeIntegerDefault(trimmed, minValue, maxValue),
            PromptVariableType.Decimal => NormalizeDecimalDefault(trimmed, minValue, maxValue),
            PromptVariableType.Boolean => NormalizeBooleanDefault(trimmed),
            PromptVariableType.Select => NormalizeSelectDefault(trimmed, allowedValues),
            _ => throw new DomainException("Variable type is invalid.", PromptLabErrorCodes.VariableTypeInvalid),
        };
    }

    private static string NormalizeTextDefault(
        string value,
        int? minLength,
        int? maxLength,
        string? validationPattern)
    {
        if (minLength is not null && value.Length < minLength)
        {
            throw new DomainException("Default value is shorter than min length.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        if (maxLength is not null && value.Length > maxLength)
        {
            throw new DomainException("Default value exceeds max length.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        if (validationPattern is not null)
        {
            try
            {
                if (!Regex.IsMatch(
                        value,
                        validationPattern,
                        RegexOptions.CultureInvariant,
                        TimeSpan.FromMilliseconds(PromptLabLimits.ValidationRegexTimeoutMs)))
                {
                    throw new DomainException(
                        "Default value does not match validation pattern.",
                        PromptLabErrorCodes.VariableDefaultInvalid);
                }
            }
            catch (RegexMatchTimeoutException)
            {
                throw new DomainException(
                    "Default value pattern validation timed out.",
                    PromptLabErrorCodes.VariableDefaultInvalid);
            }
        }

        return value;
    }

    private static string NormalizeIntegerDefault(string value, decimal? minValue, decimal? maxValue)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new DomainException("Default integer value is invalid.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        if (minValue is not null && parsed < minValue)
        {
            throw new DomainException("Default value is below min value.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        if (maxValue is not null && parsed > maxValue)
        {
            throw new DomainException("Default value is above max value.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        return parsed.ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeDecimalDefault(string value, decimal? minValue, decimal? maxValue)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new DomainException("Default decimal value is invalid.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        if (minValue is not null && parsed < minValue)
        {
            throw new DomainException("Default value is below min value.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        if (maxValue is not null && parsed > maxValue)
        {
            throw new DomainException("Default value is above max value.", PromptLabErrorCodes.VariableDefaultInvalid);
        }

        return parsed.ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeBooleanDefault(string value)
    {
        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "1", StringComparison.Ordinal)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase))
        {
            return "true";
        }

        if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "0", StringComparison.Ordinal)
            || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase))
        {
            return "false";
        }

        throw new DomainException("Default boolean value is invalid.", PromptLabErrorCodes.VariableDefaultInvalid);
    }

    private static string NormalizeSelectDefault(string value, IReadOnlyList<string> allowedValues)
    {
        foreach (var option in allowedValues)
        {
            if (string.Equals(option, value, StringComparison.Ordinal))
            {
                return option;
            }
        }

        throw new DomainException(
            "Default value must be one of the allowed select options.",
            PromptLabErrorCodes.VariableDefaultInvalid);
    }

    private static string NormalizeRequired(string value, int maxLength, string requiredCode, string invalidCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value is required.", requiredCode);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException("Value is invalid.", invalidCode);
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string invalidCode)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException("Value is invalid.", invalidCode);
        }

        return trimmed;
    }
}
