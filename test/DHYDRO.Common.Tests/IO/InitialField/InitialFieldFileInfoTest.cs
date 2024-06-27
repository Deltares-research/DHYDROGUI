using DHYDRO.Common.IO.InitialField;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
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
            void Call() => _ = new InitialFieldFileInfo(fileVersion, "some_file_type");

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
            void Call() => _ = new InitialFieldFileInfo("1.2.3", fileType);

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