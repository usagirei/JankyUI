using System;

namespace JankyUI.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class JankyPropertyAttribute : Attribute
    {
        public string Name { get; }
        public string Target { get; }
        public string DefaultValue { get; set; }

        public JankyPropertyAttribute(string name, string target)
        {
            Name = name;
            Target = target;
        }
    }
}
