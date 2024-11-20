using System;
using System.Linq;
using System.Threading;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Forms
{
    public class NetworkCoverageFunctionViewTest 
    {
        [Test, Category(TestCategory.WindowsForms), Apartment(ApartmentState.STA)]
        public void NetworkCoverageWithNoDataValues()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);
            var networkCoverage = new NetworkCoverage("cov", true) {Network = network};

            foreach (var time in Enumerable.Range(1, 10).Select(i => new DateTime(2000 + i,1,1)))
            {
                foreach (var branch in network.Branches)
                {
                    for (int ch = 0; ch < 100; ch += 10)
                    {
                        networkCoverage[time, new NetworkLocation(branch, ch)] = (double)time.Year + ch;
                    }
                }
            }

            var loc1 = new NetworkLocation(network.Branches[0], 0);
            networkCoverage.Components[0].NoDataValue = double.NaN;
            networkCoverage[networkCoverage.Time.Values[1], loc1] = double.NaN;
            var timeSeries = networkCoverage.GetTimeSeries(loc1);

            var functionView = new FunctionView { Data = timeSeries, ChartViewOption = ChartViewOptions.AllSeries };
            
            WindowsFormsTestHelper.ShowModal(functionView);
        }

        [Test]
        [Category(TestCategory.WindowsForms), Apartment(ApartmentState.STA)]
        public void ShowNetworkCoverageUsingGridView()
        {
            // create network
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node2");
            var node3 = new HydroNode("node3");
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2) { Geometry = new WKTReader().Read("LINESTRING (0 0, 100 0)") };
            var branch2 = new Channel("branch2", node1, node2) { Geometry = new WKTReader().Read("LINESTRING (0 10, 100 10)") };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            INetworkCoverage networkCoverage = new NetworkCoverage { Network = network };

            // set values
            networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;
            networkCoverage[new NetworkLocation(branch1, 100.0)] = 0.2;
            networkCoverage[new NetworkLocation(branch2, 0.0)] = 0.3;
            networkCoverage[new NetworkLocation(branch2, 50.0)] = 0.4;
            networkCoverage[new NetworkLocation(branch2, 200.0)] = 0.5;

            var gridView = new FunctionView { Data = networkCoverage };

            WindowsFormsTestHelper.ShowModal(gridView);
        }
    }
}