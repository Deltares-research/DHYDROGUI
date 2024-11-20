using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelDefinition
{
    [TestFixture]
    public class HeatFluxModelTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void GivenAGriddedHeatFluxModel_WhenSavingAsOrExporting_ThenTheFilesShouldBeCopiedAndNewPathsShouldBeSetOnlyIfSwitchToIsTrue(bool switchTo)
        {
            //Given
            using (var temp = new TemporaryDirectory())
            {
                string copyinTempGriddedHeatFluxFilePath = temp.CopyTestDataFileToTempDirectory(@"heatFluxFiles\GriddedHeatFluxModel\meteo.htc");
                string copyInTempGridFilePath = temp.CopyTestDataFileToTempDirectory(@"heatFluxFiles\GriddedHeatFluxModel\meteo.grd");

                var heatFluxModel = new HeatFluxModel()
                {
                    GriddedHeatFluxFilePath = copyinTempGriddedHeatFluxFilePath,
                    GridFilePath = copyInTempGridFilePath
                };

                string absoluteSaveFolderInTempPath = Path.Combine(temp.Path, "save");

                string targetGriddedHeatFluxFile = Path.Combine(absoluteSaveFolderInTempPath, "meteo.htc");
                string expectedTargetGridFile = Path.Combine(absoluteSaveFolderInTempPath, "meteo.grd");

                //When
                heatFluxModel.CopyTo(targetGriddedHeatFluxFile, switchTo);

                //Then
                Assert.IsTrue(File.Exists(targetGriddedHeatFluxFile));
                Assert.IsTrue(File.Exists(expectedTargetGridFile));

                if (switchTo)
                {
                    Assert.AreEqual(heatFluxModel.GriddedHeatFluxFilePath, targetGriddedHeatFluxFile, "The GriddedHeatFluxFilePath is not correctly set after a save");
                    Assert.AreEqual(heatFluxModel.GridFilePath, expectedTargetGridFile, "The GridFilePath is not correctly set after a save");
                }
                else
                {
                    Assert.AreEqual(heatFluxModel.GriddedHeatFluxFilePath, copyinTempGriddedHeatFluxFilePath, "The GriddedHeatFluxFilePath is not correctly set after an export");
                    Assert.AreEqual(heatFluxModel.GridFilePath, copyInTempGridFilePath, "The GridFilePath is not correctly set after an export");
                }
            }
        }
    }
}