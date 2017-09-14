using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using XData.Common;

namespace XData.Extentions
{
    internal static class ReflectionExtensions
    {
        #region 一般类型判断

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type NonNullableType(this Type type)
        {
            return type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
        }

        public static bool IsIntegralType(this Type t)
        {
            var tc = Type.GetTypeCode(t);
            return tc >= TypeCode.SByte && tc <= TypeCode.UInt64;
        }

        #endregion

        #region 枚举器类型判断

        public static Type GetItemType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (type.IsEnumerableOf())
            {
                var itf = type.GetInterfaces().FirstOrDefault(x => x.Name == "IEnumerable`1");
                var t = itf?.GetGenericArguments().First();
                if (!t?.ContainsGenericParameters == true)
                {
                    return t;
                }
                return null;
            }
            if (type.IsEnumerable())
            {
                return typeof(object);
            }
            throw new NotSupportedException();
        }

        public static bool IsEnumerable(this Type type)
        {
            return type.HasInterface<IEnumerable>();
        }

        public static bool IsEnumerableOf(this Type type)
        {
            return type.HasInterfaceOf(typeof(IEnumerable<>));
        }

        #endregion

        #region 接口判断

        public static bool HasInterface(this Type type, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                return false;
            }
            return interfaceType.IsAssignableFrom(type);
        }

        public static bool HasInterface<T>(this Type type)
        {
            return type.HasInterface(typeof(T));
        }

        public static bool HasInterfaceOf(this Type type, Type interfaceType, List<Type> genericArgumentTypes = null)
        {
            if (!interfaceType.IsInterface)
            {
                return false;
            }

            if (type == interfaceType)
            {
                return true;
            }
            var its = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

            if (its != null)
            {
                if (genericArgumentTypes != null)
                {
                    genericArgumentTypes.Clear();
                    genericArgumentTypes.AddRange(its.GetGenericArguments());
                }
                return true;
            }
            return false;
        }

        #endregion

        #region 匿名类型判断

        public static bool IsAnonymousType(this Type type)
        {
            if (type.IsGenericType)
            {
                var d = type.GetGenericTypeDefinition();
                if (d.IsClass && d.IsSealed && d.Attributes.HasFlag(TypeAttributes.NotPublic))
                {
                    var attributes = d.GetCustomAttribute<CompilerGeneratedAttribute>(false);
                    if (attributes != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsAnonymousType<T>()
        {
            return IsAnonymousType(typeof(T));
        }

        #endregion

        #region 默认构造函数

        public static bool HasDefaultCtor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static ConstructorInfo GetDefaultCtor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes);
        }

        #endregion

        /// <summary>
        /// 取类型的默认值，即default(T)的值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(this Type type)
        {
            var exp = Expression.Default(type);
            var lambda = Expression.Lambda(exp);
            return lambda.Compile().DynamicInvoke();
        }

        public static Expression<Func<T, object>> GetMemberAccess<T>(this MemberInfo memberInfo)
        {
            var type = typeof(T);
            if (memberInfo.DeclaringType == null || !memberInfo.DeclaringType.IsAssignableFrom(type))
            {
                throw Error.ArgumentException("类型" + type.Name + "不包含指定的成员。", nameof(memberInfo));
            }
            if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property)
            {
                throw Error.ArgumentException("指定的成员不是字段或属性。", nameof(memberInfo));
            }
            var instance = Expression.Parameter(typeof(T));
            var member = Expression.MakeMemberAccess(instance, memberInfo);
            return Expression.Lambda<Func<T, object>>(Expression.Convert(member, typeof(object)), instance);
        }
        public static Expression<Func<T, TKey>> GetMemberProperty<T, TKey>(this MemberInfo memberInfo)
        {
            var type = typeof(T);
            if (memberInfo.DeclaringType == null || !memberInfo.DeclaringType.IsAssignableFrom(type))
            {
                throw Error.ArgumentException("类型" + type.Name + "不包含指定的成员。", nameof(memberInfo));
            }
            if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property)
            {
                throw Error.ArgumentException("指定的成员不是字段或属性。", nameof(memberInfo));
            }
            var instance = Expression.Parameter(typeof(T));
            Expression member = Expression.MakeMemberAccess(instance, memberInfo);
            var keyType = typeof(TKey);
            var memType = memberInfo.GetMemberType();
            if (keyType.NonNullableType() == memType.NonNullableType())
            {
                if (memType != keyType)
                {
                    member = Expression.Convert(member, typeof(TKey));
                }
                return Expression.Lambda<Func<T, TKey>>(member, instance);
            }

            throw Error.InvalidCastException("类型不能转换");
        }

        public static Type GetMemberType(this MemberInfo value)
        {
            if (value is FieldInfo)
            {
                return ((FieldInfo)value).FieldType;
            }
            else if (value is PropertyInfo)
            {
                return ((PropertyInfo)value).PropertyType;
            }
            else if (value is MethodInfo)
            {
                return ((MethodInfo)value).ReturnType;
            }
            throw new NotSupportedException();
        }
    }
}