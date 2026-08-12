using HelpDev.SharedKernel.Ids;

namespace HelpDev.SharedKernel.Tests;

public sealed class StronglyTypedIdTests
{
    [Fact]
    public void Ids_with_same_type_and_value_are_equal()
    {
        var value = Guid.NewGuid();
        var left = new SampleId(value);
        var right = new SampleId(value);

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.False(left != right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Ids_with_different_values_are_not_equal()
    {
        var left = new SampleId(Guid.NewGuid());
        var right = new SampleId(Guid.NewGuid());

        Assert.NotEqual(left, right);
        Assert.True(left != right);
    }

    [Fact]
    public void Different_id_types_with_same_value_are_not_equal()
    {
        var value = Guid.NewGuid();
        var left = new SampleId(value);
        var right = new OtherId(value);

        Assert.False(left.Equals(right));
        Assert.NotEqual(left.GetHashCode(), right.GetHashCode());
    }

    private sealed class SampleId : StronglyTypedId<Guid>
    {
        public SampleId(Guid value)
            : base(value)
        {
        }
    }

    private sealed class OtherId : StronglyTypedId<Guid>
    {
        public OtherId(Guid value)
            : base(value)
        {
        }
    }
}
