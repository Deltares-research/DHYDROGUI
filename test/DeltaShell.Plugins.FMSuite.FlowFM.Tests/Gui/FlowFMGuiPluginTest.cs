using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FlowFMGuiPluginTest
    {
        //[TestCase(@"ReloadGrid\netfile_projected_unassigned.nc", 0, @"ReloadGrid\netfile_projected_unassigned.nc", 0)]
        //[TestCase(@"ReloadGrid\netfile_projected_unassigned.nc", 0, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)] // wgs84

        //[TestCase(@"ReloadGrid\netfile_projected_assigned.nc", 2005, @"ReloadGrid\netfile_projected_assigned.nc", 2005)] // st. kitts
        [TestCase(@"ReloadGrid\netfile_projected_assigned.nc", 2005, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]

        //[TestCase(@"ReloadGrid\netfile_spherical_assigned.nc", 4326, @"ReloadGrid\netfile_spherical_assigned.nc", 4326)]
        //[TestCase(@"ReloadGrid\netfile_spherical_assigned.nc", 4326, @"ReloadGrid\netfile_projected_unassigned.nc", 0)]

        public void ReloadGridTest(string originalNetFile, int originalEpsg, string editedNetFile, int expectedEpsg)
        {
            var mduFilePath = GetMduFilePath();
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
         
            model.ReloadGrid();

            var expectedCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(expectedEpsg);

            Assert.IsTrue(model.CoordinateSystem.EqualsTo(expectedCoordinateSystem));
            Assert.IsTrue(model.ModelDefinition.CoordinateSystem.EqualsTo(expectedCoordinateSystem));
            Assert.IsTrue(model.Grid.CoordinateSystem.EqualsTo(expectedCoordinateSystem));

            FileUtils.DeleteIfExists(workDir);
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

        private static string GetMduFilePath()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"ReloadGrid\reloadGrid.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            Assert.IsTrue(File.Exists(mduFilePath));
            return mduFilePath;
        }
    }
}
