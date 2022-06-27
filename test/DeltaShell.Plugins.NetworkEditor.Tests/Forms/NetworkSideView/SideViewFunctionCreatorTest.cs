using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Units;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class SideViewFunctionCreatorTest
    {
        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(Route route,
                                                                         IDictionary<string, IFunction> createdRoutes,
                                                                         IList<IStructure1D> activeStructures,
                                                                         IUnit waterLevelUnit,
                                                                         string paramName)
        {
            // Call
            void Call() => new SideViewFunctionCreator(route, createdRoutes, activeStructures, waterLevelUnit);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(paramName));
        }
        
        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            var route = new Route();
            var dictionary = Substitute.For<IDictionary<string, IFunction>>();
            var list = Substitute.For<IList<IStructure1D>>();
            var unit = Substitute.For<IUnit>();
            
            yield return new TestCaseData(null, dictionary, list, unit, "route")
                .SetName("route cannot be null.");
            yield return new TestCaseData(route, null, list, unit, "createdRoutes")
                .SetName("createdRoutes cannot be null.");
            yield return new TestCaseData(route, dictionary, null, unit, "activeStructures")
                .SetName("activeStructures cannot be null.");
            yield return new TestCaseData(route, dictionary, list, null, "waterLevelUnit")
                .SetName("waterLevelUnit cannot be null.");
        }

        [Test]
        [TestCaseSource(nameof(CreateSideViewFunctionMissingDataCases))]
        public void CreateWaterLevelSideViewFunction_RouteNull_ReturnsNull(Action<Route, INetworkCoverage> action)
        {
            // Setup
            var route = new Route();
            var createdRoutes = Substitute.For<IDictionary<string, IFunction>>();
            var activeStructures = Substitute.For<IList<IStructure1D>>();
            var waterLevelUnit = Substitute.For<IUnit>();
            
            
            var time = Substitute.For<IVariable<DateTime>>();
            time.Values.Count().Returns(1);
            var coverage = Substitute.For<INetworkCoverage>();
            coverage.Time.Returns(time);
            
            var sideViewFunctionCreator = new SideViewFunctionCreator(route, createdRoutes, activeStructures, waterLevelUnit);

            action(route, coverage);
            
            // Call
            IFunction result = sideViewFunctionCreator.CreateWaterLevelSideViewFunction(coverage);
            
            // Assert
            Assert.That(result, Is.Null);

        }

        private static IEnumerable<TestCaseData> CreateSideViewFunctionMissingDataCases()
        {
            void SetRouteToNull(Route route, INetworkCoverage coverage) => route = null;
            yield return new TestCaseData((Action<Route, INetworkCoverage>)SetRouteToNull)
                .SetName("Route cannot be null.");

            void SetCoverageToNull(Route route, INetworkCoverage coverage) => coverage = null;
            yield return new TestCaseData((Action<Route, INetworkCoverage>)SetCoverageToNull)
                .SetName("Coverage cannot be null.");
            
            void SetCoverageValuesToEmpty(Route route, INetworkCoverage coverage) => coverage.Time.Values = new MultiDimensionalArray<DateTime>();
            yield return new TestCaseData((Action<Route, INetworkCoverage>)SetCoverageValuesToEmpty)
                .SetName("CoverageTimeValues cannot be null if coverage is time dependent.");

        }

        [Test]
        public void CreateWaterLevelSideViewFunction_WaterLevelsNeverBelowPipeBottomLevel()
        {
            // Setup
            const double baseBottomLevel = 3.0;
            const double offset1 = 1;
            const double offset2 = 100;

            IHydroNetwork network = CreateNetworkWithTwoPipes(baseBottomLevel);
            NetworkCoverage waterLevelCoverage = CreateCoverageWithWaterLevelBelowAndAboveBottomLevels(baseBottomLevel,
                                                                                                       offset1,
                                                                                                       offset2,
                                                                                                       network);
            Route route = CreateRoute(network);

            var functionCreator = new SideViewFunctionCreator(route,
                                                              Substitute.For<IDictionary<string, IFunction>>(),
                                                              Substitute.For<IList<IStructure1D>>(),
                                                              Substitute.For<IUnit>());
            // Call
            IFunction waterLevelFunction = functionCreator.CreateWaterLevelSideViewFunction(waterLevelCoverage);

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            
            IMultiDimensionalArray values = waterLevelFunction.GetValues();
            Assert.That(values.Count, Is.EqualTo(4)); // 2 for each pipe
            Assert.That(values[0], Is.EqualTo(baseBottomLevel));
            Assert.That(values[1], Is.EqualTo(baseBottomLevel));
            Assert.That(values[2], Is.EqualTo(baseBottomLevel + offset1));
            Assert.That(values[3], Is.EqualTo(baseBottomLevel + offset2));
        }

        private static IHydroNetwork CreateNetworkWithTwoPipes(double baseBottomLevel)
        {
            var hydroNetwork = new HydroNetwork();

            var compartment1 = new Compartment("compartment1");
            var compartment2 = new Compartment("compartment2");
            var compartment3 = new Compartment("compartment3");

            var manhole = new Manhole("manhole");
            manhole.Compartments.Add(compartment1);

            var manhole2 = new Manhole("manhole2");
            manhole2.Compartments.Add(compartment2);
            
            var manhole3 = new Manhole("manhole3");
            manhole.Compartments.Add(compartment3);

            hydroNetwork.Nodes.Add(manhole);
            hydroNetwork.Nodes.Add(manhole2);
            hydroNetwork.Nodes.Add(manhole3);
            
            var pipe1 = new Pipe()
            {
                SourceCompartment = compartment1,
                TargetCompartment = compartment2,
                LevelTarget = baseBottomLevel,
                LevelSource = baseBottomLevel
            };
            
            var pipe2 = new Pipe()
            {
                SourceCompartment = compartment2,
                TargetCompartment = compartment3,
                LevelTarget = baseBottomLevel,
                LevelSource = baseBottomLevel
            };

            hydroNetwork.Branches.Add(pipe1);
            hydroNetwork.Branches.Add(pipe2);

            return hydroNetwork;
        }

        private static NetworkCoverage CreateCoverageWithWaterLevelBelowAndAboveBottomLevels(double baseBottomLevel,
                                                                                             double offset1,
                                                                                             double offset2,
                                                                                             INetwork network)
        {
            var waterLevelCoverage = new NetworkCoverage("Water level",true)
            {
                Network = network
            };

            waterLevelCoverage[new DateTime(2000,1,1), new NetworkLocation(network.Branches[0], 0.0)] = baseBottomLevel - offset1;
            waterLevelCoverage[new DateTime(2000,1,1), new NetworkLocation(network.Branches[0], 1.0)] = baseBottomLevel - offset2;
            waterLevelCoverage[new DateTime(2000, 1, 1), new NetworkLocation(network.Branches[1], 0.0)] = baseBottomLevel + offset1;
            waterLevelCoverage[new DateTime(2000, 1, 1), new NetworkLocation(network.Branches[1], 1.0)] = baseBottomLevel + offset2;

            waterLevelCoverage.Arguments[1].Name = "location";
            waterLevelCoverage.Components[0].Name = "water level";

            waterLevelCoverage.Name = NetworkSideViewDataController.WaterLevelCoverageNameInMapFile;

            return waterLevelCoverage;
        }

        private static Route CreateRoute(INetwork network)
        {
            var route = new Route
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            route[new NetworkLocation(network.Branches[0], 10)] = 1.0;
            route[new NetworkLocation(network.Branches[1], 60)] = 3.0;

            route.Components[0].Unit = new Unit("meters", "m");

            return route;
        }
    }
}