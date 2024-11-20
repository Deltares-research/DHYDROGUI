using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Units;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
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
            Assert.AreEqual(2, dataBlocks.Count);

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
        public void Read_BcFileWithCapitalPropertyKeys_PropertiesAreProcessedCaseInsensitive()
        {
            // Setup
            var fileContent = new[]
            {
                "[General]",
                "fileVersion            = 1.01",
                "fileType               = boundConds",
                "",
                "[FORCING]",
                "NAME                   = PLI1_0001",
                "FUNCTION               = TIMESERIES",
                "TIMEINTERPOLATION      = LINEAR",
                "VERTPOSITIONTYPE       = BED-SURFACE",
                "VERTINTERPOLATION      = LINEAR",
                "QUANTITY               = TIME",
                "UNIT                   = MINUTES SINCE 2013-01-01",
                "QUANTITY               = WATERLEVELBND",
                "UNIT                   = M",
                "QUANTITY               = SALINITYBND",
                "UNIT                   = PPT",
                "VERTPOSITIONINDEX      = 1",
                "QUANTITY               = SALINITYBND",
                "UNIT                   = PPT",
                "VERTPOSITIONINDEX      = 2",
                "0     0.5   22    0",
                "1440  0.65  30    0"
            };

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CreateFile("FlowFM.bc", string.Join(Environment.NewLine, fileContent));

                var bcFile = new BcFile();

                // Call
                IEnumerable<BcBlockData> bcData = bcFile.Read(filePath);

                // Assert
                BcBlockData bcSection = bcData.Single();

                Assert.That(bcSection.SupportPoint, Is.EqualTo("PLI1_0001"));
                Assert.That(bcSection.FunctionType, Is.EqualTo("TIMESERIES"));
                Assert.That(bcSection.TimeInterpolationType, Is.EqualTo("LINEAR"));
                Assert.That(bcSection.VerticalPositionType, Is.EqualTo("BED-SURFACE"));
                Assert.That(bcSection.VerticalInterpolationType, Is.EqualTo("LINEAR"));
                Assert.That(bcSection.Quantities, Has.Count.EqualTo(4));

                AssertQuantityData(bcSection.Quantities[0], "TIME", "MINUTES SINCE 2013-01-01", null, new[] { "0", "1440" });
                AssertQuantityData(bcSection.Quantities[1], "WATERLEVELBND", "M", null, new[] { "0.5", "0.65" });
                AssertQuantityData(bcSection.Quantities[2], "SALINITYBND", "PPT", "1", new[] { "22", "30" });
                AssertQuantityData(bcSection.Quantities[3], "SALINITYBND", "PPT", "2", new[] { "0", "0" });
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_BcFileWithBackwardsCompatiblePropertiesKeys_OldPropertyKeysAreRead()
        {
            // Setup
            var fileContent = new[]
            {
                "[General]",
                "fileVersion                     = 1.01",
                "fileType                        = boundConds",
                "",
                "[Forcing]",
                "Name                            = pli1_0001",
                "Function                        = timeSeries",
                "Time-interpolation              = linear",
                "Vertical position type          = percBed",
                "Vertical position specification = 20 50 70",
                "Vertical interpolation          = linear",
                "Quantity                        = x-velocity", 
                "Unit                            = m/s",
                "Vertical position               = 1"
            };

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CreateFile("FlowFM.bc", string.Join(Environment.NewLine, fileContent));

                var bcFile = new BcFile();

                // Call
                IEnumerable<BcBlockData> bcData = bcFile.Read(filePath);

                // Assert
                BcBlockData bcSection = bcData.Single();

                Assert.That(bcSection.TimeInterpolationType, Is.EqualTo("linear"));
                Assert.That(bcSection.VerticalPositionType, Is.EqualTo("percBed"));
                Assert.That(bcSection.VerticalPositionDefinition, Is.EqualTo("20 50 70"));
                Assert.That(bcSection.VerticalInterpolationType, Is.EqualTo("linear"));
                Assert.That(bcSection.Quantities[0].VerticalPosition, Is.EqualTo("1"));
            }
        }

        [Test]
        public void Read_UnsupportedSectionEncountered_LogsWarning()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                const string fileContent = "[some_section] \n property1 = value1 \n property2 = value2";
                string filePath = temp.CreateFile("FlowFM.bc", fileContent);

                var bcFile = new BcFile();

                // Call
                void Call() => _ = bcFile.Read(filePath).ToArray();

                // Assert
                IEnumerable<string> warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
                Assert.That(warnings, Does.Contain($"Section [some_section] not supported on line 1. File: {filePath}"));
            }
        }

        private void AssertQuantityData(BcQuantityData bcQuantityData, string quantity, string unit, string verticalPosition, string[] values)
        {
            Assert.That(bcQuantityData.QuantityName, Is.EqualTo(quantity));
            Assert.That(bcQuantityData.Unit, Is.EqualTo(unit));
            Assert.That(bcQuantityData.VerticalPosition, Is.EqualTo(verticalPosition));
            Assert.That(bcQuantityData.Values, Is.EqualTo(values));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWaterLevelAndSalinityLayersConditions()
        {
            string filePath = TestHelper.GetTestFilePath(@"BcFiles\WaterLevelAndSalinityLayers.bc");
            var fileReader = new BcFile();
            List<BcBlockData> dataBlocks = fileReader.Read(filePath).ToList();
            Assert.AreEqual(1, dataBlocks.Count);

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
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
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
                string filePath = Path.Combine(temp.Path, "lateral_discharge.bc");
                
                // Call
                bcFile.WriteLateralData(laterals, filePath, bcFileDataBuilder, referenceDate);

                fileLines = File.ReadAllLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            }
            
            // Assert
            Assert.That(fileLines, Has.Length.EqualTo(11));
            Assert.That(fileLines[0], Is.EqualTo("[forcing]"));
            AssertPropertyLine(fileLines[1], "name", "some_name");
            AssertPropertyLine(fileLines[2], "function", "timeseries");
            AssertPropertyLine(fileLines[3], "timeInterpolation", "linear");
            AssertPropertyLine(fileLines[4], "quantity", "time");
            AssertPropertyLine(fileLines[5], "unit", "seconds since 2023-07-31 00:00:00");
            AssertPropertyLine(fileLines[6], "quantity", "lateral_discharge");
            AssertPropertyLine(fileLines[7], "unit", "m3/s");
            AssertDataLine(fileLines[8], "60", "1.23");
            AssertDataLine(fileLines[9], "120", "2.34");
            AssertDataLine(fileLines[10], "180", "3.45");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteFlowBoundaryConditionData_WritesCorrectFile()
        {
            // Setup
            var feature = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0), 
                    new Coordinate(1, 0), 
                    new Coordinate(0, 1)
                }),
                Name = "pli1"
            };

            var salinityBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, 
                                                       BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature
            };
            
            salinityBc.AddPoint(0);
            salinityBc.PointDepthLayerDefinitions[0] =
                new VerticalProfileDefinition(VerticalProfileType.PercentageFromSurface, 100, 30, 5);
            
            IFunction salinityData = salinityBc.GetDataAtPoint(0);
            salinityData.Arguments[0].Unit = new Unit("-", "-");
            
            var times = new[]
            {
                new DateTime(2014, 1, 1, 0, 0, 0),
                new DateTime(2014, 1, 2, 0, 0, 0),
                new DateTime(2014, 1, 3, 0, 0, 0)
            };

            salinityData[times[0]] = new[] { 20, 10, 5 };
            salinityData[times[1]] = new[] { 21, 10.5, 5 };
            salinityData[times[2]] = new[] { 22, 11, 5.5 };
            
            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };
            boundaryConditionSet.BoundaryConditions.Add(salinityBc);

            var bcFile = new BcFile();
            var boundaryConditionSets = new[] { boundaryConditionSet };
            var bcFileDataBuilder = new BcFileFlowBoundaryDataBuilder();

            string[] fileLines;

            using (var temp = new TemporaryDirectory())
            {
                string filePath = Path.Combine(temp.Path, "SalinityTimeSeries.bc");
                
                // Call
                bcFile.Write(boundaryConditionSets, filePath, bcFileDataBuilder);

                fileLines = File.ReadAllLines(filePath)
                                .Where(l => !string.IsNullOrWhiteSpace(l))
                                .ToArray();
            }
            
            // Assert
            Assert.That(fileLines, Has.Length.EqualTo(21));
            Assert.That(fileLines[0], Is.EqualTo("[forcing]"));
            AssertPropertyLine(fileLines[1], "name", "pli1_0001");
            AssertPropertyLine(fileLines[2], "function", "t3d");
            AssertPropertyLine(fileLines[3], "timeInterpolation", "linear");
            AssertPropertyLine(fileLines[4], "vertPositionType", "percentage from surface");
            AssertPropertyLine(fileLines[5], "vertPositions", "100 30 5");
            AssertPropertyLine(fileLines[6], "vertInterpolation", "linear");
            AssertPropertyLine(fileLines[7], "quantity", "time");
            AssertPropertyLine(fileLines[8], "unit", "-");
            AssertPropertyLine(fileLines[9], "quantity", "salinitybnd");
            AssertPropertyLine(fileLines[10], "unit", "ppt");
            AssertPropertyLine(fileLines[11], "vertPositionIndex", "1");
            AssertPropertyLine(fileLines[12], "quantity", "salinitybnd");
            AssertPropertyLine(fileLines[13], "unit", "ppt");
            AssertPropertyLine(fileLines[14], "vertPositionIndex", "2");
            AssertPropertyLine(fileLines[15], "quantity", "salinitybnd");
            AssertPropertyLine(fileLines[16], "unit", "ppt");
            AssertPropertyLine(fileLines[17], "vertPositionIndex", "3");
            AssertDataLine(fileLines[18], "20140101000000", "20  10    5");
            AssertDataLine(fileLines[19], "20140102000000", "21  10.5  5");
            AssertDataLine(fileLines[20], "20140103000000", "22  11    5.5");
        }

        private static void AssertDataLine(string line, string timeValue, string dataValue)
        {
            string[] pair = line.Split(new []{' '}, 2);
            Assert.That(pair[0].Trim(), Is.EqualTo(timeValue));
            Assert.That(pair[1].Trim(), Is.EqualTo(dataValue));
        }

        private static void AssertPropertyLine(string line, string propertyName, string value)
        {
            string[] pair = line.Split('=');
            Assert.That(pair[0].Trim(), Is.EqualTo(propertyName));
            Assert.That(pair[1].Trim(), Is.EqualTo(value));
        }
    }
}