using System;
using FrameLog.Models;

namespace FrameLog.Exceptions
{
    public class UnknownPropertyInLogException<TPrincipal> : Exception
    {
        private const string message =
            "When retrieving history, a IPropertyChange naming property '{0}' was found in the log that did not correspond " +
            "to any property on the model, which had type '{1}'. This may be because a property has been removed from the model, " +
            "or renamed, since the log record was created. Consider migrating your log records when you make schema changes to " +
            "allow strongly-typed retrieval historic records.";

        public readonly IPropertyChange<TPrincipal> PropertyChange;

        public UnknownPropertyInLogException(IPropertyChange<TPrincipal> propertyChange)
            : base(string.Format(
                message, 
                (propertyChange != null ? propertyChange.PropertyName : "[Null]"), 
                (propertyChange != null && propertyChange.ObjectChange != null ? propertyChange.ObjectChange.TypeName : "[Null]")
            ))
        {
            PropertyChange = propertyChange;
        }
    }
}
