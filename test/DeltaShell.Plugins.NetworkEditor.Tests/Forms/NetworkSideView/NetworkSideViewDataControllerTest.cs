using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class NetworkSideViewDataControllerTest
    {
        [Test, ExpectedException(ExpectedException = typeof(InvalidOperationException), ExpectedMessage = "Network of added spatial data does not match network of the route.")]
        public void TestAddingCoverageWithoutNetwork()
        {
            var route = new Route {Network = new HydroNetwork()};
            var dataController = new NetworkSideViewDataController(route, null, null);
            var networkCoverage = new NetworkCoverage("Coverage", true);
            Assert.IsNull(networkCoverage.Network);

            dataController.AddRenderedCoverage(networkCoverage);
        }

        [Test, ExpectedException(ExpectedException = typeof(InvalidOperationException), ExpectedMessage = "Network of added spatial data does not match network of the route.")]
        public void TestAddingCoverageWithDifferentNetwork()
        {
            var route = new Route { Network = new HydroNetwork() };
            var dataController = new NetworkSideViewDataController(route, null, null);
            var networkCoverage = new NetworkCoverage("Coverage", true) {Network = new HydroNetwork()};
            
            dataController.AddRenderedCoverage(networkCoverage);
        }

        [Test, ExpectedException(ExpectedException = typeof(InvalidOperationException), ExpectedMessage = "Network spatial data not known in sideview data.")]
        public void TestAddingCoverageThatIsNotInAllNetworkCoverages()
        {
            var hydroNetwork = new HydroNetwork();
            var dataController = new NetworkSideViewDataController(new Route { Network = hydroNetwork }, null, null);
            var networkCoverage = new NetworkCoverage("Coverage", true) { Network = hydroNetwork };

            dataController.AddRenderedCoverage(networkCoverage);
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

    }
}