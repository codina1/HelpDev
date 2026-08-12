using System.Text.RegularExpressions;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Analytics.Application.Processing;

public static class AnalyticsEventValidator
{
  private static readonly Regex DimensionKeyPattern = new("^[a-z][a-z0-9_]*$", RegexOptions.CultureInvariant);

  public static void Validate(AnalyticsEventEnvelope envelope)
  {
    if (envelope.EventId == Guid.Empty)
    {
      throw new AnalyticsException("Event id is required.", AnalyticsApplicationErrorCodes.EventIdRequired);
    }

    if (string.IsNullOrWhiteSpace(envelope.EventType))
    {
      throw new AnalyticsException("Event type is required.", AnalyticsApplicationErrorCodes.EventTypeRequired);
    }

    if (!AnalyticsEventTypes.IsSupported(envelope.EventType))
    {
      throw new AnalyticsException("Event type is not supported.", AnalyticsApplicationErrorCodes.EventTypeUnsupported);
    }

    if (envelope.OccurredAtUtc == default)
    {
      throw new AnalyticsException("Event timestamp is invalid.", AnalyticsApplicationErrorCodes.EventTimestampInvalid);
    }

    if (envelope.OccurredAtUtc.Kind != DateTimeKind.Utc)
    {
      throw new AnalyticsException("Event timestamp must be UTC.", AnalyticsApplicationErrorCodes.EventTimestampInvalid);
    }

    if (envelope.SchemaVersion != 1)
    {
      throw new AnalyticsException(
        "Event schema version is not supported.",
        AnalyticsApplicationErrorCodes.EventSchemaVersionUnsupported);
    }

    if (envelope.Quantity <= 0 || envelope.Quantity > AnalyticsLimits.MaxQuantity)
    {
      throw new AnalyticsException("Event quantity is invalid.", AnalyticsApplicationErrorCodes.EventQuantityInvalid);
    }

    ValidateDimensions(envelope.EventType, envelope.Dimensions);
  }

  private static void ValidateDimensions(string eventType, IReadOnlyDictionary<string, string>? dimensions)
  {
    if (dimensions is null || dimensions.Count == 0)
    {
      var required = AnalyticsDimensionRules.GetAllowedDimensions(eventType);
      if (required.Count > 0 && required.Any(pair => pair.Value))
      {
        throw new AnalyticsException("Event dimensions are invalid.", AnalyticsApplicationErrorCodes.EventDimensionsInvalid);
      }

      return;
    }

    if (dimensions.Count > AnalyticsLimits.MaxDimensions)
    {
      throw new AnalyticsException("Event dimensions are invalid.", AnalyticsApplicationErrorCodes.EventDimensionsInvalid);
    }

    var allowed = AnalyticsDimensionRules.GetAllowedDimensions(eventType);
    foreach (var (key, value) in dimensions)
    {
      if (string.IsNullOrWhiteSpace(key)
          || key.Length > AnalyticsLimits.MaxDimensionKeyLength
          || !DimensionKeyPattern.IsMatch(key))
      {
        throw new AnalyticsException("Event dimensions are invalid.", AnalyticsApplicationErrorCodes.EventDimensionsInvalid);
      }

      if (!allowed.TryGetValue(key, out var isRequired) && !allowed.ContainsKey(key))
      {
        throw new AnalyticsException(
          $"Dimension '{key}' is not allowed for event type '{eventType}'.",
          AnalyticsApplicationErrorCodes.EventDimensionNotAllowed);
      }

      if (string.IsNullOrWhiteSpace(value) || value.Length > AnalyticsLimits.MaxDimensionValueLength)
      {
        throw new AnalyticsException("Event dimensions are invalid.", AnalyticsApplicationErrorCodes.EventDimensionsInvalid);
      }

      if (ContainsSensitiveValue(value))
      {
        throw new AnalyticsException("Event dimensions are invalid.", AnalyticsApplicationErrorCodes.EventDimensionsInvalid);
      }

      _ = isRequired;
    }

    foreach (var requiredEntry in allowed.Where(entry => entry.Value))
    {
      if (!dimensions.ContainsKey(requiredEntry.Key))
      {
        throw new AnalyticsException("Event dimensions are invalid.", AnalyticsApplicationErrorCodes.EventDimensionsInvalid);
      }
    }
  }

  private static bool ContainsSensitiveValue(string value)
  {
    if (value.Contains("http://", StringComparison.OrdinalIgnoreCase)
        || value.Contains("https://", StringComparison.OrdinalIgnoreCase)
        || value.Contains('@'))
    {
      return true;
    }

    return false;
  }
}
