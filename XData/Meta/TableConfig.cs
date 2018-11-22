using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XData.Common;
using XData.Extentions;

namespace XData.Meta
{
    public class TableConfig
    {
        public TableConfig(Type type)
        {
            Type = type;
            foreach (var info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (DbTypes.ContainsType(info.PropertyType))
                {
                    Columns.Add(new ColumnConfig(info, type));
                }
            }
            foreach (var info in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (DbTypes.ContainsType(info.FieldType))
                {
                    Columns.Add(new ColumnConfig(info, type));
                }
            }
        }
        public Type Type { get; }
        public string TableName { get; internal set; }
        public List<ColumnConfig> Columns { get; internal set; }
        public ColumnConfig[] Keys { get; internal set; }

        #region Override Object Methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is TableConfig)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Type.AssemblyQualifiedName.GetHashCode() ^ Constans.HashCodeXOr;
        }

        public override string ToString()
        {
            return $"TableName:{TableName}, Type:{Type.FullName}";
        }

        #endregion

        private void CheckType<T>(Exception exception)
        {
            if (typeof(T) != Type)
            {
                throw exception;
            }
        }

        public TableConfig HasTableName(string tableName)
        {
            if (!tableName.IsNullOrWhiteSpace())
                throw Error.ArgumentNullException(nameof(tableName));

            this.TableName = tableName;
            return this;
        }

        public TableConfig HasKey<T>(params Expression<Func<T, object>>[] properties)
        {
            if (properties.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(properties));
            }
            CheckType<T>(Error.Exception("表类型错误。"));

            if (!this.Keys.IsNullOrEmpty())
            {
                var keys = string.Join(",", this.Keys.Select(x => x.ColumnName));
                throw Error.Exception($"已经设置了类型 {Type.FullName} 对应的主键为 {keys}，不能重复设置。");
            }

            var members = properties.Select(x => x.GetMember()).Distinct().ToArray();

            if (members.Any(x => !x.IsPropertyOrField()))
            {
                throw Error.ArgumentException("参数中包含不是属性或字段的表达式。", nameof(properties));
            }

            this.Keys = new ColumnConfig[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                this.Keys[i] = this.Columns.First(x => x.MemberName == members[i].Name);
                this.Keys[i].CanUpdate = false;
                this.Keys[i].IsKey = true;
                this.Keys[i].IsIgnore = false;
            }

            return this;
        }

        public TableConfig HasIdentity<T>(Expression<Func<T, object>> property)
        {
            if (property == null)
            {
                throw Error.ArgumentNullException(nameof(property));
            }
            CheckType<T>(Error.Exception("表类型错误。"));

            var member = property.GetMember();

            if (!member.IsPropertyOrField())
            {
                throw Error.ArgumentException("参数不是属性或字段的表达式。", nameof(property));
            }

            var id = this.Columns.FirstOrDefault(x => x.IsIdentity);
            if (id != null && id.MemberName != member.Name)
            {
                throw Error.Exception($"已经设置了类型 {Type.FullName} 对应的自增列为 {id.MemberName}，不能重复设置。");
            }

            id = this.Columns.First(x => x.MemberName == member.Name);
            id.IsIdentity = true;
            id.CanInsert = false;

            return this;
        }

        public TableConfig HasIdentityKey<T>(Expression<Func<T, object>> property)
        {
            this.HasIdentity(property);
            this.HasKey(property);
            return this;
        }

        public TableConfig Ignore<T>(params Expression<Func<T, object>>[] properties)
        {
            if (properties.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(properties));
            }
            CheckType<T>(Error.Exception("表类型错误。"));
            foreach (var property in properties)
            {
                var member = property?.GetMember();
                if (member?.IsPropertyOrField() != true)
                {
                    throw Error.ArgumentException("指定的表达式不是属性或字段。", nameof(properties));
                }

                var col = this.Columns.First(x => x.MemberName == member.Name);

                if (col.IsKey)
                {
                    throw Error.Exception($"属性 {member.Name} 已经配置为主键，不能忽略。");
                }
                if (col.IsIdentity)
                {
                    throw Error.Exception($"属性 {member.Name} 已经配置为自增列，不能忽略。");
                }

                col.IsIgnore = true;
            }

            return this;
        }

        public TableConfig HasColumnName<T>(Expression<Func<T, object>> property, string columnName)
        {
            if (property == null)
            {
                throw Error.ArgumentNullException(nameof(property));
            }
            CheckType<T>(Error.Exception("表类型错误。"));

            if (columnName.IsNullOrWhiteSpace())
            {
                throw Error.ArgumentNullException(nameof(columnName));
            }

            var member = property.GetMember();

            if (!member.IsPropertyOrField())
            {
                throw Error.ArgumentException("参数不是属性或字段的表达式。", nameof(property));
            }

            var hasThisName = this.Columns.FirstOrDefault(x => x.MemberName != member.Name && x.ColumnName == columnName);
            if (hasThisName != null)
            {
                throw Error.Exception($"已有属性 {hasThisName.MemberName} 设置列名为 {columnName}。");
            }
            var id = this.Columns.First(x => x.MemberName == member.Name);
            id.ColumnName = columnName;
            return this;
        }
    }
}