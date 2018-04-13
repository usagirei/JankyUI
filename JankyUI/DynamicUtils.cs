using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace JankyUI
{
    public static class DynamicUtils
    {
        private static Dictionary<Type, MethodInfo> _emptyDelegates;
        private static Dictionary<string, (MethodInfo, MethodInfo)> _fieldAcessors;
        private static Dictionary<string, MethodInfo> _compatibleDelegates;

        private static AssemblyBuilder _dynamicAssembly;
        private static ModuleBuilder _dynamicModule;

        public static ModuleBuilder Module { get { return _dynamicModule; } }

        static DynamicUtils()
        {
            _emptyDelegates = new Dictionary<Type, MethodInfo>();
            _fieldAcessors = new Dictionary<string, (MethodInfo, MethodInfo)>();
            _compatibleDelegates = new Dictionary<string, MethodInfo>();

            _dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("JankyUI.Dynamic"),
                AssemblyBuilderAccess.RunAndSave
            );
            _dynamicModule = _dynamicAssembly.DefineDynamicModule("JankyUI.Dynamic", "JankyUI.Dynamic.dll");
        }

        public static void SaveDynamicModule()
        {
            _dynamicAssembly.Save("JankyUI.Dynamic.dll");
        }

        public static Delegate MakeEmptyDelegate(Type delegateType)
        {
            if (!_emptyDelegates.TryGetValue(delegateType, out MethodInfo method))
            {
                if (!delegateType.IsSubclassOf(typeof(Delegate)))
                    throw new ArgumentException("Generic Type is not a Delegate", nameof(delegateType));

                MethodInfo delegateSignature = delegateType.GetMethod("Invoke");
                Type delegateReturn = delegateSignature.ReturnType;
                Type[] delegateParams = delegateSignature.GetParameters().Select(x => x.ParameterType).ToArray();

                var cleanName = delegateType.ToString();
                cleanName = cleanName.Replace('`', '#');
                cleanName = cleanName.Replace(' ', '_');
                cleanName = cleanName.Replace(".", "::");
                var holderTypeName = $"{nameof(MakeEmptyDelegate)}.{cleanName}";
                var holderType = _dynamicModule.DefineType(holderTypeName, TypeAttributes.Public);

                var dynamicMethodName = "Invoke";

                var methodBuilder = holderType.DefineMethod(dynamicMethodName, MethodAttributes.Public | MethodAttributes.Static, delegateReturn, delegateParams);

                var il = methodBuilder.GetILGenerator();
                if (delegateReturn != typeof(void))
                {
                    var local = il.DeclareLocal(delegateReturn);
                    il.Emit(OpCodes.Ldloca_S, local);
                    il.Emit(OpCodes.Initobj, delegateReturn);
                    il.Emit(OpCodes.Ldloc_S, local);
                }
                il.Emit(OpCodes.Ret);

                holderType.CreateType();

                _emptyDelegates[delegateType] = method = holderType.GetMethod("Invoke", BindingFlags.Static | BindingFlags.Public);
            }

            return Delegate.CreateDelegate(delegateType, method);
        }

        public static TDelegate MakeEmptyDelegate<TDelegate>()
            where TDelegate : class
        {
            if (!typeof(TDelegate).IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("Generic Type is not a Delegate", nameof(TDelegate));

            return MakeEmptyDelegate(typeof(TDelegate)) as TDelegate;
        }

        public static TCompatible MakeCompatibleDelegate<TCompatible>(MethodInfo method, object target = null)
            where TCompatible : class
        {
            bool hasTarget = target != null;

            var cleanName = (method.DeclaringType + "." + method.Name + "[" + typeof(TCompatible) + "]");
            cleanName = cleanName.Replace('`', '#');
            cleanName = cleanName.Replace(' ', '_');
            cleanName = cleanName.Replace(".", "::");

            if (hasTarget)
                cleanName += "_[" + target.GetType() + "]";

            var holderTypeName = $"{nameof(MakeCompatibleDelegate)}.{cleanName}";

            if (!_compatibleDelegates.TryGetValue(holderTypeName, out var methodInfo))
            {
                var maskSignature = typeof(TCompatible).GetMethod("Invoke");

                var hasThis = (method.CallingConvention & CallingConventions.HasThis) == CallingConventions.HasThis;

                var returnType = method.ReturnType;
                var newRetType = maskSignature.ReturnType;

                Type[] paramTypes = (hasThis)
                    ? new[] { method.DeclaringType }.Concat(method.GetParameters().Select(x => x.ParameterType)).ToArray()
                    : method.GetParameters().Select(x => x.ParameterType).ToArray();

                Type[] newParamTypes = (hasTarget)
                    ? new[] { target.GetType() }.Concat(maskSignature.GetParameters().Select(x => x.ParameterType)).ToArray()
                    : maskSignature.GetParameters().Select(x => x.ParameterType).ToArray();

                if (paramTypes.Length != newParamTypes.Length)
                    throw new ArgumentException("Invalid argument count on delegates");

                for (int i = 0; i < paramTypes.Length; i++)
                {
                    bool isAssignable = newParamTypes[i].IsAssignableFrom(paramTypes[i]);
                    //isAssignable |= paramTypes[i].IsSubclassOfRawGeneric(newParamTypes[i]);
                    if (!isAssignable)
                    {
                        throw new ArgumentException($"Argument {i} - '{paramTypes[i]}' can't be demoted to target type '{newParamTypes[i]}'");
                    }
                }

                if (!newRetType.IsAssignableFrom(returnType))
                    throw new ArgumentException($"Return Type '{returnType}' can't be demoted to target type '{newRetType}'");

                var holderType = _dynamicModule.DefineType(holderTypeName, TypeAttributes.Public);

                var dynamicMethodName = "Invoke";

                var methodBuilder = holderType.DefineMethod(dynamicMethodName, MethodAttributes.Public | MethodAttributes.Static, newRetType, newParamTypes);

                var il = methodBuilder.GetILGenerator();
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            il.Emit(OpCodes.Ldarg_0);
                            break;

                        case 1:
                            il.Emit(OpCodes.Ldarg_1);
                            break;

                        case 2:
                            il.Emit(OpCodes.Ldarg_2);
                            break;

                        case 3:
                            il.Emit(OpCodes.Ldarg_3);
                            break;

                        case int _ when (i <= 255):
                            il.Emit(OpCodes.Ldarg_S, (byte)i);
                            break;

                        default:
                            il.Emit(OpCodes.Ldarg, i);
                            break;
                    }
                    if (paramTypes[i] != newParamTypes[i])
                        il.Emit(OpCodes.Unbox_Any, paramTypes[i]);
                }

                if (!method.IsVirtual)
                    il.Emit(OpCodes.Call, method);
                else
                    il.Emit(OpCodes.Callvirt, method);


                if (returnType != typeof(void))
                {
                    if (returnType.IsValueType)
                        il.Emit(OpCodes.Box, returnType);
                    if (returnType != newRetType && !newRetType.IsValueType)
                        il.Emit(OpCodes.Castclass, newRetType);
                }
                il.Emit(OpCodes.Ret);

                holderType.CreateType();

                methodInfo = holderType.GetMethod("Invoke", BindingFlags.Static | BindingFlags.Public);

                _compatibleDelegates[holderTypeName] = methodInfo;
            }

            if (hasTarget)
                return Delegate.CreateDelegate(typeof(TCompatible), target, methodInfo) as TCompatible;
            else
                return Delegate.CreateDelegate(typeof(TCompatible), methodInfo) as TCompatible;
        }

        public static void MakeFieldGetterSetter(FieldInfo field, out MethodInfo getter, out MethodInfo setter)
        {
            var cleanName = (field.DeclaringType + "::" + field.Name);
            cleanName = cleanName.Replace('`', '#');
            cleanName = cleanName.Replace(' ', '_');
            cleanName = cleanName.Replace(".", "::");
            var holderTypeName = $"{nameof(MakeFieldAcessors)}.{cleanName}";

            if (!_fieldAcessors.TryGetValue(holderTypeName, out var acessors))
            {
                var holderType = _dynamicModule.DefineType(holderTypeName, TypeAttributes.Public);

                var attr = MethodAttributes.Static | MethodAttributes.Public;

                if (field.IsStatic)
                {
                    var getMethod = holderType.DefineMethod("Get", attr, field.FieldType, Type.EmptyTypes);
                    var getIL = getMethod.GetILGenerator();
                    getIL.Emit(OpCodes.Ldsfld, field);
                    getIL.Emit(OpCodes.Ret);

                    var setMethod = holderType.DefineMethod("Set", attr, typeof(void), new[] { field.FieldType });
                    var setIL = setMethod.GetILGenerator();
                    setIL.Emit(OpCodes.Ldarg_1);
                    setIL.Emit(OpCodes.Stsfld, field);
                    setIL.Emit(OpCodes.Ret);
                }
                else
                {
                    var getMethod = holderType.DefineMethod("Get", attr, field.FieldType, new[] { field.DeclaringType });
                    var getIL = getMethod.GetILGenerator();
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldfld, field);
                    getIL.Emit(OpCodes.Ret);

                    var setMethod = holderType.DefineMethod("Set", attr, typeof(void), new[] { field.DeclaringType, field.FieldType });
                    var setIL = setMethod.GetILGenerator();
                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldarg_1);
                    setIL.Emit(OpCodes.Stfld, field);
                    setIL.Emit(OpCodes.Ret);
                }

                holderType.CreateType();

                var flags = BindingFlags.Static | BindingFlags.Public;
                getter = holderType.GetMethod("Get", flags);
                setter = holderType.GetMethod("Set", flags);

                _fieldAcessors[holderTypeName] = acessors = (getter, setter);
                return;
            }

            getter = acessors.Item1;
            setter = acessors.Item2;
        }

        public static void MakeFieldAcessors<TField>(FieldInfo field, out Func<object, TField> getter, out Action<object, TField> setter)
        {
            MakeFieldGetterSetter(field, out var get, out var set);

            if (field.IsStatic)
            {
                var static_getter = MakeCompatibleDelegate<Func<TField>>(get);
                var static_setter = MakeCompatibleDelegate<Action<TField>>(set);

                getter = (nil) => static_getter();
                setter = (nil, value) => static_setter(value);
            }
            else
            {
                getter = MakeCompatibleDelegate<Func<object, TField>>(get);
                setter = MakeCompatibleDelegate<Action<object, TField>>(set);
            }
        }

        public static void MakePropertyAcessors<TProp>(PropertyInfo prop, out Func<object, TProp> getter, out Action<object, TProp> setter)
        {
            if (prop.CanRead)
            {
                var get_method = prop.GetGetMethod();
                if (get_method.IsStatic)
                {
                    var static_getter = MakeCompatibleDelegate<Func<TProp>>(get_method);
                    getter = (nil) => static_getter();
                }
                else
                {
                    getter = MakeCompatibleDelegate<Func<object, TProp>>(get_method);
                }
            }
            else
            {
                getter = null;
            }

            if (prop.CanWrite)
            {
                var set_method = prop.GetSetMethod();
                if (set_method.IsStatic)
                {
                    var static_setter = MakeCompatibleDelegate<Action<TProp>>(set_method);
                    setter = (nil, value) => static_setter(value);
                }
                else
                {
                    setter = MakeCompatibleDelegate<Action<object, TProp>>(set_method);
                }
            }
            else
            {
                setter = null;
            }
        }
    }
}
