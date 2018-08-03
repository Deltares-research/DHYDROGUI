using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class DummyTest_Alex
    {
        [Test]
        [Ignore("A dummy test to generate UGridFile for TKI project")]
        public void Generate_Files()
        {
            
            var pathRead = TestHelper.GetTestFilePath(@"alex\NetworkDefinition.ini");
            var pathWrite = TestHelper.GetTestFilePath(@"alex\output_net.nc"); ;
            var network = new HydroNetwork();
            var discretization = new Discretization { Network = network};
            NetworkAndGridReader.ReadFile(pathRead, network, discretization);

            var locations = discretization.Locations.Values.ToList();
            var branches = network.Branches.ToList();

            foreach (var branch in branches)
            {
                var locationsOnBranch = locations.Where(l => l.Branch == branch).ToList();
                Console.Write("Branch " + branch.Name + "(" + branch.Length.ToString("N3") + "): [" + locationsOnBranch.Min(l => l.Chainage) + ", " + locationsOnBranch.Max(l => l.Chainage) + "] ");
            }


            UGridToNetworkAdapter.SaveNetwork(network, pathWrite, new UGridGlobalMetaData("Test model", "Generated from a script","Alex"));
            UGridToNetworkAdapter.SaveNetworkDiscretisation(discretization, pathWrite);
        }

        [Test]
        [Ignore("A dummy test to generate UGridFile for TKI project")]
        [Category(TestCategory.WindowsForms)]
        public void Show_Generate_File()
        {
            var path = TestHelper.GetTestFilePath(@"alex\output_net.nc");
            var network = UGridToNetworkAdapter.LoadNetwork(path);

            var mapView = new MapView();
            mapView.Map.Layers.Add(MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() }));


            while (mapView.MapControl.IsProcessing)
            {
                Application.DoEvents();
            }
            WindowsFormsTestHelper.ShowModal(mapView);


        }
    }
}
