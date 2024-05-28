using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Units;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class NetworkSideViewFunctionTest
    {
        private HydroNetwork network;
        private NetworkCoverage coverage;
        private IBranch firstBranch;

        [SetUp]
        public void SetUp()
        {
            network = NetworkSideViewFunctionTestHelper.GetNetwork2Branches();

            coverage = new NetworkCoverage { Network = network };
            
            firstBranch = network.Branches[0];
            coverage[new NetworkLocation(firstBranch, 0)] = 10.0;
            coverage[new NetworkLocation(firstBranch, 20)] = 30.0;
            coverage[new NetworkLocation(firstBranch, 80)] = 90.0;
        }
        
        [Test]
        public void CreateSideViewFunction()
        {
            // create network
            //add three points to the coverage on the first branch
            
            //route covers the whole first branch and add a point at the end
            var route = new Route { Network = network, SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations };
            route[new NetworkLocation(firstBranch, 0)] = 1.0;
            route[new NetworkLocation(firstBranch, 100)] = 1.0;

            var offsets = new[] { 0, 20, 80, 100 };
            var values = new[] { 10, 30, 90, 90 };

            NetworkSideViewFunctionTestHelper.AssertRouteIsCorrect(coverage, route, offsets, values);
        }
        
        [Test]
        public void PositiveRoute()
        {
            Network network = NetworkSideViewFunctionTestHelper.GetNetwork();
            Branch branch1 = (Branch) network.Branches[0];
            Branch branch2 = (Branch) network.Branches[1];

            NetworkCoverage source = new NetworkCoverage {Network = network};

            NetworkSideViewFunctionTestHelper.SetNetworkLocationAt10And20(source);

            var route = new Route
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations
            };
            // routes are coverages with sorting disabled

            route.Locations.Values.Add(new NetworkLocation(branch1, 10.0));
            route.Locations.Values.Add(new NetworkLocation(branch2, 90.0));

            // see above
            //  0 10 20 30 40 50 60 70 80 90 100
            //                                0    20    40    60    80    100
            // routePositive
            //    {10----------------------------------------------->>>90}
            // expected result:
            //    {10----------------------------------------------->>>90}
            // offsets
            //     10 20 30 40 50 60 70 80 90
            //                             90 110 130 150 170 180 
  
            // var resultRoute = new NetworkSideViewFunction(source, route);
            var offsets = new[] {0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 90, 110, 130, 150, 170, 180};
            var values = new[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 0,  20,  40,  60, 80, 90};
            NetworkSideViewFunctionTestHelper.AssertRouteIsCorrect(source, route, offsets, values);
        }

        [Test]
        public void NegativeRoute()
        {
            var network = NetworkSideViewFunctionTestHelper.GetNetwork();
            var branch1 = (Branch) network.Branches[0];
            var branch2 = (Branch) network.Branches[1];

            var source = new NetworkCoverage {Network = network};

            NetworkSideViewFunctionTestHelper.SetNetworkLocationAt10And20(source);

            var route = RouteHelper.CreateRoute(new NetworkLocation(branch2, 90.0),
                                    new NetworkLocation(branch1, 10.0));
            route.Network = network;


            // see above
            //  0 10 20 30 40 50 60 70 80 90 100  
            //                                0    20    40    60    80    100
            // routeNegative
            //     {10<<<-----------------------------------------------90}
            // expected result:
            //     {10<<<--------------------------------------------80}
            // offsets
            //     180 170 150 140 130 120 110 100 90
            //                                     90  70  50  30  10 0<---               

            var offsets = new[] {0, 10, 30, 50, 70, 90, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180};
            var values = new[] {90, 80, 60, 40, 20, 0, 100,  90,  80,  70,  60,  50,  40,  30,  20, 10};

            NetworkSideViewFunctionTestHelper.AssertRouteIsCorrect(source, route, offsets, values);
        }
        [Test]
        public void GetMinMaxForRouteExceedingCoverage()
        {
            IHydroNetwork network = NetworkSideViewFunctionTestHelper.CreateNetwork();

            NetworkCoverage source = new NetworkCoverage { Network = network };
            source[new NetworkLocation(network.Branches[0], 10.0)] = 100.0;
            source[new NetworkLocation(network.Branches[0], 100.0)] = 1000.0;

            var route = RouteHelper.CreateRoute(new NetworkLocation(network.Branches[0], 0.0)
                                    , new NetworkLocation(network.Branches[0], 110.0));

            NetworkSideViewFunctionTestHelper.AssertMinMax(source,route,1000,100);
        }
        [Test]
        public void GetMinMaxForRouteSmallerThanCoverage()
        {
            IHydroNetwork network = NetworkSideViewFunctionTestHelper.CreateNetwork();

            var source = new NetworkCoverage { Network = network };
            source[new NetworkLocation(network.Branches[0], 10.0)] = 100.0;
            source[new NetworkLocation(network.Branches[0], 100.0)] = 1000.0;

            
            var route = RouteHelper.CreateRoute(new NetworkLocation(network.Branches[0], 30.0),
                                                            new NetworkLocation(network.Branches[0], 40.0));

            

            NetworkSideViewFunctionTestHelper.AssertMinMax(source, route,400,300);
        }

        [Test]
        public void GetMinMaxForReversedRoute()
        {
            var network = NetworkSideViewFunctionTestHelper.CreateNetwork();

            var source = new NetworkCoverage { Network = network };
            source[new NetworkLocation(network.Branches[0], 0.0)] = 100.0;
            source[new NetworkLocation(network.Branches[0], 100.0)] = 1000.0;

            var route = RouteHelper.CreateRoute(
                                            new NetworkLocation(network.Branches[0], 70.0),
                                            new NetworkLocation(network.Branches[0], 10.0)
                                        );

            var function = new NetworkSideViewDataController(route,null).CreateRouteFunctionFromNetworkCoverage(source, new Unit("bla"));
            
            Assert.AreEqual(190, function.Components[0].MinValue);
            Assert.AreEqual(730, function.Components[0].MaxValue);
        }

        [Test]
        public void FeaturesFilteredCorrectForRouteInFeatureCoverage()
        {
            var network = NetworkSideViewFunctionTestHelper.CreateNetwork();
            var branch = network.Branches[0];

            var weir = new Weir {Branch = branch, Chainage = 30};
            var bridge = new Bridge {Branch = branch, Chainage = 5};
            var pump = new Pump { Branch = branch, Chainage = 75 };

            var source = new FeatureCoverage();
            source.Arguments.Add(new Variable<IFeature>());
            var variable = new Variable<double>();
            variable.NoDataValue = double.NaN;
            source.Components.Add(variable);
            
            source.Features.AddRange(new IFeature[] {weir, bridge, pump});
            source[weir] = 10.0;
            source[bridge] = 15.0;
            source[pump] = 20.0;
            
            var route = RouteHelper.CreateRoute(new NetworkLocation(branch, 70.0),
                                                new NetworkLocation(branch, 10.0));

            var function = new NetworkSideViewDataController(route,null).CreateRouteFunctionFromFeatureCoverage(source, new Unit("bla"));

            var chainagesMda = function.Arguments[0].GetValues<double>();
            Assert.AreEqual(1, chainagesMda.Count);
            Assert.AreEqual(40.0d, chainagesMda[0]); 
        }

        [Test]
        public void RouteWithNoPointsInCoverage()
        {
            var network = NetworkSideViewFunctionTestHelper.CreateNetwork();

            var source = new NetworkCoverage { Network = network };
            source[new NetworkLocation(network.Branches[0], 0.0)] = 100.0;
            source[new NetworkLocation(network.Branches[0], 100.0)] = 1000.0;
            source[new NetworkLocation(network.Branches[1], 50.0)] = -500.0;

            var route = RouteHelper.CreateRoute(new NetworkLocation(network.Branches[0], 10.0),
                                    new NetworkLocation(network.Branches[0], 60.0));

            var offSets = new[] {0, 50};
            var values = new[] {190, 640};
            NetworkSideViewFunctionTestHelper.AssertRouteIsCorrect(source,route,offSets,values);
        }
    }
}
