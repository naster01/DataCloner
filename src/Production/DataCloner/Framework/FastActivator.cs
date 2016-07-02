﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DataCloner.Core.Framework
{
    // For use with no-parameter constructors. Also contains constants and utility methods
    public static class FastActivator
    {
        // THIS VERSION NOT THREAD SAFE YET
        static readonly Dictionary<Type, Func<object>> ConstructorCache = new Dictionary<Type, Func<object>>();

        private const string DynamicMethodPrefix = "DM$_FastActivator_";

        public static object CreateInstance(Type objType)
        {
            return GetConstructor(objType)();
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object> GetConstructor(Type objType)
        {
            Func<object> constructor;
            if (!ConstructorCache.TryGetValue(objType, out constructor))
            {
                constructor = (Func<object>)BuildConstructorDelegate(objType, typeof(Func<object>), new Type[] { });
                ConstructorCache.Add(objType, constructor);
            }
            return constructor;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object BuildConstructorDelegate(Type objType, Type delegateType, Type[] argTypes)
        {
            var dynMethod = new DynamicMethod(DynamicMethodPrefix + objType.Name + "$" + argTypes.Length, objType, argTypes, objType);
            var ilGen = dynMethod.GetILGenerator();
            for (var argIdx = 0; argIdx < argTypes.Length; argIdx++)
            {
                ilGen.Emit(OpCodes.Ldarg, argIdx);
            }
            ilGen.Emit(OpCodes.Newobj, objType.GetConstructor(argTypes));
            ilGen.Emit(OpCodes.Ret);
            return dynMethod.CreateDelegate(delegateType);
        }
    }

    // For use with one-parameter constructors, argument type = T1
    public static class FastActivator<T1>
    {
        // THIS VERSION NOT THREAD SAFE YET
        readonly static Dictionary<Type, Func<T1, object>> ConstructorCache = new Dictionary<Type, Func<T1, object>>();
        public static object CreateInstance(Type objType, T1 arg1)
        {
            return GetConstructor(objType, new[] { typeof(T1) })(arg1);
        }

        public static Func<T1, object> GetConstructor(Type objType, Type[] argTypes)
        {
            if (argTypes.Length != 1)
                throw new ArgumentException($"Arguments found {argTypes.Length} : Expected : 1");
            Func<T1, object> constructor;
            if (!ConstructorCache.TryGetValue(objType, out constructor))
            {
                constructor = (Func<T1, object>)FastActivator.BuildConstructorDelegate(objType, typeof(Func<T1, object>), argTypes);
                ConstructorCache.Add(objType, constructor);
            }
            return constructor;
        }
    }

    // For use with two-parameter constructors, argument types = T1, T2
    public static class FastActivator<T1, T2>
    {
        // THIS VERSION NOT THREAD SAFE YET
        readonly static Dictionary<Type, Func<T1, T2, object>> ConstructorCache = new Dictionary<Type, Func<T1, T2, object>>();
        public static object CreateInstance(Type objType, T1 arg1, T2 arg2)
        {
            return GetConstructor(objType, new[] { typeof(T1), typeof(T2) })(arg1, arg2);
        }

        public static Func<T1, T2, object> GetConstructor(Type objType, Type[] argTypes)
        {
            if (argTypes.Length != 2)
                throw new ArgumentException($"Arguments found {argTypes.Length} : Expected : 2");
            Func<T1, T2, object> constructor;
            if (!ConstructorCache.TryGetValue(objType, out constructor))
            {
                constructor = (Func<T1, T2, object>)FastActivator.BuildConstructorDelegate(objType, typeof(Func<T1, T2, object>), argTypes);
                ConstructorCache.Add(objType, constructor);
            }
            return constructor;
        }
    }

    // For use with three-parameter constructors, argument types = T1, T2, T3
    // NB: could possibly merge these FastActivator<T1,...> classes and avoid generic type parameters
    // but would need to take care that cache entries were keyed to distinguish constructors having 
    // the same number of parameters but of different types. Keep separate for now.
    public static class FastActivator<T1, T2, T3>
    {
        // THIS VERSION NOT THREAD SAFE YET
        readonly static Dictionary<Type, Func<T1, T2, T3, object>> ConstructorCache = new Dictionary<Type, Func<T1, T2, T3, object>>();
        public static object CreateInstance(Type objType, T1 arg1, T2 arg2, T3 arg3)
        {
            return GetConstructor(objType, new[] { typeof(T1), typeof(T2), typeof(T3) })(arg1, arg2, arg3);
        }

        public static Func<T1, T2, T3, object> GetConstructor(Type objType, Type[] argTypes)
        {
            if (argTypes.Length != 3)
                throw new ArgumentException($"Arguments found {argTypes.Length} : Expected : 3");
            Func<T1, T2, T3, object> constructor;
            if (!ConstructorCache.TryGetValue(objType, out constructor))
            {
                constructor = (Func<T1, T2, T3, object>)FastActivator.BuildConstructorDelegate(objType, typeof(Func<T1, T2, T3, object>), argTypes);
                ConstructorCache.Add(objType, constructor);
            }
            return constructor;
        }
    }
}
