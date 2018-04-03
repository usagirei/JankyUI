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
        public readonly DataContextProperty<int> SelectedIndex;
        public readonly DataContextProperty<string[]> Items;
        public readonly DataContextMethod<Action<int>> OnSelect;

        protected override void OnGUI()
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
