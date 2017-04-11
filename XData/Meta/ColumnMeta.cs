using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;

namespace XData.Meta
{
    [DebuggerDisplay("ColumnName:{ColumnName}, Member:{Member}")]
    internal class ColumnMeta
    {
        #region Fields

        #endregion

        #region Constructors

        public ColumnMeta(MInfo member)
        {
            TableType = member.Type;
            Member = member.Member;
        }

        public ColumnMeta(MemberInfo member, Type type)
        {
            Member = member;
            TableType = type;
        }

        #endregion

        #region Properties

        public MemberInfo Member { get; }

        public Type TableType { get; }

        public string ColumnName { get; internal set; }

        public string Name
        {
            get { return Member?.Name; }
        }

        public Type Type
        {
            get
            {
                if (Member is PropertyInfo)
                {
                    return (Member as PropertyInfo).PropertyType;
                }
                if (Member is FieldInfo)
                {
                    return (Member as FieldInfo).FieldType;
                }
                return null;
            }
        }

        public bool CanUpdate
        {
            get
            {
                var k = MapperConfig.GetKeyMeta(TableType);
                if (k != null && k.Member.MetadataToken == Member.MetadataToken)
                {
                    return false;
                }
                var id = MapperConfig.GetIdentities(TableType);
                if (id != null && id.Member.MetadataToken == Member.MetadataToken)
                {
                    return false;
                }
                return true;
            }
        }

        public bool CanInsert
        {
            get
            {
                var id = MapperConfig.GetIdentities(TableType);
                if (id != null && id.Member.MetadataToken == Member.MetadataToken)
                {
                    return false;
                }
                return true;
            }
        }

        public Expression Expression { get; internal set; }

        #endregion

        #region Methods

        public void SetValue(object target, object value)
        {
            if (Member is PropertyInfo)
            {
                (Member as PropertyInfo).SetValue(target, value);
            }
            else if (Member is FieldInfo)
            {
                (Member as FieldInfo).SetValue(target, value);
            }
        }

        public object GetValue(object target)
        {
            if (Member is PropertyInfo)
            {
                return (Member as PropertyInfo).GetValue(target);
            }
            if (Member is FieldInfo)
            {
                return (Member as FieldInfo).GetValue(target);
            }
            return null;
        }

        public bool CanWrite()
        {
            if (Member is PropertyInfo)
            {
                return (Member as PropertyInfo).CanWrite;
            }
            if (Member is FieldInfo)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Override Object Methods

        public override int GetHashCode()
        {
            return this.Member.MetadataToken ^ this.TableType.MetadataToken ^ Constans.HashCodeXOr;
        }

        public override bool Equals(object obj)
        {
            if (obj is ColumnMeta)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        #endregion
    }
}