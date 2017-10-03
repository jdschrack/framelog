using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;

namespace FrameLog.Example.Models.Testing
{
    public class ClassWithList : ClassWithAbstractedCollection<ClassWithList, List<User>> { }
    public class ClassWithHashSet : ClassWithAbstractedCollection<ClassWithHashSet, HashSet<User>> { }
    public class ClassWithCollection : ClassWithAbstractedCollection<ClassWithCollection, Collection<User>> { }
    public class ClassWithEntityCollection : ClassWithAbstractedCollection<ClassWithEntityCollection, EntityCollection<User>> { }

    public abstract class ClassWithAbstractedCollection<T, TCollection> : ICloneable, IEquatable<T>
        where T : ClassWithAbstractedCollection<T, TCollection>, new()
        where TCollection: class, ICollection<User>, new()
    {
        public virtual int Id { get; set; }
        public virtual ICollection<User> Users { get; set; }

        protected ClassWithAbstractedCollection()
        {
            Users = new TCollection();
        }

        public static T New(IEnumerable<User> users)
        {
            var obj = new T();
            obj.Add(users);
            return obj;
        }
        
        public void Add(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        public object Clone()
        {
            var clone = new T()
            {
                Id = Id,
            };
            clone.Add(Users);
            return clone;
        }

        public bool Equals(T other)
        {
            return Enumerable.SequenceEqual(Users, other.Users);
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", typeof(T).Name, string.Join(", ", Users));
        }
    }
}
