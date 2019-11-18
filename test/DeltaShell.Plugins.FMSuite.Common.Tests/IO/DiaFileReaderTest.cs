using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.TestUtils;
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
        public void CollectAllErrorMessages_WithCutOffLine_ThenReadsAndReturnsLinesAsOneLine(string fileContent)
        {
            // Setup
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));

            // Call
            string[] result = DiaFileReader.GetAllErrorMessages(stream).ToArray();

            // Assert
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result.Single(), Is.EqualTo(errorLine));
        }

        private static IEnumerable<string> GetFileContents()
        {
            yield return errorLine;

            yield return "** WARNING  :  This is a warning!"
                         + Environment.NewLine
                         + errorLine;

            yield return "** ERROR  :     this is a" 
                         + Environment.NewLine
                         + "n error!";

            yield return "** ERROR  :     this is a"
                         + Environment.NewLine
                         + "n error!"
                         + Environment.NewLine
                         + "** WARNING  :  This is a warning!";
        }
    }
}
