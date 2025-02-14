namespace Infrastructure.Extensions;

public static class EfCore
{
    public static void FilterSoftDeletedProperties(this ModelBuilder modelBuilder)
    {
        Expression<Func<BaseEntity, bool>> filterExpr = e => !e.IsDeleted;
        foreach (IMutableEntityType mutableEntityType in modelBuilder.Model.GetEntityTypes()
                     .Where(m => m.ClrType.IsAssignableTo(typeof(BaseEntity))))
        {
            ParameterExpression parameter = Expression.Parameter(mutableEntityType.ClrType);
            Expression body = ReplacingExpressionVisitor
                .Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
            LambdaExpression lambdaExpression = Expression.Lambda(body, parameter);

            mutableEntityType.SetQueryFilter(lambdaExpression);
        }
    }

    public static IEnumerable<T> Page<T>(this IEnumerable<T> entity, int pageNumber, int pageSize)
        => entity.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
}