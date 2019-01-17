using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class NHibernateHydroModelGuiIntegrationTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void SaveLoadHydroModelAndCopyPasteTools8955()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = "wqer.dsproj";

                Action onMainWindowShown =
                    delegate
                        {
                            var project = app.Project;
                            
                            project.RootFolder.Add(ModelTestHelper.GetHydroModelForSobek());

                            app.SaveProjectAs(path);
                            app.CloseProject();
                            app.OpenProject(path);

                            var retrievedProject = app.Project;
                            var retrievedHydro = retrievedProject.RootFolder.Models.First();

                            gui.CopyPasteHandler.Copy(retrievedHydro);
                            gui.CopyPasteHandler.Paste(retrievedProject, retrievedProject.RootFolder);

                            Assert.AreEqual(2, retrievedProject.RootFolder.Models.Count());
                        };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, onMainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void SaveLoadHydroModelWithDFlowFMandRTC()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = Path.Combine("flowRtcCoupling","fmRtc.dsproj");

                Action onMainWindowShown =
                    delegate
                    {
                        app.SaveProject();

                        var builder = new HydroModelBuilder();
                        var project = app.Project;
                        var hydroModel = builder.BuildModel(ModelGroup.All);

                        hydroModel.Activities.RemoveAllWhere(a => !(a is WaterFlowFMModel|| a is RealTimeControlModel));
                        project.RootFolder.Add(hydroModel);

                        var flowfm = hydroModel.Models.OfType<WaterFlowFMModel>().First();
                        var flowFeature = new GroupableFeature2DPoint { Name = "flowFeature"};
                        flowfm.Area.ObservationPoints.Add(flowFeature);
                        var pump = new Pump2D(false);
                        pump.Geometry =
                            new LineString(new[]
                            {new Coordinate(0, 0), new Coordinate(1.0, 1.0), new Coordinate(123.0, -4.3)});
                        flowfm.Area.Pumps.Add(pump);

                        var rtc = hydroModel.Models.OfType<RealTimeControlModel>().First();
                        var input = new Input();
                        var output = new Output();
                        var controlGroup = new ControlGroup {Inputs = {input}, Outputs = {output}};
                        rtc.ControlGroups.Add(controlGroup);

                        var pumpDataItem =
                            flowfm.GetChildDataItems(pump)
                                .First(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
                        pumpDataItem.LinkTo(rtc.GetDataItemByValue(output));

                        var featureDataItem =
                            flowfm.GetChildDataItems(flowFeature)
                                .First(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
                        var rtcInputDataItem = rtc.GetDataItemByValue(input);
                        rtcInputDataItem.LinkTo(featureDataItem);

                        app.SaveProjectAs(path);

                        // check links
                        Assert.AreEqual(pumpDataItem.LinkedTo, rtc.GetDataItemByValue(output));
                        Assert.AreEqual(rtc.GetDataItemByValue(input).LinkedTo, featureDataItem);

                        app.CloseProject();
                        

                        // check file
                        Assert.IsTrue(File.Exists(hydroModel.CouplingFilePath));

                        app.OpenProject(path);

                        // confirm links
                        var loadedHydroModel = app.Project.RootFolder.Items.OfType<HydroModel>().First();
                        var loadedFlowFMModel = loadedHydroModel.Models.OfType<WaterFlowFMModel>().First();
                        var loadedRTCModel = loadedHydroModel.Models.OfType<RealTimeControlModel>().First();

                        var loadedPump = loadedFlowFMModel.Area.Pumps.First();
                        var dataItems = loadedFlowFMModel.GetChildDataItems(loadedPump);

                        var loadedPumpDataItem = dataItems.First(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);

                        var loadedFeature = loadedFlowFMModel.Area.ObservationPoints.First();
                        var loadedFeatureDataItem = loadedFlowFMModel.GetChildDataItems(loadedFeature)
                            .First(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);

                        var loadedInput = loadedRTCModel.ControlGroups.First().Inputs.First();
                        var loadedOutput = loadedRTCModel.ControlGroups.First().Outputs.First();

                        Assert.AreEqual(loadedPumpDataItem.LinkedTo, loadedRTCModel.GetDataItemByValue(loadedOutput));
                        Assert.AreEqual(loadedRTCModel.GetDataItemByValue(loadedInput).LinkedTo, loadedFeatureDataItem);
                    };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, onMainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void SaveLoadHydroModelWithDFlowFMandFlow1D()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = Path.Combine("floodingCoupling", "fmFlow1D.dsproj");

                Action onMainWindowShown =
                    delegate
                    {
                        app.SaveProject();

                        var builder = new HydroModelBuilder();
                        var project = app.Project;
                        var hydroModel = builder.BuildModel(ModelGroup.All);
                        hydroModel.Activities.RemoveAllWhere(
                            a => a is RealTimeControlModel || a is RainfallRunoffModel);
                        project.RootFolder.Add(hydroModel);
                        
                        app.SaveProject();
                        
                        var fmModel = hydroModel.Models.OfType<WaterFlowFMModel>().First();
                        var flow1DModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
                        
                        // add embankment
                        fmModel.Area.Embankments.Add(new Embankment {Region = fmModel.Area});

                        // add network with lateral
                        flow1DModel.Network = new HydroNetwork();
                        var node1 = new HydroNode("node1");
                        var node2 = new HydroNode("node2");
                        var node3 = new HydroNode("node3");
                        flow1DModel.Network.Nodes.Add(node1);
                        flow1DModel.Network.Nodes.Add(node2);
                        flow1DModel.Network.Nodes.Add(node3);
                        var branch1 = new Channel("branch1", node1, node2)
                        {
                            Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
                        };
                        var branch2 = new Channel("branch2", node2, node3)
                        {
                            Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 100 200)")
                        };
                        flow1DModel.Network.Branches.Add(branch1);
                        flow1DModel.Network.Branches.Add(branch2);
                        var lateral = new LateralSource();
                        NetworkHelper.AddBranchFeatureToBranch(lateral, branch1, 50.0);

                        // link them
                        fmModel.Area.Embankments[0].LinkTo(lateral);

                        app.SaveProjectAs(path);
                        app.CloseProject();
                        app.OpenProject(path);

                        var loadedHydroModel = app.Project.RootFolder.Items.OfType<HydroModel>().First();
                        var loadedFlowFMModel = loadedHydroModel.Models.OfType<WaterFlowFMModel>().First();
                        var loadedFlow1DModel = loadedHydroModel.Models.OfType<WaterFlowModel1D>().First();

                        Assert.IsNotNull(loadedFlow1DModel);
                        Assert.IsNotNull(loadedFlowFMModel);

                        // assert area linkage
                        Assert.AreEqual(loadedFlowFMModel.GetDataItemByValue(loadedFlowFMModel.Area).LinkedTo,
                            loadedHydroModel.GetDataItemByValue(loadedHydroModel.Region.SubRegions.OfType<HydroArea>().First()));

                        // confirm HydroLink
                        var loadedEmbankment = loadedFlowFMModel.Area.Embankments[0];
                        var loadedLateral = loadedFlow1DModel.Network.LateralSources.First();
                        Assert.IsNotNull(loadedEmbankment);
                        Assert.IsNotNull(loadedLateral);
                        Assert.AreEqual(loadedEmbankment.Links[0].Target, loadedLateral);
                    };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, onMainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void SaveHydroModelAndCopyDeletePasteTools8969()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = "eiwe.dsproj";

                Action onMainWindowShown =
                    delegate
                    {
                        var project = app.Project;

                        var hydroModel = ModelTestHelper.GetHydroModelForSobek();
                        project.RootFolder.Add(hydroModel);

                        app.SaveProjectAs(path);
                        var retrievedProject = app.Project;
                        var retrievedHydro = retrievedProject.RootFolder.Models.First();

                        gui.CopyPasteHandler.Copy(retrievedHydro);
                        gui.CommandHandler.DeleteProjectItem(retrievedHydro);
                        gui.CopyPasteHandler.Paste(retrievedProject, retrievedProject.RootFolder);

                        Assert.AreEqual(1, retrievedProject.RootFolder.Models.Count());
                    };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, onMainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadHydroModelCheckSubRegionOwner()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = "slhmcsro.dsproj";

                var hydroModel = ModelTestHelper.GetHydroModelForSobek();
                app.Project.RootFolder.Add(hydroModel);

                app.SaveProjectAs(path);
                app.CloseProject();
                app.OpenProject(path);

                var retrievedProject = app.Project;
                var retrievedHydroModel = (IModel)retrievedProject.RootFolder.Items[0];

                var retrievedNetworkDataItem = retrievedHydroModel.AllDataItems.First(di => di.Value is IHydroNetwork);
                Assert.IsNotNull(retrievedNetworkDataItem.Owner, "child data item owner not set");

                var retrievedRegionDataItem = retrievedHydroModel.DataItems.First(di => di.Value is HydroRegion);
                Assert.IsNotNull(retrievedRegionDataItem.Owner, "data item owner not set");
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void SaveHydroModelWithMapViewContext()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = TestHelper.GetCurrentMethodName() + ".dsproj";

                Action onMainWindowShown =
                    delegate
                    {
                        // add complex hydro model
                        var hydroModel = ModelTestHelper.GetHydroModelForSobek();
                        gui.Application.Project.RootFolder.Add(hydroModel);

                        // open region view
                        gui.CommandHandler.OpenView(hydroModel.Region);

                        // save project 
                        gui.Application.SaveProjectAs(path);

                        // load project
                        gui.Application.OpenProject(path);

                        // assert we got a model again, with submodels

                        var retrievedHydroModel = (HydroModel)gui.Application.Project.RootFolder.Models.First();
                        Assert.AreEqual(3, retrievedHydroModel.Activities.Count);
                    };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, onMainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void SaveHydroModelWithMapAndCoordinateSystem()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());
                gui.Run();

                var path = TestHelper.GetCurrentMethodName() + ".dsproj";

                Action onMainWindowShown = delegate
                        {
                            // add complex hydro model
                            var hydroModel = ModelTestHelper.GetHydroModelForSobek();
                            gui.Application.Project.RootFolder.Add(hydroModel);

                            // open region view
                            gui.CommandHandler.OpenView(hydroModel.Region);

                            // close all views (view context will be generated)
                            gui.DocumentViews.Clear();

                            // assign coordinate system
                            var contextManager = (IGuiContextManager)gui.Application.Project.ViewContextManager;
                            var context = (ProjectItemMapViewContext)contextManager.GetViewContext(typeof (ProjectItemMapView), hydroModel);

                            var shapefileLayer = new VectorLayer { DataSource = new ShapeFile { Path = @"..\..\..\..\..\..\test-data\DeltaShell\DeltaShell.Plugins.SharpMapGis.Tests\rivers.shp" } };
                            context.Map.Layers.Add(shapefileLayer);

                            // save project 
                            gui.Application.SaveProjectAs(path);

                            // load project
                            gui.Application.OpenProject(path);

                            // open view does not generate exception
                            var retrievedHydroModel = (HydroModel) gui.Application.Project.RootFolder.Models.First();
                            gui.CommandHandler.OpenView(retrievedHydroModel.Region);
                        };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, onMainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.WorkInProgress)]
        public void ExportWaterFlowSubModel()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = TestHelper.GetCurrentMethodName() + ".dsproj";

                Action onMainWindowShown =
                    delegate
                        {
                            var builder = new HydroModelBuilder();

                            // add hydro model
                            var hydroModel = builder.BuildModel(ModelGroup.All);
                            gui.Application.Project.RootFolder.Add(hydroModel);

                            var waterFlowModel1D = hydroModel.Models.OfType<WaterFlowModel1D>().First();
                            
                            // Fill hydroregion network
                            var network = waterFlowModel1D.Network;
                            var node1 = new HydroNode("node1"){Network = network};
                            var node2 = new HydroNode("node2"){Network = network};
                            network.Nodes.AddRange(new [] {node1,node2});
                            var branch = new Channel() {Name = "branch", Source = node1, Target = node2, Network = network};
                            network.Branches.Add(branch);
                            hydroModel.Region.SubRegions.Add(network);

                            waterFlowModel1D.Network = network;

                            // !!!!!!!!!!
                            Assert.Fail("Exporting submodel is not supported yet, flow 1d uses network which is linked from hydro model so simple clone does not work (network becomes disconnected/empty). Dedicated clone is required which takes copies of linked data items.");

                            gui.Application.ExportProjectItem(waterFlowModel1D, path, true);

                            // load project
                            gui.Application.OpenProject(path);

                            // assert we got a model again, with submodels
                            var retrievedWaterFlowModel1D = (WaterFlowModel1D)gui.Application.Project.RootFolder.Models.First();
                            Assert.AreEqual(1, retrievedWaterFlowModel1D.Network.Branches.Count);
                        };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, onMainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void Import_Run_SaveAndLoad_DWAQ_AC1_Tools8400()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = TestHelper.GetCurrentMethodName() + ".dsproj";

                var builder = new HydroModelBuilder();
                var hydroModel = builder.BuildModel(ModelGroup.All); //sobek only

                var modelImporter = new SobekHydroModelImporter();
                string pathToSobekNetwork =
                    TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                               @"DWAQ_AC1\DWAQ_AC1.lit\37\network.tp");
                modelImporter.PathSobek = pathToSobekNetwork;
                modelImporter.useFlow = true;
                modelImporter.useRTC = true;
                modelImporter.useRR = true;
                modelImporter.TargetObject = hydroModel;
                modelImporter.Import();

                var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();

                // fill missing(?) evap data
                new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries) rr.Evaporation.Data, rr.StartTime,
                                                             rr.StopTime,
                                                             new TimeSpan(1, 0, 0));

                gui.Application.Project.RootFolder.Add(hydroModel);

                hydroModel.Initialize();

                hydroModel.Execute();
                hydroModel.Cleanup();

                Assert.IsFalse(hydroModel.OutputIsEmpty);

                // save project 
                gui.Application.SaveProjectAs(path);

                // load project
                gui.Application.OpenProject(path);

                // assert we got a model again, with submodels
                var retrievedHydroModel = (HydroModel) gui.Application.Project.RootFolder.Models.First();
                Assert.AreEqual(4, retrievedHydroModel.Models.Count());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void OpenProjectAndCentralMapShouldNotCauseInefficientLazyLoadingTools19994()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());
                gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
                gui.Plugins.Add(new RainfallRunoffGuiPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());
                gui.Run();

                var path = TestHelper.GetTestFilePath("LazyLoadPerformance\\ap_perf.dsproj");

                // Note: is this test failing? perhaps the project is now going through migration?
                //       consider updating the test-data such that the project no longer migrates

                // 21+5sec on my pc (was: 19+152sec)
                Action onMainWindowShown = () => TestHelper.AssertIsFasterThan(125000, () =>
                    {
                        // open project:
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        app.OpenProject(path);
                        stopwatch.Stop();

                        Console.WriteLine("Opening project took: {0}ms", stopwatch.ElapsedMilliseconds);

                        // open central map:
                        stopwatch.Restart();
                        var hydroModel = (IModel) app.Project.RootFolder.Items[0];
                        gui.CommandHandler.OpenView(hydroModel, typeof (ProjectItemMapView));
                        stopwatch.Stop();

                        Console.WriteLine("Opening central map view (=lazy loading) took: {0}ms",
                                          stopwatch.ElapsedMilliseconds);
                    });
                WpfTestHelper.ShowModal((Control)gui.MainWindow, onMainWindowShown);
            }
        }
        
        [Test]
        [Category(TestCategory.WorkInProgress)] // TOOLS-20423
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WindowsForms)]
        public void SaveLoadHydroModelRTCModelControlGroupHasLookupSignal()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());

                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());
                gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());
                gui.Plugins.Add(new RainfallRunoffGuiPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = TestHelper.GetCurrentMethodName() + ".dsproj";

                Action onMainWindowShown =
                    delegate
                    {
                        // add complex hydro model
                        var hydroModel = ModelTestHelper.GetHydroModelForSobek();
                        gui.Application.Project.RootFolder.Add(hydroModel);

                        // open region view
                        gui.CommandHandler.OpenView(hydroModel.Region);

                        // add control group, add lookupsignal to it
                        var rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().First();
                        var controlGroup = RealTimeControlTestHelper.CreateControlGroupWithLookupSignalAndPIDRule();
                        rtcModel.ControlGroups.Add(controlGroup);

                        var model = (HydroModel)gui.Application.Project.RootFolder.Models.First();
                        rtcModel = model.Models.OfType<RealTimeControlModel>().First();
                        gui.CommandHandler.OpenView(rtcModel.ControlGroups.First());

                        // close all views (view context will be generated)
                        gui.DocumentViews.Clear();

                        // assign coordinate system
                        var contextManager = (IGuiContextManager)gui.Application.Project.ViewContextManager;
                        var context = (ProjectItemMapViewContext)contextManager.GetViewContext(typeof(ProjectItemMapView), hydroModel);

                        // save project 
                        gui.Application.SaveProjectAs(path);

                        // close project
                        gui.Application.CloseProject();

                        // load project
                        gui.Application.OpenProject(path);

                        // open control group view
                        var retrievedHydroModel = (HydroModel)gui.Application.Project.RootFolder.Models.First();
                        var retrievedRtcModel = retrievedHydroModel.Models.OfType<RealTimeControlModel>().First();
                        gui.CommandHandler.OpenView(retrievedRtcModel.ControlGroups.First());

                        //check that the lookupsignal has effect
                    };
                WpfTestHelper.ShowModal((Control)gui.MainWindow, onMainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ExportIntegratedModelWithRainfallRunoffSetsBoundaryConditionCorrectly()
        {
            var path = TestHelper.GetTestFilePath("SOBEK3-1313\\Flow1D_RR_IntegratedModel.dsproj");
            path = TestHelper.CreateLocalCopy(path);
            Assert.IsNotNull(path);
            Assert.IsTrue(File.Exists(path));

            var randomPath = Path.Combine(Path.GetDirectoryName(path), Path.GetRandomFileName());
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                app.Run();

                app.OpenProject(path);
                var integratedModel = app.GetAllModelsInProject().ToList();
                Assert.IsTrue(integratedModel.Any());

                var rrModel = integratedModel.OfType<RainfallRunoffModel>().FirstOrDefault();
                Assert.IsNotNull(rrModel);

                //rrModel is not valid because misses a hydrolink
                var validator = new RainfallRunoffModelValidator();
                var report = validator.Validate(rrModel);

                Assert.IsFalse(report.AllErrors.Any());

                //Create missing link
                var exporter = new RainfallRunoffModelExporter();
                exporter.Export(rrModel, randomPath);

                var bcFilePath = Path.Combine(randomPath, "BoundaryConditions.bc");
                Assert.IsTrue(File.Exists(bcFilePath));

                //find boundary conditions.
                var reader = new DelftBcReader();
                var bcFile = reader.ReadDelftBcFile(bcFilePath);
                //Check only one runoff boundary appears in the file.
                Assert.IsTrue(bcFile
                    .Any(c => c.Name.Equals("Boundary")
                              && c.Properties.Any(
                                  p => p.Name.Equals("name")
                                       && p.Value.Equals("RunoffBoundary1"))));
                Assert.AreEqual(1,
                    bcFile
                    .Count(c => c.Name.Equals("Boundary")
                              && c.Properties.Any(
                                  p => p.Name.Equals("name")
                                       && p.Value.Equals("RunoffBoundary1"))));
            }

            FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
        }
    }
}