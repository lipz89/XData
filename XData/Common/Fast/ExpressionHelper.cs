using System;
using System.Linq.Expressions;
using System.Reflection;

namespace XData.Common.Fast
{
    internal class ExpressionHelper
    {
        public static Func<object[], T> CreateCreatorHandler<T>(ConstructorInfo constructorInfo)
        {
            ParameterInfo[] paramsTypes = constructorInfo.GetParameters();
            var pars = new ParameterExpression[paramsTypes.Length];
            for (int i = 0; i < paramsTypes.Length; i++)
            {
                pars[i] = Expression.Parameter(paramsTypes[i].ParameterType);
            }

            var body = Expression.New(constructorInfo, pars);
            var lambda = Expression.Lambda(body, pars);
            var func = lambda.Compile();

            //var p = Expression.Parameter(typeof(object[]));
            //var funcexp = Expression.Constant(func);
            //var mi = typeof(Delegate).GetMethod("DynamicInvoke");
            //var invoke = Expression.Call(funcexp, mi, p);
            //var rtn = Expression.Convert(invoke, typeof(T));
            //var lambda2 = Expression.Lambda<Func<object[], T>>(rtn, p);
            //return lambda2.Compile();
            return objects => (T)func.DynamicInvoke(objects);
        }

        public static Func<TTarget, TValue> CreateGetterHandler<TTarget, TValue>(FieldInfo fieldInfo)
        {
            var target = Expression.Parameter(typeof(TTarget));
            var field = Expression.Field(target, fieldInfo);
            var lambda = Expression.Lambda<Func<TTarget, TValue>>(field, target);
            return lambda.Compile();
        }

        public static Action<TTarget, TValue> CreateSetterHandler<TTarget, TValue>(FieldInfo fieldInfo)
        {
            var target = Expression.Parameter(typeof(TTarget));
            var field = Expression.Field(target, fieldInfo);
            var val = Expression.Parameter(typeof(TValue));
            var body = Expression.Assign(field, val);
            var lambda = Expression.Lambda<Action<TTarget, TValue>>(body, target, val);
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