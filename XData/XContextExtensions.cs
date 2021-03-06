﻿using System;
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
        #region SqlQuery

        /// <summary>
        /// 从数据库查询数据并直接转换成目标实体
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回实体序列</returns>
        public IEnumerable<T> SqlQuery<T>(string sql, params DbParameter[] parameters)
        {
            return this.SqlQuery<T>(null, sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// 从数据库查询数据并直接转换成目标实体
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回实体序列</returns>
        public IEnumerable<T> SqlQuery<T>(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            return SqlQuery<T>(null, sql, commandType, parameters);
        }

        /// <summary>
        /// 从数据库查询数据并直接转换成目标实体
        /// </summary>
        /// <param name="page"></param>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回实体序列</returns>
        internal IEnumerable<T> SqlQuery<T>(Page page, string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            DbConnection dbConnection = CreateConnection();
            using (DbCommand dbCommand = this.CreateCommand())
            {
                dbCommand.Connection = dbConnection;
                dbCommand.CommandType = commandType;
                dbCommand.CommandText = sql;
                var pars = CheckNullParameter(parameters);
                if (!parameters.IsNullOrEmpty())
                {
                    dbCommand.Parameters.AddRange(pars);
                }

                IDataReader reader;
                this.LogFormatter(sql, commandType, parameters);
                try
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    reader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    throw NewException(ex, sql, pars, commandType);
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

        #endregion

        #region QueryBuilder

        /// <summary>
        /// 构造一个查询命令
        /// </summary>
        /// <typeparam name="T">查询结果实体类型</typeparam>
        /// <returns></returns>
        public IQuery<T> Query<T>()
        {
            return new Query<T>(this);
        }

        /// <summary>
        /// 按查询条件获取第一条数据，如果没有返回空
        /// </summary>
        /// <typeparam name="T">查询结果实体类型</typeparam>
        /// <param name="condition">条件表达式</param>
        /// <returns>返回符合条件的第一个实体，没有返回null</returns>
        public T GetFirstOrDefault<T>(Expression<Func<T, bool>> condition = null)
        {
            var query = this.Query<T>();
            if (condition != null)
            {
                return query.FirstOrDefault(condition);
            }
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 根据主键值获取实体对象
        /// </summary>
        /// <typeparam name="T">查询结果实体类型</typeparam>
        /// <param name="keys">主键值</param>
        /// <returns>返回主键等于主键值的实体对象，没有返回null</returns>
        public T GetByKey<T>(params object[] keys)
        {
            if (keys.IsNullOrEmpty())
            {
                return default(T);
            }

            var keyCondition = MapperConfig.GetKeysExpression<T>(keys);
            if (keyCondition != null)
            {
                return GetFirstOrDefault(keyCondition);
            }

            return default(T);
            //var keyMeta = MapperConfig.GetKeyMetas<T>();
            //if (keyMeta == null)
            //{
            //    throw Error.Exception("没有为模型" + typeof(T).FullName + "指定主键。");
            //}

            //if (keys.Length != keyMeta.Length)
            //{
            //    return default(T);
            //}

            //LambdaExpression body = null;
            //ParameterExpression parameter = null;
            //for (var i = 0; i < keyMeta.Length; i++)
            //{
            //    var meta = keyMeta[i];
            //    if (meta.Expression is LambdaExpression exp)
            //    {
            //        parameter = exp.Parameters.FirstOrDefault();
            //        var keyExp = Expression.Constant(keys[i]);
            //        var mem = exp.Body.ChangeType(keyExp.Type);
            //        var condition = Expression.Equal(keyExp, mem);
            //        var innerLambda = Expression.Lambda(condition, parameter);

            //        body = body.AndAlso(innerLambda);
            //    }
            //}

            //if (body == null)
            //{
            //    return default(T);
            //}
            //var lambda = Expression.Lambda<Func<T, bool>>(body.Body, parameter);
            //return GetFirstOrDefault(lambda);
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
        /// 根据实体更新数据库
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="newEntity">该实体的字段值为需要更新的字段值</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool Update<T>(T newEntity)
        {
            return new Update<T>(this, newEntity).Execute() > 0;
        }

        /// <summary>
        /// 根据两个实体的比较更新数据库
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="oldEntity">条件获取该实体中的主键</param>
        /// <param name="newEntity">比较该实体和<paramref name="oldEntity"/>的字段得到需要更新的字段</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool Update<T>(T oldEntity, T newEntity)
        {
            return new Update<T>(this, oldEntity, newEntity).Execute() > 0;
        }

        /// <summary>
        /// 根据一个实体和实体的主键更新记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="newEntity">该实体的字段值为需要更新的字段值</param>
        /// <param name="include">包含或者排除，true包含表示仅更新指定的字段，false排除表示不更新指定的字段</param>
        /// <param name="fields">要更新或要排除的字段</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool Update<T>(T newEntity, bool include = false, params Expression<Func<T, object>>[] fields)
        {
            return new Update<T>(this, newEntity, include, fields).Execute() > 0;
        }

        /// <summary>
        /// 根据一个实体和一个条件更新满足条件的记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="newEntity">该实体的字段值为需要更新的字段值</param>
        /// <param name="expression">更新的条件，如果为空表示更新所有记录</param>
        /// <param name="include">包含或者排除，true包含表示仅更新指定的字段，false排除表示不更新指定的字段</param>
        /// <param name="fields">要更新或要排除的字段</param>
        /// <returns>返回更新的行数</returns>
        public int Update<T>(T newEntity, Expression<Func<T, bool>> expression, bool include = false, params Expression<Func<T, object>>[] fields)
        {
            return new Update<T>(this, newEntity, expression, include, fields).Execute();
        }

        /// <summary>
        /// 根据指定的字段值列字典更新满足条件的记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="fieldValues">要更新的字段和值的字典</param>
        /// <param name="expression">更新的条件，如果为空表示更新所有记录</param>
        /// <returns>返回更新的行数</returns>
        public int Update<T>(IDictionary<string, object> fieldValues, Expression<Func<T, bool>> expression)
        {
            return new Update<T>(this, fieldValues, expression).Execute();
        }

        #endregion

        #region InsertBuilder

        /// <summary>
        /// 根据指定的实体插入一条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool Insert<T>(T entity)
        {
            return new Insert<T>(this, entity).Execute() > 0;
        }

        /// <summary>
        /// 根据实体和指定的字段插入一条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <param name="fields">要插入的字段</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool Insert<T>(T entity, params Expression<Func<T, object>>[] fields)
        {
            return new Insert<T>(this, entity, true, fields).Execute() > 0;
        }

        /// <summary>
        /// 根据实体和指定的字段插入一条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <param name="include">包含或者排除，true包含表示仅插入指定的字段，false排除表示不插入指定的字段</param>
        /// <param name="fields">指定的字段</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool Insert<T>(T entity, bool include = false, params Expression<Func<T, object>>[] fields)
        {
            return new Insert<T>(this, entity, include, fields).Execute() > 0;
        }

        /// <summary>
        /// 根据指定的字段和值插入一条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="fieldValues">要插入的字段和值的字典</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool Insert<T>(IDictionary<string, object> fieldValues)
        {
            return new Insert<T>(this, fieldValues).Execute() > 0;
        }

        #region InsertList
        /// <summary>
        /// 插入多条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Insert<T>(IEnumerable<T> entities)
        {
            return new InsertList<T>(this, entities).Execute();
        }
        /// <summary>
        /// 根据实体和指定的字段插入多条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">要插入的实体</param>
        /// <returns>成功返回true，否则返回false</returns>
        public int Insert<T>(params T[] entities)
        {
            return new InsertList<T>(this, entities).Execute();
        }

        /// <summary>
        /// 根据实体和指定的字段插入多条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">要插入的实体</param>
        /// <param name="fields">要插入的字段</param>
        /// <returns>成功返回true，否则返回false</returns>
        public int Insert<T>(IEnumerable<T> entities, params Expression<Func<T, object>>[] fields)
        {
            return new InsertList<T>(this, entities, true, fields).Execute();
        }

        /// <summary>
        /// 根据实体和指定的字段插入一条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">要插入的实体</param>
        /// <param name="include">包含或者排除，true包含表示仅插入指定的字段，false排除表示不插入指定的字段</param>
        /// <param name="fields">指定的字段</param>
        /// <returns>成功返回true，否则返回false</returns>
        public int Insert<T>(IEnumerable<T> entities, bool include = false, params Expression<Func<T, object>>[] fields)
        {
            return new InsertList<T>(this, entities, include, fields).Execute();
        }

        /// <summary>
        /// 根据指定的字段和值插入一条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="fieldValues">要插入的字段和值的字典</param>
        /// <returns>成功返回true，否则返回false</returns>
        public int Insert<T>(IEnumerable<IDictionary<string, object>> fieldValues)
        {
            return new InsertList<T>(this, fieldValues).Execute();
        }

        #endregion

        #endregion

        #region DeleteBuilder

        /// <summary>
        /// 根据主键值删除一条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="keys">主键值</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool DeleteByKey<T>(params object[] keys)
        {
            return new Delete<T>(this, keys).Execute() > 0;
        }

        /// <summary>
        /// 根据指定实体的主键值删除一条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">要删除的实体，根据该实体的主键删除</param>
        /// <returns>成功返回true，否则返回false</returns>
        public bool Delete<T>(T entity)
        {
            return new Delete<T>(this, entity).Execute() > 0;
        }

        /// <summary>
        /// 根据条件删除记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="expression">条件表达式，如果为空表示删除所有</param>
        /// <returns>返回删除的行数</returns>
        public int DeleteBy<T>(Expression<Func<T, bool>> expression)
        {
            return new Delete<T>(this, expression).Execute();
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

        private DbParameter[] CheckNullParameter(params DbParameter[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null)
                    parameter.Value = DBNull.Value;
            }
            return parameters;
        }

        #endregion

        //#region Transcation


        //public XContext BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        //{
        //    var ctx = new XContext(this.ConnectionString, this.ProviderName, isolationLevel);
        //    ctx.SqlLog = this.SqlLog;
        //    return ctx;
        //}

        //#endregion
    }
}
