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


        public JankyProperty<SliderTypeEnum> Type;
        public JankyProperty<float> Value;
        public JankyProperty<float> MinValue;
        public JankyProperty<float> MaxValue;
        public JankyMethod<Action<float>> OnChange;

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

