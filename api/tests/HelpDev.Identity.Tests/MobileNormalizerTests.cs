using HelpDev.Modules.Identity.Application.Common;

namespace HelpDev.Identity.Tests;

public sealed class MobileNormalizerTests
{
    [Theory]
    [InlineData("09123456789", "09123456789")]
    [InlineData(" 09123456789 ", "09123456789")]
    [InlineData("0912-345-6789", "09123456789")]
    [InlineData("0912 345 6789", "09123456789")]
    public void TryNormalize_accepts_local_iranian_format(string input, string expected)
    {
        var ok = MobileNormalizer.TryNormalize(input, out var normalized);

        Assert.True(ok);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("+989123456789", "09123456789")]
    [InlineData("989123456789", "09123456789")]
    public void TryNormalize_accepts_98_country_code_forms(string input, string expected)
    {
        var ok = MobileNormalizer.TryNormalize(input, out var normalized);

        Assert.True(ok);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("00989123456789")]
    [InlineData("9123456789")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("08123456789")]
    [InlineData("0912345678")]
    [InlineData("091234567890")]
    public void TryNormalize_rejects_unsupported_values(string? input)
    {
        var ok = MobileNormalizer.TryNormalize(input, out var normalized);

        Assert.False(ok);
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void TryNormalize_is_deterministic_for_same_input()
    {
        const string input = "+98 912 345 6789";

        var firstOk = MobileNormalizer.TryNormalize(input, out var first);
        var secondOk = MobileNormalizer.TryNormalize(input, out var second);

        Assert.True(firstOk);
        Assert.True(secondOk);
        Assert.Equal(first, second);
        Assert.Equal("09123456789", first);
    }
}
