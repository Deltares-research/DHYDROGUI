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
        [Test]
        public void GivenAGriddedHeatFluxModel_WhenSavingAs_ThenTheFilesShouldBeCopiedAndNewPathsShouldBeSet()
        {
            //Given
            string sourceGriddedHeatFluxFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo.htc");
            string sourceGridFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo.grd");
            
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
                heatFluxModel.CopyTo(targetGriddedHeatFluxFile, true);

                //Then
                Assert.IsTrue(File.Exists(targetGriddedHeatFluxFile));
                Assert.IsTrue(File.Exists(expectedTargetGridFile));

                Assert.AreEqual(heatFluxModel.GriddedHeatFluxFilePath, targetGriddedHeatFluxFile);
                Assert.AreEqual(heatFluxModel.GridFilePath, expectedTargetGridFile);
            }
        }

        [Test]
        public void GivenAGriddedHeatFluxModel_WhenExporting_ThenOnlyTheFilesShouldBeCopied()
        {
            //Given
            string sourceGriddedHeatFluxFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo.htc");
            string sourceGridFile = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo.grd");

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
                heatFluxModel.CopyTo(targetGriddedHeatFluxFile, false);

                //Then
                Assert.IsTrue(File.Exists(targetGriddedHeatFluxFile));
                Assert.IsTrue(File.Exists(expectedTargetGridFile));

                Assert.AreEqual(heatFluxModel.GriddedHeatFluxFilePath, copyGriddedHeatFluxFile);
                Assert.AreEqual(heatFluxModel.GridFilePath, copyGridFile);
            }
        }
    }
}