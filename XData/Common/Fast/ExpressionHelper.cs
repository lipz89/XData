using System;
using System.Linq.Expressions;
using System.Reflection;
using XData.Extentions;

namespace XData.Common.Fast
{
    internal static class ExpressionHelper
    {
        public static Func<object[], T> CreateCreatorHandler<T>(ConstructorInfo constructorInfo)
        {
            var p = Expression.Parameter(typeof(object[]));
            ParameterInfo[] paramsTypes = constructorInfo.GetParameters();
            var pars = new Expression[paramsTypes.Length];
            for (int i = 0; i < paramsTypes.Length; i++)
            {
                Expression item = Expression.ArrayIndex(p, Expression.Constant(i));
                if (paramsTypes[i].ParameterType != item.Type)
                {
                    item = Expression.Convert(item, paramsTypes[i].ParameterType);
                }
                pars[i] = item;
            }

            var body = Expression.New(constructorInfo, pars);
            var lambda = Expression.Lambda<Func<object[], T>>(body, p);
            return lambda.Compile();
        }

        public static Expression<Func<T, TValue>> CreateGetterExpression<T, TValue>(MemberInfo memberInfo)
        {
            var target = Expression.Parameter(typeof(T));
            Expression member = Expression.PropertyOrField(target, memberInfo.Name);
            if (member.Type != typeof(TValue))
                member = Expression.Convert(member, typeof(TValue));
            return Expression.Lambda<Func<T, TValue>>(member, target);
        }

        public static LambdaExpression CreateGetterExpression(MemberInfo memberInfo, Type targetType)
        {
            var target = Expression.Parameter(targetType);
            Expression member = Expression.PropertyOrField(target, memberInfo.Name);
            return Expression.Lambda(member, target);
        }

        public static Func<T, TValue> CreateGetterHandler<T, TValue>(MemberInfo memberInfo)
        {
            var target = Expression.Parameter(typeof(T));
            Expression member = Expression.PropertyOrField(target, memberInfo.Name);
            if (typeof(TValue) != memberInfo.GetMemberType())
                member = Expression.Convert(member, typeof(TValue));
            var lambda = Expression.Lambda<Func<T, TValue>>(member, target);
            return lambda.Compile();
        }

        public static Action<T, TValue> CreateSetterHandler<T, TValue>(MemberInfo memberInfo)
        {
            var target = Expression.Parameter(typeof(T));
            var member = Expression.PropertyOrField(target, memberInfo.Name);
            var val = Expression.Parameter(typeof(TValue));
            var body = Expression.Assign(member, val);
            var lambda = Expression.Lambda<Action<T, TValue>>(body, target, val);
            return lambda.Compile();
        }
        public static Func<TValue> CreateStaticGetterHandler<TValue>(PropertyInfo propertyInfo)
        {
            var field = Expression.Property(null, propertyInfo);
            var lambda = Expression.Lambda<Func<TValue>>(field);
            return lambda.Compile();
        }
        public static Action<TValue> CreateStaticSetterHandler<TValue>(PropertyInfo propertyInfo)
        {
            var field = Expression.Property(null, propertyInfo);
            var val = Expression.Parameter(typeof(TValue));
            var body = Expression.Assign(field, val);
            var lambda = Expression.Lambda<Action<TValue>>(body, val);
            return lambda.Compile();
        }
        public static Func<TValue> CreateStaticGetterHandler<TValue>(FieldInfo fieldInfo)
        {
            var field = Expression.Field(null, fieldInfo);
            var lambda = Expression.Lambda<Func<TValue>>(field);
            return lambda.Compile();
        }
        public static Action<TValue> CreateStaticSetterHandler<TValue>(FieldInfo fieldInfo)
        {
            var field = Expression.Field(null, fieldInfo);
            var val = Expression.Parameter(typeof(TValue));
            var body = Expression.Assign(field, val);
            var lambda = Expression.Lambda<Action<TValue>>(body, val);
            return lambda.Compile();
        }
    }
}