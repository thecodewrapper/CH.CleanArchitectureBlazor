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
        internal static IQueryable<T> ApplyQueryOptions<T>(this IQueryable<T> query, QueryOptions? options, Expression<Func<T, string>>? propertySelector = null, int defaultPageNo = 1, int defaultPageSize = 10, int maxPageSize = 250) {
            if (options is null)
                return query;

            // ---- Search ----
            if (!string.IsNullOrWhiteSpace(options.SearchTerm) && propertySelector != null) {
                var propertyName = GetPropertyName(propertySelector);
                var search = options.SearchTerm.Trim().ToLowerInvariant();

                query = query.Where(e =>
                    EF.Functions.Like(
                        EF.Property<string>(e, propertyName).ToLower(),
                        $"%{search}%"));
            }

            // ---- Ordering ----
            var orderBy = options.OrderBy?.Trim();
            if (!string.IsNullOrWhiteSpace(orderBy)) {
                var dir = options.IsAscending ? "asc" : "desc";
                query = query.OrderBy($"{orderBy} {dir}");
            }

            // ---- Paging ----
            int? take = options.PageSize;
            int? skip = options.Skip;

            int pageSize = ClampPageSize(take ?? defaultPageSize, maxPageSize);
            int pageNo = ClampPageNo(options.PageNo ?? defaultPageNo);

            if (skip is null && (options.PageNo.HasValue || options.PageSize.HasValue)) {
                skip = (pageNo - 1) * pageSize;
            }

            // Apply paging only if we have a skip value or page info was provided
            if (skip.HasValue || options.PageNo.HasValue || options.PageSize.HasValue) {
                query = query.Skip(Math.Max(0, skip ?? 0)).Take(pageSize);
            }

            return query;
        }

        private static int ClampPageNo(int pageNo) => pageNo <= 1 ? 1 : pageNo;

        private static int ClampPageSize(int pageSize, int maxPageSize) {
            if (pageSize <= 0) return 1;
            return pageSize > maxPageSize ? maxPageSize : pageSize;
        }

        private static string GetPropertyName<T>(Expression<Func<T, string>> propertySelector) {
            if (propertySelector.Body is MemberExpression member)
                return member.Member.Name;

            throw new ArgumentException("Invalid property selector");
        }
    }
}
