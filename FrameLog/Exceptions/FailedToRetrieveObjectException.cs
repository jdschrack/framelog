using System;

namespace FrameLog.Exceptions
{        
    public class FailedToRetrieveObjectException : Exception
    {
        public readonly Type Type;
        public readonly string Reference;

        private const string message = "Failed to retrieve object identified by type '{0}' and reference '{1}'";

        /// <param name="type">The type of the object.</param>
        /// <param name="reference">The FrameLog reference that identifies it. See https://bitbucket.org/MartinEden/framelog/wiki/ObjectReferences </param>
        /// <param name="innerException"></param>
        public FailedToRetrieveObjectException(Type type, string reference, Exception innerException = null)
            : base(string.Format(message, type, reference), innerException)
        {
            Type = type;
            Reference = reference;
        }
    }
}
