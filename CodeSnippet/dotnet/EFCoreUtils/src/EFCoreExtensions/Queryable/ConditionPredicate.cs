using System;
using System.Linq;
using System.Linq.Expressions;

namespace EFCoreExtensions.Queryable
{
    public static class EFCoreQueryableExtensions
    {
        /// <summary>
        /// Decide whether to use the predicate based on the condition.
        /// </summary>
        /// <example>
        /// Useful in multi condition query case:
        /// <code>
        /// DbSet.AsNoTracking()
        ///     .ConditionWhere(e => e.Tag == request.Tag, string.IsNullOrWhiteSpace(request.Tag) == false)
        ///     .ConditionWhere(e => EF.Functions.Like(e.Name, $"%{request.Name}%"), string.IsNullOrWhiteSpace(request.Name) == false)
        ///     .OrderByDescending(e => e.Id)
        ///     .Select(e => new 
        ///     {
        ///         Id=e.Id,
        ///         Name=e.Name
        ///     }).
        ///     ToListAsync();
        /// </code>
        /// </example>
        public static IQueryable<T> ConditionWhere<T>(this IQueryable<T> rawExpression, Expression<Func<T, bool>> predicate, bool condition)
        {
            if (condition)
            {
                return rawExpression.Where(predicate);
            }
            return rawExpression;
        }
    }
}
