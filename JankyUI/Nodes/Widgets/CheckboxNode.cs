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
    [JankyProperty("image", nameof(Image))]
    [JankyProperty("checked", nameof(Checked))]
    [JankyProperty("on-change", nameof(OnChange))]
    internal class CheckboxNode : LayoutNode
    {
        public JankyMethod<Action<JankyEventArgs<bool>>> OnChange;

        public JankyProperty<bool> Checked;
        public JankyProperty<string> Text;
        public JankyProperty<Texture> Image;

        private readonly GUIContent Content;

        public CheckboxNode()
        {
            Content = new GUIContent();
        }

        private void UpdateContent()
        {
            Content.text = Text;
            Content.image = Image;
        }

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Checkbox: {0} {1}", Text, Checked);
#else
            UpdateContent();
            bool oldValue = Checked;
            var newValue = GUILayout.Toggle(oldValue, Content, GetLayoutOptions());

            Checked.Value = newValue;
            if (Checked.LastSetResult != DataOperationResultEnum.Unchanged)
                OnChange.Invoke(new JankyEventArgs<bool>(Context.WindowID, Name, oldValue, newValue));
#endif
        }
    }
}
