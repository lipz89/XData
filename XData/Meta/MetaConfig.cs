using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;
using XData.Extentions;

namespace XData.Meta
{
    /// <summary>
    /// 元数据配置类
    /// </summary>
    public static class MetaConfig
    {
        private static readonly Dictionary<Type, string> TableNames = new Dictionary<Type, string>();
        private static readonly Dictionary<Type, ColumnMeta> TableKeys = new Dictionary<Type, ColumnMeta>();
        private static readonly Dictionary<MInfo, string> ColumnNames = new Dictionary<MInfo, string>();
        private static readonly List<MInfo> IgnoreColumns = new List<MInfo>();
        /// <summary>
        /// 配置模型对应的表名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        public static void MetaTableName<T>(string tableName)
        {
            if (tableName.IsNullOrWhiteSpace())
            {
                throw Error.ArgumentNullException(nameof(tableName));
            }
            var type = typeof(T);
            if (TableNames.ContainsKey(type))
            {
                TableNames[type] = tableName;
            }
            else
            {
                TableNames.Add(type, tableName);
            }
        }

        /// <summary>
        /// 配置模型属性对应的字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="columnName"></param>
        public static void MetaColumnName<T>(Expression<Func<T, object>> property, string columnName)
            where T : class
        {
            if (property == null)
            {
                throw Error.ArgumentNullException(nameof(property));
            }
            if (columnName.IsNullOrWhiteSpace())
            {
                throw Error.ArgumentNullException(nameof(columnName));
            }

            var mi = property.GetMember();
            if (mi == null)
            {
                throw Error.ArgumentException("指定表达式不是成员访问类型。", nameof(property));
            }
            if (mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property)
            {
                var m = new MInfo(mi, typeof(T));
                if (IgnoreColumns.Contains(m))
                {
                    throw Error.ArgumentException("属性已经被忽略。", nameof(property));
                }
                if (ColumnNames.ContainsKey(m))
                {
                    ColumnNames[m] = columnName;
                }
                else
                {
                    ColumnNames.Add(m, columnName);
                }
            }
            else
            {
                throw Error.ArgumentException("指定表达式不是属性或字段。", nameof(property));
            }
        }

        /// <summary>
        /// 忽略成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        public static void IgnoreColumn<T>(Expression<Func<T, object>> property)
            where T : class
        {
            if (property == null)
            {
                throw Error.ArgumentNullException(nameof(property));
            }
            var mi = property.GetMember();
            if (mi == null)
            {
                throw Error.ArgumentException("指定表达式不是成员访问类型。", nameof(property));
            }
            if (mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property)
            {
                var m = new MInfo(mi, typeof(T));
                if (TableKeys.Values.Any(x => x.Member == mi))
                {
                    throw Error.ArgumentException("属性已经配置为主键，不能忽略。", nameof(property));
                }
                if (ColumnNames.ContainsKey(m))
                {
                    throw Error.ArgumentException("属性已经配置字段名:" + ColumnNames[m] + "。", nameof(property));
                }
                if (!IgnoreColumns.Contains(m))
                {
                    IgnoreColumns.Add(m);
                }
            }
            else
            {
                throw Error.ArgumentException("指定表达式不是属性或字段。", nameof(property));
            }
        }

        /// <summary>
        /// 忽略成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="properties"></param>
        public static void IgnoreColumn<T>(params Expression<Func<T, object>>[] properties)
            where T : class
        {
            foreach (var property in properties)
            {
                IgnoreColumn(property);
            }
        }

        /// <summary>
        /// 配置模型的主键列名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="columnName"></param>
        public static void MetaKey<T>(Expression<Func<T, object>> property, string columnName = null)
            where T : class
        {
            if (property == null)
            {
                throw Error.ArgumentNullException(nameof(columnName));
            }
            var member = property.GetMember();
            if (member == null)
            {
                throw Error.ArgumentException("指定的表达式不是属性或字段。", nameof(property));
            }
            var m = new MInfo(member, typeof(T));
            if (IgnoreColumns.Contains(m))
            {
                throw Error.ArgumentException("属性已经被忽略。", nameof(property));
            }
            var key = new ColumnMeta(m)
            {
                ColumnName = columnName ?? property.GetPropertyName(),
                Expression = property
            };
            var type = typeof(T);
            if (TableKeys.ContainsKey(type))
            {
                TableKeys[type] = key;
            }
            else
            {
                TableKeys.Add(type, key);
            }
        }
        /// <summary>
        /// 返回模型对应的表名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }
        /// <summary>
        /// 返回模型对应的表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            if (TableNames.ContainsKey(type))
            {
                return TableNames[type];
            }
            return type.Name;
        }
        /// <summary>
        /// 返回模型对应的字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetColumnName<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null)
            {
                throw Error.ArgumentNullException(nameof(property));
            }
            var mi = property.GetMember();
            if (mi == null)
            {
                throw Error.ArgumentException("指定表达式不是成员访问类型。", nameof(property));
            }
            return GetColumnName(mi, typeof(T));
        }

        /// <summary>
        /// 返回模型对应的字段名
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetColumnName(MemberInfo memberInfo, Type caller)
        {
            if (memberInfo == null)
            {
                throw Error.ArgumentNullException(nameof(memberInfo));
            }
            var m = new MInfo(memberInfo, caller);
            if (ColumnNames.ContainsKey(m))
            {
                return ColumnNames[m];
            }
            return memberInfo.Name;
        }
        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetKey<T>()
        {
            return GetKey(typeof(T));
        }
        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetKey(Type type)
        {
            if (TableKeys.ContainsKey(type))
            {
                return TableKeys[type].ColumnName;
            }
            return string.Empty;
        }
        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <returns></returns>
        internal static ColumnMeta GetKeyMeta<T>()
        {
            return GetKeyMeta(typeof(T));
        }
        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static ColumnMeta GetKeyMeta(Type type)
        {
            if (TableKeys.ContainsKey(type))
            {
                return TableKeys[type];
            }
            return null;
        }

        /// <summary>
        /// 判断某字段是否被忽略
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsIgnore<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null)
            {
                throw Error.ArgumentNullException(nameof(property));
            }
            var mi = property.GetMember();
            if (mi == null)
            {
                throw Error.ArgumentException("指定表达式不是成员访问类型。", nameof(property));
            }
            return IsIgnore(mi, typeof(T));
        }

        /// <summary>
        /// 判断某字段是否被忽略
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static bool IsIgnore(MemberInfo memberInfo, Type caller)
        {
            if (memberInfo == null)
            {
                throw Error.ArgumentNullException(nameof(memberInfo));
            }
            var m = new MInfo(memberInfo, caller);
            return IgnoreColumns.Contains(m);
        }
    }
}
