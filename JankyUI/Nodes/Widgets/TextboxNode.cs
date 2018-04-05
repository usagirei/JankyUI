using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
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
        public JankyProperty<string> Text;
        public JankyProperty<TextBoxTypeEnum> Type;
        public JankyProperty<char> Mask;
        public JankyProperty<int> Length;
        public JankyMethod<Action<string>> OnChange;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Textbox: {0} {1} {2} {3}", Text, Type, Mask, Length);
#else

            string value = Text.Value;
            GUILayoutOption[] layoutOptions = GetLayoutOptions();
            int length = Length;
            char mask = Mask;

            if (Length <= 0)
            {
                switch (Type.Value)
                {
                    case TextBoxTypeEnum.Simple:
                        value = GUILayout.TextField(value, layoutOptions);
                        break;

                    case TextBoxTypeEnum.Multiline:
                        value = GUILayout.TextArea(value, layoutOptions);
                        break;

                    case TextBoxTypeEnum.Password:
                        value = GUILayout.PasswordField(value, mask, layoutOptions);
                        break;
                }
            }
            else
            {
                switch (Type.Value)
                {
                    case TextBoxTypeEnum.Simple:
                        value = GUILayout.TextField(value, length, layoutOptions);
                        break;

                    case TextBoxTypeEnum.Multiline:
                        value = GUILayout.TextArea(value, length, layoutOptions);
                        break;

                    case TextBoxTypeEnum.Password:
                        value = GUILayout.PasswordField(value, mask, length, layoutOptions);
                        break;
                }
            }

            Text.Value = value;
            if (Text.LastSetResult == DataOperationResultEnum.Success)
                OnChange.Invoke(value);
#endif
        }
    }
}
