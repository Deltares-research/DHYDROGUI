using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class DelftBcFileParserTest
    {
        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void GivenFilePathToANonExistentFile_WhenExecutingDelftBcFileReaderReadFile_ThenAFileNotFoundExceptionIsThrown()
        {
            const string nonExistingFilePath = @"This/File/Does/Not/Exist";
            DelftBcFileParser.ReadFile(nonExistingFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(FileReadingException))]
        public void GivenAnEmptyFile_WhenExecutingDelftBcFileReaderReadFile_ThenAFileReadingExceptionIsThrown()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var filePath = Path.Combine(tempDir, "anEmpty.File");
                using (File.Create(filePath)) { }
                DelftBcFileParser.ReadFile(filePath);
            });
        }
    }
}