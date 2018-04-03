using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Area")]
    [JankyProperty("x", nameof(X))]
    [JankyProperty("y", nameof(Y))]
    [JankyProperty("width", nameof(Width))]
    [JankyProperty("height", nameof(Height))]
    internal class AreaNode : Node
    {
        public readonly DataContextProperty<float> X;
        public readonly DataContextProperty<float> Y;
        public readonly DataContextProperty<float> Width;
        public readonly DataContextProperty<float> Height;

        public Rect AreaRect
        {
            get
            {
                return new Rect(X, Y, Width, Height);
            }
            set
            {
                X.Value = value.x;
                Y.Value = value.y;
                Width.Value = value.width;
                Height.Value = value.height;
            }
        }

        protected override void OnGUI()
        {
            GUILayout.BeginArea(AreaRect);

            foreach (var child in Children)
                child.Execute();

            GUILayout.EndArea();
        }
    }
}
