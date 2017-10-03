using System;
using System.Collections.Generic;

namespace FrameLog.Example.Models
{
    public class Bookshelf
    {
        public int BookshelfId { get; set; }
        public string Name { get; set; }
        public ICollection<Book> Books { get; set; }

        public Bookshelf()
        {
            Books = new HashSet<Book>();
        }

        /// <summary>
        /// Custom equality logic. If our ids are the same, we are the same thing
        /// </summary>
        /// <remarks>
        /// This is a poorly designed equality method as transient entities will (by default)
        /// have an ID of zero until they are first saved to the database. Because of this,
        /// when FrameLog attempts to log their changes for the first time, all transient entities
        /// of this type that are saved inside the same transaction will be seen as the same entity
        /// (since they would all have an ID of zero at that time).
        /// 
        /// This is not really about the 'correctness' of this methods implementation, but we need
        /// to highlight that FrameLog should NOT attempt to use this logic, instead preferring
        /// object reference equality, as that is how EntityFrameworks ChangeTracker manages equality.
        /// </remarks>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = (obj as Bookshelf);
            return (other != null) && BookshelfId.Equals(other.BookshelfId);
        }

        /// <summary>
        /// Our id is our hashcode. Same id, same object
        /// </summary>
        /// <remarks>
        /// Same as above, this is a flawed implementation.
        /// Here to test that FrameLog does not attempt to use this logic
        /// </remarks>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return BookshelfId;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
