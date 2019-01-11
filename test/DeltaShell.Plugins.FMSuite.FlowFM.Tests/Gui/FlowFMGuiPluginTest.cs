using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FlowFmGuiPluginTest
    {
        //[TestCase(@"ReloadGrid\netfile_projected_unassigned.nc", 0, @"ReloadGrid\netfile_projected_unassigned.nc", 0)]
        //[TestCase(@"ReloadGrid\netfile_projected_unassigned.nc", 0, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)] // wgs84

        //[TestCase(@"ReloadGrid\netfile_projected_assigned.nc", 2005, @"ReloadGrid\netfile_projected_assigned.nc", 2005)] // st. kitts
        [TestCase(@"ReloadGrid\netfile_projected_assigned.nc", 2005, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]

        //[TestCase(@"ReloadGrid\netfile_spherical_assigned.nc", 4326, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]
        //[TestCase(@"ReloadGrid\netfile_spherical_assigned.nc", 4326, @"ReloadGrid\netfile_projected_unassigned.nc", 0)]

        public void ReloadGridTest(string originalNetFile, int originalEpsg, string editedNetFile, int expectedEpsg)
        {
            //Given
            var mduFilePath = GetMduFilePathWithoutGrid();
            var originalNetFilePath = GetOriginalNetFilePath(originalNetFile);
            var workDir = GetWorkingDirectory(mduFilePath);

            FileUtils.CopyFile(originalNetFilePath, Path.Combine(workDir, "grid.nc"));

            var model = new WaterFlowFMModel(mduFilePath);
            var originalCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(originalEpsg);

            Assert.IsTrue(model.CoordinateSystem.EqualsTo(originalCoordinateSystem));
            Assert.IsTrue(model.ModelDefinition.CoordinateSystem.EqualsTo(originalCoordinateSystem));
            Assert.IsTrue(model.Grid.CoordinateSystem.EqualsTo(originalCoordinateSystem));

            var editedNetFilePath = GetEditedNetFilePath(editedNetFile);

            FileUtils.CopyFile(editedNetFilePath, Path.Combine(workDir, "grid.nc"));
         
            //When
            model.ReloadGrid();

            //Then
            var expectedCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(expectedEpsg);

            Assert.IsTrue(model.CoordinateSystem.EqualsTo(expectedCoordinateSystem));
            Assert.IsTrue(model.ModelDefinition.CoordinateSystem.EqualsTo(expectedCoordinateSystem));
            Assert.IsTrue(model.Grid.CoordinateSystem.EqualsTo(expectedCoordinateSystem));

            FileUtils.DeleteIfExists(workDir);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAFmModelWithoutGridAndNotInitializing_WhenValidatingModelWithValidationBeforeRunSwitchedOff_ThenModelIsValidated()
        {
            //Given
            var mduFilePath = GetMduFilePathWithoutGrid();
            var model = new WaterFlowFMModel(mduFilePath)
            {
                ValidateBeforeRun = false,
                Status = ActivityStatus.None
            };

            //When
            var result = model.Validate();

            //Then
            Assert.That(result, Is.Not.Null);
            var errors = result.AllErrors.ToList();
            Assert.That(errors.ElementAt(0).Message, Is.EqualTo("Grid is empty"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAFmModelWithoutGridAndInitializing_WhenValidatingModelWithValidationBeforeRunSwitchedOff_ThenModelIsNotValidated()
        {
            //Given
            var mduFilePath = GetMduFilePathWithoutGrid();
            var model = new WaterFlowFMModel(mduFilePath)
            {
                ValidateBeforeRun = false,
                Status = ActivityStatus.Initializing
            };

            //When
            var result = model.Validate();

            //Then
            Assert.That(result, Is.Not.Null);
            var errors = result.AllErrors.ToList();
            //We assert that there are no errors because the model is initializing,
            //this means we do not validate the model.
            Assert.That(errors, Is.Empty, "The model is validated");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAFlowFMGuiPlugin_WhenGetProjectTreeViewNodePresentersIsCalled_AnEnumerableContainingAnFMClassMapFileFunctionStoreNodePresenterIsReturned()
        {
            // Given
            var guiPlugin = new FlowFMGuiPlugin();

            // When
            var nodePresenters = guiPlugin.GetProjectTreeViewNodePresenters().ToArray();

            // Then
            var fmClassMapFileFunctionStoreNodePresenter = nodePresenters.OfType<FMClassMapFileFunctionStoreNodePresenter>().SingleOrDefault();
            Assert.NotNull(fmClassMapFileFunctionStoreNodePresenter);
            Assert.AreSame(guiPlugin, fmClassMapFileFunctionStoreNodePresenter.GuiPlugin);
        }

        private static string GetEditedNetFilePath(string editedNetFile)
        {
            var editedNetFilePath = TestHelper.GetTestFilePath(editedNetFile);

            Assert.IsTrue(File.Exists(editedNetFilePath));
            return editedNetFilePath;
        }

        private static string GetWorkingDirectory(string mduFilePath)
        {
            var workDir = new FileInfo(mduFilePath).DirectoryName;
            Assert.NotNull(workDir);
            return workDir;
        }

        private static string GetOriginalNetFilePath(string originalNetFile)
        {
            var originalNetFilePath = TestHelper.GetTestFilePath(originalNetFile);
            Assert.IsTrue(File.Exists(originalNetFilePath));
            return originalNetFilePath;
        }

        private static string GetMduFilePathWithoutGrid()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"ReloadGrid\reloadGrid.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            Assert.IsTrue(File.Exists(mduFilePath));
            return mduFilePath;
        }
    }
}
