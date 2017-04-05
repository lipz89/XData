using System;
using System.Reflection;

using XData.Common;

namespace XData.Meta
{
    class MInfo
    {
        public MInfo(MemberInfo member, Type caller)
        {
            Member = member;
            Caller = caller;
        }
        public MemberInfo Member { get; }
        public Type Caller { get; }

        public override int GetHashCode()
        {
            return Caller.MetadataToken ^ Member.MetadataToken ^ Constans.HashCodeXOr;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MInfo;
            if (other == null)
                return false;

            return Caller.MetadataToken == other.Caller.MetadataToken
                   && Member.MetadataToken == other.Member.MetadataToken;
        }
    }
}