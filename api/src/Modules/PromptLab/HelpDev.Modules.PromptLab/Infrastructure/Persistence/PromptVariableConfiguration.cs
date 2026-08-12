using System.Text.Json;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptVariableConfiguration : IEntityTypeConfiguration<PromptVariable>
{
    private const int AllowedValuesJsonMaxLength = 8000;

    public void Configure(EntityTypeBuilder<PromptVariable> builder)
    {
        builder.ToTable("promptlab_variables");

        builder.HasKey(variable => variable.Id);

        builder.Property(variable => variable.Id)
            .ValueGeneratedNever();

        builder.Property(variable => variable.PromptVersionId)
            .IsRequired()
            .HasColumnName("prompt_version_id");

        builder.Property(variable => variable.Name)
            .IsRequired()
            .HasMaxLength(PromptLabLimits.MaxVariableNameLength)
            .HasColumnName("name");

        builder.Ignore(variable => variable.NormalizedName);
        builder.Ignore(variable => variable.AllowedValues);

        builder.Property(variable => variable.Label)
            .IsRequired()
            .HasMaxLength(PromptLabLimits.MaxVariableLabelLength)
            .HasColumnName("label");

        builder.Property(variable => variable.Description)
            .HasMaxLength(PromptLabLimits.MaxVariableDescriptionLength)
            .HasColumnName("description");

        builder.Property(variable => variable.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasColumnName("type");

        builder.Property(variable => variable.IsRequired)
            .IsRequired()
            .HasColumnName("is_required");

        builder.Property(variable => variable.DefaultValue)
            .HasMaxLength(PromptLabLimits.MaxVariableValueLength)
            .HasColumnName("default_value");

        builder.Property(variable => variable.MinLength)
            .HasColumnName("min_length");

        builder.Property(variable => variable.MaxLength)
            .HasColumnName("max_length");

        builder.Property(variable => variable.MinValue)
            .HasColumnName("min_value")
            .HasPrecision(18, 6);

        builder.Property(variable => variable.MaxValue)
            .HasColumnName("max_value")
            .HasPrecision(18, 6);

        builder.Property(variable => variable.ValidationPattern)
            .HasMaxLength(PromptLabLimits.MaxValidationPatternLength)
            .HasColumnName("validation_pattern");

        builder.Property(variable => variable.DisplayOrder)
            .IsRequired()
            .HasColumnName("display_order");

        var allowedValuesConverter = new ValueConverter<List<string>, string>(
            values => SerializeAllowedValues(values),
            json => DeserializeAllowedValues(json));

        var allowedValuesComparer = new ValueComparer<List<string>>(
            (left, right) =>
                (left == null && right == null)
                || (left != null && right != null && left.SequenceEqual(right)),
            values => values.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode(StringComparison.Ordinal))),
            values => values.ToList());

        builder.Property<List<string>>("_allowedValues")
            .HasField("_allowedValues")
            .HasConversion(allowedValuesConverter, allowedValuesComparer)
            .IsRequired()
            .HasMaxLength(AllowedValuesJsonMaxLength)
            .HasColumnName("allowed_values_json");

        builder.HasIndex(variable => new { variable.PromptVersionId, variable.Name })
            .IsUnique()
            .HasDatabaseName("ux_promptlab_variables_prompt_version_id_name");

        builder.HasIndex(variable => variable.PromptVersionId)
            .HasDatabaseName("ix_promptlab_variables_prompt_version_id");

        builder.HasIndex(variable => variable.DisplayOrder)
            .HasDatabaseName("ix_promptlab_variables_display_order");
    }

    private static string SerializeAllowedValues(List<string> values) =>
        JsonSerializer.Serialize(values);

    private static List<string> DeserializeAllowedValues(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }
}
