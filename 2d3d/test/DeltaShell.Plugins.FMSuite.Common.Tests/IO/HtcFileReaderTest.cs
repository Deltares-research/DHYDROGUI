using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [Category(TestCategory.DataAccess)]
    [TestFixture]
    public class HtcFileReaderTest
    {
        [Test]
        public void GivenHtcFile_WhenReading_GridFileNameIsReturned()
        {
            string sourceGriddedHeatFluxFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo.htc");

            using (var temp = new TemporaryDirectory())
            {
                string copyGriddedHeatFluxFile = Path.Combine(temp.Path, "meteo.htc");
                FileUtils.CopyFile(sourceGriddedHeatFluxFile, copyGriddedHeatFluxFile);
                Assert.That(ReadGridFileFromHtcFile(copyGriddedHeatFluxFile), Is.EqualTo("meteo.grd"));
            }
        }

        [Test]
        public void GivenHtcFilePath_WhenReadingAndNoGridFileNameIsFound_NullShouldBeReturned()
        {
            string sourceGriddedHeatFluxFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo2.htc");

            using (var temp = new TemporaryDirectory())
            {
                string copyGriddedHeatFluxFile = Path.Combine(temp.Path, "meteo2.htc");
                FileUtils.CopyFile(sourceGriddedHeatFluxFile, copyGriddedHeatFluxFile);
                Assert.IsNull(ReadGridFileFromHtcFile(copyGriddedHeatFluxFile));
            }
        }

        [Test]
        public void GivenNotExistingHtcFilePath_WhenReading_ExceptionShouldBeThrown()
        {
            string sourceGriddedHeatFluxFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo3.htc");
            Assert.Throws<FileNotFoundException>(() => { ReadGridFileFromHtcFile(sourceGriddedHeatFluxFile); });
        }

        private static string ReadGridFileFromHtcFile(string filePath)
        {
            var htcReader = new HtcFileReader(filePath);
            return htcReader.ReadGridFileNameWithExtension();
        }
    }
}