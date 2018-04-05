using System;
using System.Collections.Generic;
using System.Linq;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyProperty("width", nameof(Width), DefaultValue = "NaN")]
    [JankyProperty("min-width", nameof(MinWidth), DefaultValue = "NaN")]
    [JankyProperty("max-width", nameof(MaxWidth), DefaultValue = "NaN")]
    [JankyProperty("height", nameof(Height), DefaultValue = "NaN")]
    [JankyProperty("min-height", nameof(MinHeight), DefaultValue = "NaN")]
    [JankyProperty("max-height", nameof(MaxHeight), DefaultValue = "NaN")]
    [JankyProperty("stretch", nameof(StrechMode), DefaultValue = "width")]
    internal abstract class LayoutNode : Node
    {

        public JankyProperty<float> Width;
        public JankyProperty<float> MinWidth;
        public JankyProperty<float> MaxWidth;
        public JankyProperty<float> Height;
        public JankyProperty<float> MinHeight;
        public JankyProperty<float> MaxHeight;
        public JankyProperty<StretchModeEnum> StrechMode;

        public GUILayoutOption[] GetLayoutOptions()
        {
            IEnumerable<GUILayoutOption> Enumerate()
            {
                if (!float.IsNaN(Width))
                    yield return GUILayout.Width(Width);
                if (!float.IsNaN(MinWidth))
                    yield return GUILayout.Width(MinWidth);
                if (!float.IsNaN(MaxWidth))
                    yield return GUILayout.Width(MaxWidth);

                if (!float.IsNaN(Height))
                    yield return GUILayout.Width(Height);
                if (!float.IsNaN(MinHeight))
                    yield return GUILayout.Width(MinHeight);
                if (!float.IsNaN(MaxHeight))
                    yield return GUILayout.Width(MaxHeight);

                var flags = StrechMode.Value;
                yield return GUILayout.ExpandWidth((flags & StretchModeEnum.Width) == StretchModeEnum.Width);
                yield return GUILayout.ExpandHeight((flags & StretchModeEnum.Height) == StretchModeEnum.Height);
            }
            return Enumerate().ToArray();
        }

        protected abstract override void OnGUI();
    }
}
