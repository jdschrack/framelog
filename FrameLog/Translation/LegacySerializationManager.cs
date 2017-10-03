using System;
using FrameLog.Translation.Serializers;

namespace FrameLog.Translation
{
    /// <summary>
    /// The legacy serialization manager found in FrameLog 1.5.X.
    /// </summary>
    /// <remarks>
    /// This serialization manager just converts values to a string using Oject.ToString()
    /// </remarks>
    [Obsolete("\"LegacySerializationManager\" is deprecated and will be removed in future versions of FrameLog. " +
        "Consider switching to \"ValueTranslationManager\", which supports both value serialization and value binding.")]
    public class LegacySerializationManager : ISerializationManager
    {
        public string Serialize(object obj)
        {
            return (obj != null)
                ? obj.ToString()
                : null;
        }
    }
}
