using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Toolbar")]
    [JankyProperty("selectedIndex", nameof(SelectedIndex))]
    [JankyProperty("onSelect", nameof(OnSelect))]
    [JankyProperty("items", nameof(Items))]
    internal class ToolbarNode : LayoutNode
    {
        public readonly PropertyBinding<int> SelectedIndex;
        public readonly PropertyBinding<string[]> Items;
        public readonly MethodBinding<Action<int>> OnSelect;

        public override void Execute()
        {
            var prevIdx = SelectedIndex.Value;
            string[] items = Items;
            var newIdx = GUILayout.Toolbar(SelectedIndex, items, GetLayoutOptions());
            if (newIdx != prevIdx)
            {
                SelectedIndex.Value = newIdx;
                OnSelect.Invoke(newIdx);
            }
        }
    }
}
