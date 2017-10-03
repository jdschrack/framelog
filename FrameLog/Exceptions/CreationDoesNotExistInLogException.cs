using System;

namespace FrameLog.Exceptions
{
    public class CreationDoesNotExistInLogException : Exception
    {
        private const string message =
            "There is no record of this object's creation in the log. Object: '{0}'.";

        public readonly object Model;

        public CreationDoesNotExistInLogException(object model)
            : base(string.Format(message, model))
        {
            Model = model;
        }
    }
}
