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
        private MInfo minfo;
        public ColumnMeta(MInfo member)
        {
            minfo = member;
        }
        public MemberInfo Member
        {
            get { return minfo.Member; }
        }
        public string ColumnName { get; internal set; }
        public string Name
        {
            get { return Member?.Name; }
        }

        public Expression Expression { get; internal set; }

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

        public override int GetHashCode()
        {
            return this.Member.GetHashCode() ^ Constans.HashCodeXOr;
        }

        public override bool Equals(object obj)
        {
            if (obj is ColumnMeta)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            return false;
        }
    }
}