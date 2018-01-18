using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class GroupableCloudImporterTest
    {
        [Test]
        public void ImportDryPointFeatureAssignsGroupName()
        {
            /* This class is located in the framework and fails to import correctly dry points. */
            var xyzFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryGroup1_dry.xyz");
            Assert.IsTrue(File.Exists(xyzFilePath));
            xyzFilePath = TestHelper.CreateLocalCopy(xyzFilePath);
            try
            {
                var importer = new GroupablePointCloudImporter();
                var dryPoints = new List<GroupablePointFeature>();
                importer.ImportItem(xyzFilePath, dryPoints);
                
                Assert.AreNotEqual(0, dryPoints.Count);
                var asGroup = dryPoints.GroupBy( g => g.GroupName).ToList();
                Assert.That(asGroup.Count, Is.EqualTo(1));
                Assert.That(asGroup.First().Key, Is.EqualTo(xyzFilePath.Replace(@"\", "/")));
            }
            finally
            {
                FileUtils.DeleteIfExists(xyzFilePath);
            }
        }

        [Test]
        public void ImportDryPointFeatureWithWrongFormatReturnsNull()
        {
            /* This class is located in the framework and fails to import correctly dry points. */
            var xyzFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\badFormatFile.xyz");
            Assert.IsTrue(File.Exists(xyzFilePath));
            xyzFilePath = TestHelper.CreateLocalCopy(xyzFilePath);
            try
            {
                var importer = new GroupablePointCloudImporter();
                var dryPoints = new List<GroupablePointFeature>();

                Assert.IsNull(importer.ImportItem(xyzFilePath, dryPoints));
            }
            finally
            {
                FileUtils.DeleteIfExists(xyzFilePath);
            }
        }
    }
}