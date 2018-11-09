using System;
using System.Collections.Generic;
using System.Linq;
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
            if (member?.IsPropertyOrField() != true)
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


        /// <summary>
        /// 组合表达式
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="compose"></param>
        /// <returns></returns>
        public static LambdaExpression Compose(this LambdaExpression first, LambdaExpression second, Func<Expression, Expression, Expression> compose)
        {
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            var secondBody = ParameterReplacer.ReplaceParameters(map, second.Body);

            return Expression.Lambda(compose(first.Body, secondBody), first.Parameters);
        }
        /// <summary>
        /// 表达式的并且运算
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static LambdaExpression AndAlso(this LambdaExpression first, LambdaExpression second)
        {
            if (first == null)
            {
                return second;
            }

            if (second == null)
            {
                return first;
            }
            return first.Compose(second, Expression.AndAlso);
        }

        /// <summary>
        /// 表达式的或运算
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static LambdaExpression OrElse<TEntity>(this LambdaExpression first, LambdaExpression second)
        {
            if (first == null)
            {
                return second;
            }

            if (second == null)
            {
                return first;
            }
            return first.Compose(second, Expression.OrElse);
        }
        /// <summary>
        /// 表达式的非运算
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static LambdaExpression Not<TEntity>(this LambdaExpression exp)
        {
            if (exp == null)
            {
                return null;
            }
            return Expression.Lambda(Expression.Not(exp.Body), exp.Parameters);
        }
        /// <summary>
        /// 表达式参数替换工具
        /// </summary>
        class ParameterReplacer : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> map;

            private ParameterReplacer(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            internal static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterReplacer(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                ParameterExpression replacement;

                if (map.TryGetValue(p, out replacement))
                    p = replacement;

                return base.VisitParameter(p);
            }
        }
    }
}