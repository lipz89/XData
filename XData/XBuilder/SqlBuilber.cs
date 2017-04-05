﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XData.Meta;

namespace XData.XBuilder
{
    /// <summary>
    /// Sql构造器
    /// </summary>
    public abstract class SqlBuilber
    {
        #region Fields
        internal readonly List<object> _parameters = new List<object>();
        internal ParameterExpression _parameter;
        internal int _parameterIndex = 0;
        #endregion

        #region Properties
        /// <summary>
        /// 上下文对象
        /// </summary>
        public XContext Context { get; internal set; }

        /// <summary>
        /// Sql参数列表
        /// </summary>
        public virtual IReadOnlyList<object> Parameters
        {
            get
            {
                return this._parameters.AsReadOnly();
            }
        }

        internal DbParameter[] DbParameters
        {
            get { return Context.ConvertParameters(this.Parameters.ToArray()); }
        }

        #endregion

        #region Contructors

        /// <summary>
        /// 构造一个Sql构造器
        /// </summary>
        /// <param name="Context"></param>
        protected SqlBuilber(XContext Context)
        {
            this.Context = Context;
        }
        #endregion

        #region 标识符处理方法

        /// <summary>
        /// 转码标识符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected internal string EscapeSqlIdentifier(string str)
        {
            return Context.DatabaseType.EscapeSqlIdentifier(str);
        }
        /// <summary>
        /// 获取下一个以数字排序命名的参数名称字符串
        /// </summary>
        /// <returns></returns>
        protected internal string GetParameterIndex()
        {
            var index = _parameterIndex;
            string s = string.Format("{0}{1}", Context.DatabaseType.GetParameterPrefix(Context.ConnectionString), index);
            _parameterIndex++;
            return s;
        }

        /// <summary>
        /// 返回转码后的表名
        /// 转码规则例如SqlServer增加方括号
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected internal string GetTableName<T>()
        {
            return Context.DatabaseType.EscapeTableName(MetaConfig.GetTableName<T>());
        }
        /// <summary>
        /// 返回转码后的表名
        /// 转码规则例如SqlServer增加方括号
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected internal string GetTableName(Type type)
        {
            return Context.DatabaseType.EscapeTableName(MetaConfig.GetTableName(type));
        }
        /// <summary>
        /// 返回转码后的表名
        /// 转码规则例如SqlServer增加方括号
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        protected internal string GetColumnName<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            return EscapeSqlIdentifier(MetaConfig.GetColumnName<T, TProperty>(property));
        }

        /// <summary>
        /// 返回转码后的列名
        /// 转码规则例如SqlServer增加方括号
        /// </summary>
        /// <param name="property"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected internal string GetColumnName(MemberInfo property, Type type)
        {
            return EscapeSqlIdentifier(MetaConfig.GetColumnName(property, type));
        }
        /// <summary>
        /// 返回转码后的主键列名
        /// 转码规则例如SqlServer增加方括号
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected internal string GetKey<T>()
        {
            return EscapeSqlIdentifier(MetaConfig.GetKey<T>());
        }
        /// <summary>
        /// 返回转码后的主键列名
        /// 转码规则例如SqlServer增加方括号
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected internal string GetKey(Type type)
        {
            return EscapeSqlIdentifier(MetaConfig.GetKey(type));
        }
        #endregion

        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns></returns>
        public abstract string ToSql();
    }

    /// <summary>
    /// 可执行的Sql接口
    /// </summary>
    public interface IExecutable
    {
        /// <summary>
        /// 执行Sql命令
        /// </summary>
        /// <returns></returns>
        int Execute();
    }
}
