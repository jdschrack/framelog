using System;

namespace FrameLog.Example.Models
{
    [Flags]
    public enum BookGenre
    {
        Horror = 0x01,
        Thriller = 0x02,
        Drama = 0x04,
        Comedy = 0x08
    }
}
