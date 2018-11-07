using System;
using System.Collections.Generic;
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
    /// ��������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Insert<T> : SqlBuilber, IExecutable
    {
        #region Fields
        private readonly Strings fieldString = new Strings();
        private readonly Strings valueString = new Strings();
        private bool hasIdentity;
        private MemberInfo identity;
        private T cachedEntity;
        #endregion

        #region Constuctors

        private Insert(XContext context) : base(context)
        {
            this.tableMeta = TableMeta.From<T>();
            this.tableName = this.tableMeta.TableName;
            this.namedType = new NamedType(this.tableMeta.Type, this.tableName);
            this.typeVisitor.Add(this.namedType);
        }

        /// <summary>
        /// ����ָ����ʵ�幹��һ����������
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        internal Insert(XContext context, T entity)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }

            var columns = tableMeta.Columns.Where(x => x.CanInsert).ToList();
            foreach (var column in columns)
            {
                fieldString.Add(EscapeSqlIdentifier(column.ColumnName));
                valueString.Add(GetParameterIndex());
                this.parameters.Add(column.GetValue(entity));
            }

            CheckIdentity(entity);
        }

        /// <summary>
        /// ����ʵ���ָ�����ֶι���һ����������
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="include">���������ų���true������ʾ������ָ�����ֶΣ�false�ų���ʾ������ָ�����ֶ�</param>
        /// <param name="fields"></param>
        internal Insert(XContext context, T entity, bool include = false, params Expression<Func<T, object>>[] fields)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }
            if (fields.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(fields));
            }
            if (fields.Any(x => x == null))
            {
                throw Error.ArgumentException("ָ���ı��ʽ�а�����Ԫ�ء�", nameof(fields));
            }
            if (fields.Any(x => x.GetMember() == null))
            {
                throw Error.ArgumentException("ָ���ı��ʽ�а������ǳ�Ա�������͵�Ԫ�ء�", nameof(fields));
            }

            var exceptFields = fields.Select(x => x.GetPropertyName());
            var columns = tableMeta.Columns.Where(x => x.CanInsert && exceptFields.Contains(x.Member.Name) == include).ToList();
            //if (columns.IsNullOrEmpty())
            //{
            //    throw Error.ArgumentException("�����������һ���ֶΡ�", nameof(fields));
            //}
            foreach (var column in columns)
            {
                fieldString.Add(EscapeSqlIdentifier(column.ColumnName));
                valueString.Add(GetParameterIndex());
                this.parameters.Add(column.GetValue(entity));
            }
            CheckIdentity(entity);
        }

        /// <summary>
        /// ����ָ�����ֶκ�ֵ����һ����������
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fieldValues"></param>
        internal Insert(XContext context, IDictionary<string, object> fieldValues)
            : this(context)
        {
            if (fieldValues.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(fieldValues));
            }

            var columns = tableMeta.Columns.Where(x => x.CanInsert && fieldValues.Keys.Contains(x.Member.Name)).ToList();
            //if (columns.IsNullOrEmpty())
            //{
            //    throw Error.Exception("�����������һ���ֶΡ�");
            //}
            foreach (var column in columns)
            {
                fieldString.Add(EscapeSqlIdentifier(column.ColumnName));
                valueString.Add(GetParameterIndex());
                this.parameters.Add(fieldValues[column.Member.Name]);
            }
        }
        #endregion

        private void CheckIdentity(T entity)
        {
            var meta = MapperConfig.GetIdentity<T>();
            if (meta != null)
            {
                hasIdentity = true;
                identity = meta.Member;
                cachedEntity = entity;
            }
        }

        #region IExecutable
        /// <summary>
        /// ִ�в������
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            if (!hasIdentity)
            {
                return Context.ExecuteNonQuery(this.ToSql(), this.DbParameters);
            }

            var id = Context.ExecuteScalar(this.ToSql() + "; SELECT @@IDENTITY", this.DbParameters);
            if (id != null && id != DBNull.Value)
            {
                var par = Expression.Parameter(typeof(T));
                var parv = Expression.Parameter(identity.GetMemberType());
                var mem = Expression.MakeMemberAccess(par, identity);
                var set = Expression.Assign(mem, parv);
                var lbd = Expression.Lambda(set, par, parv);
                var act = lbd.Compile();
                var iid = Mappers.GetMapper(parv.Type, id.GetType())(id);
                act.DynamicInvoke(cachedEntity, iid);
                return 1;
            }

            return 0;
        }
        #endregion

        #region SqlBuilder
        /// <summary>
        /// ת����Sql���
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            this.parameterIndex = 0;
            return $"INSERT INTO {tableName} ({fieldString}) VALUES ({valueString})";
        }
        #endregion
    }
}