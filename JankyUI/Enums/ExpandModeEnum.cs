using System;

namespace JankyUI.Enums
{
    [Flags]
    public enum ExpandModeEnum
    {
        None = 0,
        Width = 1,
        Height = 2,
        Both = Width | Height
    }
}
