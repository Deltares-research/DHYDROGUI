using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class BcReaderTest
    {
        [Test]
        public void Constructor_fileSystemNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BcReader(null);
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ReadBcFile_FilePathNullOrWhiteSpace_ThrowsArgumentException(string filepath)
        {
            // Setup
            var fileSystem = new MockFileSystem();
            var reader = new BcReader(fileSystem);
            
            // Call
            void Call() => reader.ReadBcFile(filepath);
            
            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }
        
        [Test]
        public void ReadBcFile_InvalidFilePath_ThrowsIOException()
        {
            // Setup
            const string filepath = @"C:\FileDoesNotExist.bc";
            var fileSystem = new MockFileSystem();
            var reader = new BcReader(fileSystem);
            
            // Call
            void Call() => reader.ReadBcFile(filepath);
            
            // Assert
            var expectedMessage = $"File {filepath} could not be found.";
            Assert.That(Call, Throws.TypeOf<IOException>()
                                    .With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void ReadBcFile_AddsInfoMessageToLogIndicatingWhichFileIsBeingRead()
        {
            const string filepath = "random.bc";

            // Setup
            IFileSystem fileSystem = GetFileSystem(filepath);
            var reader = new BcReader(fileSystem);

            // Call
            void Call() => _ = reader.ReadBcFile(filepath).ToArray();
            IEnumerable<string> infoMessages = TestHelper.GetAllRenderedMessages(Call, Level.Info);

            // Assert
            var expectedMessage = $"Reading boundary conditions from {filepath}.";
            Assert.That(infoMessages.Any(m => m.Equals(expectedMessage)));
        }

        [Test]
        public void ReadBcFile_ReturnsExpectedResult()
        {
            const string filepath = "random.bc";

            // Setup
            IFileSystem fileSystem = GetFileSystem(filepath);
            var reader = new BcReader(fileSystem);

            // Call
            IEnumerable<BcIniSection> bcIniSections = reader.ReadBcFile(filepath);

            // Assert
            Assert.That(bcIniSections.Count, Is.EqualTo(2));

            AssertThatSectionIsExpectedGeneralSection(bcIniSections.First());
            AssertThatSectionIsExpectedBoundarySection(bcIniSections.Last());
        }

        private void AssertThatSectionIsExpectedBoundarySection(BcIniSection boundarySection)
        {
            Assert.That(boundarySection.Section.Name, Is.EqualTo("forcing"));
            Assert.That(boundarySection.Section.PropertyCount, Is.EqualTo(3));
            Assert.That(boundarySection.Section.ContainsProperty("name"));
            Assert.That(boundarySection.Section.ContainsProperty("function"));
            Assert.That(boundarySection.Section.ContainsProperty("timeInterpolation"));
            
            Assert.That(boundarySection.Table.Count, Is.EqualTo(2));
            IBcQuantityData firstRow = boundarySection.Table[0];
            Assert.That(firstRow.Quantity.Value, Is.EqualTo("time"));
            Assert.That(firstRow.Unit.Value, Is.EqualTo("minutes since 2010-11-09 00:00:00"));
            Assert.That(firstRow.Values.Count, Is.EqualTo(2));
            Assert.That(firstRow.Values[0], Is.EqualTo("0"));
            Assert.That(firstRow.Values[1], Is.EqualTo("60"));
            
            IBcQuantityData secondRow = boundarySection.Table[1];
            Assert.That(secondRow.Quantity.Value, Is.EqualTo("lateral_discharge"));
            Assert.That(secondRow.Unit.Value, Is.EqualTo(@"m³/s"));
            Assert.That(secondRow.Values.Count, Is.EqualTo(2));
            Assert.That(secondRow.Values[0], Is.EqualTo("1"));
            Assert.That(secondRow.Values[1], Is.EqualTo("2"));
        }

        private void AssertThatSectionIsExpectedGeneralSection(BcIniSection generalSection)
        {
            Assert.That(generalSection.Section.Name, Is.EqualTo("General"));
            Assert.That(generalSection.Section.LineNumber, Is.EqualTo(2));
            Assert.That(generalSection.Section.PropertyCount, Is.EqualTo(2));
            Assert.That(generalSection.Table, Is.Empty);
        }

        private static IFileSystem GetFileSystem(string filepath)
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath, new MockFileData(GetFileContent()) }
            });

            return fileSystem;
        }

        private static string GetFileContent()
        {
            return @"
            [General]
            fileVersion           = 1.01                
            fileType              = boundConds          

            [forcing]
            name                  = 1057660L            
            function              = timeseries          
            timeInterpolation     = linear              
            quantity              = time                
            unit                  = minutes since 2010-11-09 00:00:00
            quantity              = lateral_discharge   
            unit                  = m³/s                
            0 1 
            60 2 
            ";
        }
    }
}