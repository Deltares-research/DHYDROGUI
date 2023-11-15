using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class BcWriterTest
    {
        [Test]
        public void Constructor_FileSystemNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BcWriter(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void WriteBcFile_InvalidFilePath_ThrowsArgumentException(string filepath)
        {
            // Setup
            var fileSystem = new MockFileSystem();
            var writer = new BcWriter(fileSystem);

            // Call
            void Call() => writer.WriteBcFile(Enumerable.Empty<BcIniSection>(), filepath);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void WriteBcFile_DirectoryDoesNotExist_CreatesDirectory()
        {
            // Setup
            const string directory = @"C:\This\path\does\not\exist\";
            var filepath = $@"{directory}\random.bc";

            var fileSystem = new MockFileSystem();

            var writer = new BcWriter(fileSystem);
            IEnumerable<BcIniSection> bcIniSections = GetBcIniSections();
            
            // Precondition
            Assert.That(fileSystem.Directory.Exists(directory), Is.False);

            // Call
            writer.WriteBcFile(bcIniSections, filepath);

            // Assert
            Assert.That(fileSystem.Directory.Exists(directory), Is.True);
        }

        [Test]
        public void WriteBcFile_AddsInfoMessageToLogIndicatingWhichFileIsBeingWritten()
        {
            const string filepath = "random.bc";

            // Setup
            IFileSystem fileSystem = new MockFileSystem();
            var writer = new BcWriter(fileSystem);

            IEnumerable<BcIniSection> bcIniSections = GetBcIniSections();

            // Call
            void Call() => writer.WriteBcFile(bcIniSections, filepath);
            IEnumerable<string> infoMessages = TestHelper.GetAllRenderedMessages(Call, Level.Info);

            // Assert
            var expectedMessage = $"Writing boundary conditions to {filepath}.";
            Assert.That(infoMessages.Any(m => m.Equals(expectedMessage)));
        }

        [Test]
        public void WriteBcFile_WritesExpectedContents()
        {
            // Setup
            const string filepath = "random.bc";

            var fileSystem = new MockFileSystem();

            var writer = new BcWriter(fileSystem);
            IEnumerable<BcIniSection> bcIniSections = GetBcIniSections();

            // Call
            writer.WriteBcFile(bcIniSections, filepath);

            // Assert
            MockFileData bcFile = fileSystem.GetFile(filepath);
            string contents = GetExpectedContents();
            Assert.That(bcFile.TextContents, Is.EqualTo(contents));
        }

        private static IEnumerable<BcIniSection> GetBcIniSections()
        {
            var bcIniSections = new List<BcIniSection>();

            bcIniSections.Add(CreateGeneralSection());
            bcIniSections.Add(CreateBcIniSection());

            return bcIniSections;
        }

        private static BcIniSection CreateGeneralSection()
        {
            var generalIniSection = new IniSection("General");
            generalIniSection.AddProperty("fileVersion", "1.01");
            generalIniSection.AddProperty("fileType", "boundConds");

            return new BcIniSection(generalIniSection);
        }

        private static BcIniSection CreateBcIniSection()
        {
            var bcSection = new BcIniSection("forcing");
            bcSection.Section.AddProperty("name", "randomName");
            bcSection.Section.AddProperty("function", "constant");
            bcSection.Section.AddProperty("timeInterpolation", "linear");

            AddTimeQuantity(bcSection);
            AddDischargeQuantity(bcSection);

            return bcSection;
        }

        private static void AddTimeQuantity(BcIniSection bcSection)
        {
            var timeQuantity = new IniProperty("quantity", "time");
            var timeUnit = new IniProperty("unit", "minutes since 2010-11-09 00:00:00");

            bcSection.Table.Add(new BcQuantityData(timeQuantity)
            {
                Unit = timeUnit,
                Values = new List<string>()
                {
                    "0",
                    "60"
                }
            });
        }

        private static void AddDischargeQuantity(BcIniSection bcSection)
        {
            var dischargeQuantity = new IniProperty("quantity", "lateral_discharge");
            var dischargeUnit = new IniProperty("unit", "m^3");

            bcSection.Table.Add(new BcQuantityData(dischargeQuantity)
            {
                Unit = dischargeUnit,
                Values = new List<string>()
                {
                    "1",
                    "2"
                }
            });
        }

        private static string GetExpectedContents()
        {
            return 
@"[General]
    fileVersion           = 1.01                
    fileType              = boundConds          

[forcing]
    name                  = randomName          
    function              = constant            
    timeInterpolation     = linear              
    quantity              = time                
    unit                  = minutes since 2010-11-09 00:00:00
    quantity              = lateral_discharge   
    unit                  = m^3
    0 1 
    60 2 

";
        }
    }
}