using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Prompts;

public sealed class PromptVersion : Entity<Guid>
{
    private readonly List<PromptVariable> _variables = [];

    private PromptVersion()
    {
    }

    private PromptVersion(Guid id)
        : base(id)
    {
    }

    public Guid PromptDefinitionId { get; private set; }

    public int VersionNumber { get; private set; }

    public string Template { get; private set; } = string.Empty;

    public string? ChangeNotes { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<PromptVariable> Variables => _variables.AsReadOnly();

    public static PromptVersion Create(
        Guid id,
        Guid promptDefinitionId,
        int versionNumber,
        string template,
        string? changeNotes,
        Guid? createdByUserId,
        IReadOnlyList<PromptVariable> variables,
        IReadOnlyList<string> placeholderNames,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Version id must not be empty.", PromptLabErrorCodes.PromptVersionInvalid);
        }

        if (promptDefinitionId == Guid.Empty)
        {
            throw new DomainException("Prompt definition id is required.", PromptLabErrorCodes.PromptVersionInvalid);
        }

        if (versionNumber < 1)
        {
            throw new DomainException("Version number must be >= 1.", PromptLabErrorCodes.PromptVersionInvalid);
        }

        if (string.IsNullOrWhiteSpace(template))
        {
            throw new DomainException("Template is required.", PromptLabErrorCodes.TemplateRequired);
        }

        var normalizedTemplate = template.Trim();
        if (normalizedTemplate.Length > PromptLabLimits.MaxTemplateLength)
        {
            throw new DomainException("Template is too long.", PromptLabErrorCodes.TemplateTooLong);
        }

        var normalizedChangeNotes = NormalizeChangeNotes(changeNotes);
        var variableList = variables ?? throw new DomainException(
            "Variables are required.",
            PromptLabErrorCodes.PromptVersionInvalid);
        var placeholders = placeholderNames ?? throw new DomainException(
            "Placeholder names are required.",
            PromptLabErrorCodes.TemplatePlaceholderInvalid);

        if (variableList.Count > PromptLabLimits.MaxVariablesPerVersion
            || placeholders.Count > PromptLabLimits.MaxVariablesPerVersion)
        {
            throw new DomainException(
                "Too many variables.",
                PromptLabErrorCodes.TemplateTooManyVariables);
        }

        EnsureVariablesBelongToVersion(id, variableList);
        EnsureUniqueVariableNames(variableList);
        EnsurePlaceholderVariableSetEquality(placeholders, variableList);

        var version = new PromptVersion(id)
        {
            PromptDefinitionId = promptDefinitionId,
            VersionNumber = versionNumber,
            Template = normalizedTemplate,
            ChangeNotes = normalizedChangeNotes,
            CreatedByUserId = createdByUserId == Guid.Empty ? null : createdByUserId,
            CreatedAtUtc = createdAtUtc,
        };

        version._variables.AddRange(variableList);
        return version;
    }

    private static string? NormalizeChangeNotes(string? changeNotes)
    {
        if (changeNotes is null)
        {
            return null;
        }

        var trimmed = changeNotes.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > PromptLabLimits.MaxChangeNotesLength)
        {
            throw new DomainException("Change notes are invalid.", PromptLabErrorCodes.PromptVersionInvalid);
        }

        return trimmed;
    }

    private static void EnsureVariablesBelongToVersion(Guid versionId, IReadOnlyList<PromptVariable> variables)
    {
        foreach (var variable in variables)
        {
            if (variable.PromptVersionId != versionId)
            {
                throw new DomainException(
                    "Variable does not belong to this version.",
                    PromptLabErrorCodes.PromptVersionInvalid);
            }
        }
    }

    private static void EnsureUniqueVariableNames(IReadOnlyList<PromptVariable> variables)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var variable in variables)
        {
            if (!seen.Add(variable.Name))
            {
                throw new DomainException(
                    "Variable names must be unique.",
                    PromptLabErrorCodes.VariableNameDuplicate);
            }
        }
    }

    private static void EnsurePlaceholderVariableSetEquality(
        IReadOnlyList<string> placeholderNames,
        IReadOnlyList<PromptVariable> variables)
    {
        var placeholderSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var placeholder in placeholderNames)
        {
            if (string.IsNullOrWhiteSpace(placeholder))
            {
                throw new DomainException(
                    "Placeholder name is invalid.",
                    PromptLabErrorCodes.TemplatePlaceholderInvalid);
            }

            if (!placeholderSet.Add(placeholder.Trim()))
            {
                throw new DomainException(
                    "Duplicate placeholder names are not allowed.",
                    PromptLabErrorCodes.TemplatePlaceholderDuplicate);
            }
        }

        var variableSet = new HashSet<string>(
            variables.Select(v => v.Name),
            StringComparer.OrdinalIgnoreCase);

        foreach (var placeholder in placeholderSet)
        {
            if (!variableSet.Contains(placeholder))
            {
                throw new DomainException(
                    "Template contains an unknown placeholder.",
                    PromptLabErrorCodes.TemplateUnknownPlaceholder);
            }
        }

        foreach (var variableName in variableSet)
        {
            if (!placeholderSet.Contains(variableName))
            {
                throw new DomainException(
                    "Variable is unused by the template.",
                    PromptLabErrorCodes.TemplateUnusedVariable);
            }
        }
    }
}
