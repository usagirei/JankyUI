using JankyUI.Attributes;
using JankyUI.Binding;
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
    [JankyProperty("type", nameof(Type), DefaultValue = "Horizontal")]
    internal class GroupNode : LayoutNode
    {
        public readonly PropertyBinding<GroupTypeEnum> Type;

        public enum GroupTypeEnum
        {
            Horizontal,
            Vertical,
        }

        public override void Execute()
        {
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
        }
    }
}
