using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class FM1DFileFunctionStoreTest
    {
        [Test]
        public void OpenMap1DFileCheckFunctions()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            const string fmModelMap1DZip = "FM_model_map.zip";
            string fmModelMap1DZipFilePath = Path.Combine(testDataFilePath, fmModelMap1DZip);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(fmModelMap1DZipFilePath, tempDir);

                FM1DFileFunctionStore store = InitializeFM1DStore(tempDir);

                Assert.AreEqual(16, store.Functions.OfType<INetworkCoverage>().Count());
            });
        }

        [Test]
        public void OpenMap1DFileCheckStore()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            const string fmModelMap1DZip = "FM_model_map.zip";
            string fmModelMap1DZipFilePath = Path.Combine(testDataFilePath, fmModelMap1DZip);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(fmModelMap1DZipFilePath, tempDir);

                FM1DFileFunctionStore store = InitializeFM1DStore(tempDir);
                Assert.That(store.CoordinateSystem.AuthorityCode, Is.EqualTo(28992));
                const string fmModelMap1DNcFile = "FM_model_map.nc";
                string fmModelMap1DNcFilePath = Path.Combine(tempDir, fmModelMap1DNcFile);

                var network = (IHydroNetwork) ReadFrom1DMapFile(fmModelMap1DNcFilePath, OutputFM1DObjectType.Network);
                foreach (IHydroObject hydroObject in network.AllHydroObjects)
                {
                    hydroObject.Name += "_output";
                }

                Assert.That(store.OutputNetwork, Is.EqualTo(network).Using(new HydroNetworkComparer()));
                var discretization = (IDiscretization) ReadFrom1DMapFile(fmModelMap1DNcFilePath, OutputFM1DObjectType.Discretization);
                foreach (INetworkLocation hydroObject in discretization.Locations.AllValues)
                {
                    hydroObject.Name += "_output";
                }

                foreach (IHydroObject hydroObject in ((IHydroNetwork) discretization.Network).AllHydroObjects)
                {
                    hydroObject.Name += "_output";
                }

                Assert.That(store.OutputDiscretization, Is.EqualTo(discretization).Using(new DiscretizationComparer()));
            });
        }

        [Test]
        public void OpenMap1DFileCheckValues()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            const string fmModelMap1DZip = "FM_model_map.zip";
            string fmModelMap1DZipFilePath = Path.Combine(testDataFilePath, fmModelMap1DZip);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(fmModelMap1DZipFilePath, tempDir);

                FM1DFileFunctionStore store = InitializeFM1DStore(tempDir);

                var waterLevelFunction = (NetworkCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_s1");

                Assert.That(waterLevelFunction, Is.Not.Null);
                Assert.That(waterLevelFunction.Time.AllValues.Count, Is.EqualTo(2281));
                Assert.That(waterLevelFunction.Locations.AllValues.Count, Is.EqualTo(86));
                IMultiDimensionalArray timeSlice = waterLevelFunction.GetValues(new VariableValueFilter<DateTime>(waterLevelFunction.Time, new DateTime(1996, 1, 1, 1, 0, 0)));

                Assert.That(timeSlice.Count, Is.EqualTo(86)); // 86 locations for this timestep
                Assert.That((double) timeSlice[0], Is.EqualTo(0.30163).Within(0.001));

                var waterDischargeFunction = (NetworkCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_q1");

                Assert.That(waterDischargeFunction, Is.Not.Null);
                Assert.That(waterDischargeFunction.Time.AllValues.Count, Is.EqualTo(2281));
                Assert.That(waterDischargeFunction.Locations.AllValues.Count, Is.EqualTo(91));
                timeSlice = waterDischargeFunction.GetValues(new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)));

                Assert.That(timeSlice.Count, Is.EqualTo(91));
                Assert.That((double) timeSlice[88], Is.EqualTo(0.011).Within(0.001));
                timeSlice = waterDischargeFunction.GetValues(new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)));

                timeSlice = waterDischargeFunction.GetValues(new IVariableFilter[]
                {
                    new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)),
                    new VariableValueFilter<INetworkLocation>(waterDischargeFunction.Locations, waterDischargeFunction.Locations.Values.First(l => l.Branch.Name.Equals("1")))
                });
                Assert.That(timeSlice.Count, Is.EqualTo(1));
                Assert.That((double) timeSlice[0], Is.EqualTo(0.015).Within(0.001));
            });
        }

        [Test]
        public void OpenMap1DFileCheckValuesForASpecificLocation()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var fmModelMap1DZip = "FM_model_map.zip";
            string fmModelMap1DZipFilePath = Path.Combine(testDataFilePath, fmModelMap1DZip);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(fmModelMap1DZipFilePath, tempDir);

                FM1DFileFunctionStore store = InitializeFM1DStore(tempDir);

                var waterLevelFunction = (NetworkCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_s1");

                Assert.That(waterLevelFunction, Is.Not.Null);
                Assert.That(waterLevelFunction.Time.AllValues.Count, Is.EqualTo(2281));
                Assert.That(waterLevelFunction.Locations.AllValues.Count, Is.EqualTo(86));
                var locationsByNetworkDataType = TypeUtils.GetField<FM1DFileFunctionStore, IDictionary<string, IList<INetworkLocation>>>(store, "locationsByNetworkDataType");
                Assert.That(locationsByNetworkDataType["mesh1d_nNodes"].Count, Is.EqualTo(86));
                IMultiDimensionalArray timeSlice = waterLevelFunction.GetValues(
                    new VariableValueFilter<DateTime>(waterLevelFunction.Time, new DateTime(1996, 1, 1, 1, 0, 0)),
                    new VariableValueFilter<INetworkLocation>(waterLevelFunction.Locations, locationsByNetworkDataType["mesh1d_nNodes"][0]));

                Assert.That(timeSlice.Count, Is.EqualTo(1)); // filterd 1 location for this timestep
                Assert.That((double) timeSlice[0], Is.EqualTo(0.30163).Within(0.001));

                var waterDischargeFunction = (NetworkCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_q1");

                Assert.That(waterDischargeFunction, Is.Not.Null);
                Assert.That(waterDischargeFunction.Time.AllValues.Count, Is.EqualTo(2281));
                Assert.That(waterDischargeFunction.Locations.AllValues.Count, Is.EqualTo(91));
                Assert.That(locationsByNetworkDataType["mesh1d_nEdges"].Count, Is.EqualTo(91));
                timeSlice = waterDischargeFunction.GetValues(new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)));

                Assert.That(timeSlice.Count, Is.EqualTo(91));
                Assert.That((double) timeSlice[88], Is.EqualTo(0.011).Within(0.001));
                timeSlice = waterDischargeFunction.GetValues(new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)));

                timeSlice = waterDischargeFunction.GetValues(new IVariableFilter[]
                {
                    new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)),
                    new VariableValueFilter<INetworkLocation>(waterDischargeFunction.Locations, waterDischargeFunction.Locations.Values.First(l => l.Branch.Name.Equals("1")))
                });
                Assert.That(timeSlice.Count, Is.EqualTo(1));
                Assert.That((double) timeSlice[0], Is.EqualTo(0.015).Within(0.001));
                timeSlice = waterDischargeFunction.GetValues(new IVariableFilter[]
                {
                    new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)),
                    new VariableValueFilter<INetworkLocation>(waterDischargeFunction.Locations, locationsByNetworkDataType["mesh1d_nEdges"][86])
                });
                Assert.That(timeSlice.Count, Is.EqualTo(1));
                Assert.That((double) timeSlice[0], Is.EqualTo(0.015).Within(0.001));
            });
        }

        [Test]
        public void GivenFM1DFileFunctionStore_ReadingAnOutputMapFileWithCustomLengthBranches_ShouldNotLeadToErrors()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath("output_mapfiles\\CustomLengthFlowFM_map.nc");

            var network = new HydroNetwork();
            var node1 = new HydroNode{Geometry = new Point(0,0)};
            var node2 = new HydroNode { Geometry = new Point(10, 10) };
            var branch = new Branch("Channel_1D_1", node1, node2, 3600)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 10)
                })
            };

            network.Branches.Add(branch);

            var store = new FM1DFileFunctionStore(network);

            // Act
            // throws error if custom length of branch from input network is not applied to output network branch
            store.Path = path; 
            
            // Assert
            Assert.IsTrue(store.OutputNetwork.Branches[0].IsLengthCustom);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenClassMapFileWithTimeZones_ShouldSetReferenceDateInFunctionsCorrectly()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string classMapFilePathTemp = tempDirectory.CopyTestDataFileToTempDirectory(TestHelper.GetTestFilePath(@"output_mapfiles\FlowFM_TimeZone_map.nc"));

                // Act
                var network = (IHydroNetwork) ReadFrom1DMapFile(classMapFilePathTemp, OutputFM1DObjectType.Network);

                var store = new FM1DFileFunctionStore(network) {Path = classMapFilePathTemp};

                // Assert
                Assert.IsInstanceOf<FM1DFileFunctionStore>(store);

                string retrievedReferenceDate = ((ICoverage) store.Functions.First()).Time.Attributes["ncRefDate"];
                Assert.AreEqual("Monday, 01 March 2021 00:00:00", retrievedReferenceDate);
            }
        }

        private static FM1DFileFunctionStore InitializeFM1DStore(string tempDir)
        {
            const string fmModelMap1DNcFile = "FM_model_map.nc";
            string fmModelMap1DNcFilePath = Path.Combine(tempDir, fmModelMap1DNcFile);
            var network = (IHydroNetwork) ReadFrom1DMapFile(fmModelMap1DNcFilePath, OutputFM1DObjectType.Network);

            var store = new FM1DFileFunctionStore(network) {Path = fmModelMap1DNcFilePath};
            return store;
        }

        private static object ReadFrom1DMapFile(string netFilePath, OutputFM1DObjectType fm1DObjectType)
        {
            if (!File.Exists(netFilePath))
            {
                return null;
            }

            int numberOfNetworks = UGridFileHelper.GetNumberOfNetworks(netFilePath);
            if (numberOfNetworks != 1)
            {
                return null;
            }

            int numberOfNetworkDiscretizations = UGridFileHelper.GetNumberOfNetworkDiscretizations(netFilePath);
            if (numberOfNetworkDiscretizations != 1)
            {
                return null;
            }

            if (!UGridFileHelper.IsUGridFile(netFilePath))
            {
                return null;
            }

            IList<BranchProperties> branchData = NetworkPropertiesHelper.ReadPropertiesPerBranchFromFile(netFilePath);
            IList<CompartmentProperties> compartmentData = NetworkPropertiesHelper.ReadPropertiesPerNodeFromFile(netFilePath);
            var discretization = new Discretization();
            var network = new HydroNetwork();
            UGridFileHelper.ReadNetworkAndDiscretisation(netFilePath, discretization, network, compartmentData,
                                                         branchData, true);
            switch (fm1DObjectType)
            {
                case OutputFM1DObjectType.Network:
                    return network;
                case OutputFM1DObjectType.Discretization:
                    return discretization;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fm1DObjectType), fm1DObjectType, null);
            }
        }

        private enum OutputFM1DObjectType
        {
            Network,
            Discretization
        }
    }
}