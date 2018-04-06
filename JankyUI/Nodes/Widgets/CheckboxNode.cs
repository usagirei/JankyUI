using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using JankyUI.EventArgs;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Checkbox")]
    [JankyProperty("text", nameof(Text))]
    [JankyProperty("checked", nameof(Checked))]
    [JankyProperty("on-change", nameof(OnChange))]
    internal class CheckboxNode : LayoutNode
    {
        public JankyMethod<Action<JankyEventArgs<bool>>> OnChange;

        public JankyProperty<bool> Checked;
        public JankyProperty<string> Text;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Checkbox: {0} {1}", Text, Checked);
#else
            bool oldValue = Checked;
            var newValue = GUILayout.Toggle(oldValue, Text, GetLayoutOptions());

            Checked.Value = newValue;
            if (Checked.LastSetResult != DataOperationResultEnum.Unchanged)
                OnChange.Invoke(new JankyEventArgs<bool>(Context.WindowID, Name, oldValue, newValue));
#endif
        }
    }
}
