using System;

namespace JankyUI.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class JankyTagAttribute : Attribute
    {
        public string Name { get; }

        public JankyTagAttribute(string name)
        {
            Name = name;
        }
    }
}
