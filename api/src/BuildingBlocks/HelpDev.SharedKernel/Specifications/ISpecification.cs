using System.Linq.Expressions;

namespace HelpDev.SharedKernel.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }

    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    Expression<Func<T, object>>? OrderBy { get; }

    Expression<Func<T, object>>? OrderByDescending { get; }

    int? Take { get; }

    int? Skip { get; }

    bool AsNoTracking { get; }

    bool IsSatisfiedBy(T entity);
}
