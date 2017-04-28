using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.UndoRedo)]
    public class UndoRedoIntegratedModelHydroRegionEditorIntegrationTest : UndoRedoHydroRegionTestBase
    {
        private WaterFlowModel1D model;
        private Action mainWindowShown;

        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());
            gui.Plugins.Add(new HydroModelGuiPlugin());

            gui.Run();

            project = app.Project;

            // add data
            model = new WaterFlowModel1D();
            project.RootFolder.Add(model);

            // show gui main window
            mainWindow = (Window) gui.MainWindow;

            // wait until gui starts
            mainWindowShown = () =>
                {
                    network = model.Network;
                    gui.CommandHandler.OpenView(model, typeof (ProjectItemMapView));

                    ProjectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

                    gui.UndoRedoManager.TrackChanges = true;

                    onMainWindowShown();
                };
        }

        [TearDown]
        public void TearDown()
        {
            gui.UndoRedoManager.TrackChanges = false;
            gui.Dispose();
            onMainWindowShown = null;
            mainWindowShown = null;
            LogHelper.ResetLogging();
        }

        [Test]
        public void UndoDeletionOfControlledWeirShouldNotThrow()
        {
            // See the text in TOOLS-10074 for full details on the steps to take for the exception to occur.
            // In this test we expand an existing model, and therefore we place only one extra weir on the
            // first branch since there is already another structure on the branch.
            var path = "undo_weir.dsproj";
            onMainWindowShown = () =>
            {
                gui.UndoRedoManager.TrackChanges = false;

                gui.Application.Project.RootFolder.Items.RemoveAt(0); // remove existing model

                // import new model
                var modelImporter = new SobekHydroModelImporter();
                string pathToSobekNetwork = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DWAQ_AC1\DWAQ_AC1.lit\37\network.tp");
                modelImporter.PathSobek = pathToSobekNetwork;
                modelImporter.useFlow = true;
                modelImporter.useRTC = true;
                modelImporter.useRR = false;
                modelImporter.Import();

                var hydroModel = (HydroModel)modelImporter.TargetObject;

                gui.Application.Project.RootFolder.Add(hydroModel);

                var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
                var rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().First();

                // fix test environment
                model = flowModel;
                network = flowModel.Network;

                // open central map view
                gui.CommandHandler.OpenView(hydroModel, typeof(ProjectItemMapView));
                ProjectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

                // add weirs to first branch
                var branch = flowModel.Network.Branches.First();
                var weir1 = AddWeir(branch.Source.Geometry.Coordinate);
                var weir1Name = weir1.Name;

                // link weir to output rtc
                var controlGroup = rtcModel.ControlGroups.First();
                var outputDataItem = rtcModel.GetDataItemByValue(controlGroup.Outputs.First());
                outputDataItem.LinkedBy[0].Unlink();
                var weirDataItem = flowModel.GetChildDataItems(weir1)
                                            .First(di => (di.Role & DataItemRole.Input) > 0);
                weirDataItem.LinkTo(outputDataItem);

                // save project 
                gui.Application.SaveProjectAs(path);

                // close project
                gui.Application.CloseProject();

                // load project
                gui.Application.OpenProject(path);
                var retrievedHydroModel = (HydroModel)gui.Application.Project.RootFolder.Models.First();
                var retrievedNetwork = (HydroNetwork)retrievedHydroModel.Region.SubRegions.First();
                var retrievedWeir1 = retrievedNetwork.Weirs.First(w => w.Name == weir1Name);

                // open central map view again
                gui.CommandHandler.OpenView(retrievedHydroModel, typeof(ProjectItemMapView));
                ProjectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

                // enable undo/redo, gooi weg en undo, toon region
                gui.UndoRedoManager.TrackChanges = true;

                var preDelete = new List<IBranchFeature>(retrievedNetwork.Branches[0].BranchFeatures.Select(TypeUtils.Unproxy));
                preDelete.Sort();

                // delete the weir
                DeleteFeature(retrievedWeir1);

                var postDelete = new List<IBranchFeature>(retrievedNetwork.Branches[0].BranchFeatures.Select(TypeUtils.Unproxy));
                postDelete.Sort();
                
                gui.UndoRedoManager.Undo();

                // sort the branch features (as the Region Contents tree view would do)
                var branchFeatures = new List<IBranchFeature>(retrievedNetwork.Branches[0].BranchFeatures.Select(TypeUtils.Unproxy));
                branchFeatures.Sort(); // this action used to throw exception
            };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }
    }
}