using System;
using System.IO;
using System.Text;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekCaseDataReaderTest
    {
        [Test]
        public void Read_StreamNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => SobekCaseDataReader.Read(null, "rootFilePath");

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("stream"));
        }

        [Test]
        public void Read_RootFilePathNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => SobekCaseDataReader.Read(Substitute.For<Stream>(), null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("rootFilePath"));
        }

        [Test]
        public void Read_StreamDoesNotSupportReading_ThrowsInvalidOperationException()
        {
            // Setup
            var stream = Substitute.For<Stream>();
            stream.CanRead.Returns(false);

            // Call
            void Call() => SobekCaseDataReader.Read(stream, "rootFilePath");

            // Assert
            var e = Assert.Throws<InvalidOperationException>(Call);
            Assert.That(e.Message, Is.EqualTo("stream does not support reading."));
        }

        [Test]
        public void Read_NoRelevantFilePathsSpecified_ReadsCorrectData()
        {
            // Setup
            string fileContent =
                @"I \SB216003\FIXED\656NOP.txt 1331 '1622702956'" + Environment.NewLine +
                @"I \SB216003\FIXED\656NOP.csv 231 '1402382291'";

            SobekCaseData caseData;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
            {
                // Call
                caseData = SobekCaseDataReader.Read(stream, "the\\root\\file.txt");
            }

            // Assert
            Assert.That(caseData.WindFile, Is.Null);
            Assert.That(caseData.PrecipitationFile, Is.Null);
        }
        
        [Test]
        public void Read_ReadsCorrectData()
        {
            // Setup
            string fileContent =
                @"I \SB216003\FIXED\656NOP.BUI 1331 '1622702956'" + Environment.NewLine +
                @"I ..\FIXED\656NOP.WDC 231 '1402382291'";
            
            SobekCaseData caseData;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
            {
                    // Call
                    caseData = SobekCaseDataReader.Read(stream, "z:\\path\\to\\the\\file.txt");
            }

            // Assert
            Assert.That(caseData.WindFile.FullName, Is.EqualTo("z:\\path\\to\\FIXED\\656NOP.WDC"));
            Assert.That(caseData.PrecipitationFile.FullName, Is.EqualTo("z:\\path\\FIXED\\656NOP.BUI"));
        }
    }
}