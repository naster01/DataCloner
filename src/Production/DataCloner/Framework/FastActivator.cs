﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace DataCloner.Framework
{
    // For use with no-parameter constructors. Also contains constants and utility methods
    public static class FastActivator
    {
        // THIS VERSION NOT THREAD SAFE YET
        static Dictionary<Type, Func<object>> constructorCache = new Dictionary<Type, Func<object>>();

        private const string DynamicMethodPrefix = "DM$_FastActivator_";

        public static object CreateInstance(Type objType)
        {
            return GetConstructor(objType)();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object> GetConstructor(Type objType)
        {
            Func<object> constructor;
            if (!constructorCache.TryGetValue(objType, out constructor))
            {
                constructor = (Func<object>)FastActivator.BuildConstructorDelegate(objType, typeof(Func<object>), new Type[] { });
                constructorCache.Add(objType, constructor);
            }
            return constructor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object BuildConstructorDelegate(Type objType, Type delegateType, Type[] argTypes)
        {
            var dynMethod = new DynamicMethod(DynamicMethodPrefix + objType.Name + "$" + argTypes.Length.ToString(), objType, argTypes, objType);
            ILGenerator ilGen = dynMethod.GetILGenerator();
            for (int argIdx = 0; argIdx < argTypes.Length; argIdx++)
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
        static Dictionary<Type, Func<T1, object>> constructorCache = new Dictionary<Type, Func<T1, object>>();
        public static object CreateInstance(Type objType, T1 arg1)
        {
            return GetConstructor(objType, new Type[] { typeof(T1) })(arg1);
        }

        public static Func<T1, object> GetConstructor(Type objType, Type[] argTypes)
        {
            if (argTypes.Length != 1)
                throw new ArgumentException(string.Format("Arguments found {0} : Expected : 1", argTypes.Length));
            Func<T1, object> constructor;
            if (!constructorCache.TryGetValue(objType, out constructor))
            {
                constructor = (Func<T1, object>)FastActivator.BuildConstructorDelegate(objType, typeof(Func<T1, object>), argTypes);
                constructorCache.Add(objType, constructor);
            }
            return constructor;
        }
    }

    // For use with two-parameter constructors, argument types = T1, T2
    public static class FastActivator<T1, T2>
    {
        // THIS VERSION NOT THREAD SAFE YET
        static Dictionary<Type, Func<T1, T2, object>> constructorCache = new Dictionary<Type, Func<T1, T2, object>>();
        public static object CreateInstance(Type objType, T1 arg1, T2 arg2)
        {
            return GetConstructor(objType, new Type[] { typeof(T1), typeof(T2) })(arg1, arg2);
        }

        public static Func<T1, T2, object> GetConstructor(Type objType, Type[] argTypes)
        {
            if (argTypes.Length != 2)
                throw new ArgumentException(string.Format("Arguments found {0} : Expected : 2", argTypes.Length));
            Func<T1, T2, object> constructor;
            if (!constructorCache.TryGetValue(objType, out constructor))
            {
                constructor = (Func<T1, T2, object>)FastActivator.BuildConstructorDelegate(objType, typeof(Func<T1, T2, object>), argTypes);
                constructorCache.Add(objType, constructor);
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
        static Dictionary<Type, Func<T1, T2, T3, object>> constructorCache = new Dictionary<Type, Func<T1, T2, T3, object>>();
        public static object CreateInstance(Type objType, T1 arg1, T2 arg2, T3 arg3)
        {
            return GetConstructor(objType, new Type[] { typeof(T1), typeof(T2), typeof(T3) })(arg1, arg2, arg3);
        }

        public static Func<T1, T2, T3, object> GetConstructor(Type objType, Type[] argTypes)
        {
            if (argTypes.Length != 3)
                throw new ArgumentException(string.Format("Arguments found {0} : Expected : 3", argTypes.Length));
            Func<T1, T2, T3, object> constructor;
            if (!constructorCache.TryGetValue(objType, out constructor))
            {
                constructor = (Func<T1, T2, T3, object>)FastActivator.BuildConstructorDelegate(objType, typeof(Func<T1, T2, T3, object>), argTypes);
                constructorCache.Add(objType, constructor);
            }
            return constructor;
        }
    }
}
