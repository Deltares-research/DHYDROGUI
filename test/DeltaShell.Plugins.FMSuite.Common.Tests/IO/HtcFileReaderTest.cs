using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class HtcFileReaderTest
    {
        [Test]
        public void GivenHtcFile_WhenReading_GridFileNameIsReturned()
        {
            var filePath = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo.htc");
            var testFilePath = TestHelper.CreateLocalCopy(filePath);
            Assert.That(ReadGridFileFromHtcFile(testFilePath), Is.EqualTo("meteo.grd"));
        }

        [Test]
        public void GivenHtcFilePath_WhenReadingAndNoGridFileNameIsFound_NullShouldBeReturned()
        {
            var filePath = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo2.htc");
            var testFilePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsNull(ReadGridFileFromHtcFile(testFilePath));
        }

        [Test]
        public void GivenNotExistingHtcFilePath_WhenReading_ExceptionShouldBeThrown()
        {
            var filePath = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo3.htc");
            var testFilePath = TestHelper.CreateLocalCopy(filePath);
            Assert.Throws<FileNotFoundException>(() => { ReadGridFileFromHtcFile(testFilePath); });

        }

        private static string ReadGridFileFromHtcFile(string filePath)
        {
            try
            {
                var htcReader = new HtcFileReader(filePath);
                return htcReader.ReadGridFileNameWithExtension();
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
            }
        }
    }
}
