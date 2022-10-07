using System;
using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Fnm;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Files.Fnm
{
    [TestFixture]
    public class FnmSubFileTest
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void Constructor_FileNameNullOrEmpty_ThrowsArgumentException(string fileName)
        {
            // Call
            void Call() => new FnmSubFile(fileName, 0, "some_description", FnmSubFileType.Input);

            // Assert
            Assert.That(Call, Throws.ArgumentException
                                    .With.Property(nameof(ArgumentException.ParamName))
                                    .EqualTo("fileName"));
        }

        [Test]
        public void Constructor_IndexNegative_ThrowsArgumentOutOfRangeException()
        {
            // Call
            void Call() => new FnmSubFile("some_file_name", -1, "", FnmSubFileType.Output);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentOutOfRangeException>()
                                    .With.Property(nameof(ArgumentException.ParamName))
                                    .EqualTo("index"));
        }

        [Test]
        public void Constructor_DescriptionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new FnmSubFile("some_file_name", 1, null, FnmSubFileType.Undefined);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName))
                                    .EqualTo("description"));
        }

        [Test]
        public void Constructor_FileTypeUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Call
            void Call() => new FnmSubFile("some_file_name", 2, "some_description", (FnmSubFileType)99);

            // Assert
            Assert.That(Call, Throws.TypeOf<InvalidEnumArgumentException>()
                                    .With.Property(nameof(ArgumentException.Message))
                                    .EqualTo("fileType"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var fnmSubFile = new FnmSubFile("some_file_name", 3, "some_description", FnmSubFileType.Undefined);

            // Assert
            Assert.That(fnmSubFile.FileName, Is.EqualTo("some_file_name"));
            Assert.That(fnmSubFile.Index, Is.EqualTo(3));
            Assert.That(fnmSubFile.Description, Is.EqualTo("some_description"));
            Assert.That(fnmSubFile.FileType, Is.EqualTo(FnmSubFileType.Undefined));
        }
    }
}