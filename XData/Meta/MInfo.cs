using System;
using System.Diagnostics;
using System.Reflection;

using XData.Common;

namespace XData.Meta
{
    [DebuggerDisplay("Member:{Member.Name}, Type:{Type.FullName}")]
    internal class MInfo
    {
        #region Constructors

        public MInfo(MemberInfo member, Type type)
        {
            Member = member;
            Type = type;
        }

        #endregion

        #region Properties

        public MemberInfo Member { get; }

        public Type Type { get; }

        #endregion

        #region Override Object Methods


        public override int GetHashCode()
        {
            return Type.MetadataToken ^ Member.Name.GetHashCode() ^ Member.GetType().GetHashCode() ^ Constans.HashCodeXOr;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is MInfo)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }

            return false;
        }

        public override string ToString()
        {
            return $"Member:{Member.Name}, Type:{Type.FullName}";
        }

        #endregion

    }
}