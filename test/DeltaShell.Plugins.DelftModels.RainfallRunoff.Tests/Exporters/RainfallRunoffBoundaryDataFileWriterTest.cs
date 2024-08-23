using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils.EqualityComparers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Builders;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Exporters
{
    [TestFixture]
    class RainfallRunoffBoundaryDataFileWriterTest
    {   
        [Test]
        public void TestRainfallRunoffBoundaryDataFileWriterGivesExpectedResults_BoundaryNodes()
        {
            RainfallRunoffModel model = new RainfallRunoffModel();
            var boundaryNodeData = model.BoundaryData;
            boundaryNodeData.Add(new RunoffBoundaryData(new RunoffBoundary() {Name = "RBC1"}) {Series = new RainfallRunoffBoundaryData() {IsConstant = true, IsTimeSeries = false, Value = 123.4} });

            var t = new DateTime(2000, 1, 1);
            model.StartTime = t;
            model.StopTime = t.AddHours(3);
            model.TimeStep = new TimeSpan(0, 30, 0);
            model.OutputTimeStep = new TimeSpan(0, 30, 0);

            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>());
            timeSeries[t] = 0.0;
            timeSeries[t.AddHours(1)] = 20.0;
            timeSeries[t.AddHours(2)] = 40.0;
            timeSeries[t.AddHours(3)] = 60.0;
            timeSeries.Arguments[0].InterpolationType = InterpolationType.Constant;
            timeSeries.Arguments[0].ExtrapolationType= ExtrapolationType.Periodic;

            boundaryNodeData.Add(new RunoffBoundaryData(new RunoffBoundary() {Name = "RBC2"}) {Series = new RainfallRunoffBoundaryData() {IsConstant = false, IsTimeSeries = true, Data = timeSeries } });
            var bcFile = Path.GetTempFileName();
            new RainfallRunoffBoundaryDataFileWriter(new BcWriter(new FileSystem())).WriteFile(bcFile, model);

            var bcReader = new BcReader(new FileSystem());
            var iniSections = bcReader.ReadBcFile(bcFile);

            // Test BoundaryNode Data
            var boundaryNodeSections = iniSections.Where(c => c.Section.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.AreEqual(2, boundaryNodeSections.Count); // 2 RR BC

            // Constant WaterLevel
            Assert.AreEqual(2, boundaryNodeSections[0].Section.Properties.Count());
            Assert.AreEqual("RBC1", boundaryNodeSections[0].Section.ReadProperty<string>(BoundaryRegion.Name.Key));
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, boundaryNodeSections[0].Section.ReadProperty<string>(BoundaryRegion.Function.Key));
            Assert.AreEqual(1, boundaryNodeSections[0].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR, boundaryNodeSections[0].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeSections[0].Table[0].Unit.Value);
            Assert.AreEqual(1, boundaryNodeSections[0].Table[0].Values.Count);
            Assert.AreEqual("123.4", boundaryNodeSections[0].Table[0].Values[0]);

            // WaterLevel TimeSeries
            Assert.AreEqual(4, boundaryNodeSections[1].Section.Properties.Count());
            Assert.AreEqual("RBC2", boundaryNodeSections[1].Section.ReadProperty<string>(BoundaryRegion.Name.Key));
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeSections[1].Section.ReadProperty<string>(BoundaryRegion.Function.Key));
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.BlockFrom, boundaryNodeSections[1].Section.ReadProperty<string>(BoundaryRegion.Interpolation.Key));
            Assert.AreEqual(true, boundaryNodeSections[1].Section.ReadProperty<bool>(BoundaryRegion.Periodic.Key));
            Assert.AreEqual(2, boundaryNodeSections[1].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeSections[1].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + model.StartTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeSections[1].Table[0].Unit.Value);
            Assert.AreEqual(4, boundaryNodeSections[1].Table[0].Values.Count);
            Assert.AreEqual("0", boundaryNodeSections[1].Table[0].Values[0]);
            Assert.AreEqual("60", boundaryNodeSections[1].Table[0].Values[1]);
            Assert.AreEqual("120", boundaryNodeSections[1].Table[0].Values[2]);
            Assert.AreEqual("180", boundaryNodeSections[1].Table[0].Values[3]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR, boundaryNodeSections[1].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeSections[1].Table[1].Unit.Value);
            Assert.AreEqual(4, boundaryNodeSections[1].Table[1].Values.Count);
            Assert.AreEqual("0", boundaryNodeSections[1].Table[1].Values[0]);
            Assert.AreEqual("20", boundaryNodeSections[1].Table[1].Values[1]);
            Assert.AreEqual("40", boundaryNodeSections[1].Table[1].Values[2]);
            Assert.AreEqual("60", boundaryNodeSections[1].Table[1].Values[3]);
        }

        [Test]
        public void Constructor_BcFileWriterNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RainfallRunoffBoundaryDataFileWriter(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("bcWriter"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void WriteFile_FilePathNullOrWhiteSpace_ThrowsArgumentException(string filePath)
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcWriter>();
            var rainfallRunoffBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(bcFileWriter);
            var rainfallRunoffModel = Substitute.For<IRainfallRunoffModel>();

            // Call
            void Call() => rainfallRunoffBoundaryDataFileWriter.WriteFile(filePath, rainfallRunoffModel);

            // Assert
            Assert.That(Call, Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("filePath"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void WriteFile_RainfallRunoffModelNull_NullOrWhiteSpace_ThrowsArgumentException(string filePath)
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcWriter>();
            var rainfallRunoffBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(bcFileWriter);

            // Call
            void Call() => rainfallRunoffBoundaryDataFileWriter.WriteFile("boundaryConditions.bc", null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("rainfallRunoffModel"));
        }

        [Test]
        public void WriteFile_WithRunoffBoundary_AddsCorrectSectionsToBcFileWriter()
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcWriter>();
            var rainfallRunoffBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(bcFileWriter);
            var rainfallRunoffModel = Substitute.For<IRainfallRunoffModel>();
            const string filePath = "boundaryConditions.bc";

            DateTime modelStartTime = DateTime.Today;
            var modelBoundaryData = new EventedList<RunoffBoundaryData>();
            var modelCatchmentData = new EventedList<CatchmentModelData>();

            rainfallRunoffModel.StartTime.Returns(modelStartTime);
            rainfallRunoffModel.BoundaryData.Returns(modelBoundaryData);
            rainfallRunoffModel.ModelData.Returns(modelCatchmentData);

            RunoffBoundaryData runoffBoundaryData1 = RunoffBoundaryDataBuilder.Start().WithName("runoff_boundary_constant")
                                                                              .WithConstantValue(1)
                                                                              .Build();
            RunoffBoundaryData runoffBoundaryData2 = RunoffBoundaryDataBuilder.Start().WithName("runoff_boundary_time_series")
                                                                              .WithTimeSeries(modelStartTime, 2, 3, 4)
                                                                              .Build();

            modelBoundaryData.Add(runoffBoundaryData1);
            modelBoundaryData.Add(runoffBoundaryData2);

            // Call
            rainfallRunoffBoundaryDataFileWriter.WriteFile(filePath, rainfallRunoffModel);

            // Assert
            var expectedSections = new List<BcIniSection>
            {
                CreateExpectedGeneralSection(),
                CreateExpectedConstantBcSection("runoff_boundary_constant", 1),
                CreateExpectedTimeSeriesBcSection("runoff_boundary_time_series", modelStartTime, 2, 3, 4)
            };

            bcFileWriter.Received(1).WriteBcFile(
                MatchingSections(expectedSections),
                filePath);
        }

        [Test]
        public void WriteFile_WithUnpavedData_AddsCorrectSectionsToBcFileWriter_AndShouldSkipCatchmentsLinkedToARunoffBoundary()
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcWriter>();
            var rainfallRunoffBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(bcFileWriter);
            var rainfallRunoffModel = Substitute.For<IRainfallRunoffModel>();
            const string filePath = "boundaryConditions.bc";

            DateTime modelStartTime = DateTime.Today;
            var modelBoundaryData = new EventedList<RunoffBoundaryData>();
            var modelCatchmentData = new EventedList<CatchmentModelData>();

            rainfallRunoffModel.StartTime.Returns(modelStartTime);
            rainfallRunoffModel.BoundaryData.Returns(modelBoundaryData);
            rainfallRunoffModel.ModelData.Returns(modelCatchmentData);

            // Constant boundaries
            CatchmentModelData unpavedData1 = UnpavedDataBuilder.Start().WithName("unpaved_catchment_constant")
                                                                .WithConstantValue(1)
                                                                .Build();
            CatchmentModelData unpavedData2 = UnpavedDataBuilder.Start().WithName("unpaved_catchment_linked_to_lateral_source_constant")
                                                                .WithLinkToLateralSource("lateral_source_constant")
                                                                .WithConstantValue(2)
                                                                .Build();
            CatchmentModelData unpavedData3 = UnpavedDataBuilder.Start().WithName("unpaved_catchment_linked_to_runoff_boundary_constant")
                                                                .WithLinkToRunoffBoundary()
                                                                .WithConstantValue(3)
                                                                .Build();
            CatchmentModelData unpavedData4 = UnpavedDataBuilder.Start().WithName("unpaved_catchment_linked_to_wwtp_constant")
                                                                .WithLinkToWasteWaterTreatmentPlant()
                                                                .WithConstantValue(4)
                                                                .Build();

            // Time series boundaries
            CatchmentModelData unpavedData5 = UnpavedDataBuilder.Start().WithName("unpaved_catchment_time_series")
                                                                .WithTimeSeries(modelStartTime, 1, 2, 3)
                                                                .Build();
            CatchmentModelData unpavedData6 = UnpavedDataBuilder.Start().WithName("unpaved_catchment_linked_to_lateral_source_time_series")
                                                                .WithLinkToLateralSource("lateral_source_time_series")
                                                                .WithTimeSeries(modelStartTime, 4, 5, 6)
                                                                .Build();
            CatchmentModelData unpavedData7 = UnpavedDataBuilder.Start().WithName("unpaved_catchment_linked_to_runoff_boundary_time_series")
                                                                .WithLinkToRunoffBoundary()
                                                                .WithTimeSeries(modelStartTime, 7, 8, 9)
                                                                .Build();
            CatchmentModelData unpavedData8 = UnpavedDataBuilder.Start().WithName("unpaved_catchment_linked_to_wwtp_time_series")
                                                                .WithLinkToWasteWaterTreatmentPlant()
                                                                .WithTimeSeries(modelStartTime, 10, 11, 12)
                                                                .Build();

            modelCatchmentData.Add(unpavedData1);
            modelCatchmentData.Add(unpavedData2);
            modelCatchmentData.Add(unpavedData3);
            modelCatchmentData.Add(unpavedData4);
            modelCatchmentData.Add(unpavedData5);
            modelCatchmentData.Add(unpavedData6);
            modelCatchmentData.Add(unpavedData7);
            modelCatchmentData.Add(unpavedData8);

            // Call
            rainfallRunoffBoundaryDataFileWriter.WriteFile(filePath, rainfallRunoffModel);

            // Assert
            var expectedSections = new List<BcIniSection>
            {
                CreateExpectedGeneralSection(),
                CreateExpectedConstantBcSection("unpaved_catchment_constant_boundary", 1),
                CreateExpectedConstantBcSection("lateral_source_constant", 2),
                CreateExpectedConstantBcSection("unpaved_catchment_linked_to_wwtp_constant_boundary", 4),
                CreateExpectedTimeSeriesBcSection("unpaved_catchment_time_series_boundary", modelStartTime, 1, 2, 3),
                CreateExpectedTimeSeriesBcSection("lateral_source_time_series", modelStartTime, 4, 5, 6),
                CreateExpectedTimeSeriesBcSection("unpaved_catchment_linked_to_wwtp_time_series_boundary", modelStartTime, 10, 11, 12)
            };

            bcFileWriter.Received(1).WriteBcFile(
                MatchingSections(expectedSections),
                filePath);
        }

        [Test]
        public void WriteFile_WithCatchmentModelData_AddsCorrectSectionsToBcFileWriter_WithDefaultBoundaries_AndShouldSkipCatchmentsLinkedToARunoffBoundary()
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcWriter>();
            var rainfallRunoffBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(bcFileWriter);
            var rainfallRunoffModel = Substitute.For<IRainfallRunoffModel>();
            const string filePath = "boundaryConditions.bc";

            DateTime modelStartTime = DateTime.Today;
            var modelBoundaryData = new EventedList<RunoffBoundaryData>();
            var modelCatchmentData = new EventedList<CatchmentModelData>();

            rainfallRunoffModel.StartTime.Returns(modelStartTime);
            rainfallRunoffModel.BoundaryData.Returns(modelBoundaryData);
            rainfallRunoffModel.ModelData.Returns(modelCatchmentData);

            CatchmentModelData catchmentModelData1 = CatchmentModelDataBuilder.Start().WithName("catchment")
                                                                              .Build();
            CatchmentModelData catchmentModelData2 = CatchmentModelDataBuilder.Start().WithName("catchment_linked_to_lateral_source")
                                                                              .WithLinkToLateralSource("lateral_source")
                                                                              .Build();
            CatchmentModelData catchmentModelData3 = CatchmentModelDataBuilder.Start().WithName("catchment_linked_to_runoff_boundary")
                                                                              .WithLinkToRunoffBoundary()
                                                                              .Build();
            CatchmentModelData catchmentModelData4 = CatchmentModelDataBuilder.Start().WithName("catchment_linked_to_wwtp")
                                                                              .WithLinkToWasteWaterTreatmentPlant()
                                                                              .Build();

            modelCatchmentData.Add(catchmentModelData1);
            modelCatchmentData.Add(catchmentModelData2);
            modelCatchmentData.Add(catchmentModelData3);
            modelCatchmentData.Add(catchmentModelData4);

            // Call
            rainfallRunoffBoundaryDataFileWriter.WriteFile(filePath, rainfallRunoffModel);

            // Assert
            var expectedSections = new List<BcIniSection>
            {
                CreateExpectedGeneralSection(),
                CreateExpectedConstantBcSection("catchment_boundary", 0),
                CreateExpectedConstantBcSection("lateral_source", 0),
                CreateExpectedConstantBcSection("catchment_linked_to_wwtp_boundary", 0)
            };

            bcFileWriter.Received(1).WriteBcFile(
                MatchingSections(expectedSections),
                filePath);
        }

        [Test]
        public void Two_unpaved_catchments_with_a_link_to_the_same_lateral_results_in_only_one_boundary_being_written()
        {
            // Setup
            var bcWriter = Substitute.For<IBcWriter>();
            var rrBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(bcWriter);
            
            var rainfallRunoffModel = Substitute.For<IRainfallRunoffModel>();
            var modelBoundaryData = new EventedList<RunoffBoundaryData>();
            var modelCatchmentData = new EventedList<CatchmentModelData>();
            rainfallRunoffModel.BoundaryData.Returns(modelBoundaryData);
            rainfallRunoffModel.ModelData.Returns(modelCatchmentData);

            const string lateralSourceName = "LateralSource1"; 
            modelCatchmentData.Add(UnpavedDataBuilder.Start()
                                                     .WithName("Catchment1")
                                                     .WithLinkToLateralSource(lateralSourceName)
                                                     .Build());
            modelCatchmentData.Add(UnpavedDataBuilder.Start()
                                                     .WithName("Catchment2")
                                                     .WithLinkToLateralSource(lateralSourceName)
                                                     .Build());

            // Call
            const string filePath = "boundaryConditions.bc";
            rrBoundaryDataFileWriter.WriteFile(filePath, rainfallRunoffModel);

            // Assert
            var expectedSections = new List<BcIniSection>
            {
                CreateExpectedGeneralSection(),
                CreateExpectedConstantBcSection(lateralSourceName, 0)
            };
            
            bcWriter.Received(1).WriteBcFile(MatchingSections(expectedSections), filePath);
        }

        [Test]
        public void WriteFile_RRModelWithWasteWaterTreatmentPlant_ReturnsExpectedIniSection()
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcWriter>();
            var rainfallRunoffBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(bcFileWriter);
            var rainfallRunoffModel = Substitute.For<IRainfallRunoffModel>();

            var modelBoundaryData = new EventedList<RunoffBoundaryData>();
            var modelCatchmentData = new EventedList<CatchmentModelData>();
            rainfallRunoffModel.BoundaryData.Returns(modelBoundaryData);
            rainfallRunoffModel.ModelData.Returns(modelCatchmentData);
            
            const string lateralName = "lateral";
            var lateral = new LateralSource() { Name = lateralName };
            
            const string wwtpName = "wwtp";
            var wwtp = new WasteWaterTreatmentPlant() { Name = wwtpName };
            var wwtps = new EventedList<WasteWaterTreatmentPlant>() { wwtp };
            rainfallRunoffModel.Basin.WasteWaterTreatmentPlants.Returns(wwtps);

            var link = new HydroLink(wwtp, lateral);
            wwtp.Links.Add(link);

            // Call
            const string filePath = "boundaryConditions.bc";
            rainfallRunoffBoundaryDataFileWriter.WriteFile(filePath, rainfallRunoffModel);

            // Assert
            var expectedSections = new List<BcIniSection>
            {
                CreateExpectedGeneralSection(),
                CreateExpectedConstantBcSection(lateralName, 0)
            };

            // Assert
            bcFileWriter.Received(1).WriteBcFile(
                MatchingSections(expectedSections),
                filePath);
        }

        private static BcIniSection CreateExpectedGeneralSection()
        {
            var generalIniSection = new IniSection("General");
            generalIniSection.AddPropertyWithOptionalComment("fileVersion", "1.01", "File version. Do not edit this.");
            generalIniSection.AddPropertyWithOptionalComment("fileType", "boundConds", "File type. Do not edit this.");

            return new BcIniSection(generalIniSection);
        }

        private static BcIniSection CreateExpectedConstantBcSection(string name, double value)
        {
            var bcSection = new BcIniSection("Boundary");
            bcSection.Section.AddPropertyWithOptionalComment("name", name, "Name of the boundary location (node id)");
            bcSection.Section.AddPropertyWithOptionalComment("function", "constant", "Possible values: TimeSeries, QHTable");
            bcSection.Section.AddPropertyWithOptionalComment("timeInterpolation", (string)null, "Possible values: linear, block-from (value holds from specified time step), block-to (value holds until next specified time step)");

            bcSection.Table.Add(CreateWaterLevelQuantityData(value));

            return bcSection;
        }

        private static BcIniSection CreateExpectedTimeSeriesBcSection(string name, DateTime startDate, params double[] values)
        {
            var bcSection = new BcIniSection("Boundary");
            bcSection.Section.AddPropertyWithOptionalComment("name", name, "Name of the boundary location (node id)");
            bcSection.Section.AddPropertyWithOptionalComment("function", "timeseries", "Possible values: TimeSeries, QHTable");
            bcSection.Section.AddPropertyWithOptionalComment("timeInterpolation", "linear", "Possible values: linear, block-from (value holds from specified time step), block-to (value holds until next specified time step)");

            double[] times = Enumerable.Range(0, values.Length).Select(i => (startDate.AddDays(i) - startDate).TotalMinutes).ToArray();
            bcSection.Table.Add(CreateTimeQuantityData(startDate, times));
            bcSection.Table.Add(CreateWaterLevelQuantityData(values));

            return bcSection;
        }

        private static BcQuantityData CreateWaterLevelQuantityData(params double[] values)
        {
            var waterLevelQuantityProperty = new IniProperty("quantity", "water_level", "Possible values (netcdf-CF standard): time, water_level, water_discharge, sea_water_salinity");
            var waterLevelUnitProperty = new IniProperty("unit", "m", "Possible values for 'time' column: yyyy-MM-dd hh:mm:ss, seconds since begintime format: yyyy-MM-dd hh:mm:ss +00:00 (+00:00: time zone), minutes since begintime, hours since begintime");
            return new BcQuantityData(waterLevelQuantityProperty, waterLevelUnitProperty, values);
        }

        private static BcQuantityData CreateTimeQuantityData(DateTime startDate, double[] values)
        {
            var timeQuantityProperty = new IniProperty("quantity", "time", "Possible values (netcdf-CF standard): time, water_level, water_discharge, sea_water_salinity");
            var timeLevelUnitProperty = new IniProperty("unit", $"minutes since {startDate:yyyy-MM-dd HH:mm:ss}", "Possible values for 'time' column: yyyy-MM-dd hh:mm:ss, seconds since begintime format: yyyy-MM-dd hh:mm:ss +00:00 (+00:00: time zone), minutes since begintime, hours since begintime");
            return new BcQuantityData(timeQuantityProperty, timeLevelUnitProperty, values);
        }

        private static IEnumerable<BcIniSection> MatchingSections(IEnumerable<BcIniSection> expectedSections)
        {
            return Arg.Is<IEnumerable<BcIniSection>>(actualSections => SectionsEqual(actualSections.ToArray(), expectedSections.ToArray()));
        }

        private static bool SectionsEqual(BcIniSection[] actualSections, BcIniSection[] expectedSections)
        {
            if (actualSections.Length != expectedSections.Length)
            {
                return false;
            }

            var bcSectionComparer = new BcIniSectionEqualityComparer();

            for (var i = 0; i < actualSections.Length; i++)
            {
                if (!bcSectionComparer.Equals(actualSections[i], expectedSections[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}