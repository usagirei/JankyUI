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
        object IJankyContext.DataContext { get; set; }
        GUISkin IJankyContext.Skin { get; set; }

        internal Node RootNode { get; set; }

        public JankyDataContextStack DataContextStack { get; }

        public JankyNodeContext(object dc)
        {
            ((IJankyContext)this).DataContext = dc;
            DataContextStack = new JankyDataContextStack(this);
        }

        public void OnGUI()
        {
            DataContextStack.Begin();

            RootNode.Execute();

            DataContextStack.End();
        }
    }
}
