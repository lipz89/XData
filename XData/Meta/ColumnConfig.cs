using System;
using System.Linq.Expressions;
using System.Reflection;
using XData.Common;
using XData.Common.Fast;
using XData.Extentions;

namespace XData.Meta
{
    public class ColumnConfig
    {
        public ColumnConfig(MemberInfo info, Type type)
        {
            this.Member = info;
            this.MemberName = this.Member.Name;
            this.TableType = type;
            this.Type = this.Member.GetMemberType();
            this.ColumnName = this.MemberName = this.Member.Name;
            this.CanWrite = this.Member.CanWrite();
            this.CanRead = this.Member.CanRead();
            this.Expression = ExpressionHelper.CreateGetterExpression(this.Member, this.TableType);
        }

        public string MemberName { get; }
        public MemberInfo Member { get; }
        public string ColumnName { get; internal set; }
        public Type Type { get; }
        public Type TableType { get; }
        public bool CanUpdate { get; internal set; } = true;
        public bool CanInsert { get; internal set; } = true;
        public bool CanWrite { get; internal set; } = true;
        public bool CanRead { get; internal set; } = true;
        public bool IsKey { get; internal set; }
        public bool IsIdentity { get; internal set; }
        public bool IsIgnore { get; internal set; }
        public Expression Expression { get; }

        public void SetValue(object target, object value)
        {
            if (Member is PropertyInfo propertyInfo)
            {
                propertyInfo.FastSetValue(target, value);
            }
            else if (Member is FieldInfo fieldInfo)
            {
                fieldInfo.FastSetValue(target, value);
            }
        }

        public object GetValue(object target)
        {
            if (Member is PropertyInfo propertyInfo)
            {
                return propertyInfo.FastGetValue(target);
            }
            if (Member is FieldInfo fieldInfo)
            {
                return fieldInfo.FastGetValue(target);
            }
            return null;
        }

        #region Override Object Methods

        public override int GetHashCode()
        {
            return this.MemberName.GetHashCode() ^ this.TableType.AssemblyQualifiedName.GetHashCode() ^ Constans.HashCodeXOr;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is ColumnConfig)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        public override string ToString()
        {
            return $"ColumnName:{ColumnName},Member:{Member}";
        }

        #endregion
    }
}