using HelpDev.SharedKernel.Results;

namespace HelpDev.SharedKernel.Tests;

public sealed class ErrorTests
{
    [Fact]
    public void Errors_with_same_code_and_message_are_equal()
    {
        var left = new Error("common.validation", "Invalid");
        var right = new Error("common.validation", "Invalid");

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.False(left != right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Errors_with_different_code_or_message_are_not_equal()
    {
        var left = new Error("common.validation", "Invalid");
        var differentCode = new Error("common.conflict", "Invalid");
        var differentMessage = new Error("common.validation", "Other");

        Assert.NotEqual(left, differentCode);
        Assert.NotEqual(left, differentMessage);
        Assert.True(left != differentCode);
    }

    [Fact]
    public void None_equals_itself()
    {
        Assert.Equal(Error.None, Error.None);
        Assert.True(Error.None == new Error(string.Empty, string.Empty));
    }
}
