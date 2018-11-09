using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using XData.Core;
using XData.Meta;

namespace XData.XBuilder
{
    /// <summary>
    /// Sql构造器
    /// </summary>
    internal abstract class SqlBuilber
    {
        #region Fields
        internal readonly List<object> parameters = new List<object>();
        internal int parameterIndex = 0;
        internal TableMeta tableMeta;
        internal string tableName;
        internal readonly TypeVisitor typeVisitor = new TypeVisitor();
        internal NamedType namedType;
        #endregion

        #region Properties
        /// <summary>
        /// 上下文对象
        /// </summary>
        public XContext Context { get; set; }

        /// <summary>
        /// Sql参数列表
        /// </summary>
        public virtual IReadOnlyList<object> Parameters
        {
            get
            {
                return this.parameters.AsReadOnly();
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
        protected SqlBuilber()
        {
        }

        /// <summary>
        /// 构造一个Sql构造器
        /// </summary>
        /// <param name="context">Sql构造器所依附的数据库上下文</param>
        protected SqlBuilber(XContext context)
        {
            this.Context = context;
        }
        #endregion

        #region 标识符处理方法

        /// <summary>
        /// 转码标识符
        /// </summary>
        /// <param name="str">要转码的标识符</param>
        /// <returns>转码后的标识符</returns>
        protected internal string EscapeSqlIdentifier(string str)
        {
            return Context.DatabaseType.EscapeSqlIdentifier(str);
        }
        /// <summary>
        /// 获取下一个以数字排序命名的参数名称字符串
        /// </summary>
        /// <returns>返回当前参数序号</returns>
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
        /// <returns>返回sql语句字符串</returns>
        public abstract string ToSql();

        //internal virtual string GetFieldSql(string field)
        //{
        //    throw Error.NotSupportedException("");
        //}
    }
}
