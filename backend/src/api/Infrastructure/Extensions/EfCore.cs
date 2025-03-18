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

    public static IQueryable<T> Page<T>(this IQueryable<T> entity, int pageNumber, int pageSize)
        => entity.Skip((pageNumber - 1) * pageSize).Take(pageSize);


    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> query,
        string? filterValue,
        Expression<Func<T, string?>> propertySelector)
    {
        if (string.IsNullOrWhiteSpace(filterValue)) return query;

        ParameterExpression parameter = propertySelector.Parameters[0];
        Expression property = propertySelector.Body;

        MethodInfo? likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            [
                typeof(DbFunctions),
                typeof(string),
                typeof(string)
            ]
        );

        Expression<Func<T, bool>> predicate;

        if (likeMethod != null)
        {
            MethodCallExpression likeCall = Expression.Call(
                likeMethod,
                Expression.Constant(EF.Functions),
                property,
                Expression.Constant($"%{filterValue}%")
            );

            predicate = Expression.Lambda<Func<T, bool>>(likeCall, parameter);
        }
        else
        {
            MethodInfo? toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
            MethodInfo? containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });

            MethodCallExpression lowerProperty = Expression.Call(property, toLowerMethod!);
            ConstantExpression lowerFilter = Expression.Constant(filterValue.ToLower());

            MethodCallExpression containsCall = Expression.Call(lowerProperty, containsMethod!, lowerFilter);

            predicate = Expression.Lambda<Func<T, bool>>(containsCall, parameter);
        }

        return query.Where(predicate);
    }
}