using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("VerticalGroup")]
    [JankyDefaultOverride("type", "vertical")]
    internal class VerticalGroupNode : GroupNode
    {
    }

    [JankyTag("HorizontalGroup")]
    [JankyDefaultOverride("type", "horizontal")]
    internal class HorizontalGroupNode : GroupNode
    {
    }

    [JankyTag("Group")]
    [JankyProperty("type", nameof(Type), DefaultValue = "horizontal")]
    internal class GroupNode : LayoutNode
    {
        public readonly JankyProperty<GroupTypeEnum> Type;

        protected override void OnGUI()
        {
#if MOCK
            Console.WriteLine("Begin GroupNode: {0}", Type);
            foreach (var child in Children)
                child.Execute();
            Console.Write("End ScrollView");
#else
            switch (Type.Value)
            {
                case GroupTypeEnum.Horizontal:
                    {
                        GUILayout.BeginHorizontal(GetLayoutOptions());

                        foreach (var child in Children)
                            child.Execute();

                        GUILayout.EndHorizontal();
                    }
                    break;

                case GroupTypeEnum.Vertical:
                    {
                        GUILayout.BeginVertical(GetLayoutOptions());

                        foreach (var child in Children)
                            child.Execute();

                        GUILayout.EndVertical();
                    }
                    break;
            }
#endif
        }
    }
}
