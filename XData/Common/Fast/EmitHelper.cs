﻿using System;
using System.Reflection;
using System.Reflection.Emit;

namespace XData.Common.Fast
{
    internal static class EmitHelper
    {
        public static Func<object[], T> CreateCreatorHandler<T>(ConstructorInfo constructorInfo)
        {
            ParameterInfo[] paramsTypes = constructorInfo.GetParameters(); 
            DynamicMethod method = new DynamicMethod("DynamicCreatorHandler", typeof(T), new[] { typeof(object[]) }, typeof(EmitHelper));

            ILGenerator il = method.GetILGenerator();

            for (int i = 0; i < paramsTypes.Length; i++)
            {
                var type = paramsTypes[i].ParameterType;
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);
                if (type.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, type);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, type);
                }
            }
            il.Emit(OpCodes.Newobj, constructorInfo);
            il.Emit(OpCodes.Ret);

            return (Func<object[], T>)method.CreateDelegate(typeof(Func<object[], T>));
        }

        public static Func<TTarget, TValue> CreateGetterHandler<TTarget, TValue>(FieldInfo fieldInfo)
        {
            DynamicMethod method = new DynamicMethod("DynamicGetterHandler", typeof(TValue), new[] { typeof(TTarget) }, typeof(EmitHelper));
            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldInfo);
            il.Emit(OpCodes.Ret);

            return (Func<TTarget, TValue>)method.CreateDelegate(typeof(Func<TTarget, TValue>));
        }

        public static Action<TTarget, TValue> CreateSetterHandler<TTarget, TValue>(FieldInfo fieldInfo)
        {
            DynamicMethod method = new DynamicMethod("DynamicSetterHandler", typeof(void), new[] { typeof(TTarget), typeof(TValue) }, typeof(EmitHelper));
            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldInfo);
            il.Emit(OpCodes.Ret);

            return (Action<TTarget, TValue>)method.CreateDelegate(typeof(Action<TTarget, TValue>));
        }

        public static Func<TValue> CreateStaticGetterHandler<TValue>(FieldInfo fieldInfo)
        {
            DynamicMethod method = new DynamicMethod("DynamicStaticGetterHandler", typeof(TValue), null, typeof(EmitHelper));
            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldsfld, fieldInfo);
            il.Emit(OpCodes.Ret);

            return (Func<TValue>)method.CreateDelegate(typeof(Func<TValue>));
        }
        public static Action<TValue> CreateStaticSetterHandler<TValue>(FieldInfo fieldInfo)
        {
            DynamicMethod method = new DynamicMethod("DynamicStaticSetterHandler", typeof(void), new[] { typeof(TValue) }, typeof(EmitHelper));
            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stsfld, fieldInfo);
            il.Emit(OpCodes.Ret);

            return (Action<TValue>)method.CreateDelegate(typeof(Action<TValue>));
        }
    }
}