using System.Collections.Generic;
using System.Linq;

namespace XData.XBuilder
{
    /// <summary>
    /// 复合查询
    /// </summary>
    /// <typeparam name="TInner"></typeparam>
    /// <typeparam name="T"></typeparam>
    internal class ComplexQuery<TInner, T> : Query<T>
    {
        /// <summary>
        /// 内部数据提供者
        /// </summary>
        public IQuery<TInner> Privoder
        {
            get
            {
                return _privoder;
            }
        }

        /// <summary>
        /// 内部数据提供者
        /// </summary>
        private Query<TInner> _privoder;

        /// <summary>
        /// 构造一个复合查询
        /// </summary>
        /// <param name="privoder"></param>
        /// <param name="tablaName"></param>
        /// <param name="fieldPart"></param>
        internal ComplexQuery(Query<TInner> privoder, string tablaName, string fieldPart)
            : base(privoder.Context, tablaName)
        {
            this._useCache = privoder._useCache;
            this._privoder = privoder;
            this._fieldsPart = fieldPart;
        }

        public override IReadOnlyList<object> Parameters
        {
            get
            {
                var ps = this._parameters.ToList();
                if (this.Privoder != null)
                {
                    ps.AddRange(this.Privoder.Parameters);
                }
                if (this.where != null)
                {
                    ps.AddRange(this.where._parameters);
                }
                return ps.AsReadOnly();
            }
        }

        protected override string GetTableNameOrInnerSql()
        {
            var innerSql = this._privoder.ToInnerSql();
            return string.Format("({0}) AS {1}", innerSql, EscapeSqlIdentifier(_tableName));
        }
    }
}