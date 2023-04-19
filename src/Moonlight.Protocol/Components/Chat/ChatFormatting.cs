using System;

namespace Moonlight.Protocol.Components.Chat
{
    [Flags]
    public enum ChatFormatting
    {
        None = 0,
        Reset = 1 << 0,
        Bold = 1 << 1,
        Italic = 1 << 2,
        Underline = 1 << 3,
        Strikethrough = 1 << 4,
        Obfuscated = 1 << 5,
    }
}
