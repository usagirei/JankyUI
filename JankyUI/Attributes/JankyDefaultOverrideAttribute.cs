using System;

namespace JankyUI.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class JankyDefaultOverrideAttribute : Attribute
    {
        public string Property { get; }
        public string Value { get; }

        public JankyDefaultOverrideAttribute(string propName, string value)
        {
            Property = propName;
            Value = value;
        }
    }
}
