using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
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
            string sourceGriddedHeatFluxFile = TestHelper.GetTestFilePath(@"heatFluxFiles\GriddedHeatFluxModel\meteo.htc");
            string sourceGridFile = TestHelper.GetTestFilePath(@"heatFluxFiles\GriddedHeatFluxModel\meteo.grd");
            
            using (var temp = new TemporaryDirectory())
            {
                string copyGriddedHeatFluxFile = Path.Combine(temp.Path, "meteo.htc");
                string copyGridFile = Path.Combine(temp.Path, "meteo.grd");

                FileUtils.CopyFile(sourceGriddedHeatFluxFile, copyGriddedHeatFluxFile);
                FileUtils.CopyFile(sourceGridFile, copyGridFile);
                
                var heatFluxModel = new HeatFluxModel()
                {
                    GriddedHeatFluxFilePath = copyGriddedHeatFluxFile,
                    GridFilePath = copyGridFile
                };

                string targetGriddedHeatFluxFile = Path.Combine(temp.Path, "save", "meteo.htc");
                string expectedTargetGridFile = Path.Combine(temp.Path, "save", "meteo.grd");

                //When
                heatFluxModel.CopyTo(targetGriddedHeatFluxFile, switchTo);

                //Then
                Assert.IsTrue(File.Exists(targetGriddedHeatFluxFile));
                Assert.IsTrue(File.Exists(expectedTargetGridFile));

                if (switchTo)
                {
                    Assert.AreEqual(heatFluxModel.GriddedHeatFluxFilePath, targetGriddedHeatFluxFile);
                    Assert.AreEqual(heatFluxModel.GridFilePath, expectedTargetGridFile);
                }
                else
                {
                    Assert.AreEqual(heatFluxModel.GriddedHeatFluxFilePath, copyGriddedHeatFluxFile);
                    Assert.AreEqual(heatFluxModel.GridFilePath, copyGridFile);
                }
            }
        }
    }
}