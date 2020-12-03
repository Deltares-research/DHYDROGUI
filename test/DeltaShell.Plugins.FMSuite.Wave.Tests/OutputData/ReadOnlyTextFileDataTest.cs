using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class ReadOnlyTextFileDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            const string documentName = "someDocumentName.doc";
            const string documentContent = "Goats!";

            // Call
            var readOnlyTextFileData = new ReadOnlyTextFileData(documentName, 
                                                                documentContent);

            // Result
            Assert.That(readOnlyTextFileData.DocumentName, Is.EqualTo(documentName));
            Assert.That(readOnlyTextFileData.Content, Is.EqualTo(documentContent));
        }

        [Test]
        public void Constructor_DocumentNameNull_ThrowsArgumentNullException()
        {
            // Setup
            const string documentContent = "Goats!";

            // Call
            void Call() => new ReadOnlyTextFileData(null, documentContent);

            // Result
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("documentName"));
        }

        [Test]
        public void Constructor_DocumentContentNull_ThrowsArgumentNullException()
        {
            // Setup
            const string documentName = "someDocumentName.doc";

            // Call
            void Call() => new ReadOnlyTextFileData(documentName, null);

            // Result
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("content"));
        }
    }
}