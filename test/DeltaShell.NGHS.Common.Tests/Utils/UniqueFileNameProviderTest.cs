using System;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class UniqueFileNameProviderTest
    {
        [Test]
        public void GetUniqueFileNameFor_ReturnsUniqueFileName()
        {
            // Setup
            var provider = new UniqueFileNameProvider();

            // Calls
            string result1 = provider.GetUniqueFileNameFor("unique.file");
            string result2 = provider.GetUniqueFileNameFor("unique.file");
            string result3 = provider.GetUniqueFileNameFor("unique.file");

            // Assert
            Assert.That(result1, Is.EqualTo("unique.file"));
            Assert.That(result2, Is.EqualTo("unique_1.file"));
            Assert.That(result3, Is.EqualTo("unique_2.file"));
        }

        [Test]
        public void AddFiles_FileNamesNull_ThrowsArgumentNullException()
        {
            // Setup
            var provider = new UniqueFileNameProvider();

            // Call
            void Call() => provider.AddFiles(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("fileNames"));
        }

        [Test]
        public void AddFiles_DuplicateFileNames_ThrowsInvalidOperationException()
        {
            // Setup
            var provider = new UniqueFileNameProvider();
            string[] fileNames =
            {
                "unique.file",
                "unique.file"
            };

            // Call
            void Call() => provider.AddFiles(fileNames);

            // Assert
            var e = Assert.Throws<InvalidOperationException>(Call);
            Assert.That(e.Message, Is.EqualTo("Cannot add a file that was already added: unique.file"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetUniqueFileName_WhenFileNamesAdded_ReturnsUniqueFileName()
        {
            // Setup
            var provider = new UniqueFileNameProvider();
            string[] fileNames =
            {
                "unique.file",
                "another_unique.file"
            };
            provider.AddFiles(fileNames);

            // Calls
            string result1 = provider.GetUniqueFileNameFor("unique.file");
            string result2 = provider.GetUniqueFileNameFor("another_unique.file");

            // Assert
            Assert.That(result1, Is.EqualTo("unique_1.file"));
            Assert.That(result2, Is.EqualTo("another_unique_1.file"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void GetUniqueFileNameFor_FileNameNullOrEmpty_ThrowsArgumentException(string fileName)
        {
            // Setup
            var provider = new UniqueFileNameProvider();

            // Call
            void Call() => provider.GetUniqueFileNameFor(null);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("fileName"));
        }
    }
}