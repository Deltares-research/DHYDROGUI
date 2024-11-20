using System.IO;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        private const string faces = "1";
        private const string nodesMeanLev = "3";
        private const string facesMeanLevFromNodes = "6";

        // D3DFMIQ-2567
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [TestCase(faces)]
        [TestCase(facesMeanLevFromNodes)]
        public void GivenAWaterFlowFMModelWithAGrid_WhenTheBedLevelTypeIsChangedAndTheGridReplaced_ThenTheBedLevelLocationAreCorrectlyUpdatedToTheValuesOnTheNewGrid(string bedlevType)
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
                Assert.That(model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).GetValueAsString(), 
                            Is.EqualTo(nodesMeanLev));

                // When
                model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(bedlevType);

                // This mimics the behaviour of the FlowFMNetImporter
                string replacementGridSourcePath = TestHelper.GetTestFilePath("WaterFlowFMModel.MorphologicalGrid/replacement_grid.nc");
                string gridTargetPath = Path.Combine(tempDir.Path, "FlowFM_net.nc");

                FileUtils.CopyFile(replacementGridSourcePath, gridTargetPath);

                model.ReloadGrid(false, true);

                // Then
                Assert.That(model.SpatialData.Bathymetry.Coordinates, Is.Not.Empty);
                Assert.That(model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).GetValueAsString(), 
                            Is.EqualTo(bedlevType));

                double[] expectedValues = 
                    UnstructuredGridFileHelper.ReadZValues(gridTargetPath,
                                                           UnstructuredGridFileHelper.BedLevelLocation.Faces);

                IMultiDimensionalArray data = model.SpatialData.Bathymetry.Components[0].Values;
                var retrievedValues = new double[data.Count]; 
                data.CopyTo(retrievedValues, 0);

                Assert.That(retrievedValues, Is.EqualTo(expectedValues));
            }
        }
    }
}