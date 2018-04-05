using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JankyUI.Binding;
using JankyUI.Enums;
using JankyUI.Nodes;
using UnityEngine;

namespace JankyUI
{
    internal class JankyDataContextStack
    {
        private Dictionary<(Type, String), (Func<object, object>, Action<object, object>)> _acessorCache
            = new Dictionary<(Type, string), (Func<object, object>, Action<object, object>)>();

        private Func<object, object> _emptyGetter
            = BindingUtils.MakeEmptyDelegate<Func<object, object>>();

        private Stack<object> Stack { get; }
                            = new Stack<object>();
        public IJankyContext JankyContext { get; }

        public JankyDataContextStack(IJankyContext jank)
        {
            JankyContext = jank;
        }

        private void GetAcessorFor(Type curType, string memberName, out Func<object, object> get, out Action<object, object> set)
        {
            var typeKey = (curType, memberName);
            if (!_acessorCache.TryGetValue(typeKey, out var getSetPair))
            {
                Console.WriteLine("[JankyStack] Creating new getter for '{0}.{1}'", curType, memberName);
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
                var targetMember = curType.GetMember(memberName, flags).FirstOrDefault();
                switch (targetMember)
                {
                    case PropertyInfo prop_info:
                        {
                            BindingUtils.MakePropertyAcessors(prop_info, out get, out set);
                        }
                        break;
                    case FieldInfo field_info:
                        {
                            BindingUtils.MakeFieldAcessors(field_info, out get, out set);
                        }
                        break;
                    case null:
                        {
                            Console.WriteLine($"[JankyStack] Target Type has no public member named '{memberName}'");
                            get = null;
                            set = null;
                        }
                        break;
                    default:
                        {
                            Console.WriteLine("[JankyStack] Unsupported Member Type: '{0}'", memberName);
                            get = null;
                            set = null;
                        }
                        break;
                }

                _acessorCache[typeKey] = getSetPair = (get, set);
            }
            get = getSetPair.Item1;
            set = getSetPair.Item2;
        }

        public void Begin()
        {
            Stack.Push(JankyContext.DataContext);
        }

        public object Current()
        {
            return Stack.Count == 0 ? null : Stack.Peek();
        }

        public void End()
        {
            // Unbalanced Stack
            if (Stack.Count != 1 || !ReferenceEquals(JankyContext.DataContext, Stack.Peek()))
            {
                throw new Exception("Unbalanced Stack");
            }
        }

        public DataOperationResultEnum GetDataContextMember<T>(string memberName, out T value)
        {
            value = default(T);

            var curCtx = Stack.Peek();
            if (curCtx == null)
                return DataOperationResultEnum.TargetNull;

            var type = curCtx.GetType();
            GetAcessorFor(type, memberName, out var getter, out _);
            if (getter == null)
                return DataOperationResultEnum.MissingAcessor;

            var retVal = getter(curCtx);
            if (retVal == null)
                return DataOperationResultEnum.TargetNull;

            value = (T)retVal;
            return DataOperationResultEnum.Success;
        }

        public void Pop()
        {
            Stack.Pop();
        }

        public void Push(string propertyName)
        {
            var curCtx = Stack.Peek();
            if (curCtx == null)
            {
                Stack.Push(null);
            }
            else
            {
                var curType = curCtx.GetType();
                GetAcessorFor(curType, propertyName, out var getter, out _);
                if (getter == null)
                {
                    Console.WriteLine("[JankyStack] Property has no (visible) getter: '{0}.{1}'", curType, propertyName);
                    Stack.Push(null);
                }
                else
                {
                    var next = getter(curCtx);
                    if(next != null && !next.GetType().IsVisible)
                    {
                        Console.WriteLine("[JankyStack] Property '{0}' is of Type '{1}', and the type is not public.", propertyName, next.GetType());
                        next = null;
                    }
                    Stack.Push(next);
                }
            }
        }

        public DataOperationResultEnum SetDataContextMember<T>(string memberName, T value)
        {
            var curCtx = Stack.Peek();
            if (curCtx == null)
                return DataOperationResultEnum.TargetNull;

            var type = curCtx.GetType();
            GetAcessorFor(type, memberName, out _, out var setter);
            if (setter == null)
                return DataOperationResultEnum.MissingAcessor;

            setter(curCtx, value);
            return DataOperationResultEnum.Success;
        }

    }
}
