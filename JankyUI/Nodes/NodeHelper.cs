using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using JankyUI.Attributes;
using JankyUI.Binding;

namespace JankyUI.Nodes
{
    internal class NodeHelper
    {
        private Dictionary<string, Action<Node, string>> _setters;
        private Dictionary<string, JankyPropertyAttribute> _properties;
        private Dictionary<string, JankyTagAttribute> _tags;

        public Type NodeType { get; protected set; }
        public IEnumerable<JankyPropertyAttribute> Properties { get { return _properties.Values; } }
        public IEnumerable<JankyTagAttribute> Tags { get { return _tags.Values; } }

        public NodeHelper(Type nodeType)
        {
            _setters = new Dictionary<string, Action<Node, string>>();
            _properties = new Dictionary<string, JankyPropertyAttribute>();
            _tags = new Dictionary<string, JankyTagAttribute>();

            NodeType = nodeType;
            _tags = NodeType.GetCustomAttributes<JankyTagAttribute>(false).ToDictionary(a => a.Name);
            _properties = NodeType.GetCustomAttributes<JankyPropertyAttribute>(true).ToDictionary(a => a.Name);
            var defaultOverrides = NodeType.GetCustomAttributes<JankyDefaultOverrideAttribute>(false);
            foreach (var def in defaultOverrides)
            {
                if (_properties.TryGetValue(def.Property, out var prop))
                    prop.DefaultValue = def.Value;
                else
                    throw new Exception("Unknown Property to Override: " + def.Property);
            }
        }

        private Action<Node, string> MakeJankyPropertySetter(string targetProp)
        {
            var prop = Properties.First(x => x.Name.Equals(targetProp, StringComparison.OrdinalIgnoreCase));
            var memberName = prop.Target;
            var targetType = NodeType;

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var member_info = targetType.GetMember(memberName, flags).First();

            MethodInfo setterMethod;
            switch (member_info)
            {
                case PropertyInfo prop_info:
                    setterMethod = prop_info.GetSetMethod(true);
                    if (setterMethod == null)
                        throw new Exception("Property Has no Setter: " + prop_info.Name);
                    break;

                case MethodInfo method_info:
                    setterMethod = method_info;
                    if (method_info.GetParameters().Length != 1)
                        throw new Exception("Invalid Method Signature. Method must have one argument.");
                    break;

                case FieldInfo field_info:
                    setterMethod = BindingUtils.MakeFieldSetter(field_info);
                    break;

                default:
                    throw new Exception("Member is not Supported");
            }

            var setterDelegate = BindingUtils.MakeCompatibleDelegate<Action<Node, object>>(setterMethod, targetType);
            var propType = setterMethod.GetParameters().Last().ParameterType;

            if (propType.IsSubclassOfRawGeneric(typeof(DataContextProperty<>)))
            {
                var innerType = propType.GetGenericArguments()[0];
                var innerConverter = TypeDescriptor.GetConverter(innerType);
                return (node, value) =>
                {
                    object instance;
                    if (value.IsNullOrWhiteSpace())
                    {
                        instance = prop.DefaultValue == null
                            ? Activator.CreateInstance(propType)
                            : Activator.CreateInstance(propType, innerConverter.ConvertFromString(prop.DefaultValue));
                    }
                    else if (value.StartsWith("@"))
                    {
                        instance = prop.DefaultValue == null
                            ? Activator.CreateInstance(propType, node, value.Substring(1))
                            : Activator.CreateInstance(propType, node, value.Substring(1), innerConverter.ConvertFromString(prop.DefaultValue));
                    }
                    else
                    {
                        if (innerType.IsArray)
                        {
                            var values = value.SplitEx('\\', ',');
                            var elementType = innerType.GetElementType();
                            var elemConverter = TypeDescriptor.GetConverter(elementType);
                            var array = (Array)Activator.CreateInstance(elementType.MakeArrayType(), values.Length);
                            for (int i = 0; i < values.Length; i++)
                                array.SetValue(elemConverter.ConvertFromString(values[i]), i);
                            instance = Activator.CreateInstance(propType, new[] { array });
                        }
                        else
                        {
                            var innerValue = innerConverter.ConvertFromString(value);
                            instance = Activator.CreateInstance(propType, innerValue);
                        }
                    }
                    setterDelegate(node, instance);
                };
            }
            else if (propType.IsSubclassOfRawGeneric(typeof(DataContextMethod<>)))
            {
                return (node, value) =>
                {
                    object instance;
                    if (value.IsNullOrWhiteSpace())
                    {
                        instance = Activator.CreateInstance(propType);
                    }
                    else
                    {
                        instance = value.StartsWith("@")
                            ? Activator.CreateInstance(propType, node, value.Substring(1))
                            : Activator.CreateInstance(propType, node, value);
                    }
                    propType.GetMethod("Validate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, null);
                    setterDelegate(node, instance);
                };
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(propType);
                return (node, value) =>
                {
                    object instance;
                    if (value.IsNullOrWhiteSpace())
                    {
                        instance = (propType.IsValueType)
                            ? Activator.CreateInstance(propType)
                            : null;
                    }
                    else
                    {
                        if (propType.IsArray)
                        {
                            var values = value.SplitEx('\\', ',');
                            var elementType = propType.GetElementType();
                            var elemConverter = TypeDescriptor.GetConverter(elementType);
                            var array = (Array)Activator.CreateInstance(elementType.MakeArrayType(), values.Length);
                            for (int i = 0; i < values.Length; i++)
                                array.SetValue(elemConverter.ConvertFrom(values[i]), i);
                            instance = Activator.CreateInstance(propType, new[] { array });
                        }
                        else
                        {
                            instance = converter.ConvertFrom(value);
                        }
                    }
                    setterDelegate(node, instance);
                };
            }
        }

        public Node Activate(JankyNodeContext context)
        {
            return Activate(context, null);
        }

        public Node Activate(JankyNodeContext context, Dictionary<string, string> initProps)
        {
            var node = (Node)FormatterServices.GetUninitializedObject(NodeType);

            node.Children = new List<Node>();
            node.Context = context;

            foreach (var kvp in _properties)
            {
                if (initProps == null || !initProps.TryGetValue(kvp.Key, out string value))
                    value = kvp.Value.DefaultValue;
                SetProperty(node, kvp.Key, value);
            }

            NodeType.GetConstructor(Type.EmptyTypes).Invoke(node, null);

            return node;
        }

        public bool HasProperty(string property)
        {
            return _properties.ContainsKey(property);
        }

        public bool SetProperty(Node node, string property, string value)
        {
            if (!HasProperty(property))
                return false;

            if (!_setters.TryGetValue(property, out var setter))
            {
                setter = _setters[property] = MakeJankyPropertySetter(property);
            }
            setter(node, value);
            return true;
        }
    }

    internal static class NodeHelper<TNode>
        where TNode : Node, new()
    {
        private static NodeHelper _instance;

        public static NodeHelper Instance
        {
            get {
                return _instance ?? (_instance = new NodeHelper(typeof(TNode)));
            }
        }
    }
}
