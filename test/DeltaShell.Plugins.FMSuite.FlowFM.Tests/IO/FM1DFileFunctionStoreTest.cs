using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

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
            var fmModelMap1DZip = "FM_model_map.zip";
            string fmModelMap1DZipFilePath = Path.Combine(testDataFilePath, fmModelMap1DZip);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(fmModelMap1DZipFilePath, tempDir);

                var store = InitializeFM1DStore(tempDir);

                Assert.AreEqual(16, store.Functions.OfType<INetworkCoverage>().Count());
            });
        }

        private FM1DFileFunctionStore InitializeFM1DStore(string tempDir)
        {
            var fmModelMap1DNcFile = "FM_model_map.nc";
            string fmModelMap1DNcFilePath = Path.Combine(tempDir, fmModelMap1DNcFile);
            var network = (IHydroNetwork)ReadFrom1DMapFile(fmModelMap1DNcFilePath, OutputFM1DObjectType.Network);

            var store = new FM1DFileFunctionStore(network) {Path = fmModelMap1DNcFilePath};
            return store;
        }

        [Test]
        public void OpenMap1DFileCheckStore()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var fmModelMap1DZip = "FM_model_map.zip";
            string fmModelMap1DZipFilePath = Path.Combine(testDataFilePath, fmModelMap1DZip);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(fmModelMap1DZipFilePath, tempDir);

                var store = InitializeFM1DStore(tempDir);
                Assert.That(store.CoordinateSystem.AuthorityCode, Is.EqualTo(28992));
                var fmModelMap1DNcFile = "FM_model_map.nc";
                string fmModelMap1DNcFilePath = Path.Combine(tempDir, fmModelMap1DNcFile);

                var network = (IHydroNetwork) ReadFrom1DMapFile(fmModelMap1DNcFilePath, OutputFM1DObjectType.Network);
                foreach (var hydroObject in network.AllHydroObjects)
                {
                    hydroObject.Name = hydroObject.Name + "_output";
                }
                Assert.That(store.OutputNetwork, Is.EqualTo(network).Using(new HydroNetworkComparer()));
                var discretization = (IDiscretization)ReadFrom1DMapFile(fmModelMap1DNcFilePath,OutputFM1DObjectType.Discretization);
                foreach (var hydroObject in discretization.Locations.AllValues)
                {
                    hydroObject.Name = hydroObject.Name + "_output";
                }
                foreach (var hydroObject in ((IHydroNetwork)discretization.Network).AllHydroObjects)
                {
                    hydroObject.Name = hydroObject.Name + "_output";
                }
                Assert.That(store.OutputDiscretization, Is.EqualTo(discretization).Using(new DiscretizationComparer()));
            });
        }

        [Test]
        public void OpenMap1DFileCheckValues()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var fmModelMap1DZip = "FM_model_map.zip";
            string fmModelMap1DZipFilePath = Path.Combine(testDataFilePath, fmModelMap1DZip);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(fmModelMap1DZipFilePath, tempDir);

                var store = InitializeFM1DStore(tempDir);

                var waterLevelFunction = (NetworkCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_s1");

                Assert.That(waterLevelFunction, Is.Not.Null);
                Assert.That(waterLevelFunction.Time.AllValues.Count, Is.EqualTo(2281));
                Assert.That(waterLevelFunction.Locations.AllValues.Count, Is.EqualTo(86));
                IMultiDimensionalArray timeSlice = waterLevelFunction.GetValues(new VariableValueFilter<DateTime>(waterLevelFunction.Time, new DateTime(1996, 1, 1,1,0,0)));

                Assert.That(timeSlice.Count, Is.EqualTo(86)); // 86 locations for this timestep
                Assert.That((double)timeSlice[0], Is.EqualTo(0.30163).Within(0.001));

                var waterDischargeFunction = (NetworkCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_q1");

                Assert.That(waterDischargeFunction, Is.Not.Null);
                Assert.That(waterDischargeFunction.Time.AllValues.Count, Is.EqualTo(2281));
                Assert.That(waterDischargeFunction.Locations.AllValues.Count, Is.EqualTo(91));
                timeSlice = waterDischargeFunction.GetValues(new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1,0,1,0)));

                Assert.That(timeSlice.Count, Is.EqualTo(91));
                Assert.That((double)timeSlice[88], Is.EqualTo(0.011).Within(0.001));timeSlice = waterDischargeFunction.GetValues(new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1,0,1,0)));

                timeSlice = waterDischargeFunction.GetValues(new IVariableFilter[]{new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)), new VariableValueFilter<INetworkLocation>(waterDischargeFunction.Locations, waterDischargeFunction.Locations.Values.First(l => l.Branch.Name.Equals("1")))});
                Assert.That(timeSlice.Count, Is.EqualTo(1));
                Assert.That((double)timeSlice[0], Is.EqualTo(0.015).Within(0.001));
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

                var store = InitializeFM1DStore(tempDir);

                var waterLevelFunction = (NetworkCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_s1");

                Assert.That(waterLevelFunction, Is.Not.Null);
                Assert.That(waterLevelFunction.Time.AllValues.Count, Is.EqualTo(2281));
                Assert.That(waterLevelFunction.Locations.AllValues.Count, Is.EqualTo(86));
                Assert.That(store.LocationsByNetworkDataType["mesh1d_nNodes"].Count, Is.EqualTo(86));
                IMultiDimensionalArray timeSlice = waterLevelFunction.GetValues(
                    new VariableValueFilter<DateTime>(waterLevelFunction.Time, new DateTime(1996, 1, 1,1,0,0)), 
                    new VariableValueFilter<INetworkLocation>(waterLevelFunction.Locations, store.LocationsByNetworkDataType["mesh1d_nNodes"][0]));

                Assert.That(timeSlice.Count, Is.EqualTo(1)); // filterd 1 location for this timestep
                Assert.That((double)timeSlice[0], Is.EqualTo(0.30163).Within(0.001));

                var waterDischargeFunction = (NetworkCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_q1");

                Assert.That(waterDischargeFunction, Is.Not.Null);
                Assert.That(waterDischargeFunction.Time.AllValues.Count, Is.EqualTo(2281));
                Assert.That(waterDischargeFunction.Locations.AllValues.Count, Is.EqualTo(91));
                Assert.That(store.LocationsByNetworkDataType["mesh1d_nEdges"].Count, Is.EqualTo(91));
                timeSlice = waterDischargeFunction.GetValues(new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1,0,1,0)));

                Assert.That(timeSlice.Count, Is.EqualTo(91));
                Assert.That((double)timeSlice[88], Is.EqualTo(0.011).Within(0.001));timeSlice = waterDischargeFunction.GetValues(new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1,0,1,0)));

                timeSlice = waterDischargeFunction.GetValues(new IVariableFilter[]{new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)), new VariableValueFilter<INetworkLocation>(waterDischargeFunction.Locations, waterDischargeFunction.Locations.Values.First(l => l.Branch.Name.Equals("1")))});
                Assert.That(timeSlice.Count, Is.EqualTo(1));
                Assert.That((double)timeSlice[0], Is.EqualTo(0.015).Within(0.001));
                timeSlice = waterDischargeFunction.GetValues(new IVariableFilter[]{new VariableValueFilter<DateTime>(waterDischargeFunction.Time, new DateTime(1996, 1, 1, 0, 1, 0)), new VariableValueFilter<INetworkLocation>(waterDischargeFunction.Locations, store.LocationsByNetworkDataType["mesh1d_nEdges"][86])});
                Assert.That(timeSlice.Count, Is.EqualTo(1));
                Assert.That((double)timeSlice[0], Is.EqualTo(0.015).Within(0.001));
            });
        }

        private object ReadFrom1DMapFile(string netFilePath, OutputFM1DObjectType fm1DObjectType)
        {

            if (!File.Exists(netFilePath)) return null;

            int numberOfNetworks = UGridFileHelper.GetNumberOfNetworks(netFilePath);
            if (numberOfNetworks != 1) return null;

            int numberOfNetworkDiscretisations = UGridFileHelper.GetNumberOfNetworkDiscretizations(netFilePath);
            if (numberOfNetworkDiscretisations != 1) return null;


            if (!UGridFileHelper.IsUGridFile(netFilePath)) return null;

            var branchData = NetworkPropertiesHelper.ReadPropertiesPerBranchFromFile(netFilePath);
            var compartmentData = NetworkPropertiesHelper.ReadPropertiesPerNodeFromFile(netFilePath);
            var discretization = new Discretization();
            var network = new HydroNetwork();
            UGridFileHelper.ReadNetworkAndDiscretisation(netFilePath, discretization, network, compartmentData,
                branchData);
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