using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

            ReadJankyAttributes();
        }

        private void ReadJankyAttributes()
        {
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

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var member_info = NodeType.GetMember(prop.Target, flags).First();

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
                    BindingUtils.MakeFieldGetterSetter(field_info, out _, out setterMethod);
                    break;

                default:
                    throw new Exception("Member is not Supported");
            }

            var setterDelegate = BindingUtils.MakeCompatibleDelegate<Action<Node, object>>(setterMethod);
            var propertyType = setterMethod.GetParameters().Last().ParameterType;

            if (propertyType.IsSubclassOfRawGeneric(typeof(JankyProperty<>)))
            {
                var __wrapper__ = propertyType;
                var __dataType__ = __wrapper__.GetGenericArguments()[0];
                var __converter__ = TypeDescriptor.GetConverter(__dataType__);
                if(!prop.DefaultValue.TryConvertTo(__dataType__, out var __defaultValue__))
                {
                    Console.WriteLine("Invalid Default Value for Property {0} in {1}", prop.Name, NodeType);
                }

                return (node, sourceValue) =>
                {
                    bool escapedSpecialName = false;
                    if (sourceValue?.StartsWith("@@") == true || sourceValue?.StartsWith("##") == true)
                    {
                        sourceValue = sourceValue.Substring(1);
                        escapedSpecialName = true;
                    }

                    object targetValue;
                    // No Value Provided
                    if (sourceValue == null)
                    {
                        targetValue = Activator.CreateInstance(__wrapper__, __defaultValue__);
                    }
                    // Method Binding
                    else if (!escapedSpecialName && sourceValue.StartsWith("@"))
                    {
                        // Strip @ Sign
                        var memberName = sourceValue.Substring(1);

                        targetValue = Activator.CreateInstance(__wrapper__, node, memberName, __defaultValue__);
                    }
                    // Static Resource
                    else if (!escapedSpecialName && sourceValue.StartsWith("#"))
                    {
                        var resourceKey = sourceValue.Substring(1);

                        // Key not Present
                        if (!node.Context.Resources.TryGetValue(resourceKey, out var resource))
                        {
                            Console.WriteLine("Resource Key not Found: {0}", resourceKey);
                            targetValue = Activator.CreateInstance(__wrapper__, __defaultValue__);
                        }
                        else
                        {
                            // Resource is EXPLICITLY set to null
                            if (resource == null && __dataType__.IsValueType)
                            {
                                resource = Activator.CreateInstance(__dataType__);
                            }
                            // Resource is string, but target type isnt, Try Converting
                            else if (resource.GetType() == typeof(string)
                                && __dataType__ != typeof(string)
                                && !((string)resource).TryConvertTo(__dataType__, out resource))
                            {
                                Console.WriteLine("Can't convert Resource String {0} to Target Type {1}", resourceKey, __dataType__);
                                resource = __defaultValue__;
                            }
                            //Else Resource is (probably) same type as target
                            targetValue = Activator.CreateInstance(__wrapper__, resource);
                        }
                    }
                    else
                    {
                        // Direct Set
                        if (__dataType__ == typeof(string))
                        {
                            targetValue = Activator.CreateInstance(__wrapper__, sourceValue);
                        }
                        // Convert and Set
                        else
                        {
                            if (!sourceValue.TryConvertTo(__dataType__, out var converted))
                            {
                                Console.WriteLine("Can't convert String '{0}' to Target Type '{1}'", sourceValue, __dataType__);
                                converted = __defaultValue__;
                            }
                            targetValue = Activator.CreateInstance(__wrapper__, converted);
                        }
                    }
                    setterDelegate(node, targetValue);
                };
            }
            else if (propertyType.IsSubclassOfRawGeneric(typeof(JankyMethod<>)))
            {
                return (node, value) =>
                {
                    object instance;
                    if (value.IsNullOrWhiteSpace())
                    {
                        instance = Activator.CreateInstance(propertyType);
                    }
                    else if (!value.StartsWith("@") || value.StartsWith("@@"))
                    {
                        instance = Activator.CreateInstance(propertyType);
#if DEBUG
                        throw new NotSupportedException("Method Bindings can't be static values");
#else
                        Console.WriteLine("Method Bindings can't be static values");
                        instance = Activator.CreateInstance(propType);
#endif
                    }
                    else
                    {
                        value = value.Substring(1);

                        instance = Activator.CreateInstance(propertyType, node, value);
                    }
                    //propType.GetMethod("Validate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, null);
                    setterDelegate(node, instance);
                };
            }
            else
            {
                var __dataType__ = propertyType;
                var __converter__ = TypeDescriptor.GetConverter(__dataType__);
                if (!prop.DefaultValue.TryConvertTo(__dataType__, out var __defaultValue__))
                {
                    Console.WriteLine("Invalid Default Value for Property {0} in {1}", prop.Name, NodeType);
                }

                var converter = TypeDescriptor.GetConverter(propertyType);
                return (node, sourceValue) =>
                {
                    bool escapedSpecialName = false;
                    if (sourceValue?.StartsWith("@@") == true || sourceValue?.StartsWith("##") == true)
                    {
                        sourceValue = sourceValue.Substring(1);
                        escapedSpecialName = true;
                    }

                    object targetValue;
                    // No Value Provided
                    if (sourceValue.IsNullOrWhiteSpace())
                    {
                        targetValue = __defaultValue__;
                    }
                    else if (!escapedSpecialName && sourceValue.StartsWith("@"))
                    {
                        Console.WriteLine("Normal Properties don't support Binding");
                        targetValue = __defaultValue__;
                    }
                    else if (!escapedSpecialName && sourceValue.StartsWith("#"))
                    {
                        var resourceKey = sourceValue.Substring(1);

                        // Key not Present
                        if (!node.Context.Resources.TryGetValue(resourceKey, out var resource))
                        {
                            Console.WriteLine("Resource Key not Found: {0}", resourceKey);
                            targetValue = __defaultValue__;
                        }
                        else
                        {
                            // Resource is EXPLICITLY set to null
                            if (resource == null && __dataType__.IsValueType)
                            {
                                resource = Activator.CreateInstance(__dataType__);
                            }
                            // Resource is string, but target type isnt, Try Converting
                            else if (resource.GetType() == typeof(string)
                                && __dataType__ != typeof(string)
                                && !((string)resource).TryConvertTo(__dataType__, out resource))
                            {
                                Console.WriteLine("Can't convert Resource String {0} to Target Type {1}", resourceKey, __dataType__);
                                resource = __defaultValue__;
                            }
                            //Else Resource is (probably) same type as target
                            targetValue = resource;
                        }
                    }
                    else
                    {
                        // Direct Set
                        if (__dataType__ == typeof(string))
                        {
                            targetValue = sourceValue;
                        }
                        // Convert and Set
                        else
                        {
                            if (!sourceValue.TryConvertTo(__dataType__, out var converted))
                            {
                                Console.WriteLine("Can't convert String '{0}' to Target Type '{1}'", sourceValue, __dataType__);
                                converted = __defaultValue__;
                            }
                            targetValue = converted;
                        }
                    }
                    setterDelegate(node, targetValue);
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
            get
            {
                return _instance ?? (_instance = new NodeHelper(typeof(TNode)));
            }
        }
    }
}
