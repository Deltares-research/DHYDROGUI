using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class UGridToNetworkAdapterIntegrationTest
    {
        private string testDirectory;
        private string netFilePath;

        [SetUp]
        public void Setup()
        {
            testDirectory = FileUtils.CreateTempDirectory();
            netFilePath = Path.Combine(testDirectory, "myNetFile.nc");
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(testDirectory);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SaveAndLoadNetworkTest(bool useSimpleNetwork)
        {
            var networkDiscretisation = useSimpleNetwork 
                ? TestNetworkAndDiscretisationProvider.CreateSimpleNetworkAndDiscretisation() 
                : TestNetworkAndDiscretisationProvider.CreateNetworkAndDiscretisation();
            var storedNetwork = (IHydroNetwork)networkDiscretisation.Network;
            var networkDataModel = new NetworkUGridDataModel(storedNetwork);

            var metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");
            UGridToNetworkAdapter.SaveNetwork(netFilePath, networkDataModel, metaData);

            var networkUGridDataModel = UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(netFilePath);
            CompareAndAssertNetworkAndNetworkUGridDataModel(storedNetwork, networkUGridDataModel);
        }

        [Test]
        public void SaveAndLoadNetwork_WithCustomizedLengthBranchTest()
        {
            var networkDiscretisation = TestNetworkAndDiscretisationProvider.CreateSimpleNetworkAndDiscretisation();
            var storedNetwork = (IHydroNetwork) networkDiscretisation.Network;

            var customizedBranch = storedNetwork.Branches.First();
            customizedBranch.IsLengthCustom = true;
            var length = customizedBranch.Length + 1000;
            customizedBranch.Length = length;

            Assert.That(customizedBranch.Length, Is.EqualTo(length));

            var metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");
            var networkDataModel = new NetworkUGridDataModel(storedNetwork);
            UGridToNetworkAdapter.SaveNetwork(netFilePath, networkDataModel, metaData);
            var loadedNetwork = NetworkDiscretisationFactory.CreateHydroNetwork(UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(netFilePath));

            var loadedCustomizedBranch = loadedNetwork.Branches.First();
            Assert.That(loadedCustomizedBranch.Length, Is.EqualTo(length));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SaveAndLoadNetworkAndDiscretisationTest(bool useSimpleNetwork)
        {
            var networkDiscretisation = useSimpleNetwork
                ? TestNetworkAndDiscretisationProvider.CreateSimpleNetworkAndDiscretisation()
                : TestNetworkAndDiscretisationProvider.CreateNetworkAndDiscretisation();
            var storedNetwork = (IHydroNetwork)networkDiscretisation.Network;

            var networkDataModel = new NetworkUGridDataModel(storedNetwork);
            var discretisationDataModel = new NetworkDiscretisationUGridDataModel(networkDiscretisation);

            var metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");
            UGridToNetworkAdapter.SaveNetwork(netFilePath, networkDataModel, metaData);
            UGridToNetworkAdapter.SaveNetworkDiscretisation(netFilePath, discretisationDataModel);

            var loadedDiscretisation = UGridToNetworkAdapter.LoadNetworkAndDiscretisation(netFilePath);
            Assert.NotNull(loadedDiscretisation);

            var loadedNetwork = (IHydroNetwork) loadedDiscretisation.Network;

            HydroNetworkTestHelper.CompareNetworks(storedNetwork, loadedNetwork);
            HydroNetworkTestHelper.CompareDiscretisations(networkDiscretisation, loadedDiscretisation);
        }

        [Test]
        [Ignore("you cannot save network with same name. Old one is not thrown away")]
        public void GivenNetworkAndDiscretisation_WhenSavingNetworkTwiceAtTheSameLocation_ThenNoExceptionsAreThrown()
        {
            var networkDiscretisation = TestNetworkAndDiscretisationProvider.CreateSimpleNetworkAndDiscretisation();
            var storedNetwork = (IHydroNetwork)networkDiscretisation.Network;

            var networkDataModel = new NetworkUGridDataModel(storedNetwork);
            var discretisationDataModel = new NetworkDiscretisationUGridDataModel(networkDiscretisation);

            var metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");
            UGridToNetworkAdapter.SaveNetwork(netFilePath, networkDataModel, metaData);
            UGridToNetworkAdapter.SaveNetworkDiscretisation(netFilePath, discretisationDataModel);


            TestHelper.AssertLogMessagesCount(() => UGridToNetworkAdapter.SaveNetwork(netFilePath, networkDataModel, metaData), 0);
            // For reader: This test is failing at the second time that we try to save a network in the same netCDF file.
            // Debug through method UGridToNetworkAdapter.SaveNetwork!
        }

        [Test]
        [Ignore("you cannot save discretization with same name. Old one is not thrown away")]
        public void GivenNetworkAndDiscretisation_WhenSavingNetworkDiscretisationTwiceAtTheSameLocation_ThenNoExceptionsAreThrown()
        {
            var networkDiscretisation = TestNetworkAndDiscretisationProvider.CreateSimpleNetworkAndDiscretisation();
            var storedNetwork = (IHydroNetwork)networkDiscretisation.Network;

            var networkDataModel = new NetworkUGridDataModel(storedNetwork);
            var discretisationDataModel = new NetworkDiscretisationUGridDataModel(networkDiscretisation);

            var metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");
            UGridToNetworkAdapter.SaveNetwork(netFilePath, networkDataModel, metaData);
            UGridToNetworkAdapter.SaveNetworkDiscretisation(netFilePath, discretisationDataModel);

            TestHelper.AssertLogMessagesCount(() => UGridToNetworkAdapter.SaveNetworkDiscretisation(netFilePath, discretisationDataModel), 0);
            // For reader: This test is failing at the second time that we try to save a network discretisation in the same netCDF file.
            // Debug through method UGridToNetworkAdapter.SaveNetworkDiscretisation!
        }

        [Test]
        [Ignore]
        public void GivenSimpleSewerNetwork_WhenSavingAndLoadingNetwork_ThenTheNetworkIsUnchanged()
        {
            const string pipeName = "myPipe";
            var sewerNetwork = TestNetworkAndDiscretisationProvider.CreateSimpleSewerNetwork(pipeName);
            var networkDataModel = new NetworkUGridDataModel(sewerNetwork);

            var metaData = new UGridGlobalMetaData(sewerNetwork.Name, "PluginName", "PluginVersion");
            UGridToNetworkAdapter.SaveNetwork(netFilePath, networkDataModel, metaData);
            var loadedNetwork = NetworkDiscretisationFactory.CreateHydroNetwork(UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(netFilePath));

            Assert.That(loadedNetwork.Pipes.Count(), Is.EqualTo(1));
            Assert.That(loadedNetwork.Manholes.Count(), Is.EqualTo(2), "Work in progress: still work to be done for retention file writer to be able to distinguish between types of nodes");
        }

        [Test]
        public void GivenNetCdfFileWithoutNetworkData_WhenReadingNetworkDataModelFromThisFile_ThenDefaultNetworkDataModelIsReturned()
        {
            var gridFilePath = TestHelper.GetTestFilePath(@"NetCDF files\FlowFM_net.nc"); // NetCDF file with only a 1 x 1-grid
            var testFilePath = TestHelper.CreateLocalCopy(gridFilePath);

            var networkDataModel = UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(testFilePath);
            Assert.IsNotNull(networkDataModel);
            Assert.That(networkDataModel.NumberOfBranches, Is.EqualTo(0));
        }

        private void CompareAndAssertNetworkAndNetworkUGridDataModel(IHydroNetwork storedNetwork, NetworkUGridDataModel networkUGridDataModel)
        {
            foreach (var branch in storedNetwork.Branches)
            {
                Assert.That(networkUGridDataModel.BranchNames.Contains(branch.Name));
                var index = Array.IndexOf(networkUGridDataModel.BranchNames, branch.Name);
                
                Assert.That(networkUGridDataModel.BranchLengths[index], Is.EqualTo(branch.Length));
                Assert.That(networkUGridDataModel.NumberOfGeometryPointsPerBranch[index], Is.EqualTo(branch.Geometry.Coordinates.Length));
                Assert.That(networkUGridDataModel.BranchDescriptions[index], Is.EqualTo(branch.Description));
            }

            CompareBranchGeometricalPoints(storedNetwork, networkUGridDataModel);

            foreach (var node in storedNetwork.Nodes)
            {
                var index = Array.IndexOf(networkUGridDataModel.NodesNames, node.Name);
                Assert.That(index >= 0);

                Assert.That(networkUGridDataModel.NodesX[index], Is.EqualTo(node.Geometry.Coordinate.X));
                Assert.That(networkUGridDataModel.NodesY[index], Is.EqualTo(node.Geometry.Coordinate.Y));
                Assert.That(networkUGridDataModel.NodesDescriptions[index], Is.EqualTo(node.Description));
            }

            Assert.That(networkUGridDataModel.NumberOfBranches, Is.EqualTo(storedNetwork.Branches.Count));
            Assert.That(networkUGridDataModel.NumberOfNodes, Is.EqualTo(storedNetwork.Nodes.Count));
            Assert.That(networkUGridDataModel.NumberOfGeometryPoints, Is.EqualTo(storedNetwork.Branches.Sum(b => b.Geometry.Coordinates.Length)));
        }

        private static void CompareBranchGeometricalPoints(IHydroNetwork storedNetwork, NetworkUGridDataModel networkUGridDataModel)
        {
            var geoCoordsPerBranch = storedNetwork.Branches.Select(b => b.Geometry.Coordinates);
            var coordinates = new List<Coordinate>();
            foreach (var coordinateSequence in geoCoordsPerBranch)
            {
                coordinates.AddRange(coordinateSequence);
            }

            var geoPointsX = networkUGridDataModel.GeopointsX;
            var geoPointsY = networkUGridDataModel.GeopointsY;
            for (var n = 0; n < coordinates.Count; n++)
            {
                Assert.That(geoPointsX[n], Is.EqualTo(coordinates[n].X));
                Assert.That(geoPointsY[n], Is.EqualTo(coordinates[n].Y));
            }
        }
    }
}
