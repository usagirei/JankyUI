using JankyUI.Nodes;
using UnityEngine;

namespace JankyUI
{
    public class JankyContext
    {
        internal Node RootNode { get; set; }

        public object DataContext { get; set; }

        internal JankyContext()
        {
        }

        public void Invoke()
        {
            RootNode.Execute();
        }
    }
}
