using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.ImportExport.Gwsw;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.Scripting.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.Toolbox;
using DeltaShell.Plugins.Toolbox.Gui;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class DummyTest_Generating_1D2DFmFiles_ToBeTested_By_JN
    {
        [Test]
        [Ignore("A dummy test to generate UGridFile for TKI project")]
        public void Generate_Files()
        {
            
            var pathRead = TestHelper.GetTestFilePath(@"alex\NetworkDefinition.ini");
            var pathWrite = TestHelper.GetTestFilePath(@"alex\output_net.nc"); ;
            var network = new HydroNetwork();
            var discretization = new Discretization { Network = network};
            var discretisationDataModel = new NetworkDiscretisationUGridDataModel(discretization);
            var networkDataModel = new NetworkUGridDataModel(network);
            NetworkAndGridReader.ReadFile(pathRead, network, discretization);

            var locations = discretization.Locations.Values.ToList();
            var branches = network.Branches.ToList();

            foreach (var branch in branches)
            {
                var locationsOnBranch = locations.Where(l => l.Branch == branch).ToList();
                Console.Write("Branch " + branch.Name + "(" + branch.Length.ToString("N3") + "): [" + locationsOnBranch.Min(l => l.Chainage) + ", " + locationsOnBranch.Max(l => l.Chainage) + "] ");
            }

            UGridToNetworkAdapter.SaveNetwork(pathWrite, networkDataModel, new UGridGlobalMetaData("Test model", "Generated from a script", "Alex"));
            UGridToNetworkAdapter.SaveNetworkDiscretisation(pathWrite, discretisationDataModel);
        }

        [Test]
        [Ignore("A dummy test to generate UGridFile for TKI project")]
        [Category(TestCategory.WindowsForms)]
        public void Show_Generate_File()
        {
            var path = TestHelper.GetTestFilePath(@"alex\output_net.nc");
            var network = NetworkDiscretisationFactory.CreateHydroNetwork(UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(path));

            var mapView = new MapView();
            mapView.Map.Layers.Add(MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() }));


            while (mapView.MapControl.IsProcessing)
            {
                Application.DoEvents();
            }
            WindowsFormsTestHelper.ShowModal(mapView);
        }

        [TestCase("YourNetCdfFilePathGoesHere")]
        [Category(TestCategory.WindowsForms)]
        public void LoadNetworkAndNetworkDiscretizationAndShowInGui(string netFilePath)
        {
            if(!File.Exists(netFilePath)) return; // Only trigger this test when an existing file path has been entered as test argument.

            using (var gui = new DeltaShellGui())
            {
                //load the plugins
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new WaveGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());
                gui.Plugins.Add(new ScriptingGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new ToolboxGuiPlugin());

                var app = gui.Application;
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var path = TestHelper.GetTestFilePath(netFilePath);
                    var fmModel = new WaterFlowFMModel {NetworkDiscretization = UGridToNetworkAdapter.LoadNetworkAndDiscretisation(path)};
                    fmModel.Network = (IHydroNetwork) fmModel.NetworkDiscretization.Network;

                    var project = app.Project;
                    project.RootFolder.Add(fmModel);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        public void Waardenburg_ImportFromGwsw_ThenExportModel()
        {
            var pathRead = TestHelper.GetTestFilePath(@"alex\Waardenburg\");
            var pathWrite = Path.Combine(FileUtils.CreateTempDirectory(), "Waardenburg.mdu");
            var model = new WaterFlowFMModel {MduFilePath = pathWrite};

            var gwswImporter = new GwswFileImporter {CsvDelimeter = ';'};
            gwswImporter.LoadFeatureFiles(Path.GetDirectoryName(pathRead));
            gwswImporter.ImportItem(null, model);

            var sewerConnections = model.Network.SewerConnections;
            var discretizationLocations = model.NetworkDiscretization.Locations.Values;
            var errorMessages = new List<string>();
            foreach (var sewerConnection in sewerConnections)
            {
                var numberOfCalculationPointsForSewerConnection = discretizationLocations.Count(dl => dl.Branch.Equals(sewerConnection));
                if (numberOfCalculationPointsForSewerConnection != 2)
                    errorMessages.Add($"There are {numberOfCalculationPointsForSewerConnection} calculation points for sewer connection {sewerConnection.Name}. There should be 2 of them!");
            }

            Assert.IsEmpty(errorMessages, string.Join("\r\n", errorMessages));
            
            //write
            WaterFlowFMModelWriter.Write(pathWrite, model);
        }

        [Test]
        public void GenerateFMFiles_DeWaal()
        {
            var pathRead = TestHelper.GetTestFilePath(@"alex\Waal\waal-model.md1d");
            var pathWrite = Path.Combine(FileUtils.CreateTempDirectory(), "Waal.mdu");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(pathRead);
            var model = new WaterFlowFMModel { MduFilePath = pathWrite };
            model.Network = waterFlowModel1D.Network;
            model.NetworkDiscretization = waterFlowModel1D.NetworkDiscretization;
            waterFlowModel1D.RoughnessSections.ForEach(rs => model.RoughnessSections.Add(rs));

            //write
            WaterFlowFMModelWriter.Write(pathWrite, model);
        }
    }
}
