using System;
using System.Runtime.Serialization;

namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// Exception thrown when <see cref="IGridApi"/> returns an error code.
    /// </summary>
    [Serializable]
    public sealed class GridApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridApiException"/> class.
        /// </summary>
        public GridApiException() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="GridApiException"/> class.
        /// </summary>
        /// <param name="message"> The exception message. </param>
        /// <param name="innerException">Optional parameter; the inner exception. Default is <c>null</c>.</param>
        public GridApiException(string message, Exception innerException = null) : base(message, innerException) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="GridApiException"/> class.
        /// Without this constructor serialization will fail.
        /// </summary>
        /// <param name="info">
        /// The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.
        /// </param>
        private GridApiException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}