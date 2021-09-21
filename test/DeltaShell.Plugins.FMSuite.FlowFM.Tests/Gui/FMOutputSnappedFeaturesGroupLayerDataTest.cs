using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    /*
     TEST NOTES:
     Due to the file locks, if one of the tests fails with an unhandled exception, the rest could also fall in waterfall (the file will remain locked until leaving this test fixture).
     Take that into account when 'fixing' tests here.         */
    [TestFixture, Apartment(ApartmentState.STA)]
    public class FMOutputSnappedFeaturesGroupLayerDataTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CheckFMOutputSnappedFeaturesGroupLayerDataAllowsRerunWithASecondModelInTheProject()
        {
            var filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(fmGuiPlugin);

                gui.Run();
                app.OpenProject(filePath); // save to initialize file repository..
                var model = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
                Assert.NotNull(model);

                var secondModel = new WaterFlowFMModel("SecondModel");
                app.Project.RootFolder.Add(secondModel);
                
                //Open view
                gui.CommandHandler.OpenView(secondModel, typeof(ProjectItemMapView));
                ActivityRunner.RunActivity(model);
            }
            try
            {
                FileUtils.DeleteIfExists(filePath);
                FileUtils.DeleteIfExists(filePath + "_data");
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void CheckFMOutputSnappedFeaturesGroupLayerDataIsCreatedWhenThereIsData()
        {
            var filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                app.OpenProject(filePath); // save to initialize file repository..
                var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

                // Should re-run activity since this project may be migrated (clears output)
                app.RunActivity(loadedModel);

                gui.CommandHandler.OpenView(loadedModel, typeof(ProjectItemMapView));
                var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(loadedModel);

                //No layer should be found.
                loadedModel.ModelDefinition.WriteSnappedFeatures = false;
                var snappedOutputLayer = modelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.OutputSnappedFeaturesLayerName) as GroupLayer;
                Assert.IsNull(snappedOutputLayer);
                    
                /* Only added as a child to the map layer if WriteOutputSnappeData is true and there are available layers.*/
                loadedModel.ModelDefinition.WriteSnappedFeatures = true;
                snappedOutputLayer = modelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.OutputSnappedFeaturesLayerName) as GroupLayer;
                Assert.IsNotNull(snappedOutputLayer);


            }
            try
            {
                FileUtils.DeleteIfExists(filePath);
                FileUtils.DeleteIfExists(filePath + "_data");
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void FMOutputSnappedFeaturesGetDefaultCoordinates()
        {
            var filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(fmGuiPlugin);

                gui.Run();

                app.OpenProject(filePath); // save to initialize file repository..
                app.SaveProject();
                var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

                gui.CommandHandler.OpenView(loadedModel, typeof(ProjectItemMapView));
                var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(loadedModel);

                try
                {
                    ActivityRunner.RunActivity(loadedModel);
                        
                    //Set coordinate system for model (and ensure it was set)
                    loadedModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wgs84
                    Assert.IsNotNull(loadedModel.CoordinateSystem);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                var snappedOutputGroup = modelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.OutputSnappedFeaturesLayerName) as GroupLayer;
                Assert.NotNull(snappedOutputGroup);
                var outputSnappedLayers = snappedOutputGroup.Layers.ToList();
                Assert.IsNotEmpty(outputSnappedLayers);

                Assert.IsFalse(outputSnappedLayers.Any(osl => osl.CoordinateSystem == null));
                Assert.IsFalse(outputSnappedLayers.Any(osl => osl.CoordinateSystem != loadedModel.CoordinateSystem));

                //Disable and enable the output features, the coordinate system should remain as it was before
                loadedModel.ModelDefinition.WriteSnappedFeatures = false;
                loadedModel.ModelDefinition.WriteSnappedFeatures = true;
                snappedOutputGroup = modelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.OutputSnappedFeaturesLayerName) as GroupLayer;
                Assert.NotNull(snappedOutputGroup);
                outputSnappedLayers = snappedOutputGroup.Layers.ToList();
                Assert.IsNotEmpty(outputSnappedLayers);
                Assert.IsFalse(outputSnappedLayers.Any(osl => osl.CoordinateSystem == null));
                Assert.IsFalse(outputSnappedLayers.Any(osl => osl.CoordinateSystem != loadedModel.CoordinateSystem));

            }
            try
            {
                FileUtils.DeleteIfExists(filePath);
                FileUtils.DeleteIfExists(filePath + "_data");
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void GetValidLayersLocationShowsErrorMessageWhenFails()
        {
            var model = new WaterFlowFMModel();
            var snappedOutputGroup =
                new FMOutputSnappedFeaturesGroupLayerData(model);
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => snappedOutputGroup.CreateLayers(),
                string.Format(Resources.FMOutputSnappedFeaturesGroupLayerData_GetValidLayersLocation_Output_snapped_feature_layers_location_not_found_at___0_, model.OutputSnappedFeaturesPath));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void RerunModelGeneratesNewSnappedFeatures(bool saving)
        {
            var filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            var newSavePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var app = new DeltaShellApplication
            {
                IsProjectCreatedInTemporaryDirectory = true
            })
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.OpenProject(filePath); // save to initialize file repository..
                /*Prevent overwritting existent data used for other tests */
                var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
                loadedModel.WorkingDirectoryPathFunc = () => TestHelper.GetTestWorkingDirectory(TestHelper.GetCurrentMethodName());
                if (saving)
                {
                    app.SaveProjectAs(newSavePath);
                }
                
                try
                {
                    ActivityRunner.RunActivity(loadedModel);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                var groupLayerData = new FMOutputSnappedFeaturesGroupLayerData(loadedModel);
                //Let's try to pull it up to 10.
                var outputSnappedLayers = groupLayerData.CreateLayers().ToList();
                try
                {
                    ActivityRunner.RunActivity(loadedModel);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }
                outputSnappedLayers.ForEach(l => l.Dispose());

                app.CloseProject();
            }
            try
            {
                FileUtils.DeleteIfExists(filePath);
                FileUtils.DeleteIfExists(filePath + "_data");
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
                FileUtils.DeleteIfExists(newSavePath);
                FileUtils.DeleteIfExists(newSavePath + "_data");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void OpenProjectThenRunItThenCheckSnappedFeaturesWereGenerated(bool saving)
        {
            var filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            var newSavePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.OpenProject(filePath); // save to initialize file repository..
                /*Prevent overwritting existent data used for other tests */
                if (saving)
                {
                    app.SaveProjectAs(newSavePath);
                }
                var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

                try
                {
                    ActivityRunner.RunActivity(loadedModel);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                var groupLayerData = new FMOutputSnappedFeaturesGroupLayerData(loadedModel);
                //Let's try to pull it up to 10.
                var outputSnappedLayers = groupLayerData.CreateLayers().ToList();

                var outputSnappedLayerNames = outputSnappedLayers.Select(osl => osl.Name).ToList();
                Assert.IsTrue(outputSnappedLayerNames.Contains("Cross Sections"));
                Assert.IsTrue(outputSnappedLayerNames.Contains("Weirs"));
                Assert.IsTrue(outputSnappedLayerNames.Contains("Gates"));
                Assert.IsTrue(outputSnappedLayerNames.Contains("Fixed weirs"));
                Assert.IsTrue(outputSnappedLayerNames.Contains("Thin dams"));
                Assert.IsTrue(outputSnappedLayerNames.Contains("Observation stations"));
                Assert.IsTrue(outputSnappedLayerNames.Contains("Dry areas"));

                /* If the assert below fails, did the FM kernel add support for more outputSnappedFeatures?
                   See about adding them to the Asserts above^ */
                Assert.AreEqual(7, outputSnappedLayers.Count, "Number of outputSnappedFeatures differs from expected");
               
                //Limitation from the tests. CreateLayers do not get disposed properly
                outputSnappedLayers.ForEach(l => l.Dispose());
                
                app.CloseProject();
            }
            try
            {
                FileUtils.DeleteIfExists(filePath);
                FileUtils.DeleteIfExists(filePath + "_data");
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
                FileUtils.DeleteIfExists(newSavePath);
                FileUtils.DeleteIfExists(newSavePath + "_data");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void OpenProjectWithOutputSnappedFeaturesTest(bool initialize)
        {
            /* Because this test loads the output snapped features directly without running, we can use
             * it to check whether the folder location is correct as well as the generation of layers. */
            var filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var app = new DeltaShellApplication() { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.OpenProject(filePath); // save to initialize file repository..
                if( initialize)
                    app.SaveProject();

                var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

                // Should re-run activity since this project may be migrated (clears output)
                app.RunActivity(loadedModel);

                var groupLayerData = new FMOutputSnappedFeaturesGroupLayerData(loadedModel);
                var outputSnappedLayers = groupLayerData.CreateLayers().ToList();

                //For the moment we have only 6 shapes in the example given.
                Assert.AreEqual(7, outputSnappedLayers.Count);
                var expectedLayers = new List<string>
                {
                    "Cross Sections",
                    "Weirs",
                    "Gates",
                    "Fixed weirs",
                    "Thin dams",
                    "Observation stations",
                    "Embankments",
                    "Dry areas"
                };
                foreach (var ol in outputSnappedLayers)
                {
                    Assert.IsTrue(expectedLayers.Contains(ol.Name), string.Format("ExpectedLayers list does not contain, or has repeated, created layer {0}", ol.Name));
                    expectedLayers.Remove(ol.Name);
                }

                //Limitation from the tests. CreateLayers do not get disposed properly
                outputSnappedLayers.ForEach(l => l.Dispose());

                app.CloseProject();
            }
            try
            {
                FileUtils.DeleteIfExists(filePath);
                FileUtils.DeleteIfExists(filePath + "_data");
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}