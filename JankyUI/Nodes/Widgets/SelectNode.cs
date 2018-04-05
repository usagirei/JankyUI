using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Select")]
    [JankyProperty("texts", nameof(Texts), DefaultValue = "")]
    [JankyProperty("images", nameof(Images), DefaultValue = "")]
    [JankyProperty("columns", nameof(Columns))]
    [JankyProperty("selected-index", nameof(SelectedIndex))]
    [JankyProperty("on-change", nameof(OnSelect))]
    internal class SelectNode : LayoutNode
    {
        public JankyProperty<string[]> Texts;
        public JankyProperty<Texture[]> Images;

        public JankyProperty<int> Columns;
        public JankyProperty<int> SelectedIndex;
        public JankyMethod<Action<int>> OnSelect;

        private GUIContent[] Contents { get; set; }

        public SelectNode()
        {
            Contents = new GUIContent[0];
        }

        private void UpdateContent()
        {
            var tSize = Texts.Value.Length;
            var iSize = Images.Value.Length;
            var aSize = Math.Max(tSize, iSize);

            if (Contents.Length != aSize) {
                Contents = new GUIContent[aSize];
                for (int i = 0; i < aSize; i++)
                    Contents[i] = new GUIContent(GUIContent.none);
            }

            for(int i = 0; i < aSize; i++)
            {
                Contents[i].text = i < tSize ? Texts.Value[i] : "";
                Contents[i].image = i < iSize ? Images.Value[i] : null;
            }
        }

        protected override void OnGUI()
        {
            UpdateContent();
#if MOCK
            Console.WriteLine("Select: {2} {0} [{1}]", Columns, string.Join(",", Texts), SelectedIndex);
#else
            var newIdx = (Columns <= 0)
                ? GUILayout.Toolbar(SelectedIndex, Contents, GetLayoutOptions())
                : GUILayout.SelectionGrid(SelectedIndex, Contents, Columns, GetLayoutOptions());

            SelectedIndex.Value = newIdx;

            if (SelectedIndex.LastSetResult != DataOperationResultEnum.Unchanged)
                OnSelect.Invoke(newIdx);
#endif
        }
    }
}
