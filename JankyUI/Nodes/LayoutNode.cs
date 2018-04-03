﻿using System;
using System.Collections.Generic;
using System.Linq;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyProperty("width", nameof(Width), DefaultValue = "NaN")]
    [JankyProperty("min-width", nameof(MinWidth), DefaultValue = "NaN")]
    [JankyProperty("max-width", nameof(MaxWidth), DefaultValue = "NaN")]
    [JankyProperty("height", nameof(Height), DefaultValue = "NaN")]
    [JankyProperty("min-height", nameof(MinHeight), DefaultValue = "NaN")]
    [JankyProperty("max-height", nameof(MaxHeight), DefaultValue = "NaN")]
    [JankyProperty("expand", nameof(ExpandMode), DefaultValue = "width")]
    internal abstract class LayoutNode : Node
    {
        [Flags]
        public enum ExpandModeEnum
        {
            None = 0,
            Width = 1,
            Height = 2,
            Both = Width | Height
        }

        public PropertyBinding<float> Width { get; set; }
        public PropertyBinding<float> MinWidth { get; set; }
        public PropertyBinding<float> MaxWidth { get; set; }
        public PropertyBinding<float> Height { get; set; }
        public PropertyBinding<float> MinHeight { get; set; }
        public PropertyBinding<float> MaxHeight { get; set; }
        public PropertyBinding<ExpandModeEnum> ExpandMode { get; set; }

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

                var flags = ExpandMode.Value;
                yield return GUILayout.ExpandWidth((flags & ExpandModeEnum.Width) == ExpandModeEnum.Width);
                yield return GUILayout.ExpandHeight((flags & ExpandModeEnum.Height) == ExpandModeEnum.Height);
            }
            return Enumerate().ToArray();
        }

        public abstract override void Execute();
    }
}