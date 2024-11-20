using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeltaShell.Plugins.FMSuite.Common.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    /// <summary>
    /// Reader for diagnostics files (*.dia).
    /// </summary>
    public static class DiaFileReader
    {
        /// <summary>
        /// Collects and returns all messages from a given <see cref="StreamReader"/>, assuming that this stream reader
        /// consists of diagnostics file data (*.dia).
        /// </summary>
        /// <param name="streamReader"> The reader to read with. </param>
        /// <returns> A dictionary containing all messages per log severity. </returns>
        /// <exception cref="ArgumentNullException">
        /// In case
        /// <param name="streamReader"/>
        /// is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// In case
        /// <param name="streamReader"/>
        /// is not readable (i.e. closed).
        /// </exception>
        /// <remarks>
        /// Messages in diagnostics files may be split up on multiple consecutive lines. They will be read as one message.
        /// Lines that start with "* " are seen as comments and will not be parsed.
        /// </remarks>
        public static Dictionary<DiaFileLogSeverity, IList<string>> GetAllMessages(StreamReader streamReader)
        {
            VerifyStreamReader(streamReader);

            var messagesDictionary = new Dictionary<DiaFileLogSeverity, IList<string>>
            {
                {DiaFileLogSeverity.Debug, new List<string>()},
                {DiaFileLogSeverity.Info, new List<string>()},
                {DiaFileLogSeverity.Warning, new List<string>()},
                {DiaFileLogSeverity.Error, new List<string>()},
                {DiaFileLogSeverity.Fatal, new List<string>()}
            };

            var messageBuilder = new MessageBuilder();

            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                bool hasExtended = messageBuilder.TryExtendMessageWithLine(line);
                if (hasExtended)
                {
                    continue;
                }

                if (messageBuilder.HasMessage)
                {
                    messagesDictionary.AddMessage(messageBuilder);
                }

                messageBuilder.StartNewMessage(line);
            }

            if (messageBuilder.HasMessage)
            {
                messagesDictionary.AddMessage(messageBuilder);
            }

            return messagesDictionary;
        }

        private static void VerifyStreamReader(StreamReader streamReader)
        {
            if (streamReader == null)
            {
                throw new ArgumentNullException(nameof(streamReader));
            }

            if (!streamReader.BaseStream.CanRead)
            {
                throw new ArgumentException(Resources.DiaFileReader_GetAllMessages_Stream_is_not_readable_,
                                            nameof(streamReader));
            }
        }

        private static void AddMessage(this IReadOnlyDictionary<DiaFileLogSeverity, IList<string>> messagesDictionary,
                                       MessageBuilder messageBuilder)
        {
            messagesDictionary[messageBuilder.MessageSeverityType].Add(messageBuilder.GetMessage());
        }

        /// <summary>
        /// Builder responsible for building messages from data in diagnostics files (*.dia).
        /// </summary>
        private class MessageBuilder
        {
            private readonly StringBuilder messageBuilder;

            /// <summary>
            /// Creates a new instance of <see cref="messageBuilder"/>.
            /// </summary>
            public MessageBuilder()
            {
                messageBuilder = new StringBuilder();
                HasMessage = false;
            }

            /// <summary>
            /// Indicates whether this builder has a message.
            /// </summary>
            public bool HasMessage { get; private set; }

            /// <summary>
            /// The message severity type.
            /// </summary>
            public DiaFileLogSeverity MessageSeverityType { get; private set; }

            /// <summary>
            /// Try to extend the current message with <paramref name="line"/>.
            /// </summary>
            /// <param name="line"> The line to extend with. </param>
            /// <returns>
            /// <c>true</c> in case the message was extended with <paramref name="line"/>.
            /// <c>false</c> in case the message could not be extended with <paramref name="line"/>.
            /// Reasons for this are:
            /// - We currently do not have a message,
            /// - <paramref name="line"/> is a new message,
            /// - <paramref name="line"/> is a comment.
            /// </returns>
            public bool TryExtendMessageWithLine(string line)
            {
                if (!HasMessage || IsNewMessage(line) || IsComment(line))
                {
                    return false;
                }

                ExtendWithLine(line);
                return true;
            }

            /// <summary>
            /// Clears the current message and starts a new message in case <paramref name="line"/>
            /// is a new message.
            /// </summary>
            /// <param name="line"> The new line. </param>
            public void StartNewMessage(string line)
            {
                if (HasMessage)
                {
                    messageBuilder.Clear();
                    HasMessage = false;
                }

                if (!IsNewMessage(line) ||
                    !TryParseMessageType(line, out DiaFileLogSeverity messageSeverityType))
                {
                    return;
                }

                HasMessage = true;
                MessageSeverityType = messageSeverityType;
                messageBuilder.Append(line.TrimEnd());
            }

            /// <summary>
            /// Gets the current message as a string.
            /// </summary>
            /// <returns> The current message. </returns>
            public string GetMessage() => messageBuilder.ToString();

            private void ExtendWithLine(string line)
            {
                messageBuilder.Append(Environment.NewLine);
                messageBuilder.Append(line.TrimEnd());
            }

            private static bool IsComment(string line)
            {
                return line.StartsWith("* ");
            }

            private static bool IsNewMessage(string line)
            {
                return line.StartsWith("** ");
            }

            private static bool TryParseMessageType(string line, out DiaFileLogSeverity messageType)
            {
                string[] messageSplit = line.Substring(3).Split(new[]
                {
                    ':'
                }, 2);

                bool hasParsedType = Enum.TryParse(messageSplit[0].Trim(), true, out messageType);
                return hasParsedType;
            }
        }
    }
}