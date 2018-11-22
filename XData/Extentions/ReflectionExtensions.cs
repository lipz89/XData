using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        public static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
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

        public static Type GetMemberType(this MemberInfo value)
        {
            if (value is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType;
            }

            if (value is PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType;
            }

            return null;
        }

        public static bool IsPropertyOrField(this MemberInfo value)
        {
            if (value is FieldInfo)
            {
                return true;
            }

            if (value is PropertyInfo)
            {
                return true;
            }

            return false;
        }
        public static bool CanWrite(this MemberInfo value)
        {
            if (value is PropertyInfo propertyInfo)
            {
                return propertyInfo.CanWrite;
            }

            if (value is FieldInfo)
            {
                return true;
            }

            return false;
        }
        public static bool CanRead(this MemberInfo value)
        {
            if (value is PropertyInfo propertyInfo)
            {
                return propertyInfo.CanRead;
            }

            if (value is FieldInfo)
            {
                return true;
            }

            return false;
        }
    }
}