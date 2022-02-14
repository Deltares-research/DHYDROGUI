using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers
{
    // TEST NOTES:
    //  Due to the file locks, if one of the tests fails with an unhandled exception, the rest could also fall in waterfall (the file will remain locked until leaving this test fixture).
    // Take that into account when 'fixing' tests here.
    [TestFixture, Apartment(ApartmentState.STA)]
    public class OutputSnappedFeaturesIntegrationTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CheckFMOutputSnappedFeaturesGroupLayerDataAllowsRerunWithASecondModelInTheProject()
        {
            string filePath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj");
            filePath = TestHelper.CreateLocalCopy(filePath);
            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                IApplication app = gui.Application;
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
                Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned));
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
            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
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
                ProjectItemMapView mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(loadedModel);

                //No layer should be found.
                loadedModel.ModelDefinition.WriteSnappedFeatures = false;
                var snappedOutputLayer = LayerTestUtils.FindLayerByNameRecursively(modelLayer, FlowFMLayerNames.OutputSnappedFeaturesLayerName) as IGroupLayer;
                Assert.IsNull(snappedOutputLayer);
                    
                /* Only added as a child to the map layer if WriteOutputSnappeData is true and there are available layers.*/
                loadedModel.ModelDefinition.WriteSnappedFeatures = true;
                
                snappedOutputLayer = LayerTestUtils.FindLayerByNameRecursively(modelLayer, FlowFMLayerNames.OutputSnappedFeaturesLayerName) as IGroupLayer;
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
            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                IApplication app = gui.Application;
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
                ProjectItemMapView mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(loadedModel);

                try
                {
                    //Set coordinate system for model (and ensure it was set)
                    loadedModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4135);
                    Assert.IsNotNull(loadedModel.CoordinateSystem);

                    ActivityRunner.RunActivity(loadedModel);
                        
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                var snappedOutputGroup = LayerTestUtils.FindLayerByNameRecursively(modelLayer, FlowFMLayerNames.OutputSnappedFeaturesLayerName) as IGroupLayer;
                Assert.NotNull(snappedOutputGroup);
                List<ILayer> outputSnappedLayers = snappedOutputGroup.Layers.ToList();
                Assert.IsNotEmpty(outputSnappedLayers);

                Assert.IsFalse(outputSnappedLayers.Any(osl => osl.CoordinateSystem == null));
                Assert.IsFalse(outputSnappedLayers.Any(osl => osl.CoordinateSystem != loadedModel.CoordinateSystem));

                //Disable and enable the output features, the coordinate system should remain as it was before
                loadedModel.ModelDefinition.WriteSnappedFeatures = false;
                loadedModel.ModelDefinition.WriteSnappedFeatures = true;
                snappedOutputGroup = LayerTestUtils.FindLayerByNameRecursively(modelLayer, FlowFMLayerNames.OutputSnappedFeaturesLayerName) as GroupLayer;
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
    }
}