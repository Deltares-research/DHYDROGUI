using System.IO;
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
        [ExpectedException(typeof(FileNotFoundException))]
        public void GivenAnEmptyFile_WhenParsingFile_ThenAFileReadingExceptionIsThrown()
        {
            const string nonExistingFilePath = @"This/File/Does/Not/Exist";
            DelftIniFileParser.ReadFile(nonExistingFilePath);
        }
    }
}