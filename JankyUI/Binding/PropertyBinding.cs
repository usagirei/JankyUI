using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JankyUI.Nodes;

namespace JankyUI.Binding
{
    internal class DataContextProperty<T>
    {
        private T _internalValue;
        private T _defaultValue;

        public string DataContextMember { get; }

        public Node TargetNode { get; }

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public DataContextProperty(Node targetNode, string property, T defaultValue)
        {
            DataContextMember = property.IsNullOrWhiteSpace() ? null : property;
            TargetNode = targetNode;
            _defaultValue = defaultValue;
        }

        public DataContextProperty(Node targetNode, string property)
        {
            DataContextMember = property.IsNullOrWhiteSpace() ? null : property;
            TargetNode = targetNode;
        }

        public DataContextProperty(T value)
        {
            TargetNode = null;
            DataContextMember = null;

            _internalValue = value;
        }

        public DataContextProperty()
        {
            TargetNode = null;
            DataContextMember = null;

            _internalValue = default(T);
        }

        private T GetValue()
        {
            if (TargetNode == null || DataContextMember == null)
                return _internalValue;

            var stack = TargetNode.Context.DataContextStack;
            var op = stack.GetDataContextMember<T>(DataContextMember, out var value);
            switch (op)
            {
                case JankyDataContextStack.DataContextOperationResult.Success:
                    return _internalValue = value;
                case JankyDataContextStack.DataContextOperationResult.PropertyNull:
                case JankyDataContextStack.DataContextOperationResult.TargetNull:
                    return _defaultValue;
                case JankyDataContextStack.DataContextOperationResult.MissingAcessor:
                    Console.WriteLine("Property has no Getter: {0}", DataContextMember);
                    return _defaultValue;
                default:
                    throw new ArgumentOutOfRangeException("Invalid Data Operation");
            }
            
        }

        private void SetValue(T value)
        {
            if (TargetNode == null || DataContextMember == null)
            {
                _internalValue = value;
                return;
            }

            if (!Equals(_internalValue, value))
            {
                var stack = TargetNode.Context.DataContextStack;
                var op = stack.SetDataContextMember<T>(DataContextMember, value);
                switch (op)
                {
                    case JankyDataContextStack.DataContextOperationResult.Success:
                    case JankyDataContextStack.DataContextOperationResult.TargetNull:
                        _internalValue = value;
                        return;
                    case JankyDataContextStack.DataContextOperationResult.MissingAcessor:
                        Console.WriteLine("Property has no Setter: {0}", DataContextMember);
                        _internalValue = value;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid Data Operation");
                }
            }
        }

        public static explicit operator DataContextProperty<T>(T value)
        {
            return new DataContextProperty<T>(null, null, default(T));
        }

        public static implicit operator T(DataContextProperty<T> binding)
        {
            return binding.Value;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? "null";
        }
    }
}
