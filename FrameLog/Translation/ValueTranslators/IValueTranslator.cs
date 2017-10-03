using System;

namespace FrameLog.Translation.ValueTranslators
{
    public interface IValueTranslator
    {
        /// <summary>
        /// Returns true if the data type is supported by this value translator
        /// </summary>
        /// <param name="type">The data type to check</param>
        /// <returns>True if supported, otherwise false</returns>
        bool Supports(Type type);
    }
}
