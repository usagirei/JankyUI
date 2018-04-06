using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using JankyUI.EventArgs;
using UnityEngine;

namespace JankyUI.Nodes
{

    [JankyTag("Scrollbar")]
    [JankyProperty("type", nameof(Type))]
    [JankyProperty("size", nameof(Size))]
    [JankyProperty("value", nameof(Value))]
    [JankyProperty("min-value", nameof(MinValue))]
    [JankyProperty("max-value", nameof(MaxValue))]
    [JankyProperty("on-change", nameof(OnChange))]
    internal class ScrollBarNode : LayoutNode
    {
        public JankyMethod<Action<JankyEventArgs<float>>> OnChange;

        public JankyProperty<ScrollBarTypeEnum> Type;
        public JankyProperty<float> Value;
        public JankyProperty<float> Size;
        public JankyProperty<float> MinValue;
        public JankyProperty<float> MaxValue;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("ScrollBar: {0}", Value);
#else
            float oldValue = Value;
            float newValue = oldValue;
            switch (Type.Value)
            {
                case ScrollBarTypeEnum.Horizontal:
                    newValue = GUILayout.HorizontalScrollbar(oldValue, Size, MinValue, MaxValue, GetLayoutOptions());
                    break;

                case ScrollBarTypeEnum.Vertical:
                    newValue = GUILayout.VerticalScrollbar(oldValue, Size, MinValue, MaxValue, GetLayoutOptions());
                    break;
            }

            Value.Value = newValue;
            if (Value.LastSetResult != DataOperationResultEnum.Unchanged)
                OnChange.Invoke(new JankyEventArgs<float>(Context.WindowID, Name, oldValue, newValue));
#endif
        }
    }
}
