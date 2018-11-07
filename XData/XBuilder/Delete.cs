using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Core;
using XData.Extentions;
using XData.Meta;

namespace XData.XBuilder
{
    /// <summary>
    /// ɾ������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Delete<T> : SqlBuilber, IExecutable
    {
        #region Fields
        private Where<T> where;
        private readonly bool hasKeyWhere;
        #endregion

        #region Properties
        /// <summary>
        /// �����б�
        /// </summary>
        public override IReadOnlyList<object> Parameters
        {
            get
            {
                var ps = this.parameters.ToList();
                if (this.where != null)
                {
                    ps.AddRange(this.where.parameters);
                }
                return ps.AsReadOnly();
            }
        }

        #endregion

        #region Constuctors
        /// <summary>
        /// ����һ��ɾ������
        /// </summary>
        /// <param name="context"></param>
        private Delete(XContext context) : base(context)
        {
            this.tableMeta = TableMeta.From<T>();
            this.tableName = this.tableMeta.TableName;
            this.namedType = new NamedType(this.tableMeta.Type, this.tableName);
            this.typeVisitor.Add(this.namedType);
        }
        internal Delete(XContext context, Expression<Func<T, bool>> expression)
            : this(context)
        {
            if (expression != null)
            {
                this.Where(expression);
            }
        }
        internal Delete(XContext context, object primaryValue)
            : this(context)
        {
            if (primaryValue == null)
            {
                throw Error.ArgumentNullException(nameof(primaryValue));
            }

            var exp = tableMeta.Key?.Expression as LambdaExpression;
            if (exp != null)
            {
                var keyExp = Expression.Constant(primaryValue);
                var mem = exp.Body.ChangeType(keyExp.Type);
                var newExp = Expression.Equal(mem, keyExp);
                var lambda = Expression.Lambda<Func<T, bool>>(newExp, exp.Parameters);
                this.Where(lambda);
                hasKeyWhere = true;
            }
            else
            {
                throw Error.Exception("û��Ϊ���� " + tableMeta.Type.Name + " ָ��������");
            }
        }

        internal Delete(XContext context, T entity, Expression<Func<T, object>> primaryKey)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }

            var exp = tableMeta.Key?.Expression as LambdaExpression;
            if (exp == null && primaryKey != null)
            {
                MapperConfig.HasKey(primaryKey);
                exp = primaryKey;
            }
            if (exp != null)
            {
                var val = exp.Compile().DynamicInvoke(entity);
                var keyExp = Expression.Constant(val);
                var mem = exp.Body.ChangeType(keyExp.Type);
                var newExp = Expression.Equal(mem, keyExp);
                var lambda = Expression.Lambda<Func<T, bool>>(newExp, exp.Parameters);
                this.Where(lambda);
                hasKeyWhere = true;
            }
            else
            {
                throw Error.Exception("û��Ϊ���� " + tableMeta.Type.Name + " ָ��������");
            }
        }
        #endregion

        #region Where

        /// <summary>
        /// ����ɾ������
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private void Where(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            if (hasKeyWhere)
            {
                throw Error.Exception("�Ѿ�ָ����������ΪWhere������");
            }
            this.where = this.where ?? new Where<T>(Context, this);
            this.where.Add(expression);
        }

        #endregion

        #region IExecutable

        /// <summary>
        /// ִ��ɾ������
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            return Context.ExecuteNonQuery(this.ToSql(), this.DbParameters);
        }
        #endregion

        #region SqlBuilder
        /// <summary>
        /// ת����Sqlɾ�����
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            this.parameterIndex = 0;
            return $"DELETE FROM {tableName} {this.where?.ToSql()}";
        }
        #endregion
    }
}