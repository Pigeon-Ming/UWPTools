using System;

namespace UWPTools.Controls
{
    [Flags]
    public enum GamepadCommandBarKeyModifiers
    {
        None = 0,
        Control = 1,
        Shift = 2,
        Menu = 4,
        Windows = 8
    }
}
