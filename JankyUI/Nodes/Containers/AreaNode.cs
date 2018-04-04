using System;
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
        public readonly JankyProperty<float> X;
        public readonly JankyProperty<float> Y;
        public readonly JankyProperty<float> Width;
        public readonly JankyProperty<float> Height;

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
#if MOCK
            Console.WriteLine("Begin Area: {0} {1} {2} {3}", X, Y, Width, Height);
            foreach (var child in Children)
                child.Execute();
            Console.WriteLine("End Area");
#else
            GUILayout.BeginArea(AreaRect);

            foreach (var child in Children)
                child.Execute();

            GUILayout.EndArea();
#endif
        }
    }
}
