using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FrameLog.Helpers
{
    /// <summary>
    /// Simple comparator that checks object references
    /// </summary>
    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
