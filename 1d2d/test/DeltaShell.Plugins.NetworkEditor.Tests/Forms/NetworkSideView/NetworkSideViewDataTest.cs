using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class NetworkSideViewDataTest
    {
        [Test]
        public void SideViewDataContainsBranchFeaturesForTheRoute()
        {
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData();
            var network = data.Network;

            //action! add a pump
            var pump = new Pump("pump1") { OffsetY = 150 ,StopDelivery = 35};
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            //assert Ymax and branchfeatures were update
            Assert.IsTrue(data.ActiveBranchFeatures.Contains(((IChannel)network.Branches[0]).CrossSections.ToArray()[0]));
            Assert.IsTrue(data.ActiveBranchFeatures.Contains(((IChannel)network.Branches[1]).CrossSections.ToArray()[0]));
            Assert.IsTrue(data.ActiveBranchFeatures.Contains(compositeBranchStructure));
            Assert.AreEqual(35, data.ZMaxValue);
        }

        [Test]
        public void MinMaxValueIsUpdatedForCrossections()
        {
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData();
            var network = data.Network;
            //add a real hight and a real low point
            var crossSection = ((HydroNetwork) network).CrossSections.First();

            crossSection.Geometry.Coordinates[0].Z = 1000;
            crossSection.Geometry.Coordinates[1].Z = -1000;

            Assert.AreEqual(-1000,data.ZMinValue);
            Assert.AreEqual(1000, data.ZMaxValue);
        }
        
        [Test]
        public void MinMaxZForWeirWithoutCrossSections()
        {
            //if no crossection are defined, no bed level can be calculated.
            //a minimum of weir.CrestLevel - 10 is used then.
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData(false);
            var network = data.Network;

            //add a composite structure
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);

            //action! add a weir
            var weir = new Weir("pump1") { OffsetY = 150, CrestLevel= 110 };
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

            //assert Ymax and branchfeatures were updated
            Assert.IsTrue(data.ActiveBranchFeatures.Contains(compositeBranchStructure));
            Assert.AreEqual(110, data.ZMaxValue);
            //when no crossection a bedlevel of 0 meter is used.
            Assert.AreEqual(0, data.ZMinValue);
        }

        [Test]
        public void MinMaxZValueIsUpdatedForPumps()
        {
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData();
            var network = data.Network;

            //add a composite structure
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);
            
            //action! add a pump 
            var pump = new Pump("pump1") { OffsetY = 150,StopDelivery = 150};
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            //and a pump 
            var pump2 = new Pump("pump1") { OffsetY = 150,StartDelivery = -250};
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump2);


            //assert Ymax and branchfeatures were update
            Assert.IsTrue(data.ActiveBranchFeatures.Contains(compositeBranchStructure));
            Assert.AreEqual(150, data.ZMaxValue);
            Assert.AreEqual(-250, data.ZMinValue);
        }

        [Test]
        public void MinMaxValueIsUpdateForWeir()
        {
            NetworkSideViewDataTestHelper.AssertMinMaxIsUpdatedForStructure(
                8, 300, new Weir { OffsetY = 150, CrestLevel = 300});
        }

        [Test]
        public void MinMaxIsUpdatedForCulvert()
        {
            //action! add a culvert
            var culvert = new Culvert
                              {
                                  GeometryType = CulvertGeometryType.Rectangle, 
                                  InletLevel = -20, 
                                  OutletLevel = 50,
                                  Width = 2,
                                  Height = 1
                              };
            //max is outlet + 1
            NetworkSideViewDataTestHelper.AssertMinMaxIsUpdatedForStructure(-20,51,culvert);
        }
        
        [Test]
        public void MinMaxZTakesAllTimesIntoAccount()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var waterLevelCoverage = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(network, 3);
            var filteredWaterLevelCoverage = waterLevelCoverage.AddTimeFilter(waterLevelCoverage.Time.Values[1]);


            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network,filteredWaterLevelCoverage);

            //should be above (19) but now we cannot interpolatei becaus too slow
            Assert.AreEqual(18, viewData.ZMaxValue);
            Assert.AreEqual(8, viewData.ZMinValue);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void MinMaxZIsFast()
        {
            //not so fast here since the we use mem based store. Netcdf is more quick
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var coverage = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(network, 1500);
            var filteredCoverage = coverage.AddTimeFilter(coverage.Time.Values[1]);

            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network,filteredCoverage);

            PreInitializeQuickGraph(network, filteredCoverage); // required to analyze single test, otherwise very strange results are obtained
            
            TestHelper.AssertIsFasterThan(19, ()=>
            {
                Assert.AreEqual(18, viewData.ZMaxValue);
                Assert.AreEqual(8, viewData.ZMinValue);
            });
        }

        private void PreInitializeQuickGraph(HydroNetwork network, INetworkCoverage coverage)
        {
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, coverage);

            var waterLevelFunction = viewData.CreateWaterLevelSideViewFunction();
            var bedLevelFunction = viewData.ProfileSideViewFunctions.FirstOrDefault(pf => pf.Name == "bed level");
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void MinMaxZIsVeryFastForARouteWithLocationsThatAreInTheSource()
        {
            var viewData = NetworkSideViewDataTestHelper.CreateDefaultViewData();
            var coverage = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(viewData.NetworkRoute.Network, 1);
            
            var route = new Route { Network = coverage.Network, SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations };
            
            //take two value of the source coverage (both on branch1)
            route[coverage.Locations.Values[0]] = 1.0;
            route[coverage.Locations.Values[1]] = 3.0;

            viewData = new NetworkSideViewDataController(route, new NetworkSideViewCoverageManager(route, null, new[] { coverage }));

            PreInitializeQuickGraph(route.Network as HydroNetwork, route);

            double max = 0;
            double min = 0;
            TestHelper.AssertIsFasterThan(40, () =>
                {
                    max = viewData.ZMaxValue;
                    min = viewData.ZMinValue;
                });

            Assert.AreEqual(18, max);
            Assert.AreEqual(8, min);
        }
        
        [Test]
        [Category(TestCategory.Performance)]
        public void MinMaxIsFastForCoverageInNetCdf()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(true, new Point(0, 0), new Point(100, 0),
                                                                  new Point(100, 100));
            
            var coverage = NetworkSideViewDataTestHelper.CreateTimeDependentWaterLevelCoverageInNetCdf(network, 4000);
            coverage.Name = NetworkSideViewDataController.WaterLevelCoverageNameInMapFile;

            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, coverage);

            Assert.AreEqual(2, viewData.NetworkRoute.Segments.Values.Count);

            // timings are unstable (reflection?)
            double zMax = 0;
            double zMin = 0;
            TestHelper.AssertIsFasterThan(100, () =>
                {
                    zMax = viewData.ZMaxValue;
                    zMin = viewData.ZMinValue;
                });

            Assert.AreEqual(18, zMax);
            Assert.AreEqual(0, zMin);
        }
        
        [Test]
        [Category(TestCategory.Performance)]
        public void MinMaxIsVeryFastSecondTimeForCoverageInNetCdf()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(true, new Point(0, 0), new Point(100, 0),
                                                                  new Point(100, 100));
            
            var coverage = NetworkSideViewDataTestHelper.CreateTimeDependentWaterLevelCoverageInNetCdf(network, 4000, "altName");
            coverage.Name = NetworkSideViewDataController.WaterLevelCoverageNameInMapFile;

            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, coverage);

            Assert.AreEqual(2, viewData.NetworkRoute.Segments.Values.Count);
            
            // retrieve values first time (heavier)
            var zMax = viewData.ZMaxValue;
            var zMin = viewData.ZMinValue;

            // second time is much faster
            TestHelper.AssertIsFasterThan(5, () =>
                                                 {
                                                     Assert.AreEqual(18, viewData.ZMaxValue);
                                                     Assert.AreEqual(0, viewData.ZMinValue);
                                                 });
        }
        [Test]
        public void AddingCoverageOfWrongNetworkThrowsException()
        {
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData();

            var newCoverage = new NetworkCoverage {Network = new Network()};
            //should throw an exception since the network does not match the network of the route
            Assert.Throws<InvalidOperationException>(() =>
            {
                data.AddRenderedCoverage(newCoverage);
            });
        }

        [Test]
        public void AddingCoverageToNetworkSideViewData()
        {
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData();

            var newCoverage = new NetworkCoverage { Network = data.NetworkRoute.Network };
            data.AllNetworkCoverages = new[] {newCoverage};
            //add it to the data
            data.AddRenderedCoverage(newCoverage);

            //assert it got 'accepted'
            Assert.AreEqual(new[] {newCoverage}, data.RenderedNetworkCoverages.ToArray());
        }

        [Test]
        public void MinMaxValueRenderedCoveragesNoTimeDependent()
        {
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData();

            var newCoverage = new NetworkCoverage { Network = data.Network };

            
            var location = new NetworkLocation(data.Network.Branches[0], 0);
            var location2 = new NetworkLocation(data.Network.Branches[0], 10);
            newCoverage[location] = 10.0;
            newCoverage[location2] = 100.0;

            //add it to the data
            data.AllNetworkCoverages = new[] { newCoverage};
            data.AddRenderedCoverage(newCoverage);

            //assert it got 'accepted'
            Assert.AreEqual(100.0, data.ZMaxValueRenderedCoverages);
            Assert.AreEqual(10.0, data.ZMinValueRenderedCoverages);
        }
        [Test]
        public void MinMaxValueRenderedCoveragesTakesAllTimesIntoAccount()
        {
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData();

            var newCoverage = new NetworkCoverage { Network = data.Network };

            newCoverage.IsTimeDependent = true;
            var location = new NetworkLocation(data.Network.Branches[0], 0);
            newCoverage[new DateTime(2000, 1, 1), location] = 10.0;
            newCoverage[new DateTime(2000, 1, 2), location] = 100.0;

            //filter it before it is added
            var filtered = newCoverage.AddTimeFilter(new DateTime(2000, 1, 1));
            //add it to the data
            data.AllNetworkCoverages = new[] {filtered};
            data.AddRenderedCoverage(filtered);

            //assert 
            Assert.AreEqual(100.0, data.ZMaxValueRenderedCoverages);
            Assert.AreEqual(10.0, data.ZMinValueRenderedCoverages);
        }
        [Test]
        public void MinMaxValueRenderedCoveragesMultipleCoverages()
        {
            var data = NetworkSideViewDataTestHelper.CreateDefaultViewData();

            var coverage1 = new NetworkCoverage { Network = data.Network };
            var coverage2 = new NetworkCoverage { Network = data.Network };

            var location = new NetworkLocation(data.Network.Branches[0], 0);
            
            coverage1[location] = 10.0;
            coverage2[location] = 50.0;

            //add it to the data
            data.AllNetworkCoverages = new[] { coverage1 ,coverage2};
            data.AddRenderedCoverage(coverage1);
            data.AddRenderedCoverage(coverage2);

            //assert it got 'accepted'
            Assert.AreEqual(50.0, data.ZMaxValueRenderedCoverages);
            Assert.AreEqual(10.0, data.ZMinValueRenderedCoverages);
        }
    }
}


