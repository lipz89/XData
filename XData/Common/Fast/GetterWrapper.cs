using System;
using System.Reflection;

namespace XData.Common.Fast
{
    internal class GetterWrapper<TTarget, TValue> : IGetValue
    {
        private readonly Func<TTarget, TValue> getter;

        public GetterWrapper(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.CanRead == false)
                throw new InvalidOperationException("属性不支持读操作。");

            MethodInfo m = propertyInfo.GetGetMethod(true);
            getter = (Func<TTarget, TValue>)Delegate.CreateDelegate(typeof(Func<TTarget, TValue>), null, m);
        }

        public GetterWrapper(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            getter = EmitHelper.CreateGetterHandler<TTarget, TValue>(fieldInfo);
        }

        public TValue GetValue(TTarget target)
        {
            return getter(target);
        }
        object IGetValue.Get(object target)
        {
            return getter((TTarget)target);
        }
    }
}