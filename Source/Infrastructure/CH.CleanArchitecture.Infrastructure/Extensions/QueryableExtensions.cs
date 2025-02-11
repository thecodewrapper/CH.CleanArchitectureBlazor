using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using CH.CleanArchitecture.Common;
using Microsoft.EntityFrameworkCore;

namespace CH.CleanArchitecture.Infrastructure.Extensions
{
    internal static class QueryableExtensions
    {
        internal static IQueryable<T> ApplyQueryOptions<T>(this IQueryable<T> query, QueryOptions options, Expression<Func<T, string>> propertySelector = null) {
            if (options == null)
                return query;

            if (!string.IsNullOrWhiteSpace(options.SearchTerm) && propertySelector != null) {
                var propertyName = GetPropertyName(propertySelector);

                query = query.Where(e =>
                    EF.Functions.Like(EF.Property<string>(e, propertyName).ToLower(), $"%{options.SearchTerm.ToLower()}%"));
            }

            if (!string.IsNullOrWhiteSpace(options?.OrderBy)) {
                query = query.OrderBy(options.OrderBy);
            }

            if (options?.Skip != null && options?.PageSize != null) {
                query = query.Skip(options.Skip);
                query = query.Take(options.PageSize);
            }

            return query;
        }

        private static string GetPropertyName<T>(Expression<Func<T, string>> propertySelector) {
            if (propertySelector.Body is MemberExpression member) {
                return member.Member.Name;
            }

            throw new ArgumentException("Invalid property selector");
        }
    }
}
