using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class DelftIniFileParserTest
    {
        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void GivenNoFile_WhenTryingToExecuteReadFile_ThenAFileNotFoundExceptionIsThrown()
        {
            const string nonExistingFilePath = @"This/File/Does/Not/Exist";
            DelftIniFileParser.ReadFile(nonExistingFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(FileReadingException))]
        public void GivenAnEmptyFile_WhenParsingFile_ThenAFileReadingExceptionIsThrown()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var filePath = Path.Combine(tempDir, "anEmpty.ini");
                using (File.Create(filePath)) { }
                DelftIniFileParser.ReadFile(filePath);
            });
        }
    }
}