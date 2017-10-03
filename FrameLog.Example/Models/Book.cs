using System;
using System.Collections.Generic;
using FrameLog.Filter;

namespace FrameLog.Example.Models
{
    public class Book : ICloneable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int NumberOfFans { get; set; }
        public Book Sequel { get; set; }
        public Style Style { get; set; }
        public DateTime? PublicationDate { get; set; }

        public Book()
        {
            Style = new Style();
        }

        // These methods allow the Book to be use in HistoryExplorer.ChangesTo(TModel model)
        public object Clone()
        {
            return Copy();
        }
        public Book Copy()
        {
            return new Book()
            {
                Title = this.Title,
                NumberOfFans = this.NumberOfFans,
                Sequel = this.Sequel,
                Style = this.Style.Copy(),
            };
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
