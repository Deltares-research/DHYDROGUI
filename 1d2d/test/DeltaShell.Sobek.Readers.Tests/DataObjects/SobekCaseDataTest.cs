using System;
using System.Collections.Generic;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.DataObjects
{
    [TestFixture]
    public class SobekCaseDataTest
    {
        [Test]
        public void Constructor_FilePathsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SobekCaseData(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("filePaths"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var caseData = new SobekCaseData(Array.Empty<string>());

            // Assert
            Assert.That(caseData.IsEmpty);
            Assert.That(caseData.PrecipitationFile, Is.Null);
            Assert.That(caseData.RksFile, Is.Null);
            Assert.That(caseData.WindFile, Is.Null);
            Assert.That(caseData.EvaporationFile, Is.Null);
            Assert.That(caseData.TemperatureFile, Is.Null);
            Assert.That(caseData.BoundaryConditionsFile, Is.Null);
            Assert.That(caseData.BoundaryConditionsTableFile, Is.Null);
        }

        [Test]
        [TestCaseSource(nameof(IsEmptyCases))]
        public void IsEmpty_ReturnsCorrectResult(IEnumerable<string> filePaths, bool expResult)
        {
            // Assert
            var caseData = new SobekCaseData(filePaths);

            // Call
            bool result = caseData.IsEmpty;

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [TestCase("file.bui")]
        [TestCase("FILE.BUI")]
        public void PrecipitationFile_ReturnsCorrectResult(string fileName)
        {
            // Setup
            var filePath = $"z:\\path\\to\\{fileName}";
            string[] filePaths =
            {
                "z:\\path\\to\\dummy.file",
                filePath
            };

            // Call
            var caseData = new SobekCaseData(filePaths);

            // Assert
            Assert.That(caseData.PrecipitationFile, Is.Not.Null);
            Assert.That(caseData.PrecipitationFile.FullName, Is.EqualTo(filePath));
        }

        [TestCase("file.wdc")]
        [TestCase("file.wnd")]
        public void WindFile_ReturnsCorrectResult(string fileName)
        {
            // Setup
            var filePath = $"z:\\path\\to\\{fileName}";
            string[] filePaths =
            {
                "z:\\path\\to\\dummy.file",
                filePath
            };

            // Call
            var caseData = new SobekCaseData(filePaths);

            // Assert
            Assert.That(caseData.WindFile, Is.Not.Null);
            Assert.That(caseData.WindFile.FullName, Is.EqualTo(filePath));
        }

        [TestCase("file.evp")]
        [TestCase("file.gem")]
        [TestCase("file.plv")]
        [TestCase("evapor.file")]
        public void EvaporationFile_ReturnsCorrectResult(string fileName)
        {
            // Setup
            var filePath = $"z:\\path\\to\\{fileName}";
            string[] filePaths =
            {
                "z:\\path\\to\\dummy.file",
                filePath
            };

            // Call
            var caseData = new SobekCaseData(filePaths);

            // Assert
            Assert.That(caseData.EvaporationFile, Is.Not.Null);
            Assert.That(caseData.EvaporationFile.FullName, Is.EqualTo(filePath));
        }

        [Test]
        public void TemperatureFile_ReturnsCorrectResult()
        {
            // Setup
            var filePath = $"z:\\path\\to\\file.tmp";
            string[] filePaths =
            {
                "z:\\path\\to\\dummy.file",
                filePath
            };

            // Call
            var caseData = new SobekCaseData(filePaths);

            // Assert
            Assert.That(caseData.TemperatureFile, Is.Not.Null);
            Assert.That(caseData.TemperatureFile.FullName, Is.EqualTo(filePath));
        }

        [Test]
        public void BoundaryConditionsFile_ReturnsCorrectResult()
        {
            // Setup
            var filePath = $"z:\\path\\to\\bound3b.3b";
            string[] filePaths =
            {
                "z:\\path\\to\\dummy.file",
                filePath
            };

            // Call
            var caseData = new SobekCaseData(filePaths);

            // Assert
            Assert.That(caseData.BoundaryConditionsFile, Is.Not.Null);
            Assert.That(caseData.BoundaryConditionsFile.FullName, Is.EqualTo(filePath));
        }

        [Test]
        public void BoundaryConditionsTableFile_ReturnsCorrectResult()
        {
            // Setup
            var filePath = $"z:\\path\\to\\bound3b.tbl";
            string[] filePaths =
            {
                "z:\\path\\to\\dummy.file",
                filePath
            };

            // Call
            var caseData = new SobekCaseData(filePaths);

            // Assert
            Assert.That(caseData.BoundaryConditionsTableFile, Is.Not.Null);
            Assert.That(caseData.BoundaryConditionsTableFile.FullName, Is.EqualTo(filePath));
        }

        private static IEnumerable<TestCaseData> IsEmptyCases()
        {
            yield return new TestCaseData(Array.Empty<string>(), true);
            yield return new TestCaseData(new[]
            {
                "some\\path"
            }, false);
            yield return new TestCaseData(new[]
            {
                "some\\file.txt",
                "some\\other\\file.txt",
            }, false);
        }
    }
}