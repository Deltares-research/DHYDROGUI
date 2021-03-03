using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        // D3DFMIQ-2567
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenAWaterFlowFMModelWithAGrid_WhenTheBedLevelTypeIsChangedAndTheGridReplaced_ThenTheBedLevelLocationAreCorrectly()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                string zipSourcePath = TestHelper.GetTestFilePath("WaterFlowFMModel.MorphologicalGrid/D3DFMIQ-2567.zip");
                ZipFileUtils.Extract(zipSourcePath, tempDir.Path);

                string mduPath = Path.Combine(tempDir.Path, "model.mdu");
                model.LoadFromMdu(mduPath);

                // Pre-condition
                Assert.That(model.SpatialData.Bathymetry.Coordinates, Is.Not.Empty);
                const string nodesMeanLev = "3";
                Assert.That(model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).GetValueAsString(), 
                            Is.EqualTo(nodesMeanLev));

                // When
                const string faces = "1";
                model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(faces);

                // This mimics the behaviour of the FlowFMNetImporter
                string replacementGridSourcePath = TestHelper.GetTestFilePath("WaterFlowFMModel.MorphologicalGrid/replacement_grid.nc");
                string gridTargetPath = Path.Combine(tempDir.Path, "FlowFM_net.nc");

                FileUtils.CopyFile(replacementGridSourcePath, gridTargetPath);

                model.ReloadGrid(false, true);

                // Then
                // TODO: extend this when the model.ReloadGrid does not throw anymore.
                Assert.That(model.SpatialData.Bathymetry.Coordinates, Is.Not.Empty);
                Assert.That(model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).GetValueAsString(), 
                            Is.EqualTo(faces));
            }
        }
    }
}