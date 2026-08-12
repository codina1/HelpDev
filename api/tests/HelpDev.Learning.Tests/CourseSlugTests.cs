using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Tests;

public sealed class CourseSlugTests
{
    [Fact]
    public void Create_accepts_valid_slug()
    {
        var slug = CourseSlug.Create("intro-to-csharp");

        Assert.Equal("intro-to-csharp", slug.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_rejects_null_or_whitespace(string? input)
    {
        var created = CourseSlug.TryCreate(input, out var slug);

        Assert.False(created);
        Assert.Null(slug);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("-leading")]
    [InlineData("Trailing-")]
    [InlineData("Upper Case")]
    [InlineData("has_underscore")]
    public void TryCreate_rejects_invalid_patterns(string input)
    {
        Assert.False(CourseSlug.TryCreate(input, out _));
    }

    [Theory]
    [InlineData("  Hello-World  ", "hello-world")]
    [InlineData("React-19", "react-19")]
    [InlineData("dotnet", "dotnet")]
    public void TryCreate_normalizes_trim_and_case(string input, string expected)
    {
        var created = CourseSlug.TryCreate(input, out var slug);

        Assert.True(created);
        Assert.NotNull(slug);
        Assert.Equal(expected, slug!.Value);
    }

    [Fact]
    public void Create_throws_for_invalid_slug()
    {
        Assert.Throws<ArgumentException>(() => CourseSlug.Create(" "));
    }

    [Fact]
    public void Equal_slugs_are_equal()
    {
        var left = CourseSlug.Create("same-slug");
        var right = CourseSlug.Create("same-slug");

        Assert.Equal(left, right);
        Assert.True(left == right);
    }
}
