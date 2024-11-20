using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [Category(TestCategory.DataAccess)]
    [TestFixture]
    public class HtcFileTest
    {
        [Test]
        public void GivenNullForHtcFile_WhenReading_ExceptionShouldBeThrown()
        {
            Assert.Throws<ArgumentNullException>(() => { HtcFile.GetCorrespondingGridFilePath(null); },
                                                 "Heat flux file path is not valid");
        }

        [Test]
        public void GivenHtcFile_WhenReading_GridFileNameIsReturned()
        {
            // Given
            string sourceGriddedHeatFluxFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo.htc");

            using (var temp = new TemporaryDirectory())
            {
                string copyGriddedHeatFluxFile = Path.Combine(temp.Path, "meteo.htc");
                FileUtils.CopyFile(sourceGriddedHeatFluxFile, copyGriddedHeatFluxFile);

                string htcDir = Path.GetDirectoryName(copyGriddedHeatFluxFile);
                Assert.IsNotNull(htcDir);
                // When
                string gridFilePath = HtcFile.GetCorrespondingGridFilePath(copyGriddedHeatFluxFile);

                // Then
                Assert.That(gridFilePath,
                            Is.EqualTo(Path.Combine(htcDir, "meteo.grd")));
            }
        }

        [Test]
        public void GivenHtcFilePath_WhenReadingAndNoGridFileNameIsFound_ExceptionShouldBeThrown()
        {
            // Given
            string sourceGriddedHeatFluxFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo2.htc");

            using (var temp = new TemporaryDirectory())
            {
                string copyGriddedHeatFluxFile = Path.Combine(temp.Path, "meteo2.htc");
                FileUtils.CopyFile(sourceGriddedHeatFluxFile, copyGriddedHeatFluxFile);

                string htcDir = Path.GetDirectoryName(copyGriddedHeatFluxFile);
                Assert.IsNotNull(htcDir);

                // When Then
                Assert.Throws<InvalidOperationException>(
                    () => { HtcFile.GetCorrespondingGridFilePath(copyGriddedHeatFluxFile); },
                    "Relative Grid file path is missing in the *.htc file");
            }
        }
    }
}