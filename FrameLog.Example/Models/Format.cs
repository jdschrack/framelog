using System;

namespace FrameLog.Example.Models
{
    public class Format : ICloneable
    {
        public string Name { get; set; }

        public object Clone()
        {
            return Copy();
        }
        public Format Copy()
        {
            return new Format() { Name = Name };
        }

        public override bool Equals(object obj)
        {
            var other = obj as Format;
            return other != null
                && Name == other.Name;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
