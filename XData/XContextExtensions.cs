using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Core;
using XData.Extentions;
using XData.Meta;
using XData.XBuilder;

namespace XData
{
    public partial class XContext
    {
        #region Query

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <returns>返回数据缓存</returns>
        public IEnumerable<T> SqlQuery<T>(string sql)
        {
            return this.SqlQuery<T>(sql, CommandType.Text);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <returns>返回数据缓存</returns>
        public IEnumerable<T> SqlQuery<T>(string sql, CommandType commandType)
        {
            return this.SqlQuery<T>(null, sql, commandType);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回数据缓存</returns>
        public IEnumerable<T> SqlQuery<T>(string sql, params DbParameter[] parameters)
        {
            return this.SqlQuery<T>(null, sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回数据缓存</returns>
        public IEnumerable<T> SqlQuery<T>(string sql, CommandType commandType, params DbParameter[] parameters)
        {
            return SqlQuery<T>(null, sql, commandType, parameters);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="page"></param>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回数据缓存</returns>
        public IEnumerable<T> SqlQuery<T>(Page page, string sql, CommandType commandType, params DbParameter[] parameters)
        {
            using (DbConnection dbConnection = this.CreateConnection())
            {
                dbConnection.Open();
                using (DbCommand dbCommand = this.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandType = commandType;
                    dbCommand.CommandText = sql;
                    if (!parameters.IsNullOrEmpty())
                    {
                        dbCommand.Parameters.AddRange(CheckNullParameter(parameters));
                    }

                    IDataReader reader;
                    var log = this.LogFormatter(new List<string> { sql }, commandType, parameters);
                    try
                    {
                        reader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection);
                        this.Log(LogLevel.Information, string.Format("SQL语句执行Query成功！{0}{1}", Environment.NewLine, log));
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("SQL语句执行失败！{0}{1}", Environment.NewLine, log);
                        this.Log(LogLevel.Error, message, ex);
                        throw;
                    }

                    var skip = page?.Skip;
                    var take = page?.PageSize;
                    var skipCount = 0;
                    var takeCount = 0;
                    while (reader.Read())
                    {
                        if (skip.HasValue && skipCount++ < skip)
                        {
                            continue;
                        }
                        if (take.HasValue && takeCount++ >= take.Value)
                        {
                            yield break;
                        }
                        var item = DataFactory.Read<T>(reader);
                        yield return item;
                    }
                    reader.Close();
                }
            }
        }

        #endregion

        #region QueryBuilder

        /// <summary>
        /// 构造一个查询命令
        /// </summary>
        /// <typeparam name="T">查询结果实体类型</typeparam>
        /// <param name="tableName">表名称，如果表名和类型<typeparamref name="T"/>名称一致或者在<see cref="MapperConfig"/>中配置了表名，本参数可以省略</param>
        /// <param name="useCache">true表示使用已缓存结果，false表示每次获取结果都从数据库读取</param>
        /// <returns></returns>
        public IQuery<T> Query<T>(string tableName = null, bool useCache = true)
        {
            return new Query<T>(this, tableName, useCache);
        }

        /// <summary>
        /// 按查询条件获取第一条数据，如果没有返回空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public T GetFirstOrDefault<T>(Expression<Func<T, bool>> condition)
        {
            var query = this.Query<T>().Where(condition).Top(1);
            return query.ToList().FirstOrDefault();
        }

        /// <summary>
        /// 根据主键值获取实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public T GetByKey<T>(object key, Expression<Func<T, object>> primaryKey = null)
        {
            if (key == null)
            {
                return default(T);
            }
            var keyMeta = MapperConfig.GetKeyMeta<T>();
            var exp = keyMeta?.Expression as LambdaExpression;
            if (exp == null && primaryKey != null)
            {
                MapperConfig.HasKey(primaryKey);
                exp = primaryKey;
            }
            if (exp == null)
            {
                throw Error.Exception("没有为模型" + typeof(T).FullName + "指定主键。");
            }
            var keyExp = Expression.Constant(key);
            var mem = exp.Body.ChangeType(keyExp.Type);
            var condition = Expression.Equal(keyExp, mem);
            var lambda = Expression.Lambda<Func<T, bool>>(condition, exp.Parameters);
            return GetFirstOrDefault(lambda);
        }

        #endregion

        #region JoinBuilder
        /*
        /// <summary>
        /// 内连接查询
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public Join<T1, T2> InnerJoin<T1, T2>(Expression<Func<T1, T2, bool>> on)
        {
            return new Join<T1, T2>(this, JoinType.Inner, on);
        }
        /// <summary>
        /// 左外连接查询
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public Join<T1, T2> LeftJoin<T1, T2>(Expression<Func<T1, T2, bool>> on)
        {
            return new Join<T1, T2>(this, JoinType.Left, on);
        }
        /// <summary>
        /// 右外连接查询
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public Join<T1, T2> RightJoin<T1, T2>(Expression<Func<T1, T2, bool>> on)
        {
            return new Join<T1, T2>(this, JoinType.Right, on);
        }
        /// <summary>
        /// 外连接查询
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public Join<T1, T2> FullJoin<T1, T2>(Expression<Func<T1, T2, bool>> on)
        {
            return new Join<T1, T2>(this, JoinType.Full, on);
        }
        /// <summary>
        /// 交叉查询
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public Join<T1, T2> CrossJoin<T1, T2>()
        {
            return new Join<T1, T2>(this, JoinType.Cross);
        }
        //*/
        #endregion

        #region UpdateBuilder

        /// <summary>
        /// 根据两个实体的比较构造一个更新命令,如果<paramref name="oldEntity"/>中没有指定主键，需要在命令后面指定where条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public Update<T> Update<T>(T oldEntity, T newEntity, Expression<Func<T, object>> primaryKey = null)
        {
            return new Update<T>(this, oldEntity, newEntity, primaryKey);
        }

        /// <summary>
        /// 根据一个实体构造一个更新命令,如果<paramref name="newEntity"/>中没有指定主键，需要在命令后面指定where条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newEntity"></param>
        /// <param name="include">包含或者排除，true包含表示仅更新指定的字段，false排除表示不更新指定的字段</param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Update<T> Update<T>(T newEntity, bool include = false, params Expression<Func<T, object>>[] fields)
        {
            return new Update<T>(this, newEntity, include, fields);
        }
        /// <summary>
        /// 根据指定实体构造一个更新指定字段的命令,如果<paramref name="newEntity"/>中没有指定主键，需要在命令后面指定where条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newEntity"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Update<T> Update<T>(T newEntity, params Expression<Func<T, object>>[] fields)
        {
            return new Update<T>(this, newEntity, true, fields);
        }
        /// <summary>
        /// 根据指定的字段值列字典构造一个更新命令,需要在命令后面指定where条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldValues"></param>
        /// <returns></returns>
        public Update<T> Update<T>(IDictionary<string, object> fieldValues)
        {
            return new Update<T>(this, fieldValues);
        }

        #endregion

        #region InsertBuilder
        /// <summary>
        /// 根据指定的实体构造一个插入命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Insert<T> Insert<T>(T entity)
        {
            return new Insert<T>(this, entity);
        }
        /// <summary>
        /// 根据实体和指定的字段构造一个插入命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Insert<T> Insert<T>(T entity, params Expression<Func<T, object>>[] fields)
        {
            return new Insert<T>(this, entity, true, fields);
        }

        /// <summary>
        /// 根据实体和指定的字段构造一个插入命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="include">包含或者排除，true包含表示仅插入指定的字段，false排除表示不插入指定的字段</param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Insert<T> Insert<T>(T entity, bool include = false, params Expression<Func<T, object>>[] fields)
        {
            return new Insert<T>(this, entity, include, fields);
        }
        /// <summary>
        /// 根据指定的字段和值构造一个插入命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldValues"></param>
        /// <returns></returns>
        public Insert<T> Insert<T>(IDictionary<string, object> fieldValues)
        {
            return new Insert<T>(this, fieldValues);
        }

        #endregion

        #region DeleteBuilder

        /// <summary>
        /// 构造一个删除命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public Delete<T> Delete<T>(T entity, Expression<Func<T, object>> primaryKey = null)
        {
            return new Delete<T>(this, entity, primaryKey);
        }
        /// <summary>
        /// 构造一个删除命令,需要在命令后面指定where条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Delete<T> Delete<T>()
        {
            return new Delete<T>(this);
        }

        #endregion

        #region ConvertDbParameters

        internal DbParameter[] ConvertParameters(object[] parameters)
        {
            var list = new List<DbParameter>();
            for (int i = 0; i < parameters.Length; i++)
            {
                var value = DatabaseType.MapParameterValue(parameters[i]) ?? DBNull.Value;
                var dbParameter = CreateParameter(DatabaseType.GetParameterPrefix(ConnectionString) + i, value);
                list.Add(dbParameter);
            }
            return list.ToArray();
        }

        internal DbParameter[] CheckNullParameter(params DbParameter[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null)
                    parameter.Value = DBNull.Value;
            }
            return parameters;
        }

        #endregion
    }
}
