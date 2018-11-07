using System;
using System.Reflection;

namespace XData.Common.Fast
{
    internal class StaticGetterWrapper<TValue> : IGetValue
    {
        private readonly Func<TValue> getter;
        public StaticGetterWrapper(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.CanRead == false)
                throw new InvalidOperationException("属性不支持读操作。");

            MethodInfo m = propertyInfo.GetGetMethod(true);
            getter = (Func<TValue>)Delegate.CreateDelegate(typeof(Func<TValue>), null, m);
        }

        public StaticGetterWrapper(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            getter = EmitHelper.CreateStaticGetterHandler<TValue>(fieldInfo);
        }

        public TValue GetValue()
        {
            return getter();
        }

        object IGetValue.Get(object target)
        {
            return getter();
        }
    }
}