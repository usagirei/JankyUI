using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using JankyUI.EventArgs;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Textbox")]
    [JankyProperty("text", nameof(Text), DefaultValue = "")]
    [JankyProperty("type", nameof(Type), DefaultValue = "simple")]
    [JankyProperty("mask", nameof(Mask), DefaultValue = "*")]
    [JankyProperty("max-length", nameof(Length), DefaultValue = "0")]
    [JankyProperty("on-change", nameof(OnChange))]
    internal class TextboxNode : LayoutNode
    {
        public JankyMethod<Action<JankyEventArgs<string>>> OnChange;

        public JankyProperty<string> Text;
        public JankyProperty<TextBoxTypeEnum> Type;
        public JankyProperty<char> Mask;
        public JankyProperty<int> Length;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Textbox: {0} {1} {2} {3}", Text, Type, Mask, Length);
#else

            string oldValue = Text.Value;
            string newValue = oldValue;
            GUILayoutOption[] layoutOptions = GetLayoutOptions();
            int length = Length;
            char mask = Mask;

            if (Length <= 0)
            {
                switch (Type.Value)
                {
                    case TextBoxTypeEnum.Simple:
                        newValue = GUILayout.TextField(oldValue, layoutOptions);
                        break;

                    case TextBoxTypeEnum.Multiline:
                        newValue = GUILayout.TextArea(oldValue, layoutOptions);
                        break;

                    case TextBoxTypeEnum.Password:
                        newValue = GUILayout.PasswordField(oldValue, mask, layoutOptions);
                        break;
                }
            }
            else
            {
                switch (Type.Value)
                {
                    case TextBoxTypeEnum.Simple:
                        newValue = GUILayout.TextField(oldValue, length, layoutOptions);
                        break;

                    case TextBoxTypeEnum.Multiline:
                        newValue = GUILayout.TextArea(oldValue, length, layoutOptions);
                        break;

                    case TextBoxTypeEnum.Password:
                        newValue = GUILayout.PasswordField(oldValue, mask, length, layoutOptions);
                        break;
                }
            }

            Text.Value = newValue;
            if (Text.LastSetResult == DataOperationResultEnum.Success)
                OnChange.Invoke(new JankyEventArgs<string>(Context.WindowID, Name, oldValue, newValue));
#endif
        }
    }
}
