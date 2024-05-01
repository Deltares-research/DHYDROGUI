using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class GroupablePointCloudImporterTest
    {
        [Test]
        [Category(TestCategory.Performance)]
        public void ImportLargeAmountOfDryPoints()
        {
            var xyzFilePath = TestHelper.GetTestFilePath(@"xyzFiles\largeSampleFile.xyz");
            var tempDir = FileUtils.CreateTempDirectory();
            var testFilePath = Path.Combine(tempDir, Path.GetFileName(xyzFilePath));
            FileUtils.CopyFile(xyzFilePath, testFilePath);

            try
            {
                var pluginsToAdd = new List<IPlugin>()
                {
                    new SharpMapGisApplicationPlugin(),
                    new NetworkEditorApplicationPlugin(),
                    new FlowFMApplicationPlugin(),
                    new ProjectExplorerGuiPlugin(),
                    new SharpMapGisGuiPlugin(),
                    new FlowFMGuiPlugin(),
                    new NetworkEditorGuiPlugin(),
                };
                using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
                {
                    var app = gui.Application;
                    gui.Run();
                    app.CreateNewProject();
                    
                    Action mainWindowShown = delegate
                    {
                        app.Project.RootFolder.Add(new WaterFlowFMModel());
                        var targetModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                        Assert.IsNotNull(targetModel);

                        var dryPointsImporter =
                            app.FileImporters.OfType<GroupablePointCloudImporter>().FirstOrDefault();
                        Assert.IsNotNull(dryPointsImporter);
                        var activity = new FileImportActivity(dryPointsImporter, targetModel.Area.DryPoints)
                        {
                            Files = new[] {testFilePath}
                        };

                        // Importing large amount of dry points
                        /* Before fixes from rev 39371 (DELFT3DFM-1374) performance was around 150 seconds. */
                        /* Personal machine : 20 seconds avg. */
                        /* x1.5 factor acceptance factor */
                        /* x3 factor TeamCity acceptance factor */
                        TestHelper.AssertIsFasterThan(90000, () => { gui.Application.RunActivity(activity); });

                        Assert.AreEqual(1048576, targetModel.Area.DryPoints.Count);
                    };
                    WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDir);
            }
        }

        [Test]
        public void PointCloudImporterCanImportOnRootLevel()
        {
            var importer = new PointCloudImporter<PointFeature>();
            Assert.IsTrue(importer.CanImportOnRootLevel);
        }
        
        [Test]
        public void GroupablePointCloudImporterCanImportOnRootLevel()
        {
            var importer = new GroupablePointCloudImporter();
            Assert.IsFalse(importer.CanImportOnRootLevel);
        }

        [TestCase(null)]
        [TestCase("NotListObject")]
        public void GivenGroupableCloudImporter_WhenImportingWithTargetThatIsNotAListOrNullObject_ThenNullIsReturned(string target)
        {
            var xyzFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryGroup1_dry.xyz");
            xyzFilePath = TestHelper.CreateLocalCopy(xyzFilePath);
            try
            {
                var importer = new GroupablePointCloudImporter();
                var importedFeatures = importer.ImportItem(xyzFilePath, target);
                Assert.IsNull(importedFeatures);
            }
            finally
            {
                FileUtils.DeleteIfExists(xyzFilePath);
            }
        }

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