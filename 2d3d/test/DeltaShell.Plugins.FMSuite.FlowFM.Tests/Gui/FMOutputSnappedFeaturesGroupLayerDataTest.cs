using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    /*
     TEST NOTES:
     Due to the file locks, if one of the tests fails with an unhandled exception, the rest could also fall in waterfall (the file will remain locked until leaving this test fixture).
     Take that into account when 'fixing' tests here.         */
    [TestFixture]
    public class FMOutputSnappedFeaturesGroupLayerDataTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CheckFMOutputSnappedFeaturesGroupLayerDataAllowsRerunWithASecondModelInTheProject()
        {
            string filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var gui = CreateGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                gui.Run();
                
                Project project = projectService.OpenProject(filePath); // save to initialize file repository..
                var model = (WaterFlowFMModel)project.RootFolder.Items[0];
                Assert.NotNull(model);

                var secondModel = new WaterFlowFMModel();
                project.RootFolder.Add(secondModel);

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
            string filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var gui = CreateGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                IApplication app = gui.Application;
                gui.Run();

                Project project = projectService.OpenProject(filePath); // save to initialize file repository..
                var loadedModel = (WaterFlowFMModel)project.RootFolder.Items[0];

                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                loadedModel.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();

                // Should re-run activity since this project may be migrated (clears output)
                app.RunActivity(loadedModel);

                gui.CommandHandler.OpenView(loadedModel, typeof(ProjectItemMapView));
                ProjectItemMapView mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
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
            string filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var gui = CreateGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                gui.Run();

                Project project = projectService.OpenProject(filePath); // save to initialize file repository..
                projectService.SaveProject();
                var loadedModel = (WaterFlowFMModel)project.RootFolder.Items[0];

                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                loadedModel.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();

                gui.CommandHandler.OpenView(loadedModel, typeof(ProjectItemMapView));
                ProjectItemMapView mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
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
                List<ILayer> outputSnappedLayers = snappedOutputGroup.Layers.ToList();
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
            string filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            string newSavePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);

            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings = ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            
            using (IApplication app = CreateApplication(userSettings))
            {
                IProjectService projectService = app.ProjectService;

                Project project = projectService.OpenProject(filePath); // save to initialize file repository..
                /*Prevent overwritting existent data used for other tests */
                if (saving)
                {
                    projectService.SaveProjectAs(newSavePath);
                }

                var loadedModel = (WaterFlowFMModel)project.RootFolder.Items[0];

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
                List<ILayer> outputSnappedLayers = groupLayerData.CreateLayers().ToList();
                try
                {
                    ActivityRunner.RunActivity(loadedModel);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                outputSnappedLayers.ForEach(l => l.Dispose());

                projectService.CloseProject();
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
        public void OpenProjectThenRunItThenCheckSnappedFeaturesWereGenerated(bool saving)
        {
            string filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            string newSavePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);

            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings =
                ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);

            using (IApplication app = CreateApplication(userSettings))
            {
                IProjectService projectService = app.ProjectService;

                Project project = projectService.OpenProject(filePath); // save to initialize file repository..
                /*Prevent overwritting existent data used for other tests */
                if (saving)
                {
                    projectService.SaveProjectAs(newSavePath);
                }

                var loadedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                loadedModel.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();

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
                List<ILayer> outputSnappedLayers = groupLayerData.CreateLayers().ToList();

                List<string> outputSnappedLayerNames = outputSnappedLayers.Select(osl => osl.Name).ToList();
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

                projectService.CloseProject();
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
        public void OpenProjectWithOutputSnappedFeaturesTest(bool initialize)
        {
            /* Because this test loads the output snapped features directly without running, we can use
             * it to check whether the folder location is correct as well as the generation of layers. */
            string filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);

            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings =
                ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            
            using (IApplication app = CreateApplication(userSettings))
            {
                IProjectService projectService = app.ProjectService;

                Project project = projectService.OpenProject(filePath); // save to initialize file repository..
                if (initialize)
                {
                    projectService.SaveProject();
                }

                var loadedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                loadedModel.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();

                // Should re-run activity since this project may be migrated (clears output)
                app.RunActivity(loadedModel);

                var groupLayerData = new FMOutputSnappedFeaturesGroupLayerData(loadedModel);
                List<ILayer> outputSnappedLayers = groupLayerData.CreateLayers().ToList();

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
                    "Dry areas"
                };
                foreach (ILayer ol in outputSnappedLayers)
                {
                    Assert.IsTrue(expectedLayers.Contains(ol.Name), string.Format("ExpectedLayers list does not contain, or has repeated, created layer {0}", ol.Name));
                    expectedLayers.Remove(ol.Name);
                }

                //Limitation from the tests. CreateLayers do not get disposed properly
                outputSnappedLayers.ForEach(l => l.Dispose());

                projectService.CloseProject();
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

        private static IApplication CreateApplication(ApplicationSettingsBase userSettings)
        {
            IApplication application = new DHYDROApplicationBuilder().WithFlowFM().Build();
            application.UserSettings = userSettings;
            application.Run();
            return application;
        }

        private static IGui CreateGui()
        {
            return new DHYDROGuiBuilder().WithFlowFM().Build();
        }
    }
}