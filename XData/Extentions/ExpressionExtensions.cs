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
                throw Error.ArgumentException("ָ�����ʽ�����ֶλ����ԡ�", nameof(expression));
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
        /// ��ϱ��ʽ
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
        /// ���ʽ�Ĳ�������
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
        /// ���ʽ�Ļ�����
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
        /// ���ʽ�ķ�����
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
        /// ���ʽ�����滻����
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