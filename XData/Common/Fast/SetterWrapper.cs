using System;
using System.Reflection;

namespace XData.Common.Fast
{
    internal class SetterWrapper<TTarget, TValue> : ISetValue
    {
        private readonly Action<TTarget, TValue> setter;

        public SetterWrapper(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.CanWrite == false)
                throw new NotSupportedException("属性不支持写操作。");

            MethodInfo m = propertyInfo.GetSetMethod(true);
            setter = (Action<TTarget, TValue>)Delegate.CreateDelegate(typeof(Action<TTarget, TValue>), null, m);
        }
        public SetterWrapper(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            setter = EmitHelper.CreateSetterHandler<TTarget, TValue>(fieldInfo);
        }

        public void SetValue(TTarget target, TValue val)
        {
            setter(target, val);
        }
        void ISetValue.Set(object target, object val)
        {
            setter((TTarget)target, (TValue)val);
        }
    }
}