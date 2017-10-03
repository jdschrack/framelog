using System;
using System.Data.Entity.ModelConfiguration;

namespace FrameLog.Example.Models.Testing
{
    public class ClassWithProtectedProperty : ICloneable
    {
        public virtual int Id { get; set; }
        protected virtual int number { get; set; }

        public int GetNumber()
        {
            return number;
        }
        public void SetNumber(int number)
        {
            this.number = number;
        }

        public object Clone()
        {
            return new ClassWithProtectedProperty()
            {
                Id = Id,
                number = number,
            };
        }
        
        public class ClassWithProtectedPropertyConfiguration : EntityTypeConfiguration<ClassWithProtectedProperty>
        {
            public ClassWithProtectedPropertyConfiguration()
            {
                Property(p => p.number);
            }
        }
    }
}
