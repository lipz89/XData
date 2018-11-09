using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Common.Fast;
using XData.Core;
using XData.Extentions;
using XData.Meta;

namespace XData.XBuilder
{
    /// <summary>
    /// 更新命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Update<T> : SqlBuilber, IExecutable
    {
        #region Fields
        private bool hasKeyWhere;
        private readonly Strings setterString = new Strings();
        private Where<T> where;
        #endregion

        #region Properties
        /// <summary>
        /// 参数列表
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

        private Update(XContext context)
            : base(context)
        {
            this.tableMeta = TableMeta.From<T>();
            this.tableName = this.tableMeta.TableName;
            this.namedType = new NamedType(this.tableMeta.Type, this.tableName);
            this.typeVisitor.Add(this.namedType);
        }

        /// <summary>
        /// 根据两个实体的比较构造一个更新命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        internal Update(XContext context, T oldEntity, T newEntity)
            : this(context)
        {
            if (oldEntity == null)
            {
                throw Error.ArgumentNullException(nameof(oldEntity));
            }
            if (newEntity == null)
            {
                throw Error.ArgumentNullException(nameof(newEntity));
            }

            var columns = tableMeta.Columns.Where(x => x.CanUpdate).ToList();
            foreach (var column in columns)
            {
                var memAccess = ExpressionHelper.CreateGetterExpression<T, dynamic>(column.Member)?.Compile();
                if (memAccess != null)
                {
                    var newValue = memAccess(newEntity);
                    this.setterString.Add(string.Format("{0}={1}", EscapeSqlIdentifier(column.ColumnName), GetParameterIndex()));
                    this.parameters.Add(newValue);
                }
            }
            if (!this.parameters.Any())
            {
                throw Error.Exception("必须更新至少一个字段。");
            }

            AddConditionByKey(oldEntity);
        }

        /// <summary>
        /// 根据实体构造一个更新命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        internal Update(XContext context, T entity)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }

            var columns = tableMeta.Columns.Where(x => x.CanUpdate).ToList();
            foreach (var column in columns)
            {
                var memAccess = ExpressionHelper.CreateGetterExpression<T, dynamic>(column.Member)?.Compile();
                if (memAccess != null)
                {
                    var newValue = memAccess(entity);
                    this.setterString.Add(string.Format("{0}={1}", EscapeSqlIdentifier(column.ColumnName), GetParameterIndex()));
                    this.parameters.Add(newValue);
                }
            }
            if (!this.parameters.Any())
            {
                throw Error.Exception("必须更新至少一个字段。");
            }

            AddConditionByKey(entity);
        }

        private void AddConditionByKey(T oldEntity)
        {
            var condition = MapperConfig.GetKeysExpression<T>(oldEntity);
            if (condition != null)
            {
                this.Where(condition);
                this.hasKeyWhere = true;
            }
            //if (exp != null)
            //{
            //    var val = exp.Compile().DynamicInvoke(oldEntity);
            //    var keyExp = Expression.Constant(val);
            //    var mem = exp.Body.ChangeType(keyExp.Type);
            //    var newExp = Expression.Equal(mem, keyExp);
            //    var lambda = Expression.Lambda<Func<T, bool>>(newExp, exp.Parameters);
            //    this.Where(lambda);
            //    this.hasKeyWhere = true;
            //}
            //else
            //{
            //    throw Error.Exception("没有为类型 " + tableMeta.Type.Name + " 指定主键。");
            //}
        }

        /// <summary>
        /// 根据指定实体构造一个更新指定字段的命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="include">包含或者排除，true包含表示仅更新指定的字段，false排除表示不更新指定的字段</param>
        /// <param name="fields"></param>
        internal Update(XContext context, T entity, bool include, Expression<Func<T, object>>[] fields)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }

            var exceptFields = fields.Select(x => x.GetPropertyName());
            var columns = tableMeta.Columns.Where(x => x.CanUpdate);
            columns = columns.Where(x => exceptFields.Contains(x.Member.Name) == include).ToList();
            foreach (var column in columns)
            {
                var memAccess = ExpressionHelper.CreateGetterExpression<T, dynamic>(column.Member)?.Compile();
                if (memAccess != null)
                {
                    var newValue = memAccess(entity);

                    this.setterString.Add(string.Format("{0}={1}", EscapeSqlIdentifier(column.ColumnName), GetParameterIndex()));
                    this.parameters.Add(newValue);
                }
            }
            if (!this.parameters.Any())
            {
                throw Error.Exception("必须更新至少一个字段。");
            }
            AddConditionByKey(entity);
        }

        /// <summary>
        /// 根据指定实体构造一个更新指定字段的命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="expression"></param>
        /// <param name="include">包含或者排除，true包含表示仅更新指定的字段，false排除表示不更新指定的字段</param>
        /// <param name="fields"></param>
        internal Update(XContext context, T entity, Expression<Func<T, bool>> expression, bool include, Expression<Func<T, object>>[] fields)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }

            var exceptFields = fields.Select(x => x.GetPropertyName());
            var columns = tableMeta.Columns.Where(x => x.CanUpdate);
            columns = columns.Where(x => exceptFields.Contains(x.Member.Name) == include).ToList();
            foreach (var column in columns)
            {
                var memAccess = ExpressionHelper.CreateGetterExpression<T, dynamic>(column.Member)?.Compile();
                if (memAccess != null)
                {
                    var newValue = memAccess(entity);

                    this.setterString.Add(string.Format("{0}={1}", EscapeSqlIdentifier(column.ColumnName), GetParameterIndex()));
                    this.parameters.Add(newValue);
                }
            }
            if (!this.parameters.Any())
            {
                throw Error.Exception("必须更新至少一个字段。");
            }
            if (expression != null)
            {
                this.Where(expression);
            }
        }

        /// <summary>
        /// 根据指定的字段值列字典构造一个更新命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fieldValues"></param>
        /// <param name="expression"></param>
        internal Update(XContext context, IDictionary<string, object> fieldValues, Expression<Func<T, bool>> expression)
            : this(context)
        {
            if (fieldValues.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(fieldValues));
            }

            var columns = tableMeta.Columns.Where(x => x.CanUpdate && fieldValues.Keys.Contains(x.Member.Name)).ToList();
            foreach (var column in columns)
            {
                this.setterString.Add(string.Format("{0}={1}", EscapeSqlIdentifier(column.ColumnName), GetParameterIndex()));
                this.parameters.Add(fieldValues[column.Member.Name]);
            }
            if (!this.parameters.Any())
            {
                throw Error.Exception("必须更新至少一个字段。");
            }
            if (expression != null)
            {
                this.Where(expression);
            }
        }

        #endregion

        #region Where

        /// <summary>
        /// 更新条件
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
                throw Error.Exception("已经指定了主键列为Where条件。");
            }
            this.where = this.where ?? new Where<T>(Context, this);
            this.where.Add(expression);
        }

        #endregion

        #region IExecutable
        /// <summary>
        /// 执行更新命令
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            return Context.ExecuteNonQuery(this.ToSql(), this.DbParameters);
        }
        #endregion

        #region SqlBuilder
        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            return string.Format("UPDATE {0} SET {1} {2}", tableName, this.setterString, this.where?.ToSql());
        }
        #endregion
    }
}