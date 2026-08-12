using System.Linq.Expressions;

namespace HelpDev.SharedKernel.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];

    public Expression<Func<T, bool>>? Criteria { get; private set; }

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    public Expression<Func<T, object>>? OrderBy { get; private set; }

    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    public int? Take { get; private set; }

    public int? Skip { get; private set; }

    public bool AsNoTracking { get; private set; } = true;

    protected void Where(Expression<Func<T, bool>> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        Criteria = Criteria is null ? criteria : Criteria.And(criteria);
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        ArgumentNullException.ThrowIfNull(includeExpression);
        _includes.Add(includeExpression);
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        ArgumentNullException.ThrowIfNull(orderByExpression);
        OrderBy = orderByExpression;
        OrderByDescending = null;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        ArgumentNullException.ThrowIfNull(orderByDescendingExpression);
        OrderByDescending = orderByDescendingExpression;
        OrderBy = null;
    }

    protected void ApplyPaging(int skip, int take)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        Skip = skip;
        Take = take;
    }

    protected void EnableTracking() => AsNoTracking = false;

    public virtual bool IsSatisfiedBy(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (Criteria is null)
        {
            return true;
        }

        return Criteria.Compile().Invoke(entity);
    }

    public Specification<T> And(Specification<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new CompositeSpecification<T>(this, other, CompositeMode.And);
    }

    public Specification<T> Or(Specification<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new CompositeSpecification<T>(this, other, CompositeMode.Or);
    }
}
