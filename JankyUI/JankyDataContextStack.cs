using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JankyUI.Nodes.Binding;
using JankyUI.Enums;
using JankyUI.Nodes;
using UnityEngine;
using System.ComponentModel;

namespace JankyUI
{
    internal class JankyDataContextStack
    {
        private Dictionary<Type, TypeConverter> _converterCache
            = new Dictionary<Type, TypeConverter>();

        private Dictionary<(Type, String), (Func<object, object>, Action<object, object>, Type)> _acessorCache
            = new Dictionary<(Type, string), (Func<object, object>, Action<object, object>, Type)>();

        private Func<object, object> _emptyGetter
            = DynamicUtils.MakeEmptyDelegate<Func<object, object>>();

        private Stack<object> Stack { get; }
                            = new Stack<object>();
        public IJankyContext JankyContext { get; }

        public JankyDataContextStack(IJankyContext jank)
        {
            JankyContext = jank;
        }

        private void GetAcessorFor(Type curType, string memberName, out Func<object, object> get, out Action<object, object> set, out Type targetType)
        {
            var typeKey = (curType, memberName);
            if (!_acessorCache.TryGetValue(typeKey, out var getSetPair))
            {
                Console.WriteLine("[JankyStack] Creating new getter for '{0}.{1}'", curType, memberName);
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
                var targetMember = curType.GetMember(memberName, flags).FirstOrDefault();
                switch (targetMember)
                {
                    case PropertyInfo prop_info:
                        {
                            DynamicUtils.MakePropertyAcessors(prop_info, out get, out set);
                            targetType = prop_info.PropertyType;
                        }
                        break;
                    case FieldInfo field_info:
                        {
                            DynamicUtils.MakeFieldAcessors(field_info, out get, out set);
                            targetType = field_info.FieldType;
                        }
                        break;
                    case null:
                        {
                            Console.WriteLine($"[JankyStack] Target Type has no public member named '{memberName}'");
                            get = null;
                            set = null;
                            targetType = typeof(void);
                        }
                        break;
                    default:
                        {
                            Console.WriteLine("[JankyStack] Unsupported Member Type: '{0}'", memberName);
                            get = null;
                            set = null;
                            targetType = typeof(void);
                        }
                        break;
                }

                _acessorCache[typeKey] = getSetPair = (get, set, targetType);
            }
            get = getSetPair.Item1;
            set = getSetPair.Item2;
            targetType = getSetPair.Item3;
        }

        public void Begin()
        {
            //Stack.Push(JankyContext.DataContext);
            Stack.Clear();
        }

        public object Current()
        {
            return Stack.Count == 0 ? JankyContext.DataContext : Stack.Peek();
        }

        public void End()
        {
            // Unbalanced Stack
            if (Stack.Count != 0)
            {
                throw new Exception("Unbalanced Stack");
            }
        }

        public DataOperationResultEnum GetDataContextMember<TDest>(string memberName, out TDest destValue)
        {
            destValue = default(TDest);

            var curCtx = Current();
            if (curCtx == null)
                return DataOperationResultEnum.TargetNull;

            var type = curCtx.GetType();
            GetAcessorFor(type, memberName, out var getter, out _, out var srcType);
            if (getter == null)
                return DataOperationResultEnum.MissingAcessor;

            object srcValue = null;
            try
            {
                srcValue = getter(curCtx);
            }
            catch
            {
                return DataOperationResultEnum.TargetException;
            }

            if (srcValue == null)
                return DataOperationResultEnum.TargetNull;

            var dstType = typeof(TDest);

            if (srcType == dstType)
            {
                destValue = (TDest)srcValue;
                return DataOperationResultEnum.Success;
            }
            
            if(!_converterCache.TryGetValue(srcType, out var converter))
                converter = _converterCache[srcType] = TypeDescriptor.GetConverter(srcType);

            if (!converter.CanConvertTo(dstType))
                return DataOperationResultEnum.IncompatibleTypes;

            destValue = (TDest)converter.ConvertTo(srcValue, dstType);
            return DataOperationResultEnum.Success;
        }


        public DataOperationResultEnum SetDataContextMember<TSource>(string memberName, TSource srcValue)
        {
            var curCtx = Current();
            if (curCtx == null)
                return DataOperationResultEnum.TargetNull;

            var type = curCtx.GetType();
            GetAcessorFor(type, memberName, out _, out var setter, out var dstType);
            if (setter == null)
                return DataOperationResultEnum.MissingAcessor;

            var dstValue = dstType.IsValueType ? Activator.CreateInstance(dstType) : null;
            var srcType = typeof(TSource);
            if (srcType != dstType)
            {
                if (!_converterCache.TryGetValue(dstType, out var converter))
                    converter = _converterCache[dstType] = TypeDescriptor.GetConverter(dstType);

                if (!converter.CanConvertFrom(srcType))
                    return DataOperationResultEnum.IncompatibleTypes;

                dstValue = converter.ConvertFrom(srcValue);
            }

            try
            {
                setter(curCtx, dstValue);
            }
            catch
            {
                return DataOperationResultEnum.TargetException;
            }

            return DataOperationResultEnum.Success;
        }

        public void Pop()
        {
            Stack.Pop();
        }

        public void PushValue(object value)
        {
            if (value == null)
            {
                Stack.Push(null);
            }
            else if (!value.GetType().IsVisible)
            {
                Console.WriteLine("[JankyStack] Value type is not public '{0}'", value);
                Stack.Push(null);
            }
            else
            {
                Stack.Push(value);
            }
        }

        public void PushProperty(string propertyName)
        {
            var curCtx = Current();
            if (curCtx == null)
            {
                Stack.Push(null);
            }
            else
            {
                var curType = curCtx.GetType();
                GetAcessorFor(curType, propertyName, out var getter, out _, out _);
                if (getter == null)
                {
                    Console.WriteLine("[JankyStack] Property has no (visible) getter: '{0}.{1}'", curType, propertyName);
                    Stack.Push(null);
                }
                else
                {
                    object next;
                    try
                    {
                        next = getter(curCtx);
                        if (next != null && !next.GetType().IsVisible)
                        {
                            Console.WriteLine("[JankyStack] Property '{0}' is of Type '{1}', and the type is not public.", propertyName, next.GetType());
                            next = null;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("[JankyStack] Exception Thrown by Target Property '{0}'", propertyName);
                        next = null;
                    }

                    Stack.Push(next);
                }
            }
        }


    }
}
