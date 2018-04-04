using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Checkbox")]
    [JankyProperty("text", nameof(Text))]
    [JankyProperty("checked", nameof(Checked))]
    [JankyProperty("on-change", nameof(OnChange))]
    internal class CheckboxNode : LayoutNode
    {
        public readonly JankyProperty<bool> Checked;
        public readonly JankyProperty<string> Text;
        public readonly JankyMethod<Action<bool>> OnChange;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Checkbox: {0} {1}", Text, Checked);
#else
            var value = GUILayout.Toggle(Checked, Text, GetLayoutOptions());
            Checked.Value = value;
            if (Checked.LastSetResult != DataOperationResultEnum.Unchanged)
                OnChange.Invoke(value);
#endif
        }
    }
}
