using System;
using System.Diagnostics;
using System.Reflection;

using XData.Common;

namespace XData.Meta
{
    [DebuggerDisplay("Member:{Member}, Type:{Type}")]
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
            return Type.MetadataToken ^ Member.MetadataToken ^ Constans.HashCodeXOr;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MInfo;
            if (other == null)
                return false;

            return Type.MetadataToken == other.Type.MetadataToken
                   && Member.MetadataToken == other.Member.MetadataToken;
        }

        #endregion

    }
}