using HelpDev.SharedKernel.Guards;

namespace HelpDev.SharedKernel.Tests;

public sealed class GuardTests
{
    [Fact]
    public void AgainstNull_returns_value_when_not_null()
    {
        var value = Guard.AgainstNull("helpdev", "name");

        Assert.Equal("helpdev", value);
    }

    [Fact]
    public void AgainstNull_throws_when_null()
    {
        Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull<string>(null, "name"));
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_rejects_blank_values()
    {
        Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrWhiteSpace("  ", "name"));
        Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrWhiteSpace(null, "name"));
    }

    [Fact]
    public void AgainstEmpty_rejects_empty_guid()
    {
        Assert.Throws<ArgumentException>(() => Guard.AgainstEmpty(Guid.Empty, "id"));
    }

    [Fact]
    public void AgainstNegative_rejects_negative_values()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstNegative(-1, "count"));
        Assert.Equal(0, Guard.AgainstNegative(0, "count"));
    }

    [Fact]
    public void AgainstOutOfRange_rejects_values_outside_bounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstOutOfRange(11, 1, 10, "score"));
        Assert.Equal(5, Guard.AgainstOutOfRange(5, 1, 10, "score"));
    }
}
