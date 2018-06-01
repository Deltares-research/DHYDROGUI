using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FlowFMGuiPluginTest
    {
        [TestCase(@"ReloadGrid\netfile_projected_unassigned.nc", 0, @"ReloadGrid\netfile_projected_unassigned.nc", 0)]
        [TestCase(@"ReloadGrid\netfile_projected_unassigned.nc", 0, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)] // wgs84

        [TestCase(@"ReloadGrid\netfile_projected_assigned.nc", 2005, @"ReloadGrid\netfile_projected_assigned.nc", 2005)] // st. kitts
        [TestCase(@"ReloadGrid\netfile_projected_assigned.nc", 2005, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]
        
        [TestCase(@"ReloadGrid\netfile_spherical_assigned.nc", 4326, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]
        [TestCase(@"ReloadGrid\netfile_spherical_assigned.nc", 4326, @"ReloadGrid\netfile_projected_unassigned.nc", 0)]

        public void ReloadGridTest(string originalNetFile, int originalEpsg, string editedNetFile, int expectedEpsg)
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"ReloadGrid\reloadGrid.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            Assert.IsTrue(File.Exists(mduFilePath));

            var originalNetFilePath = TestHelper.GetTestFilePath(originalNetFile);
            Assert.IsTrue(File.Exists(originalNetFilePath));

            var workDir = new FileInfo(mduFilePath).DirectoryName;
            Assert.NotNull(workDir);
            FileUtils.CopyFile(originalNetFilePath, Path.Combine(workDir, "grid.nc"));
            
            var model = new WaterFlowFMModel(mduFilePath);

            var originalCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(originalEpsg);

            Assert.IsTrue(model.CoordinateSystem.EqualsTo(originalCoordinateSystem));
            Assert.IsTrue(model.ModelDefinition.CoordinateSystem.EqualsTo(originalCoordinateSystem));
            Assert.IsTrue(model.Grid.CoordinateSystem.EqualsTo(originalCoordinateSystem));

            var editedNetFilePath = TestHelper.GetTestFilePath(editedNetFile);
            Assert.IsTrue(File.Exists(editedNetFilePath));
            FileUtils.CopyFile(editedNetFilePath, Path.Combine(workDir, "grid.nc"));
            
            var view = new WaterFlowFMModelView();

            TypeUtils.CallPrivateStaticMethod(typeof(FlowFMGuiPlugin), "ReloadGrid", model, view);

            var expectedCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(expectedEpsg);

            Assert.IsTrue(model.CoordinateSystem.EqualsTo(expectedCoordinateSystem));
            Assert.IsTrue(model.ModelDefinition.CoordinateSystem.EqualsTo(expectedCoordinateSystem));
            Assert.IsTrue(model.Grid.CoordinateSystem.EqualsTo(expectedCoordinateSystem));

            FileUtils.DeleteIfExists(workDir);
        }
    }
}
