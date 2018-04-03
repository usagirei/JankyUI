using System;
using System.Linq;
using System.Reflection;
using JankyUI.Nodes;

namespace JankyUI.Binding
{
    internal class DataContextProperty<T>
    {
        private Type _propType;
        private T _internalValue;

        private Func<object, T> _getter;
        private Action<object, T> _setter;

        public string DataContextMember { get; }
        public Node TargetNode { get; }

        public bool IsBound
        {
            get
            {
                return TargetNode != null && DataContextMember !=  null;
            }
        }
        public bool CanRead
        {
            get
            {
                return TargetNode == null || DataContextMember == null || _getter != null;
            }
        }

        public bool CanWrite
        {
            get
            {
                return TargetNode == null || DataContextMember == null || _setter != null;
            }
        }

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public PropertyBinding(Node targetNode, string property)
        {
            DataContextMember = property.IsNullOrWhiteSpace() ? null : property;
            TargetNode = targetNode;
        }

        public PropertyBinding(T value)
        {
            TargetNode = null;
            DataContextMember = null;

            _internalValue = value;
        }

        public PropertyBinding()
        {
            TargetNode = null;
            DataContextMember = null;

            _internalValue = default(T);
        }

        private T GetValue()
        {
            if (TargetNode == null || DataContextMember == null)
                return _internalValue;

            var dc = TargetNode.DataContext;
            if (dc == null)
            {
                //throw new NullReferenceException("Target DataContext is Null");
                return _internalValue;
            }

            Validate();
            if (!CanRead)
                throw new NotSupportedException("Property has no Getter");

            return _internalValue = _getter(dc);
        }

        private void SetValue(T value)
        {
            if (TargetNode == null || DataContextMember == null)
            {
                _internalValue = value;
                return;
            }

            var dc = TargetNode.DataContext;
            if (dc == null)
            {
                //throw new NullReferenceException("Target DataContext is Null");
                _internalValue = value;
                return;
            }

            Validate();
            if (!CanWrite)
                throw new NotSupportedException("Property has no Setter");

            if (!object.Equals(_internalValue, value))
            {
                _internalValue = value;
                _setter(dc, value);
            }
        }

        private void Validate()
        {
            var dc = TargetNode.DataContext;
            var curType = dc.GetType();
            if (_propType != curType)
            {
                _propType = curType;
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
                var targetMember = curType.GetMember(DataContextMember, flags).FirstOrDefault();
                if (targetMember == null)
                    throw new MissingMemberException($"Target has no public member named '{DataContextMember}'");

                switch (targetMember)
                {
                    case PropertyInfo prop:
                        BindingUtils.MakePropertyAcessors(prop, out _getter, out _setter);
                        break;
                    case FieldInfo field:
                        BindingUtils.MakeFieldAcessors(field, out _getter, out _setter);
                        break;
                    default:
                        throw new ArgumentException("Member Type is not supported: " + targetMember);
                }
            }
        }

        public static explicit operator PropertyBinding<T>(T value)
        {
            return new PropertyBinding<T>(null, null);
        }

        public static implicit operator T(PropertyBinding<T> binding)
        {
            return binding.Value;
        }
    }
}
