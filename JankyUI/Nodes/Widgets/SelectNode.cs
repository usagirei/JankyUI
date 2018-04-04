using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Select")]
    [JankyProperty("items", nameof(Items), DefaultValue = "")]
    [JankyProperty("columns", nameof(Columns))]
    [JankyProperty("selected-index", nameof(SelectedIndex))]
    [JankyProperty("on-change", nameof(OnSelect))]
    internal class SelectNode : LayoutNode
    {
        public readonly JankyProperty<string[]> Items;
        public readonly JankyProperty<int> Columns;
        public readonly JankyProperty<int> SelectedIndex;
        public readonly JankyMethod<Action<int>> OnSelect;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Select: {2} {0} [{1}]", Columns, string.Join(",", Items), SelectedIndex);
#else
            var newIdx = (Columns <= 0)
                ? GUILayout.Toolbar(SelectedIndex, Items, GetLayoutOptions())
                : GUILayout.SelectionGrid(SelectedIndex, Items, Columns, GetLayoutOptions());

            SelectedIndex.Value = newIdx;

            if (SelectedIndex.LastSetResult != DataOperationResultEnum.Unchanged)
                OnSelect.Invoke(newIdx);
#endif
        }
    }
}
