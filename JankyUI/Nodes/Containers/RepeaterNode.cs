using System;
using System.Collections;
using JankyUI.Attributes;
using JankyUI.Nodes.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Repeater")]
    [JankyProperty("items", nameof(Items))]
    internal class RepeaterNode : Node
    {
        public JankyProperty<object> Items;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Begin Area: {0} {1} {2} {3}", X, Y, Width, Height);
            foreach (var child in Children)
                child.Execute();
            Console.WriteLine("End Area");
#else

            if (Items.Value is IEnumerable coll)
            {
                foreach (var dc in coll)
                {
                    Context.DataContextStack.PushValue(dc);
                    foreach (var child in Children)
                    {
                        child.Execute();
                    }
                    Context.DataContextStack.Pop();
                }
            }
            else
            {
                Context.DataContextStack.PushValue(Items.Value);
                foreach (var child in Children)
                {
                    child.Execute();
                }
                Context.DataContextStack.Pop();
            }


#endif
        }
    }
}
