using System;
using System.Runtime.Serialization;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Exception thrown when parsing the file function store went wrong.
    /// </summary>
    [Serializable]
    public class NetCdfFileParsingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetCdfFileParsingException"/> class 
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NetCdfFileParsingException(string message)
            : base(message) {}

        /// <summary>
        /// Initializes a new instance of <see cref="NetCdfFileParsingException"/> with
        /// serialized data.</summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is
        /// <c>null</c>.</exception>
        /// <exception cref="SerializationException">The class name is <c>null</c> or
        /// <see cref="Exception.HResult" /> is zero (0).</exception>
        protected NetCdfFileParsingException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}