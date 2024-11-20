using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
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
using Arg = NSubstitute.Arg;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class SideViewFunctionCreatorTest
    {
        private const string name = "expectedName";
        private const double noDataValue = -999.0d;
        private const double maxWaterLevel = 40.0d;
        
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
            IFunction result = sideViewFunctionCreator.CreateWaterLevelSideViewFunction(coverage, Substitute.For<IFunction>());
            
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
            IFunction waterLevelFunction = functionCreator.CreateWaterLevelSideViewFunction(waterLevelCoverage, Substitute.For<IFunction>());

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            
            IMultiDimensionalArray values = waterLevelFunction.GetValues();
            Assert.That(values.Count, Is.EqualTo(4)); // 2 for each pipe
            Assert.That(values[0], Is.EqualTo(baseBottomLevel));
            Assert.That(values[1], Is.EqualTo(baseBottomLevel));
            Assert.That(values[2], Is.EqualTo(baseBottomLevel + offset1));
            Assert.That(values[3], Is.EqualTo(baseBottomLevel + offset2));
        }
        
        [Test]
        public void CreateWaterLevelSideViewFunction_WhenBedLevelNull_WaterLevelsNeverBelowPipeBottomLevel()
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
            IFunction waterLevelFunction = functionCreator.CreateWaterLevelSideViewFunction(waterLevelCoverage, null);

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            
            IMultiDimensionalArray values = waterLevelFunction.GetValues();
            Assert.That(values.Count, Is.EqualTo(4)); // 2 for each pipe
            Assert.That(values[0], Is.EqualTo(baseBottomLevel));
            Assert.That(values[1], Is.EqualTo(baseBottomLevel));
            Assert.That(values[2], Is.EqualTo(baseBottomLevel + offset1));
            Assert.That(values[3], Is.EqualTo(baseBottomLevel + offset2));
        }
        
        [Test]
        public void CreateWaterLevelSideViewFunction_GivenWaterLevelWithNoDataValueSetButDataAllSet_WaterLevelsNeverBelowPipeBottomLevel()
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

            var bedLevel = Substitute.For<IFunction>();
            waterLevelCoverage.Components[0].NoDataValue = -999;
            const double bedLevelValueNoDataValueIsChangedTo = 30.0d;
            bedLevel.Evaluate<double>(Arg.Any<object>()).Returns(bedLevelValueNoDataValueIsChangedTo);
            
            // Call
            IFunction waterLevelFunction = functionCreator.CreateWaterLevelSideViewFunction(waterLevelCoverage, bedLevel);

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            
            IMultiDimensionalArray values = waterLevelFunction.GetValues();
            Assert.That(values.Count, Is.EqualTo(4)); // 2 for each pipe
            Assert.That(values[0], Is.EqualTo(baseBottomLevel));
            Assert.That(values[1], Is.EqualTo(baseBottomLevel));
            Assert.That(values[2], Is.EqualTo(baseBottomLevel + offset1));
            Assert.That(values[3], Is.EqualTo(baseBottomLevel + offset2));
        }
        
        [Test]
        public void CreateWaterLevelSideViewFunction_GivenWaterLevelWithNoDataValueSetOnDataElementInWaterLevel_WaterLevelForDataElementOnNoDataValueReplacedWithBedLevelValue()
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

            var bedLevel = Substitute.For<IFunction>();
            waterLevelCoverage.Components[0].NoDataValue = baseBottomLevel + offset2;
            const double bedLevelValueNoDataValueIsChangedTo = 30.0d;
            bedLevel.Evaluate<double>(Arg.Any<object>()).Returns(bedLevelValueNoDataValueIsChangedTo);
            
            // Call
            IFunction waterLevelFunction = functionCreator.CreateWaterLevelSideViewFunction(waterLevelCoverage, bedLevel);

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            
            IMultiDimensionalArray values = waterLevelFunction.GetValues();
            Assert.That(values.Count, Is.EqualTo(4)); // 2 for each pipe
            Assert.That(values[0], Is.EqualTo(baseBottomLevel));
            Assert.That(values[1], Is.EqualTo(baseBottomLevel));
            Assert.That(values[2], Is.EqualTo(baseBottomLevel + offset1));
            Assert.That(values[3], Is.EqualTo(bedLevelValueNoDataValueIsChangedTo));
        }

        [Test]
        public void CreateMaxWaterLevelFunction_GivenNetworkCoverageNull_ReturnNull()
        {
            // Setup
            SideViewFunctionCreator functionCreator = CreateSideViewFunctionCreator();
            
            var bedLevel = Substitute.For<IFunction>();
            
            // Call
            IFunction waterLevelFunction = functionCreator.CreateMaxWaterLevelFunction(null, bedLevel);
            
            // Assert
            Assert.That(waterLevelFunction, Is.Null);
        }
        
        [Test]
        public void CreateMaxWaterLevelFunction_GivenNoCachedMaxWaterLevelValues_ReturnFunctionWithMaxWaterLevelValues()
        {
            // Setup
            SideViewFunctionCreator functionCreator = CreateSideViewFunctionCreator();
            var expectedName = $"Max {name}";

            var mda = Substitute.For<IMultiDimensionalArray<double>>();
            INetworkCoverage networkCoverage = GetNetworkCoverage(mda);

            var bedLevel = Substitute.For<IFunction>();
            
            // Call
            IFunction waterLevelFunction = functionCreator.CreateMaxWaterLevelFunction(networkCoverage, bedLevel);

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            Assert.That(waterLevelFunction.Name, Is.EqualTo(expectedName));
            
            IMultiDimensionalArray values = waterLevelFunction.GetValues();
            Assert.That(values.Contains(double.NaN), Is.False);
            Assert.That(values.Contains(noDataValue), Is.False);
            
            bedLevel.Received(4).Evaluate<double>(Arg.Any<double>());
        }

        [Test]
        public void CreateMaxWaterLevelFunction_GivenMultiDimensionalArrayIsNull_ReturnFunctionWithMaxWaterLevelValues()
        {
            // Setup
            SideViewFunctionCreator functionCreator = CreateSideViewFunctionCreator();
            var expectedName = $"Max {name}";

            INetworkCoverage networkCoverage = GetNetworkCoverage(null);
            var bedLevel = Substitute.For<IFunction>();
            
            // Call
            IFunction waterLevelFunction = functionCreator.CreateMaxWaterLevelFunction(networkCoverage, bedLevel);

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            Assert.That(waterLevelFunction.Name, Is.EqualTo(expectedName));
            
            bedLevel.Received(4).Evaluate<double>(Arg.Any<double>());
        }
        
        [Test]
        [TestCaseSource(nameof(GetMultiDimensionalArrayWithValues))]
        public void CreateMaxWaterLevelFunction_GivenValuesIncludingAtLeastOneValidValue_ReturnFunctionWithMaxWaterLevelValues(MultiDimensionalArray<double> mda)
        {
            // Setup
            SideViewFunctionCreator functionCreator = CreateSideViewFunctionCreator();
            var expectedName = $"Max {name}";
            
            INetworkCoverage networkCoverage = GetNetworkCoverage(mda);
            
            var bedLevel = Substitute.For<IFunction>();
            
            // Call
            IFunction waterLevelFunction = functionCreator.CreateMaxWaterLevelFunction(networkCoverage, bedLevel);

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            Assert.That(waterLevelFunction.Name, Is.EqualTo(expectedName));
            
            IMultiDimensionalArray values = waterLevelFunction.GetValues();
            
            bedLevel.Received(0).Evaluate<double>(Arg.Any<double>());
            
            Assert.That(values[0], Is.EqualTo(maxWaterLevel));
            Assert.That(values[1], Is.EqualTo(maxWaterLevel));
            Assert.That(values[2], Is.EqualTo(maxWaterLevel));
            Assert.That(values[3], Is.EqualTo(maxWaterLevel));
        }
        
        [Test]
        [TestCaseSource(nameof(GetMultiDimensionalArrayWithNanAndNoDataValue))]
        public void CreateMaxWaterLevelFunction_GivenGetMultiDimensionalArrayWithNanAndNoDataValue_ReturnFunctionWithMaxWaterLevelValuesSetToBedLevel(MultiDimensionalArray<double> mda)
        {
            // Setup
            SideViewFunctionCreator functionCreator = CreateSideViewFunctionCreator();
            const double bedLevelValue = 10.0d;
            var expectedName = $"Max {name}";

            INetworkCoverage networkCoverage = GetNetworkCoverage(mda);
            
            var bedLevel = Substitute.For<IFunction>();
            
            bedLevel.Evaluate<double>(Arg.Any<double>()).Returns(bedLevelValue);
            
            // Call
            IFunction waterLevelFunction = functionCreator.CreateMaxWaterLevelFunction(networkCoverage, bedLevel);

            // Assert
            Assert.That(waterLevelFunction, Is.Not.Null);
            Assert.That(waterLevelFunction.Name, Is.EqualTo(expectedName));
            
            IMultiDimensionalArray values = waterLevelFunction.GetValues();
            
            bedLevel.Received(4).Evaluate<double>(Arg.Any<double>());
            
            Assert.That(values[0], Is.EqualTo(bedLevelValue));
            Assert.That(values[1], Is.EqualTo(bedLevelValue));
            Assert.That(values[2], Is.EqualTo(bedLevelValue));
            Assert.That(values[3], Is.EqualTo(bedLevelValue));
        }
        
        private static IEnumerable<TestCaseData> GetMultiDimensionalArrayWithValues()
        {
            var mda = new MultiDimensionalArray<double>
            {
                10.0d,
                20.0d,
                30.0d,
                maxWaterLevel
            };

            yield return new TestCaseData(mda).SetName("MultiDimensionalArray with only valid values");
            
            mda = new MultiDimensionalArray<double>
            {
                noDataValue,
                20.0d,
                30.0d,
                maxWaterLevel
            };

            yield return new TestCaseData(mda).SetName("MultiDimensionalArray with valid values and one no data value");
            
            mda = new MultiDimensionalArray<double>
            {
                double.NaN,
                20.0d,
                30.0d,
                maxWaterLevel
            };

            yield return new TestCaseData(mda).SetName("MultiDimensionalArray with valid values and one nan value");
            
            mda = new MultiDimensionalArray<double>
            {
                double.NaN,
                20.0d,
                noDataValue,
                maxWaterLevel
            };

            yield return new TestCaseData(mda).SetName("MultiDimensionalArray with valid values and one no data value and one nan value");
            
            mda = new MultiDimensionalArray<double>
            {
                double.NaN,
                noDataValue,
                noDataValue,
                maxWaterLevel
            };

            yield return new TestCaseData(mda).SetName("MultiDimensionalArray with single valid value");
        }
        
        private static IEnumerable<TestCaseData> GetMultiDimensionalArrayWithNanAndNoDataValue()
        {
            var mda = new MultiDimensionalArray<double>
            {
                noDataValue,
                noDataValue,
                noDataValue,
                noDataValue
            };

            yield return new TestCaseData(mda).SetName("MultiDimensionalArray with only no data values");
            
            mda = new MultiDimensionalArray<double>
            {
                double.NaN,
                double.NaN,
                double.NaN,
                double.NaN
            };

            yield return new TestCaseData(mda).SetName("MultiDimensionalArray with only nan values");
            
            mda = new MultiDimensionalArray<double>
            {
                noDataValue,
                noDataValue,
                double.NaN,
                double.NaN
            };

            yield return new TestCaseData(mda).SetName("MultiDimensionalArray with a mix of no data values and nan values");
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
        
        private static SideViewFunctionCreator CreateSideViewFunctionCreator()
        {
            const double baseBottomLevel = 3.0;
            IHydroNetwork network = CreateNetworkWithTwoPipes(baseBottomLevel);
            Route route = CreateRoute(network);
            var functionCreator = new SideViewFunctionCreator(route,
                                                              Substitute.For<IDictionary<string, IFunction>>(),
                                                              Substitute.For<IList<IStructure1D>>(),
                                                              Substitute.For<IUnit>());
            return functionCreator;
        }
        
        
        private static INetworkCoverage GetNetworkCoverage(IMultiDimensionalArray<double> mda)
        {
            var networkCoverage = Substitute.For<INetworkCoverage>();
            networkCoverage.Name.Returns(name);
            var variable = Substitute.For<IVariable<INetworkLocation>>();
            variable.ValueType.Returns(typeof(NetworkLocation));
            networkCoverage.Locations.Returns(variable);
            networkCoverage.Components[0].NoDataValue = noDataValue;
            networkCoverage.GetValues<double>(Arg.Any<IVariableValueFilter>()).Returns(mda);
            return networkCoverage;
        }
    }
}