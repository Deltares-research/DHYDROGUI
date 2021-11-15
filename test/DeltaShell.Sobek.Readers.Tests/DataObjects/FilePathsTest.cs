using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.DataObjects
{
    [TestFixture]
    public class FilePathsTest
    {
        [Test]
        public void Constructor_FilePathsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new FilePaths(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("filePaths"));
        }

        [Test]
        public void GetByExtensions_FileNotPresent_ReturnsNull()
        {
            var files = new[]
            {
                "z:\\dir\\file.abc"
            };

            // Setup
            var filePaths = new FilePaths(files);

            // Call
            FileInfo result = filePaths.GetByExtensions(".def");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetByExtensionsCases))]
        public void GetByExtensions_FilePresent_ReturnsCorrectResult(string[] files, string[] extensions, string expPath)
        {
            // Setup
            var filePaths = new FilePaths(files);

            // Call
            FileInfo result = filePaths.GetByExtensions(extensions);

            // Assert
            Assert.That(result.FullName, Is.EqualTo(expPath));
        }

        [Test]
        public void GetByName_FileNotPresent_ReturnsNull()
        {
            var files = new[]
            {
                "z:\\dir\\file.abc"
            };

            // Setup
            var filePaths = new FilePaths(files);

            // Call
            FileInfo result = filePaths.GetByName("other.abc");

            // Assert
            Assert.That(result, Is.Null);
        }

        [TestCase("file1.abc")]
        [TestCase("file2.abc")]
        [TestCase("file1.def")]
        [TestCase("file2.def")]
        public void GetByName_FilePresent_ReturnsCorrectsResult(string fileName)
        {
            // Setup
            var files = new[]
            {
                "z:\\dir\\file1.abc",
                "z:\\dir\\file2.abc",
                "z:\\dir\\file1.def",
                "z:\\dir\\file2.def",
            };

            var filePaths = new FilePaths(files);

            // Call
            FileInfo result = filePaths.GetByName(fileName);

            // Assert
            Assert.That(result.FullName, Is.EqualTo($"z:\\dir\\{fileName}"));
        }

        [Test]
        public void GetByNameWithoutExtension_FileNotPresent_ReturnsNull()
        {
            var files = new[]
            {
                "z:\\dir\\file.abc"
            };

            // Setup
            var filePaths = new FilePaths(files);

            // Call
            FileInfo result = filePaths.GetByNameWithoutExtension("other.abc");

            // Assert
            Assert.That(result, Is.Null);
        }

        [TestCase("file1", "file1.abc")]
        [TestCase("file2", "file2.abc")]
        [TestCase("file3", "file3.def")]
        public void GetByNameWithoutExtension_FilePresent_ReturnsCorrectResult(string fileName, string expFileName)
        {
            // Setup
            var files = new[]
            {
                "z:\\dir\\file1.abc",
                "z:\\dir\\file2.abc",
                "z:\\dir\\file1.def",
                "z:\\dir\\file3.def",
            };

            var filePaths = new FilePaths(files);

            // Call
            FileInfo result = filePaths.GetByNameWithoutExtension(fileName);

            // Assert
            Assert.That(result.FullName, Is.EqualTo($"z:\\dir\\{expFileName}"));
        }

        private static IEnumerable<TestCaseData> GetByExtensionsCases()
        {
            // file paths, extensions parameters, expected result
            yield return new TestCaseData(new[]
            {
                "z:\\dir\\file.abc"
            }, new[]
            {
                ".abc"
            }, "z:\\dir\\file.abc");
            yield return new TestCaseData(new[]
            {
                "z:\\dir\\file.abc",
                "z:\\dir\\file.def"
            }, new[]
            {
                ".def"
            }, "z:\\dir\\file.def");
            yield return new TestCaseData(new[]
            {
                "z:\\dir\\file.abc",
                "z:\\dir\\file.def"
            }, new[]
            {
                ".def",
                "abc"
            }, "z:\\dir\\file.def");
            yield return new TestCaseData(new[]
            {
                "z:\\dir\\file.abc",
                "z:\\dir\\file.def"
            }, new[]
            {
                ".abc",
                "def"
            }, "z:\\dir\\file.abc");
        }
    }
}