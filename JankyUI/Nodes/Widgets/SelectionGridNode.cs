using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{


    [JankyTag("SelectionGrid")]
    [JankyProperty("items", nameof(Items))]
    [JankyProperty("columns", nameof(Columns))]
    [JankyProperty("selectedIndex", nameof(SelectedIndex))]
    [JankyProperty("onSelect", nameof(OnSelect))]
    internal class SelectionGridNode : LayoutNode
    {
        public readonly DataContextProperty<string[]> Items;
        public readonly DataContextProperty<int> Columns;
        public readonly DataContextProperty<int> SelectedIndex;
        public readonly DataContextMethod<Action<int>> OnSelect;

        public override void Execute()
        {
            var prevIdx = SelectedIndex.Value;
            string[] items = Items;
            var newIdx = GUILayout.SelectionGrid(SelectedIndex, items, Columns, GetLayoutOptions());
            if (newIdx != prevIdx)
            {
                SelectedIndex.Value = newIdx;
                OnSelect.Invoke(newIdx);
            }
        }
    }
}
