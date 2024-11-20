using System.IO;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Properties;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests
{
    [TestFixture]
    public class NGHSFileBaseTest
    {
        private string testDirectory;
        private string randomFile;

        [SetUp]
        public void Setup()
        {
            testDirectory = FileUtils.CreateTempDirectory();
            randomFile = Path.Combine(testDirectory, "random.file");
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(testDirectory);
        }

        [Test]
        public void OpenInputFile_FilePathIsLocked_ThrowsIOException()
        {
            var nghsFileBase = new TestNGHSFileBase();
            // open and lock the file
            using (File.Create(randomFile))
            {
                // Precondition
                Assert.That(FileUtils.IsFileLocked(randomFile));

                // Call
                TestDelegate call = () => nghsFileBase.TestOpenInputFile(randomFile);

                // Assert
                string expectedMessage = string.Format(Resources.NGHSFileBase_File_is_locked, randomFile);
                Assert.That(call, Throws.TypeOf<IOException>()
                                        .With.Message.EqualTo(expectedMessage));
            }
        }
        
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        [TestCase(@"C:\ThisFile\DoesNot\Exist.404")]
        public void OpenInputFile_IncorrectFilePath_ThrowsFileNotFoundException(string filepath)
        {
            var nghsFileBase = new TestNGHSFileBase();

            // Call
            TestDelegate call = () => nghsFileBase.TestOpenInputFile(filepath);

            // Assert
            string expectedMessage = string.Format(Resources.NGHSFileBase_File_could_not_be_found, filepath);
            Assert.That(call, Throws.TypeOf<FileNotFoundException>()
                                    .With.Message.EqualTo(expectedMessage));
        }

        /// <summary>
        /// Test NGHSFileBase class to test protected members.
        /// </summary>
        private class TestNGHSFileBase : NGHSFileBase
        {
            public void TestOpenInputFile(string filePath)
            {
                OpenInputFile(filePath);
            }
        }
    }
}