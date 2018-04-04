using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JankyUI.Enums;
using JankyUI.Nodes;

namespace JankyUI.Binding
{
    internal class JankyProperty<T>
    {
        private T _defaultValue;
        private T _internalValue;
        public string DataContextMember { get; }

        public DataOperationResultEnum LastGetResult { get; private set; }
        public DataOperationResultEnum LastSetResult { get; private set; }

        public Node TargetNode { get; }

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public JankyProperty(Node targetNode, string property, T defaultValue)
        {
            DataContextMember = property.IsNullOrWhiteSpace() ? null : property;
            TargetNode = targetNode;
            _defaultValue = defaultValue;
        }

        public JankyProperty(T value)
        {
            TargetNode = null;
            DataContextMember = null;

            _internalValue = value;
        }

        public JankyProperty()
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
                case DataOperationResultEnum.Success:
                    LastGetResult = (!Equals(_internalValue, value))
                        ? op
                        : DataOperationResultEnum.Unchanged;
                    return _internalValue = value;
                case DataOperationResultEnum.PropertyNull:
                case DataOperationResultEnum.TargetNull:
                    return _defaultValue;
                case DataOperationResultEnum.MissingAcessor:
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
                LastSetResult = stack.SetDataContextMember<T>(DataContextMember, value);
                switch (LastSetResult)
                {
                    case DataOperationResultEnum.Success:
                    case DataOperationResultEnum.TargetNull:
                        _internalValue = value;
                        return;
                    case DataOperationResultEnum.MissingAcessor:
                        Console.WriteLine("Property has no Setter: {0}", DataContextMember);
                        _internalValue = value;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid Data Operation");
                }
            }else
            {
                LastSetResult = DataOperationResultEnum.Unchanged;
            }
        }

        public static explicit operator JankyProperty<T>(T value)
        {
            return new JankyProperty<T>(null, null, default(T));
        }

        public static implicit operator T(JankyProperty<T> binding)
        {
            return binding.Value;
        }

        public override string ToString()
        {
            return (Value?.ToString() ?? "null");
        }
    }
}
