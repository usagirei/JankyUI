using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("ScrollView")]
    [JankyProperty("x-offset", nameof(ScrollX))]
    [JankyProperty("y-offset", nameof(ScrollY))]
    internal class ScrollViewNode : LayoutNode
    {
        public JankyProperty<float> ScrollX;
        public JankyProperty<float> ScrollY;

        private Vector2 ScrollPos
        {
            get
            {
                return new Vector2(ScrollX, ScrollY);
            }
            set
            {
                ScrollX.Value = value.x;
                ScrollY.Value = value.y;
            }
        }

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Begin ScrollView: {0} {1}", ScrollX, ScrollY);
            foreach (var child in Children)
                child.Execute();
            Console.Write("End ScrollView");
#else
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GetLayoutOptions());

            foreach (var child in Children)
                child.Execute();

            GUILayout.EndScrollView();
#endif
        }
    }
}
