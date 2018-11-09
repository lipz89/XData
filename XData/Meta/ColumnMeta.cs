using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;
using XData.Common.Fast;

namespace XData.Meta
{
    internal class ColumnMeta
    {
        #region Fields

        #endregion

        #region Constructors

        public ColumnMeta(MemberInfo member, Type type)
        {
            Member = member;
            TableType = type;
        }

        #endregion

        #region Properties

        public MemberInfo Member { get; }

        public Type TableType { get; }

        public string ColumnName
        {
            get { return MapperConfig.GetColumnName(Member, TableType); }
        }

        public string Name
        {
            get { return Member.Name; }
        }

        public Type Type
        {
            get
            {
                if (Member is PropertyInfo propertyInfo)
                {
                    return propertyInfo.PropertyType;
                }
                if (Member is FieldInfo fieldInfo)
                {
                    return fieldInfo.FieldType;
                }
                return null;
            }
        }

        public bool CanUpdate
        {
            get
            {
                var ks = MapperConfig.GetKeyMetas(TableType);
                if (ks != null && ks.Any(x => x.Member.Name == Member.Name))
                {
                    return false;
                }
                var id = MapperConfig.GetIdentity(TableType);
                if (id != null && id.Member.Name == Member.Name)
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
                var id = MapperConfig.GetIdentity(TableType);
                if (id != null && id.Member.Name == Member.Name)
                {
                    return false;
                }
                return true;
            }
        }

        public Expression Expression
        {
            get { return ExpressionHelper.CreateGetterExpression(Member, TableType); }
        }

        #endregion

        #region Methods

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

        public bool CanWrite
        {
            get
            {
                if (Member is PropertyInfo propertyInfo)
                {
                    return propertyInfo.CanWrite;
                }

                if (Member is FieldInfo)
                {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Override Object Methods

        public override int GetHashCode()
        {
            return this.Member.Name.GetHashCode() ^ this.TableType.AssemblyQualifiedName.GetHashCode() ^ Constans.HashCodeXOr;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is ColumnMeta)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        public override string ToString()
        {
            return $"Member:{Name}, Type:{Type.FullName}";
        }

        #endregion
    }
}