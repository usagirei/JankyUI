using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace JankyUI.Binding
{
    public static class BindingUtils
    {
        private static Dictionary<Type, MethodInfo> _emptyDelegates;
        private static Dictionary<string, (MethodInfo, MethodInfo)> _fieldAcessors;
        private static Dictionary<string, MethodInfo> _compatibleDelegates;

        private static AssemblyBuilder _dynamicAssembly;
        private static ModuleBuilder _dynamicModule;

        static BindingUtils()
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

        public static TCompatible MakeCompatibleDelegate<TCompatible>(MethodInfo method, Type dynamicMethodOwnerType = null)
            where TCompatible : class
        {
            var cleanName = (method.DeclaringType + "." + method.Name + "[" + typeof(TCompatible) + "]");
            cleanName = cleanName.Replace('`', '#');
            cleanName = cleanName.Replace(' ', '_');
            cleanName = cleanName.Replace(".", "::");
            var holderTypeName = $"{nameof(MakeCompatibleDelegate)}.{cleanName}";

            if (!_compatibleDelegates.TryGetValue(holderTypeName, out var methodInfo))
            {
                var maskSignature = typeof(TCompatible).GetMethod("Invoke");

                var hasThis = (method.CallingConvention & CallingConventions.HasThis) == CallingConventions.HasThis;
                var returnType = method.ReturnType;
                Type[] paramTypes;
                if (hasThis)
                    paramTypes = new[] { method.DeclaringType }.Concat(method.GetParameters().Select(x => x.ParameterType)).ToArray();
                else
                    paramTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();

                var newRetType = maskSignature.ReturnType;
                var newParamTypes = maskSignature.GetParameters().Select(x => x.ParameterType).ToArray();

                if (paramTypes.Length != newParamTypes.Length)
                    throw new ArgumentException("Invalid argument count on delegates");

                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (!newParamTypes[i].IsAssignableFrom(paramTypes[i]))
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

            getter = MakeCompatibleDelegate<Func<object, TField>>(get);
            setter = MakeCompatibleDelegate<Action<object, TField>>(set);
        }

        public static void MakePropertyAcessors<TProp>(PropertyInfo prop, out Func<object, TProp> getter, out Action<object, TProp> setter)
        {
            getter = (prop.CanRead)
                ? MakeCompatibleDelegate<Func<object, TProp>>(prop.GetGetMethod())
                : null;
            setter = (prop.CanWrite)
                ? MakeCompatibleDelegate<Action<object, TProp>>(prop.GetSetMethod())
                : null;
        }

        /*
        private static HashSet<string> _propertyGetSet;
        private static Dictionary<string, DynamicMethod> _propertyGetters;
        private static Dictionary<string, DynamicMethod> _propertySetters;
        
        [Obsolete("Use the Demoted Delegate Version")]
        public static void MakePropertyAcessorOld<TProp>(PropertyInfo prop, out Func<object, TProp> getter, out Action<object, TProp> setter)
        {
            void MakePropertyAcessor_AutoCast(out DynamicMethod _get, out DynamicMethod _set)
            {
                var getter_info = prop.GetGetMethod();
                var setter_info = prop.GetSetMethod();

                var sourceType = prop.DeclaringType;
                var propType = prop.PropertyType;

                _set = null;
                _get = null;

                if (prop.CanRead)
                {
                    _get = new DynamicMethod(
                        "<MakePropertyAcessor>_Getter<" + sourceType.FullName + "," + typeof(TProp).FullName + ">",
                        typeof(TProp),
                        new[] { typeof(object) },
                        sourceType);
                    var il = _get.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, sourceType);
                    il.Emit(OpCodes.Callvirt, getter_info);
                    if (propType != typeof(TProp))
                        il.Emit(OpCodes.Unbox_Any, typeof(TProp));
                    il.Emit(OpCodes.Ret);
                }

                if (prop.CanWrite)
                {
                    _set = new DynamicMethod(
                        "<MakePropertyAcessor>_Setter<" + sourceType.FullName + "," + typeof(TProp).FullName + ">",
                        typeof(void),
                        new[] { typeof(object), typeof(TProp) },
                        sourceType);
                    var il = _set.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, sourceType);
                    il.Emit(OpCodes.Ldarg_1);
                    if (propType != typeof(TProp))
                        il.Emit(OpCodes.Unbox_Any, typeof(TProp));
                    il.Emit(OpCodes.Callvirt, setter_info);
                    il.Emit(OpCodes.Ret);
                }
            }

            var assignable = typeof(TProp).IsAssignableFrom(prop.PropertyType);
            if (!assignable)
                throw new ArgumentException("Generic argument and property type are not compatible");

            DynamicMethod dm_get = null, dm_set = null;
            getter = null;
            setter = null;

            var propIdentifier = prop.DeclaringType.FullName + "::" + prop.Name + "::" + typeof(TProp).FullName;
            if (!_propertyGetSet.Contains(propIdentifier))
            {
                _propertyGetSet.Add(propIdentifier);

                MakePropertyAcessor_AutoCast(out dm_get, out dm_set);

                if (dm_get != null)
                    _propertyGetters[propIdentifier] = dm_get;
                if (dm_set != null)
                    _propertySetters[propIdentifier] = dm_set;
            }
            else
            {
                _propertyGetters.TryGetValue(propIdentifier, out dm_get);
                _propertySetters.TryGetValue(propIdentifier, out dm_set);
            }

            if (dm_get != null)
            {
                getter = (Func<object, TProp>)dm_get.CreateDelegate(typeof(Func<object, TProp>));
            }
            if (dm_set != null)
            {
                setter = (Action<object, TProp>)dm_set.CreateDelegate(typeof(Action<object, TProp>));
            }
        }
        */
    }
}
