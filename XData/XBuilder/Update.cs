using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Extentions;
using XData.Meta;

namespace XData.XBuilder
{
    /// <summary>
    /// ��������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Update<T> : SqlBuilber, IExecutable
    {
        #region Fields
        private readonly bool _hasWhere;
        private readonly string setterString = string.Empty;
        private Where<T> where;
        #endregion

        #region Properties
        /// <summary>
        /// �����б�
        /// </summary>
        public override IReadOnlyList<object> Parameters
        {
            get
            {
                var ps = this._parameters.ToList();
                if (this.where != null)
                {
                    ps.AddRange(this.where._parameters);
                }
                return ps.AsReadOnly();
            }
        }
        #endregion

        #region Constuctors

        /// <summary>
        /// ��������ʵ��ıȽϹ���һ����������
        /// </summary>
        /// <param name="context"></param>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <param name="primaryKey"></param>
        internal Update(XContext context, T oldEntity, T newEntity, Expression<Func<T, object>> primaryKey = null) : base(context)
        {
            if (oldEntity == null)
            {
                throw Error.ArgumentNullException(nameof(oldEntity));
            }
            if (newEntity == null)
            {
                throw Error.ArgumentNullException(nameof(newEntity));
            }

            var tableMeta = TableMeta.From<T>();
            var keyMeta = tableMeta.Key;

            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member).ToList();
            foreach (var column in columns)
            {
                var memAccess = column.Member.GetMemberAccess<T>()?.Compile();
                if (memAccess != null)
                {
                    var oldValue = memAccess(oldEntity);
                    var newValue = memAccess(newEntity);
                    if (!oldValue.Equals(newValue))
                    {
                        this.setterString += string.Format("{0}={1},",
                            EscapeSqlIdentifier(column.ColumnName), GetParameterIndex());
                        this._parameters.Add(newValue);
                    }
                }
            }
            if (this._parameters.Any())
            {
                setterString = setterString.Substring(0, setterString.Length - 1);
            }
            else
            {
                throw Error.Exception("�����������һ���ֶΡ�");
            }

            var exp = keyMeta?.Expression as LambdaExpression ?? primaryKey;
            if (exp != null)
            {
                var val = exp.Compile().DynamicInvoke(oldEntity);
                var mem = exp.Body;
                if (mem is UnaryExpression)
                {
                    mem = (mem as UnaryExpression).Operand;
                }
                var newExp = Expression.Equal(mem, Expression.Constant(val));
                var lambda = Expression.Lambda<Func<T, bool>>(newExp, exp.Parameters);
                this.where = new Where<T>(Context);
                this.where.Add(lambda);
                _hasWhere = true;
            }
        }
        /// <summary>
        /// ����ָ��ʵ�幹��һ������ָ���ֶε�����
        /// </summary>
        /// <param name="context"></param>
        /// <param name="newEntity"></param>
        /// <param name="include">���������ų���true������ʾ������ָ�����ֶΣ�false�ų���ʾ������ָ�����ֶ�</param>
        /// <param name="fields"></param>
        internal Update(XContext context, T newEntity, bool include = false, params Expression<Func<T, object>>[] fields) : base(context)
        {
            if (newEntity == null)
            {
                throw Error.ArgumentNullException(nameof(newEntity));
            }

            var tableMeta = TableMeta.From<T>();
            var keyMeta = tableMeta.Key;

            var exceptFields = fields.Select(x => x.GetPropertyName());
            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member);
            columns = columns.Where(x => exceptFields.Contains(x.Member.Name) == include).ToList();
            foreach (var column in columns)
            {
                var memAccess = column.Member.GetMemberAccess<T>()?.Compile();
                if (memAccess != null)
                {
                    var newValue = memAccess(newEntity);

                    this.setterString += string.Format("{0}={1},",
                        EscapeSqlIdentifier(column.ColumnName), GetParameterIndex());
                    this._parameters.Add(newValue);
                }
            }
            if (this._parameters.Any())
            {
                setterString = setterString.Substring(0, setterString.Length - 1);
            }
            else
            {
                throw Error.Exception("�����������һ���ֶΡ�");
            }

            var exp = keyMeta?.Expression as LambdaExpression;
            if (exp != null)
            {
                var val = exp.Compile().DynamicInvoke(newEntity);
                var mem = exp.Body;
                if (mem is UnaryExpression)
                {
                    mem = (mem as UnaryExpression).Operand;
                }
                var newExp = Expression.Equal(mem, Expression.Constant(val));
                var lambda = Expression.Lambda<Func<T, bool>>(newExp, exp.Parameters);
                this.where = new Where<T>(Context);
                this.where.Add(lambda);
                _hasWhere = true;
            }
        }

        /// <summary>
        /// ����ָ�����ֶ�ֵ���ֵ乹��һ����������
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fieldValues"></param>
        internal Update(XContext context, IDictionary<string, object> fieldValues) : base(context)
        {
            if (fieldValues.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(fieldValues));
            }

            var tableMeta = TableMeta.From<T>();
            var keyMeta = tableMeta.Key;

            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member && fieldValues.Keys.Contains(x.Member.Name)).ToList();
            foreach (var column in columns)
            {
                var memAccess = column.Member.GetMemberAccess<T>()?.Compile();
                if (memAccess != null)
                {
                    this.setterString += string.Format("{0}={1},",
                        EscapeSqlIdentifier(column.ColumnName), GetParameterIndex());
                    this._parameters.Add(fieldValues[column.Member.Name]);
                }
            }
            if (this._parameters.Any())
            {
                setterString = setterString.TrimEnd(',');
            }
            else
            {
                throw Error.Exception("�����������һ���ֶΡ�");
            }
        }

        #endregion

        #region Where
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Update<T> Where(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            if (_hasWhere)
            {
                throw Error.Exception("�Ѿ�ָ����������ΪWhere������");
            }
            this.where = this.where ?? new Where<T>(Context);
            this.where.Add(expression);
            return this;
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Update<T> WhereOr(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            if (_hasWhere)
            {
                throw Error.Exception("�Ѿ�ָ����������ΪWhere������");
            }
            this.where = this.where ?? new Where<T>(Context);
            this.where.AddOr(expression);
            return this;
        }
        /// <summary>
        /// �����ѯ����
        /// </summary>
        /// <returns></returns>
        public Update<T> ClearWhere()
        {
            if (!this._hasWhere)
            {
                this.where = null;
            }
            return this;
        }

        #endregion

        #region IExecutable
        /// <summary>
        /// ִ�и�������
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            return Context.ExecuteNonQuery(this.ToSql(), this.DbParameters);
        }
        #endregion

        #region SqlBuilder
        /// <summary>
        /// ת����Sql���
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            var tableName = GetTableName<T>();
            var wherePart = string.Empty;
            if (this.where != null)
            {
                this.where._parameterIndex = this._parameterIndex;
                wherePart = this.where.ToSql();
            }
            return string.Format("UPDATE {0} {1} {2}", tableName, this.setterString, wherePart);
        }
        #endregion
    }
}