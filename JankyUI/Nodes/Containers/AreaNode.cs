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
        public readonly PropertyBinding<float> X;
        public readonly PropertyBinding<float> Y;
        public readonly PropertyBinding<float> Width;
        public readonly PropertyBinding<float> Height;

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

        public override void Execute()
        {
            GUILayout.BeginArea(AreaRect);

            foreach (var child in Children)
                child.Execute();

            GUILayout.EndArea();
        }
    }
}
