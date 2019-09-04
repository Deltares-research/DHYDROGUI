using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.TestUtils;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class SpatialDataFileWriterTests
    {
        private List<NetworkLocation> locations;
        private IHydroNetwork network;

        [SetUp]
        public void SetUp()
        {
            //50 branches 
            network = HydroNetworkHelper.GetSnakeHydroNetwork(50, true);

            //10 offsets
            var offsets = new[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 };

            //500 locations
            locations = offsets.SelectMany(o => network.Branches, (o, b) => new NetworkLocation(b, o)).ToList();
        }

        [TearDown]
        public void TearDown()
        {
        }
        
        [Test]
        public void TestSpatialDataFileWriterInitialWaterLevelConstantInterPolType()
        {
            var initialFlow = new NetworkCoverage("Initial Water Flow", false, "Water Flow", "m³/s") { Network = network };
            initialFlow.Locations.InterpolationType = InterpolationType.Constant;
            initialFlow.Locations.SetValues(locations.OrderBy(l => l));
            initialFlow.SetValues(Enumerable.Range(0, 50).Select(i => i * 10.0));

            var targetDirectory = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var initialWaterLevelFile = Path.Combine(targetDirectory, SpatialDataFileNames.InitialWaterLevel);
            SpatialDataFileWriter.WriteFile(initialWaterLevelFile, SpatialDataQuantity.InitialWaterLevel, initialFlow);
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(initialWaterLevelFile);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, categories.Count(c => c.Name == SpatialDataRegion.ContentIniHeader));
            Assert.AreEqual(500, categories.Count(d => d.Name == SpatialDataRegion.DefinitionIniHeader));

            var content = categories.Where(c => c.Name == SpatialDataRegion.ContentIniHeader).ToList().First();

            var quantityProperty = content.Properties.First(p => p.Name == SpatialDataRegion.Quantity.Key);
            Assert.AreEqual(SpatialDataQuantity.InitialWaterLevel, quantityProperty.Value);
            
            var interpolateProperty = content.Properties.First(p => p.Name == SpatialDataRegion.Interpolate.Key);
            Assert.AreEqual("0", interpolateProperty.Value);

            var definition = categories.Where(c => c.Name == SpatialDataRegion.DefinitionIniHeader).ToList().Last();

            var branchidProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.BranchId.Key);
            Assert.AreEqual("branch50", branchidProperty.Value);

            var chainageProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.Chainage.Key);
            Assert.AreEqual("90.000", chainageProperty.Value);

            var valueProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.Value.Key);
            Assert.AreEqual("490.00000", valueProperty.Value);

        }

        [Test]
        public void TestSpatialDataFileWriterInitialWaterDepthLinearInterPolType()
        {
            var initialDepth = new NetworkCoverage("Initial Water Depth", false, "Water Depth", "m") { Network = network };
            initialDepth.Locations.InterpolationType = InterpolationType.Linear;
            initialDepth.Locations.SetValues(locations.OrderBy(l => l));
            initialDepth.SetValues(Enumerable.Range(0, 50).Select(i => i * 10.0));

            var targetDirectory = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var initialWaterDepthFile = Path.Combine(targetDirectory, SpatialDataFileNames.InitialWaterDepth);
            SpatialDataFileWriter.WriteFile(initialWaterDepthFile, SpatialDataQuantity.InitialWaterDepth, initialDepth);
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(initialWaterDepthFile);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, categories.Count(c => c.Name == SpatialDataRegion.ContentIniHeader));
            Assert.AreEqual(500, categories.Count(d => d.Name == SpatialDataRegion.DefinitionIniHeader));

            var content = categories.Where(c => c.Name == SpatialDataRegion.ContentIniHeader).ToList().First();

            var quantityProperty = content.Properties.First(p => p.Name == SpatialDataRegion.Quantity.Key);
            Assert.AreEqual(SpatialDataQuantity.InitialWaterDepth, quantityProperty.Value);

            var interpolateProperty = content.Properties.First(p => p.Name == SpatialDataRegion.Interpolate.Key);
            Assert.AreEqual("1", interpolateProperty.Value);

            var definition = categories.Where(c => c.Name == SpatialDataRegion.DefinitionIniHeader).ToList().Last();

            var branchidProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.BranchId.Key);
            Assert.AreEqual("branch50", branchidProperty.Value);

            var chainageProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.Chainage.Key);
            Assert.AreEqual("90.000", chainageProperty.Value);

            var valueProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.Value.Key);
            Assert.AreEqual("490.00000", valueProperty.Value);
        }

        [Test]
        public void TestSpatialDataFileWriterInitialTemperatureLinearInterPolType()
        {
            var initialTemperature = new NetworkCoverage("Initial Temperature", false, "Temperature", "degrees C") { Network = network };
            initialTemperature.Locations.InterpolationType = InterpolationType.Linear;
            initialTemperature.Locations.SetValues(locations.OrderBy(l => l));
            initialTemperature.SetValues(Enumerable.Range(0, 50).Select(i => i * 10.0));

            var targetDirectory = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var initialTemperatureFile = Path.Combine(targetDirectory, SpatialDataFileNames.InitialTemperature);
            SpatialDataFileWriter.WriteFile(initialTemperatureFile, SpatialDataQuantity.InitialTemperature, initialTemperature);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(initialTemperatureFile);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, categories.Count(c => c.Name == SpatialDataRegion.ContentIniHeader));
            Assert.AreEqual(500, categories.Count(d => d.Name == SpatialDataRegion.DefinitionIniHeader));

            var content = categories.Where(c => c.Name == SpatialDataRegion.ContentIniHeader).ToList().First();

            var quantityProperty = content.Properties.First(p => p.Name == SpatialDataRegion.Quantity.Key);
            Assert.AreEqual(SpatialDataQuantity.InitialTemperature, quantityProperty.Value);

            var interpolateProperty = content.Properties.First(p => p.Name == SpatialDataRegion.Interpolate.Key);
            Assert.AreEqual("1", interpolateProperty.Value);

            var definition = categories.Where(c => c.Name == SpatialDataRegion.DefinitionIniHeader).ToList().Last();

            var branchidProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.BranchId.Key);
            Assert.AreEqual("branch50", branchidProperty.Value);

            var chainageProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.Chainage.Key);
            Assert.AreEqual("90.000", chainageProperty.Value);

            var valueProperty = definition.Properties.First(p => p.Name == SpatialDataRegion.Value.Key);
            Assert.AreEqual("490.00000", valueProperty.Value);
        }

    }
}