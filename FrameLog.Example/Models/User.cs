using System;

namespace FrameLog.Example.Models
{
    public class User : IEquatable<User>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name ?? Id.ToString();
        }

        public bool Equals(User other)
        {
            return Id == other.Id
                && Name == other.Name;
        }
    }
}
