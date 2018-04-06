using System;
using System.Collections.Generic;
using JankyUI.Nodes;
using UnityEngine;

namespace JankyUI
{
    internal class JankyNodeContext : IJankyContext
    {
        internal Node RootNode { get; set; }
        public bool Active { get; set; }
        public object DataContext { get; set; }
        public JankyDataContextStack DataContextStack { get; }
        public Dictionary<string, object> Resources { get; }
        public GUISkin Skin { get; set; }
        public int WindowID { get; set; }
        public JankyNodeContext(object dataContext)
        {
            if (dataContext != null && !dataContext.GetType().IsVisible)
            {
                Console.WriteLine("[JankyContext] DataContext must be a public type.");
                dataContext = null;
            }
            DataContext = dataContext;
            DataContextStack = new JankyDataContextStack(this);
            Resources = new Dictionary<string, object>();
        }

        public void OnGUI()
        {
            if (!Active)
                return;

            DataContextStack.Begin();

            RootNode.Execute();

            DataContextStack.End();
        }
    }
}
