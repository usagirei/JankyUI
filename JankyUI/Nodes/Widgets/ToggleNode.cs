using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Toggle")]
    [JankyProperty("text", nameof(Text))]
    [JankyProperty("checked", nameof(Checked))]
    internal class ToggleNode : LayoutNode
    {
        public readonly PropertyBinding<bool> Checked;
        public readonly PropertyBinding<string> Text;

        public override void Execute()
        {
            Checked.Value = GUILayout.Toggle(Checked, Text, GetLayoutOptions());
        }
    }
}
