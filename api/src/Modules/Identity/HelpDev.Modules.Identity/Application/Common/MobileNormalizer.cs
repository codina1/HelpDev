using System.Text.RegularExpressions;

namespace HelpDev.Modules.Identity.Application.Common;

public static partial class MobileNormalizer
{
    private static readonly Regex IranianMobileRegex = IranianMobilePattern();

    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("98", StringComparison.Ordinal) && digits.Length == 12)
        {
            digits = "0" + digits[2..];
        }

        if (!IranianMobileRegex.IsMatch(digits))
        {
            return false;
        }

        normalized = digits;
        return true;
    }

    [GeneratedRegex(@"^09\d{9}$")]
    private static partial Regex IranianMobilePattern();
}
