using System;
using JankyUI.Attributes;
using JankyUI.Nodes.Binding;
using JankyUI.Enums;
using JankyUI.EventArgs;
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
        public JankyMethod<Action<JankyEventArgs<float>>> OnChange;

        public JankyProperty<SliderTypeEnum> Type;
        public JankyProperty<float> Value;
        public JankyProperty<float> MinValue;
        public JankyProperty<float> MaxValue;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Slider: {0}", Value);
#else
            float oldValue = Value;
            float newValue = oldValue;
            switch (Type.Value)
            {
                case SliderTypeEnum.Horizontal:
                    newValue = GUILayout.HorizontalSlider(oldValue, MinValue, MaxValue, GetLayoutOptions());
                    break;

                case SliderTypeEnum.Vertical:
                    newValue = GUILayout.VerticalSlider(oldValue, MinValue, MaxValue, GetLayoutOptions());
                    break;
            }

            Value.Value = newValue;
            if (Value.LastSetResult != DataOperationResultEnum.Unchanged)
                OnChange.Invoke(new JankyEventArgs<float>(Context.WindowID, Name, oldValue, newValue));
#endif
        }
    }
}

