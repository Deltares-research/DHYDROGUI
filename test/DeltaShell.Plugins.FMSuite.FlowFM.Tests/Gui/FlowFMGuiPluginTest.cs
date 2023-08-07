using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.Gui.Restart;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using GeoAPI.Extensions.CoordinateSystems;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FlowFmGuiPluginTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAFmModelWithoutGridAndNotInitializing_WhenValidatingModelWithValidationBeforeRunSwitchedOff_ThenModelIsValidated()
        {
            //Given
            string mduFilePath = GetMduFilePathWithoutGrid();

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);

            model.ValidateBeforeRun = false;
            model.Status = ActivityStatus.None;

            //When
            ValidationReport result = model.Validate();

            //Then
            Assert.That(result, Is.Not.Null);
            List<ValidationIssue> errors = result.AllErrors.ToList();
            Assert.That(errors.ElementAt(0).Message, Is.EqualTo("Grid is empty"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAFmModelWithoutGridAndInitializing_WhenValidatingModelWithValidationBeforeRunSwitchedOff_ThenModelIsNotValidated()
        {
            //Given
            string mduFilePath = GetMduFilePathWithoutGrid();

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);

            model.ValidateBeforeRun = false;
            model.Status = ActivityStatus.Initializing;

            //When
            ValidationReport result = model.Validate();

            //Then
            Assert.That(result, Is.Not.Null);
            List<ValidationIssue> errors = result.AllErrors.ToList();
            //We assert that there are no errors because the model is initializing,
            //this means we do not validate the model.
            Assert.That(errors, Is.Empty, "The model is validated");
        }

        [Test]
        public void GetProjectTreeViewNodePresenters_ContainsCorrectNodePresenters()
        {
            // Given
            var guiPlugin = new FlowFMGuiPlugin();

            // When
            ITreeNodePresenter[] nodePresenters = guiPlugin.GetProjectTreeViewNodePresenters().ToArray();

            // Then
            var classMapFileNodePresenter = Contains<FMClassMapFileFunctionStoreNodePresenter>(nodePresenters);
            Assert.That(classMapFileNodePresenter.GuiPlugin, Is.SameAs(guiPlugin));

            var restartFileNodePresenter = Contains<RestartFileNodePresenter<WaterFlowFMRestartFile>>(nodePresenters);
            Assert.That(restartFileNodePresenter.GuiPlugin, Is.SameAs(guiPlugin));
        }

        private static T Contains<T>(ITreeNodePresenter[] source)
        {
            List<T> items = source.OfType<T>().ToList();
            Assert.That(items, Has.Count.EqualTo(1), $"Collection should contain one {typeof(T).Name}");

            return items[0];
        }

        [Category(TestCategory.Jira)] // D3DFMIQ-614
        [Category(TestCategory.DataAccess)]
        [TestCase(@"ReloadGrid\netfile_projected_unassigned.nc", 0, @"ReloadGrid\netfile_projected_unassigned.nc", 0)]
        [TestCase(@"ReloadGrid\netfile_projected_unassigned.nc", 0, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]  // wgs84
        [TestCase(@"ReloadGrid\netfile_projected_assigned.nc", 2005, @"ReloadGrid\netfile_projected_assigned.nc", 2005)] // st. kitts
        [TestCase(@"ReloadGrid\netfile_projected_assigned.nc", 2005, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]
        [TestCase(@"ReloadGrid\netfile_spherical_assigned.nc", 4326, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]
        [TestCase(@"ReloadGrid\netfile_spherical_assigned.nc", 4326, @"ReloadGrid\netfile_projected_unassigned.nc", 0)]
        public void ReloadGridTest(string originalNetFile, int originalEpsg, string editedNetFile, int expectedEpsg)
        {
            //Given
            string mduFilePath = GetMduFilePathWithoutGrid();
            string originalNetFilePath = GetOriginalNetFilePath(originalNetFile);
            string workDir = GetWorkingDirectory(mduFilePath);

            FileUtils.CopyFile(originalNetFilePath, Path.Combine(workDir, "grid.nc"));

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);

            ICoordinateSystem originalCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(originalEpsg);

            Assert.IsTrue(model.CoordinateSystem.EqualsTo(originalCoordinateSystem));
            Assert.IsTrue(model.ModelDefinition.CoordinateSystem.EqualsTo(originalCoordinateSystem));
            Assert.IsTrue(model.Grid.CoordinateSystem.EqualsTo(originalCoordinateSystem));

            string editedNetFilePath = GetEditedNetFilePath(editedNetFile);

            FileUtils.CopyFile(editedNetFilePath, Path.Combine(workDir, "grid.nc"));

            //When
            model.ReloadGrid();

            //Then
            ICoordinateSystem expectedCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(expectedEpsg);

            Assert.IsTrue(model.CoordinateSystem.EqualsTo(expectedCoordinateSystem));
            Assert.IsTrue(model.ModelDefinition.CoordinateSystem.EqualsTo(expectedCoordinateSystem));
            Assert.IsTrue(model.Grid.CoordinateSystem.EqualsTo(expectedCoordinateSystem));

            FileUtils.DeleteIfExists(workDir);
        }

        private static string GetEditedNetFilePath(string editedNetFile)
        {
            string editedNetFilePath = TestHelper.GetTestFilePath(editedNetFile);

            Assert.IsTrue(File.Exists(editedNetFilePath));
            return editedNetFilePath;
        }

        private static string GetWorkingDirectory(string mduFilePath)
        {
            string workDir = new FileInfo(mduFilePath).DirectoryName;
            Assert.NotNull(workDir);
            return workDir;
        }

        private static string GetOriginalNetFilePath(string originalNetFile)
        {
            string originalNetFilePath = TestHelper.GetTestFilePath(originalNetFile);
            Assert.IsTrue(File.Exists(originalNetFilePath));
            return originalNetFilePath;
        }

        private static string GetMduFilePathWithoutGrid()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"ReloadGrid\reloadGrid.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            Assert.IsTrue(File.Exists(mduFilePath));
            return mduFilePath;
        }
    }
}