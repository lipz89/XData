using System;
using System.Reflection;
using XData.Extentions;

namespace XData.Common.Fast
{
    /// <summary>
    /// 快速访问的扩展类
    /// </summary>
    public static class FastOperator
    {
        /// <summary>
        /// 快速创建一个指定类型的对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object FastCreate(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var constructorInfo = type.GetDefaultConstructor();
            if (constructorInfo == null)
                throw new ArgumentException("类型没有公开的默认构造函数。");

            return FastFactory.GetObjectCreatorWrapper(constructorInfo).Create();
        }
        /// <summary>
        /// 利用构造函数和参数快速实例化一个对象
        /// </summary>
        /// <param name="constructorInfo"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object FastCreate(this ConstructorInfo constructorInfo, params object[] parameters)
        {
            if (constructorInfo == null)
                throw new ArgumentNullException(nameof(constructorInfo));

            return FastFactory.GetObjectCreatorWrapper(constructorInfo).Create(parameters);
        }
        /// <summary>
        /// 快速获取一个属性的值
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object FastGetValue(this PropertyInfo propertyInfo, object obj)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            return FastFactory.GetPropertyGetterWrapper(propertyInfo).Get(obj);
        }
        /// <summary>
        /// 快速设置一个属性的值
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void FastSetValue(this PropertyInfo propertyInfo, object obj, object value)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            FastFactory.GetPropertySetterWrapper(propertyInfo).Set(obj, value);
        }
        /// <summary>
        /// 快速获取一个字段的值
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object FastGetValue(this FieldInfo fieldInfo, object obj)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            return FastFactory.GetPropertyGetterWrapper(fieldInfo).Get(obj);
        }
        /// <summary>
        /// 快速设置一个字段的值
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void FastSetValue(this FieldInfo fieldInfo, object obj, object value)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            FastFactory.GetPropertySetterWrapper(fieldInfo).Set(obj, value);
        }
    }
}