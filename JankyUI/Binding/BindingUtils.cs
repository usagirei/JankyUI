using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace JankyUI.Binding
{
    internal static class BindingUtils
    {
        private static Dictionary<Type, DynamicMethod> _emptyDelegates;

        static BindingUtils()
        {
            _emptyDelegates = new Dictionary<Type, DynamicMethod>();
        }

        public static Delegate MakeEmptyDelegate(Type delegateType)
        {
            if (!_emptyDelegates.TryGetValue(delegateType, out DynamicMethod dm))
            {
                if (!delegateType.IsSubclassOf(typeof(Delegate)))
                    throw new ArgumentException("Generic Type is not a Delegate", nameof(delegateType));

                MethodInfo delegateSignature = delegateType.GetMethod("Invoke");
                Type delegateReturn = delegateSignature.ReturnType;
                Type[] delegateParams = delegateSignature.GetParameters().Select(x => x.ParameterType).ToArray();

                var dynamicMethodName = 
                    "<" + nameof(MakeEmptyDelegate) + ">_" 
                    + delegateReturn.FullName + "_" 
                    + delegateType.FullName 
                    + "[" + string.Join(",", delegateParams.Select(x => x.FullName).ToArray()) + "]";

                _emptyDelegates[delegateType] = dm = new DynamicMethod(
                    dynamicMethodName,
                    delegateReturn,
                    delegateParams,
                    delegateType);

                var il = dm.GetILGenerator();
                if (delegateReturn != typeof(void))
                {
                    var local = il.DeclareLocal(delegateReturn);
                    il.Emit(OpCodes.Ldloca_S, local);
                    il.Emit(OpCodes.Initobj, delegateReturn);
                    il.Emit(OpCodes.Ldloc_S, local);
                }
                il.Emit(OpCodes.Ret);
            }

            return dm.CreateDelegate(delegateType);
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

            Type ownerType = (method is DynamicMethod) ? dynamicMethodOwnerType : method.DeclaringType;

            var dynamicMethodName =
                "<" + nameof(MakeCompatibleDelegate) + ">_"
                + returnType.FullName
                + "_" + ownerType.FullName + "::"
                + method.Name + "[" + string.Join(",", paramTypes.Select(t => t.FullName).ToArray()) + "]";

            var dyn = new DynamicMethod(dynamicMethodName, newRetType, newParamTypes, ownerType);

            var il = dyn.GetILGenerator();
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

            if (method.IsFinal || method is DynamicMethod)
                il.Emit(OpCodes.Call, method);
            else
                il.Emit(OpCodes.Callvirt, method);
                

            if (returnType != typeof(void))
            {
                if (returnType != newRetType)
                    il.Emit(OpCodes.Unbox_Any, newRetType);
            }
            il.Emit(OpCodes.Ret);

            return dyn.CreateDelegate(typeof(TCompatible)) as TCompatible;
        }

        public static DynamicMethod MakeFieldSetter(FieldInfo field)
        {
            var dynamicMethodName =
                "<" + nameof(MakeFieldSetter) + ">_"
                + field.FieldType.FullName + "_"
                + field.DeclaringType.FullName + "::" + field.Name;

            var dm = new DynamicMethod(
                dynamicMethodName,
                typeof(void),
                new[] { field.DeclaringType, field.FieldType },
                field.DeclaringType);

            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, field);
            gen.Emit(OpCodes.Ret);
            return dm;
        }

        public static DynamicMethod MakeFieldGetter(FieldInfo field)
        {
            var dynamicMethodName =
                "<" + nameof(MakeFieldGetter) + ">_"
                + field.FieldType.FullName + "_"
                + field.DeclaringType.FullName + "::" + field.Name;

            var dm = new DynamicMethod(
                dynamicMethodName,
                field.FieldType,
                new[] { field.DeclaringType },
                field.DeclaringType);
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, field);
            gen.Emit(OpCodes.Ret);

            return dm;
        }

        public static void MakeFieldAcessors<TField>(FieldInfo field, out Func<object, TField> getter, out Action<object, TField> setter)
        {
            getter = MakeCompatibleDelegate<Func<object, TField>>(MakeFieldGetter(field), field.DeclaringType);
            setter = MakeCompatibleDelegate<Action<object, TField>>(MakeFieldSetter(field), field.DeclaringType);
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
