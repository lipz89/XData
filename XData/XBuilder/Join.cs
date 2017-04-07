using System;
using System.Linq.Expressions;

using XData.Common;

namespace XData.XBuilder
{
    /// <summary>
    /// 连接查询
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class Join<T1, T2> : SqlBuilber
    {
        private readonly JoinType joinType;
        private readonly Expression<Func<T1, T2, bool>> expression;
        /// <summary>
        /// 左表
        /// </summary>
        public T1 Left { get; set; }
        /// <summary>
        /// 右表
        /// </summary>
        public T2 Right { get; set; }

        internal Join(XContext context, JoinType joinType, Expression<Func<T1, T2, bool>> on = null) : base(context)
        {
            if (!Enum.IsDefined(typeof(JoinType), joinType))
            {
                throw Error.ArgumentException("未定义的连接类型：" + joinType, nameof(joinType));
            }
            if (joinType != JoinType.Cross && on == null)
            {
                throw Error.ArgumentException("连接类型为" + joinType + "时，连接条件不能为空。", nameof(on));
            }
            this.joinType = joinType;
            this.expression = @on;
        }

        #region 复合连接
        /// <summary>
        /// 在连接查询的基础上再做内连接查询
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public Join<Join<T1, T2>, T3> InnerJoin<T3>(Expression<Func<Join<T1, T2>, T3, bool>> on)
        {
            return new Join<Join<T1, T2>, T3>(this.Context, JoinType.Inner, on);
        }
        /// <summary>
        /// 在连接查询的基础上再做左外连接查询
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public Join<Join<T1, T2>, T3> LeftJoin<T3>(Expression<Func<Join<T1, T2>, T3, bool>> on)
        {
            return new Join<Join<T1, T2>, T3>(this.Context, JoinType.Left, on);
        }
        /// <summary>
        /// 在连接查询的基础上再做右外连接查询
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public Join<Join<T1, T2>, T3> RightJoin<T3>(Expression<Func<Join<T1, T2>, T3, bool>> on)
        {
            return new Join<Join<T1, T2>, T3>(this.Context, JoinType.Right, on);
        }
        /// <summary>
        /// 在连接查询的基础上再做外连接查询
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public Join<Join<T1, T2>, T3> FullJoin<T3>(Expression<Func<Join<T1, T2>, T3, bool>> on)
        {
            return new Join<Join<T1, T2>, T3>(this.Context, JoinType.Full, on);
        }
        /// <summary>
        /// 在连接查询的基础上再做交叉查询
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public Join<Join<T1, T2>, T3> CrossJoin<T3>()
        {
            return new Join<Join<T1, T2>, T3>(this.Context, JoinType.Cross);
        }
        #endregion

        #region Select

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, TResult>> selector)
        {
            return new Query<TResult>(this.Context);
        }

        #endregion

        private string GetJoinType()
        {
            switch (joinType)
            {
                case JoinType.Inner:
                    return "INNER JOIN";
                case JoinType.Left:
                    return "LEFT JOIN";
                case JoinType.Right:
                    return "RIGHT JOIN";
                case JoinType.Full:
                    return "FULL JOIN";
                case JoinType.Cross:
                    return "CROSS JOIN";
            }
            return string.Empty;
        }

        public override string ToSql()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 连接类型
    /// </summary>
    public enum JoinType
    {
        /// <summary> 内连接 </summary>
        Inner,
        /// <summary> 左外连接 </summary>
        Left,
        /// <summary> 右外连接 </summary>
        Right,
        /// <summary> 外连接 </summary>
        Full,
        /// <summary> 交叉连接 </summary>
        Cross
    }
}
