using System;
using System.Reflection;

namespace XData.Common.Fast
{
    internal class StaticSetterWrapper<TValue> : ISetValue
    {
        private readonly Action<TValue> setter;

        public StaticSetterWrapper(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.CanWrite == false)
                throw new NotSupportedException("属性不支持写操作。");

            MethodInfo m = propertyInfo.GetSetMethod(true);
            setter = (Action<TValue>)Delegate.CreateDelegate(typeof(Action<TValue>), null, m);
        }

        public StaticSetterWrapper(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            setter = EmitHelper.CreateStaticSetterHandler<TValue>(fieldInfo);
        }

        public void SetValue(TValue val)
        {
            setter(val);
        }
        void ISetValue.Set(object target, object val)
        {
            setter((TValue)val);
        }
    }
}