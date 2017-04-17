using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using XData.Core;

namespace XData.Common
{
    /// <summary>
    /// 常量和常用静态数据
    /// </summary>
    internal static class Constans
    {
        static Constans()
        {
            MethodStringContains = typeof(string).GetMethod("Contains");
            MethodStringStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodStringEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            MethodObjectEquals = typeof(object).GetMethod("Equals", new[] { typeof(object) });
            MethodEnumerableContains = typeof(Enumerable).GetMethods().First(x => x.Name == "Contains" && x.GetParameters().Length == 2);
            MethodListContains = typeof(List<>).GetMethod("Contains");
            MethodStringSqlLike = typeof(XSqlFunctions).GetMethod("SqlLike");
            MethodBetween = typeof(XSqlFunctions).GetMethod("Between");
            DictionaryContainsKey = typeof(Dictionary<string, object>).GetMethod("ContainsKey");
            DictionaryIndex = typeof(Dictionary<string, object>).GetProperty("Item");
        }

        public const int HashCodeXOr = 0x01010101;

        #region 方法名标记

        public const string StringContains = "StringContains";
        public const string StringStartsWith = "StringStartsWith";
        public const string StringEndsWith = "StringEndsWith";
        public const string StringSqlLike = "StringSqlLike";
        public const string EnumerableContains = "EnumerableContains";
        public const string ObjectEquals = "ObjectEquals";
        public const string ObjectBetween = "ObjectBetween";

        #endregion

        #region 方法

        public static readonly MethodInfo MethodStringContains;
        public static readonly MethodInfo MethodStringStartsWith;
        public static readonly MethodInfo MethodStringEndsWith;
        public static readonly MethodInfo MethodStringSqlLike;
        public static readonly MethodInfo MethodBetween;
        public static readonly MethodInfo MethodEnumerableContains;
        public static readonly MethodInfo MethodListContains;
        public static readonly MethodInfo MethodObjectEquals;
        public static readonly MethodInfo DictionaryContainsKey;
        public static readonly PropertyInfo DictionaryIndex;

        #endregion

        public static bool IsListContains(this MethodInfo method)
        {
            var declaringType = method.DeclaringType;
            if (declaringType == null)
            {
                return false;
            }

            if (declaringType.IsGenericType && declaringType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return true;
            }

            return false;
        }

        public static bool IsBetween(this MethodInfo method)
        {
            return method.IsGenericMethod && method.GetGenericMethodDefinition() == MethodBetween;
        }
    }
}