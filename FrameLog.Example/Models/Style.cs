using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameLog.Example.Models
{
    [ComplexType]
    public class Style : ICloneable
    {
        public bool Hardcover { get; set; }
        public bool HasCoverArt { get; set; }
        public Format Format { get; set; }

        public Style()
        {
            Format = new Format();
        }
       
        public override bool Equals(object obj)
        {
            var other = obj as Style;
            return other != null
                && Hardcover == other.Hardcover
                && HasCoverArt == other.HasCoverArt;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} cover art", 
                Hardcover ? "Hardcover" : "Paperback",
                HasCoverArt ? "with" : "with no");
        }

        public Style Copy()
        {
            return new Style()
            {
                Hardcover = Hardcover,
                HasCoverArt = HasCoverArt,
                Format = Format.Copy(),
            };
        }

        public object Clone()
        {
            return Copy();
        }
    }
}
