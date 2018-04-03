using System;
using JankyUI.Nodes;

namespace JankyUI.Binding
{
    // TODO: make Lazy Binding like PropertyBinding (Only Recreate on type change, not on target)
    // That won't crash with invalid method signature but rather do nothing
    internal class MethodBinding<TDelegate>
        where TDelegate : class
    {
        public string DataContextMethod { get; }
        public Node TargetNode { get; }
        private TDelegate _delegate;

        static MethodBinding()
        {
            Empty = BindingUtils.MakeEmptyDelegate<TDelegate>();
        }

        public TDelegate Invoke
        {
            get
            {
                if (TargetNode == null || DataContextMethod.IsNullOrWhiteSpace())
                    return default(TDelegate);

                Validate();
                return _delegate;
            }
        }

        public MethodBinding(Node targetNode, string method)
        {
            if (!typeof(TDelegate).IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("Generic Type is not a Delegate", nameof(TDelegate));

            DataContextMethod = method;
            TargetNode = targetNode;
        }

        public MethodBinding()
        {
            DataContextMethod = null;
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
                var targetMethod = targetType.GetMethod(DataContextMethod);

                if (targetMethod == null)
                {
                    Console.WriteLine("Method '{0}' does not exist on '{1}'",
                        DataContextMethod,
                        targetType.FullName
                    );
                    _delegate = Empty;
                    return;
                }
                if (!targetMethod.IsCompatibleWithDelegate<TDelegate>())
                {
                    Console.WriteLine("Target Method '{0}' is not Compatible with '{1}'",
                        targetType.FullName + "::" + targetMethod.Name,
                        typeof(TDelegate).FullName + "::" + typeof(TDelegate).Name
                    );
                    _delegate = Empty;
                    return;
                }

                _delegate = Delegate.CreateDelegate(typeof(TDelegate), targetObj, targetMethod) as TDelegate;
            }
        }

        private static TDelegate Empty { get; }
    }
}
