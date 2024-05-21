using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField.Data
{
    [TestFixture]
    public class InitialFieldFileInfoTest
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Constructor_FileVersionNullOrWhitespace_ThrowsArgumentException(string fileVersion)
        {
            // Call
            void Call()
            {
                new InitialFieldFileInfo(fileVersion, "some_file_type");
            }

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Constructor_FileTypeNullOrWhitespace_ThrowsArgumentException(string fileType)
        {
            // Call
            void Call()
            {
                new InitialFieldFileInfo("1.2.3", fileType);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var general = new InitialFieldFileInfo("1.2.3", "some_file_type");

            // Assert
            Assert.That(general.FileVersion, Is.EqualTo("1.2.3"));
            Assert.That(general.FileType, Is.EqualTo("some_file_type"));
        }
    }
}