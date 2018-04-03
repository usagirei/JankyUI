using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("ScrollView")]
    [JankyProperty("scroll-x", nameof(ScrollX))]
    [JankyProperty("scroll-y", nameof(ScrollY))]
    internal class ScrollViewNode : LayoutNode
    {
        public readonly DataContextProperty<float> ScrollX;
        public readonly DataContextProperty<float> ScrollY;

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

        public override void Execute()
        {
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GetLayoutOptions());

            foreach (var child in Children)
                child.Execute();

            GUILayout.EndScrollView();
        }
    }
}
