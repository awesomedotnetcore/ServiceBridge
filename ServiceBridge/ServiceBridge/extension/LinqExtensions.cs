using ServiceBridge.core;
using ServiceBridge.helper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBridge.extension
{
    /// <summary>
    /// 对Linq的扩展
    /// </summary>
    public static class LinqExtensions
    {
        public static T FirstOrThrow<T>(this IEnumerable<T> query, string error_msg) =>
            query.AsQueryable().FirstOrThrow(error_msg);

        public static T FirstOrThrow<T>(this IQueryable<T> query, string error_msg)
        {
            var model = query.FirstOrDefault();
            Com.AssertNotNull(model, error_msg);
            return model;
        }

        /// <summary>
        /// 返回非null list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<T> NotNullList<T>(this IQueryable<T> query)
        {
            return ConvertHelper.NotNullList(query.ToList());
        }

        /// <summary>
        /// 如果条件不为空就使用条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereIfNotNull<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
        {
            if (where != null)
            {
                query = query.Where(where);
            }
            return query;
        }

        /// <summary>
        /// take别名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IQueryable<T> Limit<T>(this IQueryable<T> query, int count) => query.Take(count);

        /// <summary>
        /// 动态生成or条件
        /// http://www.albahari.com/nutshell/predicatebuilder.aspx
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        /// <summary>
        /// 动态生成and条件
        /// http://www.albahari.com/nutshell/predicatebuilder.aspx
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
