using System;
using System.Collections;
using System.Reflection;

namespace XData.Common.Fast
{
    internal static class FastFactory
    {
        private static readonly Hashtable sGetterDict = Hashtable.Synchronized(new Hashtable(1024));
        private static readonly Hashtable sSetterDict = Hashtable.Synchronized(new Hashtable(1024));
        private static readonly Hashtable creatorDict = Hashtable.Synchronized(new Hashtable(1024));

        public static ICreator GetObjectCreatorWrapper(ConstructorInfo constructorInfo)
        {
            ICreator creator = (ICreator)creatorDict[constructorInfo];
            if (creator == null)
            {
                creator = CreateCreatorWrapper(constructorInfo);
                creatorDict[constructorInfo] = creator;
            }
            return creator;
        }

        public static IGetValue GetPropertyGetterWrapper(MemberInfo memberInfo)
        {
            IGetValue property = (IGetValue)sGetterDict[memberInfo];
            if (property == null)
            {
                property = CreatePropertyGetterWrapper(memberInfo);
                sGetterDict[memberInfo] = property;
            }
            return property;
        }

        public static ISetValue GetPropertySetterWrapper(MemberInfo memberInfo)
        {
            ISetValue property = (ISetValue)sSetterDict[memberInfo];
            if (property == null)
            {
                property = CreatePropertySetterWrapper(memberInfo);
                sSetterDict[memberInfo] = property;
            }
            return property;
        }

        private static ICreator CreateCreatorWrapper(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
                throw new ArgumentNullException(nameof(constructorInfo));
            var type = constructorInfo.DeclaringType;

            Type instanceType = typeof(CreatorWrapper<>).MakeGenericType(type);
            return (ICreator)Activator.CreateInstance(instanceType, constructorInfo);
        }

        private static IGetValue CreatePropertyGetterWrapper(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            if (memberInfo is PropertyInfo propertyInfo)
            {
                if (propertyInfo.CanRead == false)
                    throw new InvalidOperationException("属性不支持读操作。");

                MethodInfo mi = propertyInfo.GetGetMethod(true);

                if (mi.GetParameters().Length > 0)
                    throw new NotSupportedException("不支持构造索引器属性的委托。");

                if (mi.IsStatic)
                {
                    Type instanceType = typeof(StaticGetterWrapper<>).MakeGenericType(propertyInfo.PropertyType);
                    return (IGetValue)Activator.CreateInstance(instanceType, propertyInfo);
                }
                else
                {
                    Type instanceType = typeof(GetterWrapper<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
                    return (IGetValue)Activator.CreateInstance(instanceType, propertyInfo);
                }
            }

            if (memberInfo is FieldInfo fieldInfo)
            {
                if (fieldInfo.IsStatic)
                {
                    Type instanceType = typeof(StaticGetterWrapper<>).MakeGenericType(fieldInfo.FieldType);
                    return (IGetValue)Activator.CreateInstance(instanceType, fieldInfo);
                }
                else
                {
                    Type instanceType = typeof(GetterWrapper<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);
                    return (IGetValue)Activator.CreateInstance(instanceType, fieldInfo);
                }
            }

            throw new NotSupportedException("不支持构造 " + memberInfo.MemberType + " 成员类型的委托。");
        }
        private static ISetValue CreatePropertySetterWrapper(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            if (memberInfo is PropertyInfo propertyInfo)
            {
                if (propertyInfo.CanWrite == false)
                    throw new InvalidOperationException("属性不支持写操作。");

                MethodInfo mi = propertyInfo.GetSetMethod(true);

                if (mi.GetParameters().Length > 1)
                    throw new NotSupportedException("不支持构造索引器属性的委托。");

                if (mi.IsStatic)
                {
                    Type instanceType = typeof(StaticSetterWrapper<>).MakeGenericType(propertyInfo.PropertyType);
                    return (ISetValue)Activator.CreateInstance(instanceType, propertyInfo);
                }
                else
                {
                    Type instanceType = typeof(SetterWrapper<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
                    return (ISetValue)Activator.CreateInstance(instanceType, propertyInfo);
                }
            }

            if (memberInfo is FieldInfo fieldInfo)
            {
                if (fieldInfo.IsStatic)
                {
                    Type instanceType = typeof(StaticSetterWrapper<>).MakeGenericType(fieldInfo.FieldType);
                    return (ISetValue)Activator.CreateInstance(instanceType, fieldInfo);
                }
                else
                {
                    Type instanceType = typeof(SetterWrapper<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);
                    return (ISetValue)Activator.CreateInstance(instanceType, fieldInfo);
                }
            }

            throw new NotSupportedException("不支持构造 " + memberInfo.MemberType + " 成员类型的委托。");
        }
    }
}