using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Label")]
    [JankyProperty("text", nameof(Text))]
    [JankyProperty("image", nameof(Image))]
    internal class LabelNode : LayoutNode
    {
        public JankyProperty<string> Text;
        public JankyProperty<Texture> Image;

        private readonly GUIContent Content;

        public LabelNode()
        {
            Content = new GUIContent();
        }

        protected override void OnGUI()
        {
#if MOCK
            UpdateContent();
            Console.WriteLine("Label: {0}", Text);
#else
            UpdateContent();
            GUILayout.Label(Content, GetLayoutOptions());
#endif
        }

        private void UpdateContent()
        {
            Content.text = Text;
            Content.image = Image;
        }
    }

    [JankyTag("Box")]
    [JankyProperty("text", nameof(Text))]
    [JankyProperty("image", nameof(Image))]
    internal class BoxNode : LayoutNode
    {
        public JankyProperty<string> Text;
        public JankyProperty<Texture> Image;

        private readonly GUIContent Content;

        public BoxNode()
        {
            Content = new GUIContent();
        }

        protected override void OnGUI()
        {
            UpdateContent();
            GUILayout.Box(Content, GetLayoutOptions());
        }

        private void UpdateContent()
        {
            Content.text = Text;
            Content.image = Image;
        }
    }
}
