using System;

namespace JankyUI.Enums
{
    [Flags]
    public enum StretchModeEnum
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = Horizontal | Vertical
    }
}
