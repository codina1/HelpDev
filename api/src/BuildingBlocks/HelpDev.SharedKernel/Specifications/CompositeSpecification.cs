using System.Linq.Expressions;

namespace HelpDev.SharedKernel.Specifications;

internal enum CompositeMode
{
    And,
    Or,
}

internal sealed class CompositeSpecification<T> : Specification<T>
{
    public CompositeSpecification(Specification<T> left, Specification<T> right, CompositeMode mode)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (left.Criteria is null && right.Criteria is null)
        {
            return;
        }

        if (left.Criteria is null)
        {
            Where(right.Criteria!);
            return;
        }

        if (right.Criteria is null)
        {
            Where(left.Criteria);
            return;
        }

        Where(mode == CompositeMode.And
            ? left.Criteria.And(right.Criteria)
            : left.Criteria.Or(right.Criteria));
    }
}
