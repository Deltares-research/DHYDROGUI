using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
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
        [Category(TestCategory.VerySlow)]
        public void ImportLargeAmountOfDryPoints()
        {
            string xyzFilePath = TestHelper.GetTestFilePath(@"xyzFiles\largeSampleFile.xyz");
            string tempDir = FileUtils.CreateTempDirectory();
            string testFilePath = Path.Combine(tempDir, Path.GetFileName(xyzFilePath));
            FileUtils.CopyFile(xyzFilePath, testFilePath);

            try
            {
                using (var gui = DeltaShellCoreFactory.CreateGui())
                {
                    IApplication app = gui.Application;
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new FlowFMApplicationPlugin());

                    gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                    gui.Plugins.Add(new SharpMapGisGuiPlugin());
                    gui.Plugins.Add(new FlowFMGuiPlugin());
                    gui.Plugins.Add(new NetworkEditorGuiPlugin());

                    gui.Run();
                    app.CreateNewProject();

                    Action mainWindowShown = delegate
                    {
                        app.Project.RootFolder.Add(new WaterFlowFMModel());
                        WaterFlowFMModel targetModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                        Assert.IsNotNull(targetModel);

                        GroupablePointCloudImporter dryPointsImporter =
                            app.FileImporters.OfType<GroupablePointCloudImporter>().FirstOrDefault();
                        Assert.IsNotNull(dryPointsImporter);
                        var activity = new FileImportActivity(dryPointsImporter, targetModel.Area.DryPoints)
                        {
                            Files = new[]
                            {
                                testFilePath
                            }
                        };

                        // Importing large amount of dry points
                        /* Before fixes from rev 39371 (DELFT3DFM-1374) performance was around 150 seconds. */
                        /* Personal machine : 20 seconds avg. */
                        /* x1.5 factor acceptance factor */
                        /* x3 factor TeamCity acceptance factor */
                        TestHelper.AssertIsFasterThan(90000, () => { gui.Application.RunActivity(activity); });

                        Assert.AreEqual(1048576, targetModel.Area.DryPoints.Count);
                    };
                    WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
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

        [Test]
        public void ImportDryPointFeatureAssignsGroupName()
        {
            /* This class is located in the framework and fails to import correctly dry points. */
            string xyzFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryGroup1_dry.xyz");
            Assert.IsTrue(File.Exists(xyzFilePath));
            xyzFilePath = TestHelper.CreateLocalCopy(xyzFilePath);
            try
            {
                var importer = new GroupablePointCloudImporter();
                var dryPoints = new List<GroupablePointFeature>();
                importer.ImportItem(xyzFilePath, dryPoints);

                Assert.AreNotEqual(0, dryPoints.Count);
                List<IGrouping<string, GroupablePointFeature>> asGroup = dryPoints.GroupBy(g => g.GroupName).ToList();
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
            string xyzFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\badFormatFile.xyz");
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

        [Test]
        public void GivenGroupablePointCloudImporter_WhenImportingGroupablePointFeaturesOnModel_ThenProgressMessagesAreAsExpected()
        {
            // Arrange
            var progressMessages = new List<string>();
            var importer = new GroupablePointCloudImporter
            {
                GetRegion = myObject => new HydroArea(),
                ProgressChanged = (text, currentStep, totalAmountOfSteps) =>
                {
                    var progressText = $"{text} {currentStep}/{totalAmountOfSteps}";
                    progressMessages.Add(progressText);
                }
            };

            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string filePath = Path.Combine(temporaryDirectory.Path, "myFile.xyz");
                string[] fileContent =
                {
                    "0.50 0.50 -2.50",
                    "1.50 0.50 -3.50"
                };
                File.WriteAllLines(filePath, fileContent);

                importer.GetRootDirectory = list => temporaryDirectory.Path;
                importer.GetBaseDirectory = list => temporaryDirectory.Path;

                // Act
                importer.ImportItem(filePath, new List<GroupablePointFeature>());
            }

            // Assert
            var expectedProgressMessages = new[]
            {
                "Importing 2 point features 1/3",
                "Importing point features : 0 / 2 1/3",
                "Finished importing 2 point features 2/3",
                "Setting group names 0 / 2 2/3",
                "Finished importing 2 point features 3/3"
            };

            Assert.That(progressMessages, Is.EqualTo(expectedProgressMessages));
        }

        [TestCase(null)]
        [TestCase("NotListObject")]
        public void GivenGroupableCloudImporter_WhenImportingWithTargetThatIsNotAListOrNullObject_ThenNullIsReturned(string target)
        {
            string xyzFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryGroup1_dry.xyz");
            xyzFilePath = TestHelper.CreateLocalCopy(xyzFilePath);
            try
            {
                var importer = new GroupablePointCloudImporter();
                object importedFeatures = importer.ImportItem(xyzFilePath, target);
                Assert.IsNull(importedFeatures);
            }
            finally
            {
                FileUtils.DeleteIfExists(xyzFilePath);
            }
        }
    }
}