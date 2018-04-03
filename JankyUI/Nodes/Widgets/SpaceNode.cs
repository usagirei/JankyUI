using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Space")]
    [JankyProperty("size", nameof(Size), DefaultValue = "NaN")]
    internal class SpaceNode : Node
    {
        public readonly PropertyBinding<float> Size;

        public override void Execute()
        {
            if (float.IsNaN(Size))
                GUILayout.FlexibleSpace();
            else
                GUILayout.Space(Size);
        }
    }
}
