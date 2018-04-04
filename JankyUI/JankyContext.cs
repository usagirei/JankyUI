using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JankyUI.Binding;
using JankyUI.Nodes;
using UnityEngine;

namespace JankyUI
{
    internal partial class JankyNodeContext : IJankyContext
    {
        public object DataContext { get; set; }
        public GUISkin Skin { get; set; }
        public Dictionary<string,object> Resources { get; }

        public int WindowID { get; set; }
        internal Node RootNode { get; set; }

        public JankyDataContextStack DataContextStack { get; }

        public JankyNodeContext(object dataContext)
        {
            DataContext = dataContext;
            DataContextStack = new JankyDataContextStack(this);
            Resources = new Dictionary<string, object>();
        }

        public void OnGUI()
        {
            DataContextStack.Begin();

            RootNode.Execute();

            DataContextStack.End();
        }
    }
}
