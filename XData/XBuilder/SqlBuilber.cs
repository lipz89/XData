using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;
using XData.Core;
using XData.Extentions;
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
        internal int parameterIndex = 0;
        internal TableMeta tableMeta;
        internal string tableName;
        internal TypeVisitor typeVisitor = new TypeVisitor();
        internal NamedType namedType;
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
            var index = this.parameterIndex;
            string s = string.Format("{0}{1}", Context.DatabaseType.GetParameterPrefix(Context.ConnectionString), index);
            parameterIndex++;
            return s;
        }
        #endregion

        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns></returns>
        public abstract string ToSql();

        //internal virtual string GetFieldSql(string field)
        //{
        //    throw Error.NotSupportedException("");
        //}
    }
}
