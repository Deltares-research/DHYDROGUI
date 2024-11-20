using System;
using System.Runtime.Serialization;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.CopyHandlers
{
    /// <summary>
    /// <see cref="FileCopyException"/> wraps the exceptions that might be thrown by a
    /// <see cref="ICopyHandler"/>.
    /// </summary>
    /// <seealso cref="Exception"/>
    [Serializable]
    public class FileCopyException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="FileCopyException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public FileCopyException(string message, Exception inner)
            : base(message, inner) {}

        /// <summary>
        /// Creates a new <see cref="FileCopyException"/>.
        /// </summary>
        /// <param name="info">
        /// The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.
        /// </param>
        protected FileCopyException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}