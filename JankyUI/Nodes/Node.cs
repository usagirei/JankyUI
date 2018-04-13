
using System;
using System.Collections.Generic;
using JankyUI.Attributes;
using JankyUI.Nodes.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyProperty("data-context", nameof(DataContextName))]
    [JankyProperty("name", nameof(Name))]
    internal class Node
    {
        public JankyProperty<string> Name;
        public string DataContextName;

        public List<Node> Children { get; set; }
        public JankyNodeContext Context { get; set; }
    
        public virtual GUISkin Skin
        {
            get
            {
                return Context.Skin;
            }
        }

        public object DataContext
        {
            get
            {
                return Context.DataContextStack.Current();
            }
        }

        protected void PushDataContext()
        {
            if (DataContextName.IsNullOrWhiteSpace())
                return;

        }

        public void Execute()
        {
            if (!DataContextName.IsNullOrWhiteSpace())
            {
                if (DataContextName.IndexOf('.') != -1)
                {
                    var parts = DataContextName.Split('.');
                    for (int i = 0; i < parts.Length; i++)
                        Context.DataContextStack.Push(parts[i]);
                    OnGUI();
                    for (int i = 0; i < parts.Length; i++)
                        Context.DataContextStack.Pop();
                }else
                {
                    Context.DataContextStack.Push(DataContextName);
                    OnGUI();
                    Context.DataContextStack.Pop();
                }
            }else
            {
                OnGUI();
            }
        }

        protected virtual void OnGUI()
        {
            foreach (var child in Children)
                child.Execute();
        }
    }
}
