using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Learning.Tests;

public sealed class ProgressPercentageTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void From_accepts_values_from_zero_to_one_hundred(int value)
    {
        var percentage = ProgressPercentage.From(value);

        Assert.Equal(value, percentage.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void From_rejects_values_outside_zero_to_one_hundred(int value)
    {
        Assert.Throws<DomainException>(() => ProgressPercentage.From(value));
    }

    [Fact]
    public void Equal_percentages_are_equal()
    {
        var left = ProgressPercentage.From(25);
        var right = ProgressPercentage.From(25);

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.NotEqual(ProgressPercentage.From(25), ProgressPercentage.From(50));
    }
}
