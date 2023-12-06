using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DelftTools.Units;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class NetworkSideViewDataControllerTest
    {
        private const double noDataValue = -999.0d;
        private const double maxWaterLevel = 40.0d;
        
        [Test]
        public void TestAddingCoverageWithoutNetwork()
        {
            var route = new Route {Network = new HydroNetwork()};
            var dataController = new NetworkSideViewDataController(route, null, null);
            var networkCoverage = new NetworkCoverage("Coverage", true);
            Assert.IsNull(networkCoverage.Network);

            var error = Assert.Throws<InvalidOperationException>(() =>
            {
                dataController.AddRenderedCoverage(networkCoverage);
            });
            Assert.AreEqual("Network of added spatial data does not match network of the route.", error.Message);
        }

        [Test]
        public void TestAddingCoverageWithDifferentNetwork()
        {
            var route = new Route { Network = new HydroNetwork() };
            var dataController = new NetworkSideViewDataController(route, null, null);
            var networkCoverage = new NetworkCoverage("Coverage", true) {Network = new HydroNetwork()};

            var error = Assert.Throws<InvalidOperationException>(() =>
            {
                dataController.AddRenderedCoverage(networkCoverage);
            });
            Assert.AreEqual("Network of added spatial data does not match network of the route.", error.Message);
        }

        [Test]
        public void TestAddingCoverageThatIsNotInAllNetworkCoverages()
        {
            var hydroNetwork = new HydroNetwork();
            var dataController = new NetworkSideViewDataController(new Route { Network = hydroNetwork }, null, null);
            var networkCoverage = new NetworkCoverage("Coverage", true) { Network = hydroNetwork };

            var error = Assert.Throws<InvalidOperationException>(() =>
            {
                dataController.AddRenderedCoverage(networkCoverage);
            });
            Assert.AreEqual("Network spatial data not known in sideview data.", error.Message);
        }

        [Test]
        public void SetNetworkToNullShouldNotCrash()
        {
            var route = new Route { Network = new HydroNetwork() };
            using (new NetworkSideViewDataController(route, null, null))
            {
                route.Network = null;
            }
        }

        [Test]
        public void TestRemovingNetworkCoverageThatIsRenderedWithFilter()
        {
            var hydroNetwork = new HydroNetwork();
            var networkCoverage = new NetworkCoverage("Coverage", true) {Network = hydroNetwork};
            var filteredNetworkCoverage = (NetworkCoverage)networkCoverage.Filter();

            var dataController = new NetworkSideViewDataController(new Route {Network = hydroNetwork}, null, null)
                                     {
                                         AllNetworkCoverages = new List<INetworkCoverage> { filteredNetworkCoverage }
                                     };

            dataController.AddRenderedCoverage(filteredNetworkCoverage);
            Assert.AreEqual(1, dataController.RenderedNetworkCoverages.Count);

            dataController.RemoveRenderedCoverage(networkCoverage);
            Assert.AreEqual(0, dataController.RenderedNetworkCoverages.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void BedLevelNetworkCoverageShouldRefreshAtBranchLengthChange()
        {
            var hydroNetwork = new HydroNetwork();
            var branch = new Branch()
                             {
                                 Name = "branch",
                                 Network = hydroNetwork,
                                 IsLengthCustom = true,
                                 Length = 100,
                                 OrderNumber = 1,
                                 Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 0) })
                             };

            hydroNetwork.Branches.Add(branch);
            var route = new Route() {Network = hydroNetwork};
            hydroNetwork.Routes.Add(route);

            var dataController = new NetworkSideViewDataController(route, null, null);
            var profileCoverages = dataController.ProfileNetworkCoverages;
            
            Assert.IsNotEmpty(profileCoverages.ToList(),"profile coverages exist");

            branch.Length = 200;

            Assert.IsNotEmpty(dataController.ProfileNetworkCoverages.ToList(),
                              "profile coverages exist after branch length change.");
            Assert.IsFalse(dataController.ProfileNetworkCoverages.Equals(profileCoverages),
                           "profile coverages have been recreated after branch length change");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void BedLevelNetworkCoverageShouldRefreshAtBranchOrderNumberChange()
        {
            var hydroNetwork = new HydroNetwork();
            var branch = new Branch()
            {
                Name = "branch",
                Network = hydroNetwork,
                IsLengthCustom = true,
                Length = 100,
                OrderNumber = 1,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 0) })
            };

            hydroNetwork.Branches.Add(branch);
            var route = new Route() { Network = hydroNetwork };
            hydroNetwork.Routes.Add(route);

            var dataController = new NetworkSideViewDataController(route, null, null);
            var profileCoverages = dataController.ProfileNetworkCoverages;

            Assert.IsNotEmpty(profileCoverages.ToList(), "profile coverages exist");
            
            branch.OrderNumber = 2;

            Assert.IsNotEmpty(dataController.ProfileNetworkCoverages.ToList(),
                              "profile coverages exist after branch order number change.");
            Assert.IsFalse(dataController.ProfileNetworkCoverages.Equals(profileCoverages),
                           "profile coverages have been recreated after branch order number change");
            
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void BedLevelNetworkCoverageShouldNotRefreshAtBranchNameChange()
        {
            var hydroNetwork = new HydroNetwork();
            var branch = new Branch()
            {
                Name = "branch",
                Network = hydroNetwork,
                IsLengthCustom = true,
                Length = 100,
                OrderNumber = 1,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 0) })
            };

            hydroNetwork.Branches.Add(branch);
            var route = new Route() { Network = hydroNetwork };
            hydroNetwork.Routes.Add(route);

            var dataController = new NetworkSideViewDataController(route, null, null);
            var profileCoverages = dataController.ProfileNetworkCoverages;

            Assert.IsNotEmpty(profileCoverages.ToList(), "profile coverages exist");

            branch.Name = "other";

            Assert.IsNotEmpty(dataController.ProfileNetworkCoverages.ToList(),
                             "profile coverages exist after branch name change.");
            Assert.IsTrue(dataController.ProfileNetworkCoverages.Equals(profileCoverages),
                           "profile coverages have not been recreated after branch name change");
        }


        [Test]
        public void UpdateMinMaxFromFunctionValuesShouldNotReturnNaN()
        {
            double minValue = double.NaN;
            double maxValue = double.NaN;

            var nc1 = new Function();
            nc1.Arguments.Add(new Variable<int>());
            nc1.Components.Add(new Variable<double>());
            var nc2 = new Function();
            nc2.Arguments.Add(new Variable<int>());
            nc2.Components.Add(new Variable<double>());

            nc1[0] = 1.1d;
            nc1[1] = 2.2d;
            nc2[0] = 3.3d;
            nc2[1] = double.NaN;

            NetworkSideViewDataController.UpdateMinMaxFromFunctionValues(nc1, ref minValue, ref maxValue);
            NetworkSideViewDataController.UpdateMinMaxFromFunctionValues(nc2, ref minValue, ref maxValue);

            Assert.AreEqual(1.1d, minValue);
            Assert.AreEqual(3.3d, maxValue);

            minValue = maxValue = double.NaN;

            nc1[0] = double.NaN;
            nc1[1] = double.NaN;

            NetworkSideViewDataController.UpdateMinMaxFromFunctionValues(nc1, ref minValue, ref maxValue);
            NetworkSideViewDataController.UpdateMinMaxFromFunctionValues(nc2, ref minValue, ref maxValue);

            Assert.AreEqual(3.3d, minValue);
            Assert.AreEqual(3.3d, maxValue);
        }
        
        
        [Test]
        public void UpdateMinMaxFromFunctionValuesShouldNotReturnNoDataValues()
        {
            double minValue = double.NaN;
            double maxValue = double.NaN;
            const double noDataValue = -999d;

            var nc1 = new Function();
            nc1.Arguments.Add(new Variable<int>());
            nc1.Components.Add(new Variable<double>());
            nc1.Components.First().NoDataValue = noDataValue;
            var nc2 = new Function();
            nc2.Arguments.Add(new Variable<int>());
            nc2.Components.Add(new Variable<double>());
            nc2.Components.First().NoDataValue = noDataValue;

            nc1[0] = 1.1d;
            nc1[1] = 2.2d;
            nc2[0] = 3.3d;
            nc2[1] = noDataValue;

            NetworkSideViewDataController.UpdateMinMaxFromFunctionValues(nc1, ref minValue, ref maxValue);
            NetworkSideViewDataController.UpdateMinMaxFromFunctionValues(nc2, ref minValue, ref maxValue);

            Assert.AreEqual(1.1d, minValue);
            Assert.AreEqual(3.3d, maxValue);
        }
        
        [Test]
        [TestCaseSource(nameof(GetMultiDimensionalArrayWithValues))]
        public void GivenRouteForSettingUpFromScratch_WhenMaxWaterLevelFunction_ThenReturnExpectedWaterLevels(MultiDimensionalArray<double> mda)
        {
            // Arrange
            NetworkSideViewDataController dataController = CreateNetworkSideViewDataController(mda);
            
            // Act
            IFunction maxWaterLevelFunction = dataController.MaxWaterLevelFunction;
            IMultiDimensionalArray values = maxWaterLevelFunction.GetValues();
            
            // Assert
            Assert.That(values[0], Is.EqualTo(maxWaterLevel));
            Assert.That(values[1], Is.EqualTo(maxWaterLevel));
            Assert.That(values[2], Is.EqualTo(maxWaterLevel));
            Assert.That(values[3], Is.EqualTo(maxWaterLevel));
        }
        
        [Test]
        [TestCaseSource(nameof(GetMultiDimensionalArrayWithValues))]
        public void GivenRouteForSettingUpWithCaching_WhenMaxWaterLevelFunction_ThenReturnExpectedWaterLevels(MultiDimensionalArray<double> mda)
        {
            // Arrange
            NetworkSideViewDataController dataController = CreateNetworkSideViewDataController(mda);
            
            IFunction maxWaterLevelFunction = dataController.MaxWaterLevelFunction;
            IMultiDimensionalArray values = maxWaterLevelFunction.GetValues();
            
            // Act
            IFunction maxWaterLevelFunction2 = dataController.MaxWaterLevelFunction;
            IMultiDimensionalArray values2 = maxWaterLevelFunction2.GetValues();
            
            // Assert
            Assert.That(values2[0], Is.EqualTo(maxWaterLevel));
            Assert.That(values2[1], Is.EqualTo(maxWaterLevel));
            Assert.That(values2[2], Is.EqualTo(maxWaterLevel));
            Assert.That(values2[3], Is.EqualTo(maxWaterLevel));
            
            Assert.That(values2[0], Is.EqualTo(values[0]));
            Assert.That(values2[1], Is.EqualTo(values[1]));
            Assert.That(values2[2], Is.EqualTo(values[2]));
            Assert.That(values2[3], Is.EqualTo(values[3]));
        }

        private static NetworkSideViewDataController CreateNetworkSideViewDataController(MultiDimensionalArray<double> mda)
        {
            IHydroNetwork network = CreateNetworkWithTwoPipes(3.0);
            Route route = CreateRoute(network);
            var nsvcm = new NetworkSideViewCoverageManager(route, null, Substitute.For<IEnumerable<ICoverage>>());

            var dataController = new NetworkSideViewDataController(route, nsvcm, null);
            var bedLevel = GetNetworkCoverage(mda);
            bedLevel.Name.Returns(BedLevelNetworkCoverageBuilder.BedLevelCoverageName);
            bedLevel.Time.Values.Count.Returns(1);
            dataController.ProfileNetworkCoverages[0] = bedLevel;
            
            var coverage = Substitute.For<INetworkCoverage>();
            coverage.IsTimeDependent.Returns(true);
            coverage.Name.Returns("water level (mesh1d_s1)");
            coverage.Network.Returns(route.Network);

            var parentCoverage = GetNetworkCoverage(mda);
            coverage.Parent.Returns(parentCoverage);
            nsvcm.OnCoverageAddedToProject.Invoke(coverage);
            return dataController;
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
        
        private static INetworkCoverage GetNetworkCoverage(IMultiDimensionalArray<double> mda)
        {
            var networkCoverage = Substitute.For<INetworkCoverage>();
            var variable = Substitute.For<IVariable<INetworkLocation>>();
            variable.ValueType.Returns(typeof(NetworkLocation));
            networkCoverage.Locations.Returns(variable);
            networkCoverage.Components[0].NoDataValue = noDataValue;
            networkCoverage.GetValues<double>(Arg.Any<IVariableValueFilter>()).Returns(mda);
            return networkCoverage;
        }
    }
}