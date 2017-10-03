using System;

namespace FrameLog.Exceptions
{
    public class ChangesNotDetectedException : Exception
    {
        private const string message =
            "FrameLog was unable to prepare its log objects whilst intercepting DbContext.SaveChanges(). This has most likely " +
            "happened because the ObjectContext.SavingChanges event has been reset whilst FrameLog is still saving the changes. " +
            "If you are not utilising the SavingChanges event, then this is FrameLog's fault. " +
            "You can raise an issue with the open source project. " +
            "See https://bitbucket.org/MartinEden/framelog/issues.";

        public ChangesNotDetectedException(Exception innerException = null)
            : base(message,innerException)
        {
        }
    }
}
