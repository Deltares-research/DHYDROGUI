using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// Formats exception messages for a <see cref="GridApiException"/>.
    /// </summary>
    public static class GridApiExceptionMessage
    {
        /// <summary>
        /// Formats a message including the error code and a message.
        /// </summary>
        /// <param name="errorCode"> The error code returned by the Grid Api. </param>
        /// <param name="message"> The message. </param>
        /// <returns>
        /// The formatted message.
        /// </returns>
        public static string Format(int errorCode, string message)
        {
            return string.Format(Resources.GridApiException_Message_ErrorCodeWithMessage, errorCode, message);
        }

        /// <summary>
        /// Formats a message including the error code.
        /// </summary>
        /// <param name="errorCode"> The error code returned by the Grid Api. </param>
        /// <returns>
        /// The formatted message.
        /// </returns>
        public static string Format(int errorCode)
        {
            return string.Format(Resources.GridApiException_Message_ErrorCode, errorCode);
        }
    }
}