using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using UnityEngine;

namespace JankyUI.Nodes
{

    [JankyTag("Slider")]
    [JankyProperty("type", nameof(Type))]
    [JankyProperty("value", nameof(Value))]
    [JankyProperty("min-value", nameof(MinValue))]
    [JankyProperty("max-value", nameof(MaxValue))]
    [JankyProperty("on-change", nameof(MaxValue))]
    internal class SliderNode : LayoutNode
    {


        public readonly JankyProperty<SliderTypeEnum> Type;
        public readonly JankyProperty<float> Value;
        public readonly JankyProperty<float> MinValue;
        public readonly JankyProperty<float> MaxValue;
        public readonly JankyMethod<Action<float>> OnChange;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Slider: {0}", Value);
#else
            switch (Type.Value)
            {
                case SliderTypeEnum.Horizontal:
                    Value.Value = GUILayout.HorizontalSlider(Value, MinValue, MaxValue, GetLayoutOptions());
                    break;

                case SliderTypeEnum.Vertical:
                    Value.Value = GUILayout.VerticalSlider(Value, MinValue, MaxValue, GetLayoutOptions());
                    break;
            }

            if (Value.LastSetResult != DataOperationResultEnum.Unchanged)
                OnChange.Invoke(Value);
#endif
        }
    }
}

