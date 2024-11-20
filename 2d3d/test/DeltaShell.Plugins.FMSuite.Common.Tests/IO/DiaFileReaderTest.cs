using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class DiaFileReaderTest
    {
        private const string errorLine = "** ERROR  :     this is an error!";

        [Test]
        [TestCaseSource(nameof(GetFileContents))]
        public void GetAllMessages_MultipleFileContentCases_ThenErrorMessageIsReadAsExpected(string fileContent, string expectedErrorMessage)
        {
            // Setup - Call
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(fileContent))))
            {
                Dictionary<DiaFileLogSeverity, IList<string>> result = DiaFileReader.GetAllMessages(reader);

                // Assert
                IList<string> errorMessages = result[DiaFileLogSeverity.Error];
                Assert.That(errorMessages.Count, Is.EqualTo(1));
                Assert.That(errorMessages.Single(), Is.EqualTo(expectedErrorMessage));
            }
        }

        [Test]
        public void GetAllMessages_WithNonMessagesLines_ThenOnlyMessagesAreReturned()
        {
            // Setup
            string fileContent = "** INFO   : My info message"
                                 + Environment.NewLine
                                 + "** DEBUG   : My debug message";

            // Call
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(fileContent))))
            {
                Dictionary<DiaFileLogSeverity, IList<string>> result = DiaFileReader.GetAllMessages(reader);

                // Assert
                IList<string> infoMessages = result[DiaFileLogSeverity.Info];
                Assert.That(infoMessages.Count, Is.EqualTo(1));
                Assert.That(infoMessages.Single(), Is.EqualTo("** INFO   : My info message"));

                IList<string> debugMessages = result[DiaFileLogSeverity.Debug];
                Assert.That(debugMessages.Count, Is.EqualTo(1));
                Assert.That(debugMessages.Single(), Is.EqualTo("** DEBUG   : My debug message"));
            }
        }

        [Test]
        public void GetAllMessages_WithUnknownSeverityKeyInMessage_ThenLineIsNotParsed()
        {
            // Setup
            var fileContent = "** YOLO   : My yolo message";

            // Call
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(fileContent))))
            {
                Dictionary<DiaFileLogSeverity, IList<string>> result = DiaFileReader.GetAllMessages(reader);

                // Assert
                foreach (DiaFileLogSeverity severity in result.Keys)
                {
                    Assert.IsEmpty(result[severity]);
                }
            }
        }

        [Test]
        public void GetAllMessages_WithCommentLine_ThenCommentLineIsNotParsed()
        {
            // Setup
            var fileContent = "* My yolo message";

            // Call
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(fileContent))))
            {
                Dictionary<DiaFileLogSeverity, IList<string>> result = DiaFileReader.GetAllMessages(reader);

                // Assert
                foreach (DiaFileLogSeverity severity in result.Keys)
                {
                    Assert.IsEmpty(result[severity]);
                }
            }
        }

        [Test]
        public void GetAllMessages_WithNullStream_ThenThrowsArgumentNullException()
        {
            // Call
            void Call() => DiaFileReader.GetAllMessages(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("streamReader"));
        }

        [Test]
        public void GetAllMessages_WithNonReadableStream_ThenThrowsArgumentException()
        {
            using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes("myContent"))))
            {
                reader.BaseStream.Close();

                // Call
                void Call() => DiaFileReader.GetAllMessages(reader);

                // Assert
                var exception = Assert.Throws<ArgumentException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("streamReader"));
                Assert.That(exception.Message, Is.EqualTo($"Stream is not readable.{Environment.NewLine}Parameter name: streamReader"));
            }
        }

        private static IEnumerable<TestCaseData> GetFileContents()
        {
            string warningError = "** WARNING  :  This is a warning!"
                                  + Environment.NewLine
                                  + errorLine;

            string errorOnMultipleLines = "** ERROR  :     this is a"
                                          + Environment.NewLine
                                          + "n error!";

            string errorOnMultipleLinesAndWarning = "** ERROR  :     this is a"
                                                    + Environment.NewLine
                                                    + "n error!"
                                                    + Environment.NewLine
                                                    + "** WARNING  :  This is a warning!";

            string errorAndComment = errorLine
                                     + Environment.NewLine
                                     + "* This is not an ERROR message";

            yield return new TestCaseData(errorLine, errorLine);
            yield return new TestCaseData(warningError, errorLine);
            yield return new TestCaseData(errorOnMultipleLines, errorOnMultipleLines);
            yield return new TestCaseData(errorOnMultipleLinesAndWarning, errorOnMultipleLines);
            yield return new TestCaseData(errorAndComment, errorLine);
        }
    }
}