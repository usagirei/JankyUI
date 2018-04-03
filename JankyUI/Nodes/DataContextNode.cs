using System;
using System.Reflection;
using JankyUI.Attributes;
using JankyUI.Binding;

namespace JankyUI.Nodes
{
    [JankyTag("DataContext")]
    [JankyProperty("property", nameof(PropertyName))]
    internal class DataContextNode : Node
    {
        private object _dataContext;
        private string _propertyName;

        private Type _curType;
        private Func<object, object> _getter;

        public override object DataContext
        {
            get
            {
                return _getter(RawDataContext);
            }
        }

        public object RawDataContext
        {
            get
            {
                return _dataContext ?? base.DataContext;
            }
            set
            {
                _dataContext = value;
                Validate();
            }
        }

        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                    _propertyName = null;
                else if (value[0] == '@')
                    _propertyName = value.Substring(1);
                else
                    _propertyName = value;
                Validate();
            }
        }

        private void Validate()
        {
            var dataContext = RawDataContext;
            if (_propertyName == null || dataContext == null)
            {
                _getter = (x) => x;
                return;
            }
            if (_curType != dataContext.GetType())
            {
                _curType = dataContext.GetType();

                var prop = _curType.GetProperty(PropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                {
                    Console.WriteLine("Type {0} has no Visible Property '{1}'", _curType, PropertyName);
                    _getter = (_) => null;
                    return;
                }
                BindingUtils.MakePropertyAcessors(prop, out _getter, out _);
            }
        }

        public override void Execute()
        {
            foreach (var child in Children)
                child.Execute();
        }

        public void SetDataContextRaw(object dc)
        {
            _dataContext = dc;
        }

        public object GetDataContextRaw()
        {
            return _dataContext ?? ParentNode.DataContext;
        }
    }
}
