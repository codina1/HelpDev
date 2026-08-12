using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Execution;

namespace HelpDev.Toolbox.Tests;

internal static class ToolExecutionTestHelpers
{
    public static ToolExecutionInput ParseInput(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new ToolExecutionInput(document.RootElement.Clone());
    }

    public static string GetString(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetString()!;

    public static bool GetBool(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetBoolean();

    public static int GetInt(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetInt32();
}
