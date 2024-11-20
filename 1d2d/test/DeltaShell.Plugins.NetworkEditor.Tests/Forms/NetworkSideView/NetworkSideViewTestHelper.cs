using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    public static class NetworkSideViewTestHelper
    {
        public static void AddCrossSection(IChannel branch1, double offset, double depth)
        {
            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(100.0, 0.0),
                                        new Coordinate(150.0, depth),
                                        new Coordinate(300.0, depth),
                                        new Coordinate(350.0, 0.0),
                                        new Coordinate(500.0, 0.0)
                                    };

            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, offset, yzCoordinates, "crs1");
        }

        public static NetworkSideViewDataController CreateDefaultViewData()
        {
            // create network
            HydroNetwork hydroNetwork = GetDefaultHydroNetwork();
            return CreateDefaultViewData(hydroNetwork);
        }
        
        public static HydroNetwork GetDefaultHydroNetwork()
        {
            var hydroNetwork = new HydroNetwork();

            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node2");
            var node3 = new HydroNode("node3");

            hydroNetwork.Nodes.Add(node1);
            hydroNetwork.Nodes.Add(node2);
            hydroNetwork.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2) { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)") };
            var branch2 = new Channel("branch2", node2, node3) { Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 300 0)") };

            hydroNetwork.Branches.Add(branch1);
            hydroNetwork.Branches.Add(branch2);

            List<Coordinate> yzCoordinates = new List<Coordinate>
                                                  {
                                                      new Coordinate(0.0, 18.0),
                                                      new Coordinate(100.0, 18.0),
                                                      new Coordinate(150.0, 10.0),
                                                      new Coordinate(300.0, 10.0),
                                                      new Coordinate(350.0, 18.0),
                                                      new Coordinate(500.0, 18.0)
                                                  };
            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, 20.0, yzCoordinates);
            
            yzCoordinates = new List<Coordinate>
                                {
                                    new Coordinate(0.0, 14.0),
                                    new Coordinate(100.0, 14.0),
                                    new Coordinate(150.0, 8.0),
                                    new Coordinate(300.0, 8.0),
                                    new Coordinate(350.0, 14.0),
                                    new Coordinate(500.0, 14.0)
                                };
            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch2, 20.0, yzCoordinates);
            
            return hydroNetwork;
        }

        public static NetworkSideViewDataController CreateDefaultViewData(IHydroNetwork hydroNetwork)
        {
            var waterLevel = new NetworkCoverage("Water level",true) { Network = hydroNetwork };
            waterLevel[new DateTime(2000,1,1),new NetworkLocation(hydroNetwork.Branches[0], 0.0)] = 18.0;
            waterLevel[new DateTime(2000, 1, 1), new NetworkLocation(hydroNetwork.Branches[0], 20.0)] = 16.0;
            waterLevel[new DateTime(2000, 1, 1), new NetworkLocation(hydroNetwork.Branches[1], 50.0)] = 11.0;
            waterLevel.Arguments[1].Name = "x";
            waterLevel.Components[0].Name = "y";
            waterLevel.Name = NetworkSideViewDataController.WaterLevelCoverageNameInMapFile;
            
            return CreateDefaultViewData(hydroNetwork, waterLevel);
        }

        public static NetworkSideViewDataController CreateDefaultViewData(IHydroNetwork hydroNetwork, INetworkCoverage waterLevel)
        {
            // create route
            var route = new Route { Network = hydroNetwork, SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations };
            route[new NetworkLocation(hydroNetwork.Branches[0], 10.0)] = 1.0;
            route[new NetworkLocation(hydroNetwork.Branches[1], 90.0)] = 3.0;
            route.Components[0].Unit = new Unit("meters", "m");
            // create water level network coverage
            return new NetworkSideViewDataController(route, new NetworkSideViewCoverageManager(route, null, new[] { waterLevel }), modelNameDelegate);
        }

        public static NetworkSideViewDataController.ModelNameForCoverageDelegate modelNameDelegate;

        public static NetworkSideViewDataController CreateDefaultViewDataWithWeir(IHydroNetwork hydroNetwork)
        {
            // create route
            var route = new Route { Network = hydroNetwork, SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations };
            route[new NetworkLocation(hydroNetwork.Branches[0], 10.0)] = 1.0;
            route[new NetworkLocation(hydroNetwork.Branches[1], 90.0)] = 3.0;
            route.Components[0].Unit = new Unit("meters", "m");

            var timeDependendWaterLevelCoverage = CreateTimeDependendWaterLevelCoverage(hydroNetwork, 20);
            var timeValues = timeDependendWaterLevelCoverage.Time.Values;
            
            var weir = new Weir("weir1") { OffsetY = 170 };
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, hydroNetwork.Branches[0], 50);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

            // todo: add feature coverage drawer into sideview
            // todo: add line above weir to see crest level changes

            var featureCoverage = new FeatureCoverage("crest level");// {IsTimeDependent = true};
            var timeVariable = new Variable<DateTime>("time");
            featureCoverage.Arguments.Add(timeVariable);
            featureCoverage.Arguments.Add(new Variable<IFeature>("feature"));
            featureCoverage.Components.Add(new Variable<double>("value"));
            featureCoverage.Features = new EventedList<IFeature>(new [] {weir});
            featureCoverage.FeatureVariable.SetValues(new []{weir});            

            const double delta = 0.4;
            double level = 1.0;
            foreach (var timeValue in timeValues)
            {
                featureCoverage.SetValues(new[] { level }, new VariableValueFilter<DateTime>(timeVariable, timeValue));
                level += delta;
            }

            return new NetworkSideViewDataController(route,
                                            new NetworkSideViewCoverageManager(route, null,
                                                                            new ICoverage[]
                                                                                {
                                                                                    timeDependendWaterLevelCoverage, 
                                                                                    featureCoverage
                                                                                }));
        }

        public static INetworkCoverage GetTimeDependendNetworkCoverageOnHydroNetwork(int timeSlices, int numberOfBranches, bool storeInNetCdf, string path)
        {
            //create a network with one horizontal branch
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(numberOfBranches,storeInNetCdf);
            var networkCoverage = new NetworkCoverage("test", true);

            if (storeInNetCdf)
            {
                var store = new NetCdfFunctionStore();
                store.CreateNew(path);
                store.Functions.Add(networkCoverage);
                networkCoverage.Network = network;
            }

            networkCoverage.Locations.FixedSize = 9 * numberOfBranches;
            var locations = new List<INetworkLocation>();
            for (int i = 0; i < network.Branches.Count; i++)
            {
                //9 locations per branch
                locations.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                                       .Select(j => (INetworkLocation)new NetworkLocation(network.Branches[i], j * 10)));
            }
            networkCoverage.SetLocations(locations);

            //5 timesteps set values
            var startTime = new DateTime(2000, 1, 1);
            //1..900
            var values = Enumerable.Range(1, 9 * numberOfBranches);
            var locationsFilter = new VariableIndexRangeFilter(networkCoverage.Locations, 0, (9 * numberOfBranches) - 1);
            foreach (int i in Enumerable.Range(1, timeSlices))
            {
                networkCoverage.Time.AddValues(new[] { startTime.AddMinutes(i) });


                var timeFilter = new VariableIndexRangeFilter(
                    networkCoverage.Time, i - 1, i - 1);

                networkCoverage.SetValues(values.Select(j => (double)(j * i)),
                                          new[] { locationsFilter, timeFilter });
                /*networkCoverage.AddValuesForTime(values.Select(j => j*i),
                                                 startTime.AddMinutes(i));*/
            }
            return networkCoverage;
        }

        public static INetworkCoverage CreateTimeDependendWaterLevelCoverage(INetwork network,int timeSteps)
        {
            var times = new List<DateTime>();
            var time = DateTime.Now;
            times.Add(time);
            for (int i = 1; i < timeSteps; i++)
            {
                times.Add(time.AddSeconds(i));    
            }

            var waterLevel = new NetworkCoverage {Network = network, IsTimeDependent = true};
            for (int i = 0; i < times.Count; i++)
            {
                waterLevel[times[i], new NetworkLocation(network.Branches[0], 0.0)] = 18.0 + i%20;
                waterLevel[times[i], new NetworkLocation(network.Branches[0], 20.0)] = 16.0 + i%10;
                waterLevel[times[i], new NetworkLocation(network.Branches[1], 50.0)] = 11.0 + i%5;
            }

            waterLevel.Arguments[0].Name = "t";
            waterLevel.Arguments[1].Name = "x";
            waterLevel.Components[0].Name = "y";
            waterLevel.Name = NetworkSideViewDataController.WaterLevelCoverageNameInMapFile;

            return waterLevel;
        }

        public static NetworkCoverage GetCoverageBasedOnOtherCoverage(INetworkCoverage sourceCoverage, double initialComponentValue, double componentDelta, Unit unit, string coverageName)
        {
            var addedNetworkCoverage = new NetworkCoverage {Network = sourceCoverage.Network};

            foreach (var location in sourceCoverage.Locations.Values)
            {
                addedNetworkCoverage[new NetworkLocation(location.Branch, location.Chainage)] = initialComponentValue;
                initialComponentValue += componentDelta;
            }
            //some dummy unit to get the right axis going
            addedNetworkCoverage.Components[0].Unit = unit;
            addedNetworkCoverage.Name = coverageName;
            return addedNetworkCoverage;
        }
        public static void ShowInSideView(IBranchFeature branchFeature)
        {
            var hydroNetwork = GetDefaultHydroNetwork();
            var viewData = CreateDefaultViewData(hydroNetwork, null);
            var network = viewData.Network;

            //a composite structure with a weir in it
            NetworkHelper.AddBranchFeatureToBranch(branchFeature, network.Branches[0], 50);
            
            ShowSideViewFor(viewData, branchFeature);
        }

        private static void ShowSideViewFor(NetworkSideViewDataController viewData, IBranchFeature branchFeature)
        {
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                   Data = viewData.NetworkRoute,
                                   DataController = viewData,
                                   SelectedFeature = branchFeature
                               };

            WindowsFormsTestHelper.ShowModal(sideView);
        }

        public static void ShowInSideView(IStructure1D weir)
        {
            var hydroNetwork = GetDefaultHydroNetwork();
            var viewData = CreateDefaultViewData(hydroNetwork,null);
            var network = viewData.Network;

            //a composite structure with a weir in it
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

            ShowSideViewFor(viewData,weir);
        }

        public static void ShowBranchFeatureInSideView(IBranchFeature feature)
        {
            var hydroNetwork = GetDefaultHydroNetwork();
            var viewData = CreateDefaultViewData(hydroNetwork, null);
            var network = viewData.Network;

            NetworkHelper.AddBranchFeatureToBranch(feature, network.Branches[0], 50);

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                   Data = viewData.NetworkRoute,
                                   DataController = viewData
                               };

            WindowsFormsTestHelper.ShowModal(sideView);
        }
    }
}