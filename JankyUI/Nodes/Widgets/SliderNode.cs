using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Slider")]
    [JankyProperty("type", nameof(Type))]
    [JankyProperty("value", nameof(Value))]
    [JankyProperty("min-value", nameof(MinValue))]
    [JankyProperty("max-value", nameof(MaxValue))]
    internal class SliderNode : LayoutNode
    {
        public enum SliderTypeEnum
        {
            Horizontal,
            Vertical
        }

        public readonly DataContextProperty<SliderTypeEnum> Type;
        public readonly DataContextProperty<float> Value;
        public readonly DataContextProperty<float> MinValue;
        public readonly DataContextProperty<float> MaxValue;

        public override void Execute()
        {
            switch (Type.Value)
            {
                case SliderTypeEnum.Horizontal:
                    Value.Value = GUILayout.HorizontalSlider(Value, MinValue, MaxValue, GetLayoutOptions());
                    break;

                case SliderTypeEnum.Vertical:
                    Value.Value = GUILayout.VerticalSlider(Value, MinValue, MaxValue, GetLayoutOptions());
                    break;
            }
        }
    }
}
