using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Textbox")]
    [JankyProperty("text", nameof(Text))]
    [JankyProperty("type", nameof(Type), DefaultValue = "simple")]
    [JankyProperty("mask", nameof(Mask), DefaultValue = "*")]
    [JankyProperty("length", nameof(Length), DefaultValue = "-1")]
    internal class TextboxNode : LayoutNode
    {
        public enum TextBoxTypeEnum
        {
            Simple,
            Multiline,
            Password
        }

        public readonly PropertyBinding<string> Text;
        public readonly PropertyBinding<TextBoxTypeEnum> Type;
        public readonly PropertyBinding<char> Mask;
        public readonly PropertyBinding<int> Length;

        public override void Execute()
        {
            if (Length <= 0)
            {
                switch (Type.Value)
                {
                    case TextBoxTypeEnum.Simple:
                        Text.Value = GUILayout.TextField(Text, GetLayoutOptions());
                        break;

                    case TextBoxTypeEnum.Multiline:
                        Text.Value = GUILayout.TextArea(Text, GetLayoutOptions());
                        break;

                    case TextBoxTypeEnum.Password:
                        Text.Value = GUILayout.PasswordField(Text, Mask, GetLayoutOptions());
                        break;
                }
            }else
            {
                switch (Type.Value)
                {
                    case TextBoxTypeEnum.Simple:
                        Text.Value = GUILayout.TextField(Text, Length, GetLayoutOptions());
                        break;

                    case TextBoxTypeEnum.Multiline:
                        Text.Value = GUILayout.TextArea(Text, Length, GetLayoutOptions());
                        break;

                    case TextBoxTypeEnum.Password:
                        Text.Value = GUILayout.PasswordField(Text, Mask, Length, GetLayoutOptions());
                        break;
                }
            }
        }
    }
}
