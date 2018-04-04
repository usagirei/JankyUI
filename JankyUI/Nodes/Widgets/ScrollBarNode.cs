using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
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
        public readonly JankyProperty<ScrollBarTypeEnum> Type;
        public readonly JankyProperty<float> Value;
        public readonly JankyProperty<float> Size;
        public readonly JankyProperty<float> MinValue;
        public readonly JankyProperty<float> MaxValue;
        public readonly JankyMethod<Action<float>> OnChange;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("ScrollBar: {0}", Value);
#else
            switch (Type.Value)
            {
                case ScrollBarTypeEnum.Horizontal:
                    Value.Value = GUILayout.HorizontalScrollbar(Value, Size, MinValue, MaxValue, GetLayoutOptions());
                    break;

                case ScrollBarTypeEnum.Vertical:
                    Value.Value = GUILayout.VerticalScrollbar(Value, Size, MinValue, MaxValue, GetLayoutOptions());
                    break;
            }

            if (Value.LastSetResult != DataOperationResultEnum.Unchanged)
                OnChange.Invoke(Value);
#endif
        }
    }
}
