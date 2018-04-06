using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using JankyUI.EventArgs;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Button")]
    [JankyProperty("text", nameof(Text))]
    [JankyProperty("image", nameof(Image))]
    [JankyProperty("on-click", nameof(OnClick))]
    [JankyProperty("type", nameof(Type))]
    internal class ButtonNode : LayoutNode
    {
        public JankyMethod<Action<JankyEventArgs>> OnClick;

        public JankyProperty<string> Text;
        public JankyProperty<ButtonTypeEnum> Type;
        public JankyProperty<Texture> Image;

        private readonly GUIContent Content;

        public ButtonNode()
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
            UpdateContent();
#if MOCK
            Console.WriteLine("Button: {0} {1}", Text, Type);
            OnClick.Invoke(new JankyEventArgs(Context.WindowID, Name));
#else
            var args = new JankyEventArgs(Context.WindowID, Name);
            if (Type.Value == ButtonTypeEnum.Repeat)
            {
                if (GUILayout.RepeatButton(Content, GetLayoutOptions()))
                    OnClick.Invoke(args);
            }
            else
            {
                if (GUILayout.Button(Content, GetLayoutOptions()))
                    OnClick.Invoke(args);
            }
#endif
        }
    }
}
