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
    public static class MapperConfig
    {
        #region 静态字段

        private static readonly Dictionary<Type, string> CachedTableNames = new Dictionary<Type, string>();
        private static readonly Dictionary<Type, ColumnMeta[]> CachedTableKeys = new Dictionary<Type, ColumnMeta[]>();
        private static readonly Dictionary<ColumnMeta, string> CachedColumnNames = new Dictionary<ColumnMeta, string>();
        private static readonly List<ColumnMeta> CachedIgnoreColumns = new List<ColumnMeta>();
        private static readonly Dictionary<Type, ColumnMeta> CachedTableIdentities = new Dictionary<Type, ColumnMeta>();

        #endregion

        #region 配置方法

        /// <summary>
        /// 配置模型对应的表名
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="tableName">模型对应的表名称</param>
        public static void HasTableName<T>(string tableName)
        {
            HasTableName(typeof(T), tableName);
        }

        internal static void HasTableName(Type type, string tableName)
        {
            if (tableName.IsNullOrWhiteSpace())
            {
                throw Error.ArgumentNullException(nameof(tableName));
            }

            lock (CachedTableNames)
            {
                if (CachedTableNames.ContainsKey(type))
                {
                    throw Error.Exception($"已经设置了类型 {type.FullName} 对应的表名称为 {CachedTableNames[type]}，不能重复设置。");
                }
                CachedTableNames.Add(type, tableName);
            }
        }

        /// <summary>
        /// 配置模型属性对应的字段名
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="property">模型属性的表达式</param>
        /// <param name="columnName">模型属性对应的字段名称</param>
        public static void HasColumnName<T>(Expression<Func<T, object>> property, string columnName)
        {
            var member = property?.GetMember();
            if (member?.IsPropertyOrField() != true)
            {
                throw Error.ArgumentException("指定的表达式不是属性或字段。", nameof(property));
            }
            if (columnName.IsNullOrWhiteSpace())
            {
                throw Error.ArgumentNullException(nameof(columnName));
            }

            var m = new ColumnMeta(member, typeof(T));
            //if (CachedIgnoreColumns.Contains(m))
            //{
            //    throw Error.Exception($"属性 {m} 已经被忽略，不能设置列名。");
            //}
            lock (CachedColumnNames)
            {
                if (CachedColumnNames.ContainsKey(m))
                {
                    throw Error.Exception($"已经设置了字段 {m} 对应的列名称为 {CachedColumnNames[m]}，不能重复设置。");
                }
                CachedColumnNames.Add(m, columnName);
            }
        }

        /// <summary>
        /// 配置模型的唯一主键列名
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="properties">模型主键属性的表达式</param>
        public static void HasKey<T>(params Expression<Func<T, object>>[] properties)
        {
            if (properties.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(properties));
            }

            var members = properties.Select(x => x.GetMember()).Distinct().ToArray();

            if (members.Any(x => !x.IsPropertyOrField()))
            {
                throw Error.ArgumentException("参数中包含不是属性或字段的表达式。", nameof(properties));
            }

            lock (CachedTableKeys)
            {
                var type = typeof(T);
                if (CachedTableKeys.ContainsKey(type))
                {
                    var keys = string.Join(",", GetKey<T>());
                    throw Error.Exception($"已经设置了类型 {type.FullName} 对应的主键为 {keys}，不能重复设置。");
                    //CachedTableKeys[type] = key;
                }

                var metas = new ColumnMeta[members.Length];
                for (var index = 0; index < members.Length; index++)
                {
                    var member = members[index];
                    var m = new ColumnMeta(member, type);
                    if (CachedIgnoreColumns.Contains(m))
                    {
                        throw Error.Exception($"属性 {m} 已经被忽略，不能设置为主键。");
                    }

                    metas[index] = m;
                }

                CachedTableKeys.Add(type, metas);
            }
        }

        /// <summary>
        /// 配置模型的自增主键
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="property">模型自增列属性的表达式</param>
        /// <param name="columnName">自增主键列对应的字段名称</param>
        public static void HasIdentity<T>(Expression<Func<T, object>> property, string columnName = null)
        {
            var member = property?.GetMember();
            if (member?.IsPropertyOrField() != true)
            {
                throw Error.ArgumentException("指定的表达式不是属性或字段。", nameof(property));
            }

            lock (CachedTableIdentities)
            {
                var m = new ColumnMeta(member, typeof(T));
                var type = typeof(T);
                if (CachedTableIdentities.ContainsKey(type))
                {
                    throw Error.Exception(
                        $"已经设置了类型 {type.FullName} 对应的自增列为 {CachedTableIdentities[type].ColumnName}，不能重复设置。");
                }

                CachedTableIdentities.Add(type, m);
                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    HasColumnName(property, columnName);
                }
            }
        }
        /// <summary>
        /// 配置模型的自增主键
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="property">模型自增列属性的表达式</param>
        /// <param name="columnName">自增主键列对应的字段名称</param>
        public static void HasIdentityKey<T>(Expression<Func<T, object>> property, string columnName = null)
        {
            HasIdentity(property, columnName);
            HasKey(property);
        }

        /// <summary>
        /// 忽略成员
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="property">要忽略的模型属性的表达式</param>
        public static void IgnoreColumn<T>(Expression<Func<T, object>> property)
        {
            var member = property?.GetMember();
            if (member?.IsPropertyOrField() != true)
            {
                throw Error.ArgumentException("指定的表达式不是属性或字段。", nameof(property));
            }

            lock (CachedIgnoreColumns)
            {
                var keys = GetKeyMetas<T>();
                var m = new ColumnMeta(member, typeof(T));
                if (keys != null && keys.Any(x => x.Member == member))
                {
                    throw Error.Exception($"属性 {m} 已经配置为主键，不能忽略。");
                }

                //if (CachedColumnNames.ContainsKey(m))
                //{
                //    throw Error.ArgumentException("属性已经配置字段名:" + CachedColumnNames[m] + "。", nameof(property));
                //}
                if (!CachedIgnoreColumns.Contains(m))
                {
                    CachedIgnoreColumns.Add(m);
                }
            }
        }

        /// <summary>
        /// 忽略成员
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="properties">要忽略的模型属性的表达式数组</param>
        public static void IgnoreColumns<T>(params Expression<Func<T, object>>[] properties)
            where T : class
        {
            if (properties.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(properties));
            }
            foreach (var property in properties)
            {
                if (property != null)
                {
                    IgnoreColumn(property);
                }
            }
        }

        #endregion

        #region 取配置方法

        /// <summary>
        /// 返回模型对应的表名
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>返回模型对应的表名</returns>
        public static string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }

        /// <summary>
        /// 返回模型对应的表名
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>返回模型对应的表名</returns>
        public static string GetTableName(Type type)
        {
            lock (CachedTableNames)
            {
                if (CachedTableNames.ContainsKey(type))
                {
                    return CachedTableNames[type];
                }

                return type.Name;
            }
        }

        /// <summary>
        /// 返回模型对应的字段名
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <typeparam name="TProperty">模型的成员类型</typeparam>
        /// <param name="property">模型成员表达式</param>
        /// <returns>成员对应的列名称</returns>
        public static string GetColumnName<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            var member = property?.GetMember();
            if (member?.IsPropertyOrField() != true)
            {
                throw Error.ArgumentException("指定的表达式不是属性或字段。", nameof(property));
            }
            return GetColumnName(member, typeof(T));
        }

        /// <summary>
        /// 返回模型对应的字段名
        /// </summary>
        /// <param name="member">模型的成员信息</param>
        /// <param name="type">模型类型</param>
        /// <returns>成员对应的列名称</returns>
        public static string GetColumnName(MemberInfo member, Type type)
        {
            if (member?.IsPropertyOrField() != true)
            {
                throw Error.ArgumentException("指定的表达式不是属性或字段。", nameof(member));
            }
            var m = new ColumnMeta(member, type);
            return GetColumnName(m);
        }
        internal static string GetColumnName(ColumnMeta columnMeta)
        {
            if (columnMeta == null)
            {
                throw Error.ArgumentNullException(nameof(columnMeta));
            }

            lock (CachedColumnNames)
            {
                if (CachedColumnNames.ContainsKey(columnMeta))
                {
                    return CachedColumnNames[columnMeta];
                }

                return columnMeta.Name;
            }
        }

        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>返回模型的主键列名称</returns>
        public static string[] GetKey<T>()
        {
            return GetKey(typeof(T));
        }

        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>返回模型的主键列名称</returns>
        public static string[] GetKey(Type type)
        {
            lock (CachedTableKeys)
            {
                if (CachedTableKeys.ContainsKey(type))
                {
                    return CachedTableKeys[type].Select(x => x.ColumnName).ToArray();
                }

                return null;
            }
        }

        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>返回模型的主键列信息</returns>
        internal static ColumnMeta[] GetKeyMetas<T>()
        {
            return GetKeyMetas(typeof(T));
        }

        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>返回模型的主键列信息</returns>
        internal static ColumnMeta[] GetKeyMetas(Type type)
        {
            lock (CachedTableKeys)
            {
                if (CachedTableKeys.ContainsKey(type))
                {
                    return CachedTableKeys[type];
                }

                return null;
            }
        }
        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <returns>返回模型的主键列信息</returns>
        internal static Expression<Func<T, bool>> GetKeysExpression<T>(params object[] keys)
        {
            var keyMeta = MapperConfig.GetKeyMetas<T>();
            if (keyMeta == null)
            {
                throw Error.Exception("没有为模型" + typeof(T).FullName + "指定主键。");
            }

            if (keys.Length != keyMeta.Length)
            {
                return null;
                //throw Error.Exception("主键的列数目和给定的键值数目不相等。");
            }

            LambdaExpression body = null;
            ParameterExpression parameter = null;
            for (var i = 0; i < keyMeta.Length; i++)
            {
                var meta = keyMeta[i];
                if (meta.Expression is LambdaExpression exp)
                {
                    parameter = exp.Parameters.FirstOrDefault();
                    var keyExp = Expression.Constant(keys[i]);
                    var mem = exp.Body.ChangeType(keyExp.Type);
                    var condition = Expression.Equal(keyExp, mem);
                    var innerLambda = Expression.Lambda(condition, parameter);

                    body = body.AndAlso(innerLambda);
                }
            }

            if (body != null)
            {
                return Expression.Lambda<Func<T, bool>>(body.Body, parameter);
            }
            return null;
        }
        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <returns>返回模型的主键列信息</returns>
        internal static Expression<Func<T, bool>> GetKeysExpression<T>(T entity)
        {
            var keyMeta = MapperConfig.GetKeyMetas<T>();
            if (keyMeta == null)
            {
                throw Error.Exception("没有为模型" + typeof(T).FullName + "指定主键。");
            }

            LambdaExpression body = null;
            ParameterExpression parameter = null;
            for (var i = 0; i < keyMeta.Length; i++)
            {
                var meta = keyMeta[i];
                if (meta.Expression is LambdaExpression exp)
                {
                    parameter = exp.Parameters.FirstOrDefault();
                    var key = exp.Compile().DynamicInvoke(entity);
                    var keyExp = Expression.Constant(key);
                    //var keyExp = Expression.MakeMemberAccess(constvar, meta.Member);
                    var mem = exp.Body.ChangeType(keyExp.Type);
                    var condition = Expression.Equal(keyExp, mem);
                    var innerLambda = Expression.Lambda(condition, parameter);

                    body = body.AndAlso(innerLambda);
                }
            }

            if (body != null)
            {
                return Expression.Lambda<Func<T, bool>>(body.Body, parameter);
            }
            return null;
        }

        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>返回模型的自增列信息</returns>
        internal static ColumnMeta GetIdentity<T>()
        {
            return GetIdentity(typeof(T));
        }

        /// <summary>
        /// 返回模型对应的主键字段名
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>返回模型的自增列信息</returns>
        internal static ColumnMeta GetIdentity(Type type)
        {
            lock (CachedTableIdentities)
            {
                if (CachedTableIdentities.ContainsKey(type))
                {
                    return CachedTableIdentities[type];
                }

                return null;
            }
        }

        /// <summary>
        /// 判断某字段是否被忽略
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="property">判断是否忽略的属性表达式</param>
        /// <returns>返回该字段是否忽略映射</returns>
        public static bool IsIgnore<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            var member = property?.GetMember();
            if (member?.IsPropertyOrField() != true)
            {
                throw Error.ArgumentException("指定的表达式不是属性或字段。", nameof(property));
            }
            return IsIgnore(member, typeof(T));
        }

        /// <summary>
        /// 判断某字段是否被忽略
        /// </summary>
        /// <param name="memberInfo">属性或字段成员信息</param>
        /// <param name="type">模型类型</param>
        /// <returns>返回该字段是否忽略映射</returns>
        public static bool IsIgnore(MemberInfo memberInfo, Type type)
        {
            if (memberInfo == null)
            {
                throw Error.ArgumentNullException(nameof(memberInfo));
            }

            lock (CachedIgnoreColumns)
            {
                var m = new ColumnMeta(memberInfo, type);
                return CachedIgnoreColumns.Contains(m);
            }
        }

        #endregion
    }
}
