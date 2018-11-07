using System;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;

namespace XData.Extentions
{
    internal static class ExpressionExtensions
    {
        public static MemberInfo GetMember(this Expression expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }

            Expression exp = expression;
            if (expression is LambdaExpression lambda)
            {
                exp = lambda.Body;
            }

            if (exp is UnaryExpression unary)
            {
                exp = unary.Operand;
            }

            if (exp is MemberExpression member)
            {
                return member.Member;
            }
            return null;
        }

        public static string GetPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }

            var member = expression.GetMember();
            if (member == null)
            {
                throw Error.ArgumentException("指定表达式不是字段或属性。", nameof(expression));
            }
            return member.Name;
        }

        public static Expression ChangeType(this Expression expression, Type type)
        {
            var exp = expression;
            while (type != exp.Type)
            {
                if (exp is UnaryExpression unary)
                {
                    exp = unary.Operand;
                }
                else
                {
                    exp = Expression.Convert(exp, type);
                }
            }
            return exp;
        }
    }
}