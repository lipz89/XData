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
        public static string ObtainOriginalName(this Type type)
        {
            if (simpleName.ContainsKey(type))
            {
                return simpleName[type];
            }
            return type.ObtainOriginalNameCore();
        }
        private static string ObtainOriginalNameCore(this Type type)
        {
            if (type.IsArray)
            {
                var n = type.Name;
                var etype = type.GetElementType();
                return n.Replace(etype.Name, etype.ObtainOriginalName());
            }
            if (type.IsGenericType)
            {
                var gt = ExtractName(type.FullName);
                var gtp = ExtractGenericArguments(type.GetGenericArguments());
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return gtp + "?";
                }
                return gt + "<" + gtp + ">";
            }
            return type.FullName;
        }
        public static string GetGenericName(this Type type)
        {
            if (type.IsGenericType)
            {
                return ExtractName(type.Name);
            }
            return string.Empty;
        }
        private static string ExtractName(string name)
        {
            int length = name.IndexOf("`", StringComparison.Ordinal);
            if (length > 0)
            {
                name = name.Substring(0, length);
            }
            return name;
        }
        private static string ExtractGenericArguments(this IEnumerable<Type> names)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Type type in names)
            {
                if (builder.Length > 1)
                {
                    builder.Append(", ");
                }
                builder.Append(type.ObtainOriginalName());
            }
            return builder.ToString();
        }
        public static string ObtainOriginalMethodName(this MethodInfo method)
        {
            if (!method.IsGenericMethod)
            {
                return method.Name;
            }
            return ExtractName(method.Name) + "<" + ExtractGenericArguments(method.GetGenericArguments()) + ">";
        }

        private static readonly Dictionary<Type, string> simpleName = new Dictionary<Type, string>
        {
            { typeof(object), "object"},
            { typeof(string), "string"},
            { typeof(bool), "bool"},
            { typeof(char) ,"char"},
            { typeof(int), "int"},
            { typeof(uint), "uint"},
            { typeof(byte), "byte"},
            { typeof(sbyte), "sbyte"},
            { typeof(short), "short"},
            { typeof(ushort), "ushort"},
            { typeof(long), "long"},
            { typeof(ulong), "ulong"},
            { typeof(float), "float"},
            { typeof(double), "double"},
            { typeof(decimal), "decimal"}
        };

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
        public static bool HasInterfaceOf(this Type type, Type interfaceType, List<Type> genericTypes = null)
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
                if (genericTypes != null)
                {
                    genericTypes.Clear();
                    genericTypes.AddRange(its.GetGenericArguments());
                }
                return true;
            }
            return false;
        }

        public static bool IsAssignableFromOneOf(this Type type, IEnumerable<Type> types, out Type outType)
        {
            foreach (var t in types)
            {
                if (type.IsAssignableFrom(t))
                {
                    outType = t;
                    return true;
                }
            }
            outType = null;
            return false;
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
            return Expression.Lambda<Func<T, object>>(Expression.Convert(member,typeof(object)), instance);
        }

        #region Anonymous

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

        public static bool HasDefaultCtor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
        public static ConstructorInfo GetDefaultCtor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes);
        }
    }
}