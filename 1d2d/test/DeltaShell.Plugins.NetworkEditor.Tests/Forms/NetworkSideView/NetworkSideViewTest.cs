using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Units;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class NetworkSideViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CanCreateSideViewForRouteAndNetworkCoverage()
        {
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData();

            // Set up the view
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                    Dock = DockStyle.Fill,
                                    Data = viewData.NetworkRoute,
                                    DataController = viewData
                               };

            WindowsFormsTestHelper.ShowModal(sideView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Performance)]
        public void ModifyingCoverageShownSideViewIsFast()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var waterLevelNetworkCoverage = GetBigNetworkCoverage(network);
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, waterLevelNetworkCoverage);

            // Set up the view
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Dock = DockStyle.Fill,
                Data = viewData.NetworkRoute,
                DataController = viewData
            };

            //make sure we show it in the view
            viewData.AddRenderedCoverage(waterLevelNetworkCoverage);

            WindowsFormsTestHelper.ShowModal(
                sideView,
                f => TestHelper.AssertIsFasterThan(275,
                                                   () =>
                                                       {
                                                           //clear the coverage (slowly)
                                                           var locations = waterLevelNetworkCoverage.Locations;
                                                           while (locations.Values.Count > 0)
                                                           {
                                                               var nextTime = locations.Values.First();
                                                               waterLevelNetworkCoverage.RemoveValues(
                                                                   new VariableValueFilter<INetworkLocation>(locations, nextTime));
                                                           }
                                                       }));
        }

        private static NetworkCoverage GetBigNetworkCoverage(HydroNetwork network)
        {
            var waterLevelNetworkCoverage = new NetworkCoverage("newCoverage", false) {Network = network};

            var branch = network.Branches.First();
            for(int i = 0; i < 1000; i++)
            {
                var chainage = i * (branch.Length/1000.0);
                waterLevelNetworkCoverage[new NetworkLocation(branch, chainage)] = (double)i;
            }
            return waterLevelNetworkCoverage;
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CanCreateSideViewForRouteAndTimeDependendNetworkCoverage()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var waterLevelNetworkCoverage = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(network, 3);
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, waterLevelNetworkCoverage);
            Assert.IsNotNull(viewData.WaterLevelNetworkCoverage);

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                   Dock = DockStyle.Fill,
                                   Data = viewData.NetworkRoute,
                                   DataController = viewData
                               };

            WindowsFormsTestHelper.ShowModal(sideView, viewData.WaterLevelNetworkCoverage);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowingSideViewShouldBeFastForBigTimeDependendCoverages()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var waterLevelNetworkCoverage = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(network,
                                                                                                            1000);
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, waterLevelNetworkCoverage);

            // next set the filter
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Dock = DockStyle.Fill,
                Data = viewData.NetworkRoute,
                DataController = viewData
            };

            WindowsFormsTestHelper.ShowModal(sideView, viewData.WaterLevelNetworkCoverage);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanHandleNoWaterLevel()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network,null);
            
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                               Data = viewData.NetworkRoute,
                               DataController = viewData
                           };
            WindowsFormsTestHelper.ShowModal(sideView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanHandleInterpolationOverCrossSections()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            network.Branches.ToList().ForEach(b => b.OrderNumber = 1);
            
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, null);

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Data = viewData.NetworkRoute,
                DataController = viewData
            };
            WindowsFormsTestHelper.ShowModal(sideView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayWeir()
        {
            var weir = new Weir("Weir") {OffsetY = 150,CrestLevel = 14};
            NetworkSideViewTestHelper.ShowInSideView(weir);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayRoundWeir()
        {
            var weir = new Weir("Weir") { OffsetY = 150, CrestLevel = 14, CrestShape = CrestShape.Round, WeirFormula = new RiverWeirFormula()};
            NetworkSideViewTestHelper.ShowInSideView(weir);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayBroadWeir()
        {
            var weir = new Weir("Weir") { OffsetY = 150, CrestLevel = 14, CrestShape = CrestShape.Broad, WeirFormula = new RiverWeirFormula() };
            NetworkSideViewTestHelper.ShowInSideView(weir);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayTriangularWeir()
        {
            var weir = new Weir("Weir") { OffsetY = 150, CrestLevel = 14, CrestShape = CrestShape.Triangular, WeirFormula = new RiverWeirFormula() };
            NetworkSideViewTestHelper.ShowInSideView(weir);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayLateralSource()
        {
            //a branch feature depicting a lateral
            var lateral = new LateralSource() { Chainage = 150 };
            NetworkSideViewTestHelper.ShowBranchFeatureInSideView(lateral);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayDiffuseLateralSource()
        {
            //a branch feature depicting a lateral
            var lateral = new LateralSource() { Chainage = 150, Length = 47, Name = "diffuse lateral source"};
            NetworkSideViewTestHelper.ShowBranchFeatureInSideView(lateral);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayRetention()
        {
            //a branch feature depicting a retention
            var retention = new Retention() {Name="test", Chainage = 150 };
            NetworkSideViewTestHelper.ShowBranchFeatureInSideView(retention);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayBridge()
        {
            var bridge = new Bridge();
            bridge.Length = 30;
            bridge.BridgeType = BridgeType.Rectangle;
            bridge.Height = 3;
            bridge.Shift = 20;
            NetworkSideViewTestHelper.ShowInSideView(bridge);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayGatedWeir()
        {
            //a composite structure with a weir in it
            var weir = new Weir("Weir") { OffsetY = 150, CrestLevel = 14 };
            var gatedWeirFormula = new GatedWeirFormula {GateOpening = 2};
            weir.WeirFormula = gatedWeirFormula;
            NetworkSideViewTestHelper.ShowInSideView(weir);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayCompositeStructures()
        {
            //a composite structure with a pump in it
            var pump = new Pump("pump1") { OffsetY = 150, StartSuction = 17, StopSuction = 11, StartDelivery = 11, StopDelivery = 17 };
            NetworkSideViewTestHelper.ShowInSideView(pump);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayObservationPoints()
        {
            var observationPoint = new ObservationPoint();
            NetworkSideViewTestHelper.ShowInSideView(observationPoint);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewUpdatesWhenPumpChanges()
        {
            //this test should show a pump going up and down :)
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, null);
            

            //a composite structure with a pump in it
            var pump = new Pump("pump1") { OffsetY = 150 };
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Data = viewData.NetworkRoute,
                DataController = viewData,
                SelectedFeature = pump
            };

            // funny but make it run faster :)

            WindowsFormsTestHelper.Show(sideView);
            for (int i = 0;i< 20;i++)
            {
                Thread.Sleep(50);
                pump.StopDelivery += 0.2;
                if (i % 3 == 0)
                {
                    pump.DirectionIsPositive = !pump.DirectionIsPositive;    
                }
                Application.DoEvents();
            }
            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(50);
                
                if (i % 3 == 0)
                {
                    pump.DirectionIsPositive = !pump.DirectionIsPositive;
                }
                Application.DoEvents();
            }

            WindowsFormsTestHelper.CloseAll();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewRespondsToTimeSerieNavigator()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var timeDependendWaterLevelCoverage = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(network, 20);
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, timeDependendWaterLevelCoverage);
            
            // next set the filter
            var waterLevel = viewData.WaterLevelNetworkCoverage;

            // set up the min and max values of the axes
            /*viewData.ZMinValue = 0;
            viewData.ZMaxValue = 20;
            */

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                   Dock = DockStyle.Fill,
                                   Data = viewData.NetworkRoute,
                                   DataController = viewData
                               };

            Assert.AreEqual(timeDependendWaterLevelCoverage.Time.Values, sideView.Times.ToArray());

            var navigationControl = new TimeSeriesNavigator
                                {
                                    Data = sideView, // time
                                    Dock = DockStyle.Bottom
                                };

            WindowsFormsTestHelper.ShowModal(new List<Control>{sideView, navigationControl}, waterLevel);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDrawFeatureCoverages()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var channel = network.Channels.First();
            var yzCoordinates = new List<Coordinate>
                                                  {
                                                      new Coordinate(0.0, 18.0),
                                                      new Coordinate(100.0, 0.0),
                                                      new Coordinate(150.0, 10.0),
                                                      new Coordinate(300.0, 10.0),
                                                      new Coordinate(350.0, 18.0),
                                                      new Coordinate(500.0, 18.0)
                                                  };

            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(channel, 20.0, yzCoordinates, "crosssection1");
            
            var location1 = new NetworkLocation(channel, 0.0);
            var location2 = new NetworkLocation(channel, 20.0);
            
            var weir = new Weir("weir1");
            NetworkHelper.AddBranchFeatureToBranch(weir, channel, 50);

            var weirFeatureCoverage = FeatureCoverage.GetTimeDependentFeatureCoverage<Weir, double>();
            var waterLevelNetworkCoverage = new NetworkCoverage(NetworkSideViewDataController.WaterLevelCoverageNameInMapFile, true) { Network = network };
            
            weirFeatureCoverage.Components[0].Unit = new Unit("m AD", "m AD");
            waterLevelNetworkCoverage.Components[0].Unit = new Unit("m AD", "m AD");

            weirFeatureCoverage.Features.Add(weir);         
            var time = DateTime.Now;
            var timeStep = new TimeSpan(0, 0, 10, 0);
            const int timsteps = 20;
            for (int i = 0; i < timsteps; i++)
            {
                waterLevelNetworkCoverage[time, location1] = 18.0d + i%20;
                waterLevelNetworkCoverage[time, location2] = 16.0d + i % 20;
                weirFeatureCoverage[time, weir] = 16.0d + i%10;
                time += timeStep;
            }

            var locations = new[]
                            {
                                new NetworkLocation(channel, 10.0),
                                new NetworkLocation(channel, 100.0)
                            };

            var route = RouteHelper.CreateRoute(locations);
            var coverages = new ICoverage[] {waterLevelNetworkCoverage, weirFeatureCoverage};
            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(route, null, coverages);
            var sideViewDataController = new NetworkSideViewDataController(route, networkSideViewCoverageManager);
            
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView { Data = route, DataController = sideViewDataController, Text = "sideview" };
            
            Assert.AreEqual(timsteps, sideView.Times.Count());

            var navigationControl = new TimeSeriesNavigator
            {
                Data = sideView, // time
                Dock = DockStyle.Bottom
            };

            WindowsFormsTestHelper.ShowModal(new List<Control> { sideView, navigationControl}, waterLevelNetworkCoverage);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewRespondsToTimeSerieNavigatorAndAdjustsCrestLevel()
        {
            HydroNetwork network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            NetworkSideViewDataController viewData = NetworkSideViewTestHelper.CreateDefaultViewDataWithWeir(network);

            // next set the filter
            INetworkCoverage waterLevel = viewData.WaterLevelNetworkCoverage;

            // set up the min and max values of the axes
            /*viewData.ZMinValue = 0;
            viewData.ZMaxValue = 20;
            */

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Dock = DockStyle.Fill,
                Data = viewData.NetworkRoute,
                DataController = viewData
            };


            var navigationControl = new TimeSeriesNavigator
            {
                Data = sideView, // time
                Dock = DockStyle.Bottom
            };


            WindowsFormsTestHelper.ShowModal(new List<Control> { sideView, navigationControl }, waterLevel);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void SideViewIsFastForBigCoverageInNetCdf()
        {
            var coverage = NetworkSideViewTestHelper.GetTimeDependendNetworkCoverageOnHydroNetwork(100, 100, true, TestHelper.GetCurrentMethodName() + ".nc");
            coverage.Store.CacheVariable(coverage.Time);
            coverage.Store.CacheVariable(coverage.Locations);
            var random = new Random();
            var hydroNetwork = (HydroNetwork) coverage.Network;
            foreach (var branch in hydroNetwork.Channels)
            {
                NetworkSideViewTestHelper.AddCrossSection(branch, branch.Length/2, random.Next(10));        
            }
            
            //get sideview data from 10 to 20
            var route = RouteHelper.CreateRoute(new NetworkLocation(hydroNetwork.Branches[9], 0),
                                                new NetworkLocation(hydroNetwork.Branches[19], 0));
            var viewData = new NetworkSideViewDataController(route, new NetworkSideViewCoverageManager(route, null, null));

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                   Dock = DockStyle.Fill,
                                   Data = viewData.NetworkRoute,
                                   DataController = viewData,
            };

            WindowsFormsTestHelper.Show(sideView);
            TestHelper.AssertIsFasterThan(2400, () =>
                                                    {
                                                        //50 stappen doorlopen
                                                        for (int i = 0; i < 50; i++)
                                                        {
                                                            sideView.SetCurrentTimeSelection((DateTime?) coverage.Time.Values[i], (DateTime?) coverage.Time.Values[i]);
                                                            Application.DoEvents();
                                                        }
                                                    }, false, true);
            
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSideViewWithExtraFunctions()
        {
            //network side view is given all coverages known by default. In the view the visibility is changed.
            //this way the networkside 
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData();

            var waterLevel = viewData.WaterLevelNetworkCoverage;
            //add another coverage on the same network..some random values on the same locations as the waterlevel
            var addedNetworkCoverage = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 15.0, 0.5,
                                                                                                 new Unit("dummy",
                                                                                                          "kg/s"),
                                                                                                 "Waste dumped");
            var addedNetworkCoverage2 = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 150.0, 20,
                                                                                                  new Unit(
                                                                                                      "ping reply", "ms"),
                                                                                                  "Google ping reply");
            var addedNetworkCoverage3 = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 150.0, 20,
                                                                                                  new Unit(
                                                                                                      "height", "m"),
                                                                                                  "Building height");

            
            viewData.AllNetworkCoverages = new[] {addedNetworkCoverage, addedNetworkCoverage2,addedNetworkCoverage3};

            // Set up the view
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                   Dock = DockStyle.Fill,
                                   Data = viewData.NetworkRoute,
                                   DataController = viewData
                               };
            WindowsFormsTestHelper.ShowModal(sideView);

        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSideViewWithExtraFunctionsForTwoModels()
        {
            //network side view is given all coverages known by default. In the view the visibility is changed.
            //this way the networkside 
            NetworkSideViewTestHelper.modelNameDelegate = c => c.Name.Contains("e ") ? "Model 1" : "Model 2";
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData();
            NetworkSideViewTestHelper.modelNameDelegate = null; //reset

            var waterLevel = viewData.WaterLevelNetworkCoverage;
            //add another coverage on the same network..some random values on the same locations as the waterlevel
            var addedNetworkCoverage = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 15.0, 0.5,
                                                                                                 new Unit("dummy",
                                                                                                          "kg/s"),
                                                                                                 "Waste dumped");
            var addedNetworkCoverage2 = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 150.0, 20,
                                                                                                  new Unit(
                                                                                                      "ping reply", "ms"),
                                                                                                  "Google ping reply");
            var addedNetworkCoverage3 = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 150.0, 20,
                                                                                                  new Unit(
                                                                                                      "height", "m"),
                                                                                                  "Building height");


            viewData.AllNetworkCoverages = new[] { addedNetworkCoverage, addedNetworkCoverage2, addedNetworkCoverage3 };

            // Set up the view
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Dock = DockStyle.Fill,
                Data = viewData.NetworkRoute,
                DataController = viewData
            };
            WindowsFormsTestHelper.ShowModal(sideView);

        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSideViewWithExtraFunctionsForMultipleModels()
        {
            //network side view is given all coverages known by default. In the view the visibility is changed.
            //this way the networkside 
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData();

            var waterLevel = viewData.WaterLevelNetworkCoverage;
            //add another coverage on the same network..some random values on the same locations as the waterlevel
            var addedNetworkCoverage = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 15.0, 0.5,
                                                                                                 new Unit("dummy",
                                                                                                          "kg/s"),
                                                                                                 "Waste dumped");
            var addedNetworkCoverage2 = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 150.0, 20,
                                                                                                  new Unit(
                                                                                                      "ping reply", "ms"),
                                                                                                  "Google ping reply");
            var addedNetworkCoverage3 = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 150.0, 20,
                                                                                                  new Unit(
                                                                                                      "height", "m"),
                                                                                                  "Building height");


            viewData.AllNetworkCoverages = new[] { addedNetworkCoverage, addedNetworkCoverage2, addedNetworkCoverage3 };

            // Set up the view
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Dock = DockStyle.Fill,
                Data = viewData.NetworkRoute,
                DataController = viewData
            };
            WindowsFormsTestHelper.ShowModal(sideView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSideViewWithoutContextMenu()
        {
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData();

            // Set up the view
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Dock = DockStyle.Fill,
                Data = viewData.NetworkRoute,
                DataController = viewData,
                ContextMenuStripEnabled =  false
            };

            WindowsFormsTestHelper.ShowModal(sideView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SideViewCanDisplayWeirAndOutputCoverage()
        {
            // This test is meant to capture the following situation:
            // showing an output coverage should not affect the display of structures and crosssections: these
            // should always be displayed as black boxes
            // See line below commented with (*)

            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData();
            var network = viewData.Network;
            var branch = network.Branches.FirstOrDefault();

            var compositeStructure = new CompositeBranchStructure { Chainage = 95.0, Branch = branch};
            var weir = new Weir("Weir")
                           {
                               Chainage = 95.0, 
                               CrestLevel = 14,
                               Geometry = new Point(95.0, 0.0),
                               Branch = branch,
                               Network = network
                           };
            compositeStructure.Structures.Add(weir);

            NetworkHelper.AddBranchFeatureToBranch(compositeStructure, branch, 95.0);

            // Set up the view
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                    Dock = DockStyle.Fill,
                                    Data = viewData.NetworkRoute,
                                    DataController = viewData
                               };

            var featureCoverage = new FeatureCoverage("crestLevels");
            featureCoverage.Arguments.Add(new Variable<IWeir>("weir"));
            featureCoverage.Components.Add(new Variable<double>("crestLevel") {NoDataValue = 0.0});
            featureCoverage.Features.Add(weir);
            
            featureCoverage[weir] = 1.0;

            viewData.AllFeatureCoverages.Add(featureCoverage);
            viewData.AddRenderedCoverage(featureCoverage); // (*) enabling/disabling this line used to give different results
            // (black boxes around structures and crosssections diseappeared when line was enabled)

            WindowsFormsTestHelper.ShowModal(sideView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CreateSideViewForFullCoverage()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 150));

            var location1 = new NetworkLocation(network.Branches[0], 0);
            var location2 = new NetworkLocation(network.Branches[0], 100);
            var location3 = new NetworkLocation(network.Branches[1], 150);
            var locations = new[] { location1, location2, location3 };

            var route = RouteHelper.CreateRoute(locations);

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(route, null, null);
            var sideViewDataController = new NetworkSideViewDataController(route, networkSideViewCoverageManager);

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView { Data = route, DataController = sideViewDataController, Text = "sideview" };

            WindowsFormsTestHelper.ShowModal(sideView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CreateSideViewForCoveragesWithTheSameName()
        {
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData();
            var waterLevel = viewData.WaterLevelNetworkCoverage;

            // Create and add two coverages with the same name
            var networkCoverage1 = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 15.0, 0.5, (Unit)waterLevel.Components[0].Unit, "Duplicate name");
            var networkCoverage2 = NetworkSideViewTestHelper.GetCoverageBasedOnOtherCoverage(waterLevel, 15.0, 0.5, (Unit)waterLevel.Components[0].Unit, "Duplicate name");

            viewData.AllNetworkCoverages = new[] { networkCoverage1, networkCoverage2 };
            viewData.AddRenderedCoverage(networkCoverage1);
            viewData.AddRenderedCoverage(networkCoverage2);

            // Set up the view
            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
                               {
                                   Dock = DockStyle.Fill,
                                   Data = viewData.NetworkRoute,
                                   DataController = viewData
                               };

            WindowsFormsTestHelper.ShowModal(sideView);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SuspendingAndResumingTheSideView_DoesNotThrowException()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var waterLevelNetworkCoverage = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(network, 3);
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, waterLevelNetworkCoverage);
            Assert.IsNotNull(viewData.WaterLevelNetworkCoverage);

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Dock = DockStyle.Fill,
                Data = viewData.NetworkRoute,
                DataController = viewData
            };

            
            var waterLevelNetworkCoverage2 = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(network, 3);
            var viewData2 = NetworkSideViewTestHelper.CreateDefaultViewData(network, waterLevelNetworkCoverage2);
            Action<Form> viewAction = delegate
            {
                sideView.SuspendUpdates();
                sideView.DataController = viewData2;
                sideView.ResumeUpdates();
            };
            
            TestDelegate action = () => WindowsFormsTestHelper.ShowModal(sideView, viewAction, viewData.WaterLevelNetworkCoverage);
            
            Assert.That(action, Throws.Nothing);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SuspendingAndResumingTheSideView_TimePropertyChangedNotFired_AndDoesNotThrowException()
        {
            var network = NetworkSideViewTestHelper.GetDefaultHydroNetwork();
            var waterLevelNetworkCoverage = NetworkSideViewTestHelper.CreateTimeDependendWaterLevelCoverage(network, 3);
            var viewData = NetworkSideViewTestHelper.CreateDefaultViewData(network, waterLevelNetworkCoverage);
            Assert.IsNotNull(viewData.WaterLevelNetworkCoverage);

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Dock = DockStyle.Fill,
                Data = viewData.NetworkRoute,
                DataController = viewData
            };
            
            Action<Form> viewAction = delegate
            {
                sideView.SuspendUpdates();
                sideView.SetCurrentTimeSelection(DateTime.Now, DateTime.Now.AddDays(1));
                sideView.ResumeUpdates();
            };
            
            TestDelegate action = () => WindowsFormsTestHelper.ShowModal(sideView, viewAction, viewData.WaterLevelNetworkCoverage);
            
            Assert.That(action, Throws.Nothing);
        }

        [Test]
        public void GivenRouteOnNetworkInBogota21818Projection_WhenCheckNetworkSideViewIsRouteValid_ThenValidationIsTrue()

        { 
            //arrange
            if (Map.CoordinateSystemFactory == null)
            {
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            }
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(359203.35276676994, 345909.50645773654), new Point(622768.57877548016, 1238197.7900166281));
            network.CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(21818);
            IBranch branch = network.Branches[0];

            branch.GeodeticLength = GeodeticDistance.Length(branch.Network.CoordinateSystem, branch.Geometry);
            Route route = CreateRouteOnBranch(branch);
            Gui.Forms.NetworkSideView.NetworkSideView sideView = GetNetworkSideViewWithRoute(route);
            string errorMessage = "No route defined for network side view.";
            TypeUtils.SetField(sideView, "routeChecked", false);

            //act
            var routeValidation = TypeUtils.CallPrivateMethod<bool>(sideView, "IsRouteValid", errorMessage);

            //assert
            Assert.That(routeValidation, Is.True);
        }

        private static Gui.Forms.NetworkSideView.NetworkSideView GetNetworkSideViewWithRoute(Route route)
        {
            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(route, null, null);
            var sideViewDataController = new NetworkSideViewDataController(route, networkSideViewCoverageManager);

            var sideView = new Gui.Forms.NetworkSideView.NetworkSideView
            {
                Data = route,
                DataController = sideViewDataController,
                Text = "sideview"
            };
            return sideView;
        }

        private static Route CreateRouteOnBranch(IBranch branch)
        {
            double quarterGeometryLength = branch.Geometry.Length / 4;
            var location1 = new NetworkLocation(branch, quarterGeometryLength);
            var location2 = new NetworkLocation(branch, quarterGeometryLength * 3);
            var locations = new[] { location1, location2 };
            var route = RouteHelper.CreateRoute(locations);
            return route;
        }
    }
}