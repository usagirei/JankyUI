using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{

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
