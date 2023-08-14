using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class BcFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestGroupBoundaryConditions()
        {
            // setup
            var boundaryConditionSet1 = new BoundaryConditionSet();
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries));

            var boundaryConditionSet2 = new BoundaryConditionSet();
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Temperature, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries));

            var boundaryConditions = new List<BoundaryConditionSet>()
            {
                boundaryConditionSet1,
                boundaryConditionSet2
            };

            // group boundary conditions
            var bcFile = new BcFile();
            List<IGrouping<string, Tuple<IBoundaryCondition, BoundaryConditionSet>>> groupings = bcFile.GroupBoundaryConditions(boundaryConditions).ToList();

            // check that morphology related boundary conditions are filtered out
            List<FlowBoundaryCondition> groupedBoundaryConditions = groupings.SelectMany(g => g).Select(g => g.Item1).OfType<FlowBoundaryCondition>().ToList();
            Assert.AreEqual(8, groupedBoundaryConditions.Count);

            Assert.AreEqual(0, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed));
            Assert.AreEqual(1, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Salinity));
            Assert.AreEqual(2, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration));
            Assert.AreEqual(1, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Temperature));
            Assert.AreEqual(2, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer));
            Assert.AreEqual(2, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTwoAstroWaterLevelBoundaryConditions()
        {
            string filePath = TestHelper.GetTestFilePath(@"BcFiles\TwoAstroWaterLevels.bc");
            var fileReader = new BcFile();
            List<BcBlockData> dataBlocks = fileReader.Read(filePath).ToList();
            Assert.AreEqual(2, dataBlocks.Count());

            BcBlockData firstBlock = dataBlocks.First();
            Assert.AreEqual("pli1_0001", firstBlock.SupportPoint);
            Assert.AreEqual("astronomic", firstBlock.FunctionType);
            Assert.AreEqual(3, firstBlock.Quantities.Count);

            BcQuantityData quantity = firstBlock.Quantities[0];

            Assert.AreEqual("astronomic component", quantity.QuantityName);
            Assert.AreEqual("-", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "M2",
                "S2"
            }, quantity.Values);

            quantity = firstBlock.Quantities[1];

            Assert.AreEqual("waterlevelbnd amplitude", quantity.QuantityName);
            Assert.AreEqual("m", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "0.9",
                "0.95"
            }, quantity.Values);

            quantity = firstBlock.Quantities[2];

            Assert.AreEqual("waterlevelbnd phase", quantity.QuantityName);
            Assert.AreEqual("rad/deg/minutes", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "10",
                "-7.5"
            }, quantity.Values);

            BcBlockData secondBlock = dataBlocks.ElementAt(1);
            Assert.AreEqual("pli1_0002", secondBlock.SupportPoint);
            Assert.AreEqual("astronomic", secondBlock.FunctionType);
            Assert.AreEqual(3, secondBlock.Quantities.Count);

            quantity = secondBlock.Quantities[0];

            Assert.AreEqual("astronomic component", quantity.QuantityName);
            Assert.AreEqual("-", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "M2",
                "S2"
            }, quantity.Values);

            quantity = secondBlock.Quantities[1];

            Assert.AreEqual("waterlevelbnd amplitude", quantity.QuantityName);
            Assert.AreEqual("m", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "0.8",
                "1.1"
            }, quantity.Values);

            quantity = secondBlock.Quantities[2];

            Assert.AreEqual("waterlevelbnd phase", quantity.QuantityName);
            Assert.AreEqual("rad/deg/minutes", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "20",
                "-11.5"
            }, quantity.Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWaterLevelAndSalinityLayersConditions()
        {
            string filePath = TestHelper.GetTestFilePath(@"BcFiles\WaterLevelAndSalinityLayers.bc");
            var fileReader = new BcFile();
            List<BcBlockData> dataBlocks = fileReader.Read(filePath).ToList();
            Assert.AreEqual(1, dataBlocks.Count());

            BcBlockData firstBlock = dataBlocks.First();
            Assert.AreEqual("pli1_0001", firstBlock.SupportPoint);
            Assert.AreEqual("timeseries", firstBlock.FunctionType);
            Assert.AreEqual(4, firstBlock.Quantities.Count);

            BcQuantityData quantity = firstBlock.Quantities[0];

            Assert.AreEqual("time", quantity.QuantityName);
            Assert.AreEqual("minutes since 2013-01-01", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "0",
                "1440"
            }, quantity.Values);

            quantity = firstBlock.Quantities[1];

            Assert.AreEqual("waterlevelbnd", quantity.QuantityName);
            Assert.AreEqual("m", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "0.5",
                "0.65"
            }, quantity.Values);

            quantity = firstBlock.Quantities[2];

            Assert.AreEqual("salinitybnd", quantity.QuantityName);
            Assert.AreEqual("ppt", quantity.Unit);
            Assert.AreEqual("1", quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "22",
                "30"
            }, quantity.Values);

            quantity = firstBlock.Quantities[3];

            Assert.AreEqual("salinitybnd", quantity.QuantityName);
            Assert.AreEqual("ppt", quantity.Unit);
            Assert.AreEqual("2", quantity.VerticalPosition);
            Assert.AreEqual(new[]
            {
                "0",
                "0"
            }, quantity.Values);
        }

        [Test]
        public void WriteLateralData_LateralsNull_ThrowsArgumentNullException()
        {
            // Setup
            var bcFile = new BcFile();
            const string filePath = "some_file_path";
            var bcFileDataBuilder = new BcFileFlowBoundaryDataBuilder();
            DateTime referenceDate = DateTime.Today;

            // Call
            void Call() => bcFile.WriteLateralData(null, filePath, bcFileDataBuilder, referenceDate);
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void WriteLateralData_BoundaryDataBuilderNull_ThrowsArgumentNullException()
        {
            // Setup
            var bcFile = new BcFile();
            IEnumerable<Lateral> laterals = Enumerable.Empty<Lateral>();
            const string filePath = "some_file_path";
            DateTime referenceDate = DateTime.Today;

            // Call
            void Call() => bcFile.WriteLateralData(laterals, filePath, null, referenceDate);
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void WriteLateralData_FilePathNullOrWhiteSpace_ThrowsArgumentException(string filePath)
        {
            // Setup
            var bcFile = new BcFile();
            IEnumerable<Lateral> laterals = Enumerable.Empty<Lateral>();
            var bcFileDataBuilder = new BcFileFlowBoundaryDataBuilder();
            DateTime referenceDate = DateTime.Today;

            // Call
            void Call() => bcFile.WriteLateralData(laterals, filePath, bcFileDataBuilder, referenceDate);
            
            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteLateralData_WritesCorrectFile()
        {
            // Setup
            var bcFile = new BcFile();
            
            var referenceDate = new DateTime(2023, 7, 31);
            
            var feature = new Feature2D { Name = "some_name" };
            var lateral = new Lateral { Feature = feature };
            lateral.Data.Discharge.Type = LateralDischargeType.TimeSeries;
            lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(60)] = 1.23;
            lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(120)] = 2.34;
            lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(180)] = 3.45;
            lateral.Data.Discharge.TimeSeries.Time.InterpolationType = InterpolationType.Linear;

            IEnumerable<Lateral> laterals = new[] { lateral };
            var bcFileDataBuilder = new BcFileFlowBoundaryDataBuilder();
            
            string[] fileLines; 
            
            using (var temp = new TemporaryDirectory())
            {
                string filePath = System.IO.Path.Combine(temp.Path, "lateral_discharge.bc");
                
                // Call
                bcFile.WriteLateralData(laterals, filePath, bcFileDataBuilder, referenceDate);

                fileLines = System.IO.File.ReadAllLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            }
            
            // Assert
            Assert.That(fileLines, Has.Length.EqualTo(11));
            Assert.That(fileLines[0], Is.EqualTo("[forcing]"));
            AssertPropertyLine(fileLines[1], "Name", "some_name");
            AssertPropertyLine(fileLines[2], "Function", "timeseries");
            AssertPropertyLine(fileLines[3], "Time-interpolation", "linear");
            AssertPropertyLine(fileLines[4], "Quantity", "time");
            AssertPropertyLine(fileLines[5], "Unit", "seconds since 2023-07-31 00:00:00");
            AssertPropertyLine(fileLines[6], "Quantity", "lateral_discharge");
            AssertPropertyLine(fileLines[7], "Unit", "m3/s");
            AssertDataLine(fileLines[8], "60", "1.23");
            AssertDataLine(fileLines[9], "120", "2.34");
            AssertDataLine(fileLines[10], "180", "3.45");
        }

        private static void AssertDataLine(string fileLine, string timeValue, string dataValue)
        {
            string[] parts = fileLine.Split(new char[] {}, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(parts[0].Trim(), Is.EqualTo(timeValue));
            Assert.That(parts[1].Trim(), Is.EqualTo(dataValue));
        }

        private static void AssertPropertyLine(string line, string propertyName, string value)
        {
            string[] pair = line.Split('=');
            Assert.That(pair[0].Trim(), Is.EqualTo(propertyName));
            Assert.That(pair[1].Trim(), Is.EqualTo(value));
        }
    }
}