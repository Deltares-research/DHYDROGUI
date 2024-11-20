using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers {
    
    [TestFixture]
    public class SobekReaderTest
    {
        private string testDirectory;
        private string randomFile;

        [SetUp]
        public void Setup()
        {
            // Setup
            testDirectory = FileUtils.CreateTempDirectory();
            randomFile = Path.Combine(testDirectory, "random.file");
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(testDirectory);
        }

        [Test]
        public void SobekFileRead_FilePathIsLocked_LogsMessage()
        {
            var sobekReader  = new TestSobekReader();
            // open and lock the file
            using (File.Create(randomFile))
            {
                // Precondition
                Assert.That(FileUtils.IsFileLocked(randomFile));

                // Call
                Action call = () => sobekReader.Read(randomFile).ToArray(); // ToArray() to execute the function, it is enumerable
                
                // Assert
                Assert.That(call, Throws.Nothing);
                
                // Assert
                string expectedMessage = $"Could not read file {randomFile}: it is locked by another process.";
                TestHelper.AssertLogMessageIsGenerated(call, expectedMessage, 1);
            }

        }

        private class TestSobekReader : SobekReader<TestSobekObject>
        {
            public override IEnumerable<TestSobekObject> Parse(string text)
            {
                throw new NotImplementedException();
            }
        }

        private class TestSobekObject{ }
    }
}