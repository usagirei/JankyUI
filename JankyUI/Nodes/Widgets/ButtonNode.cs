using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Button")]
    [JankyProperty("text", nameof(Text))]
    [JankyProperty("image", nameof(Image))]
    [JankyProperty("onClick", nameof(OnClick))]
    [JankyProperty("repeat", nameof(IsRepeat))]
    internal class ButtonNode : LayoutNode
    {
        public readonly MethodBinding<Action> OnClick;
        public readonly PropertyBinding<string> Text;
        public readonly PropertyBinding<bool> IsRepeat;
        public readonly PropertyBinding<Texture> Image;

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

        public override void Execute()
        {
            UpdateContent();
            if (IsRepeat?.Value ?? false)
            {
                if (GUILayout.RepeatButton(Content, GetLayoutOptions()))
                    OnClick.Invoke();
            }
            else
            {
                if (GUILayout.Button(Content, GetLayoutOptions()))
                    OnClick.Invoke();
            }
        }
    }
}
