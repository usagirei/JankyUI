﻿using System;
using JankyUI.Nodes;

namespace JankyUI.Nodes.Binding
{
    // TODO: make Lazy Binding like PropertyBinding (Only Recreate on type change, not on target)
    // That won't crash with invalid method signature but rather do nothing
    internal class JankyMethod<TDelegate>
        where TDelegate : class
    {
        public string MethodName { get; }
        public Node TargetNode { get; }
        private TDelegate _delegate;

        static JankyMethod()
        {
            Empty = DynamicUtils.MakeEmptyDelegate<TDelegate>();
        }

        // TODO: Try-Catch on JankyMethod Callers
        public TDelegate Invoke
        {
            get
            {
                if (TargetNode == null || MethodName.IsNullOrWhiteSpace())
                    return Empty;
                Validate();
                return _delegate;
            }
        }

        public JankyMethod(Node targetNode, string method)
        {
            if (!typeof(TDelegate).IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("Generic Type is not a Delegate", nameof(TDelegate));

            MethodName = method;
            TargetNode = targetNode;
        }

        public JankyMethod()
        {
            MethodName = null;
            TargetNode = null;
        }

        private void Validate()
        {
            var del = _delegate as Delegate;
            if (TargetNode == null || TargetNode.DataContext == null)
            {
                _delegate = Empty;
            }
            else if (del == null || !ReferenceEquals(del.Target, TargetNode.DataContext))
            {
                var targetObj = TargetNode.DataContext;
                var targetType = targetObj.GetType();
                var targetMethod = targetType.GetMethod(MethodName,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static
                );

                if (targetMethod == null)
                {
                    Console.WriteLine("[JankyMethod] Method '{0}' does not exist on '{1}' or is not public",
                        MethodName,
                        targetType.FullName
                    );
                    _delegate = Empty;
                    return;
                }


                try
                {
                    _delegate = (targetMethod.IsStatic)
                        ? DynamicUtils.MakeCompatibleDelegate<TDelegate>(targetMethod)
                        : DynamicUtils.MakeCompatibleDelegate<TDelegate>(targetMethod, targetObj);
                }
                catch
                {
                    Console.WriteLine("[JankyMethod] Target Method '{0}' is not Compatible with '{1}'",
                        targetType + "." + targetMethod.Name,
                        typeof(TDelegate).ToString()
                    );
                    _delegate = Empty;
                    return;
                }
            }
        }

        private static TDelegate Empty { get; }
    }
}
