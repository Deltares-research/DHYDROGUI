using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
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
            new RainfallRunoffBoundaryDataFileWriter(new DelftBcWriter()).WriteFile(bcFile, model);

            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(bcFile);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            
            // Test BoundaryNode Data
            var boundaryNodeCategories = categories.Where(c => c.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.AreEqual(2, boundaryNodeCategories.Count); // 2 RR BC

            // Constant WaterLevel
            Assert.AreEqual(2, boundaryNodeCategories[0].Properties.Count);
            Assert.AreEqual("RBC1", boundaryNodeCategories[0].ReadProperty<string>(BoundaryRegion.Name.Key));
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, boundaryNodeCategories[0].ReadProperty<string>(BoundaryRegion.Function.Key));
            Assert.AreEqual(1, boundaryNodeCategories[0].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR, boundaryNodeCategories[0].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeCategories[0].Table[0].Unit.Value);
            Assert.AreEqual(1, boundaryNodeCategories[0].Table[0].Values.Count);
            Assert.AreEqual("123.4", boundaryNodeCategories[0].Table[0].Values[0]);

            // WaterLevel TimeSeries
            Assert.AreEqual(4, boundaryNodeCategories[1].Properties.Count);
            Assert.AreEqual("RBC2", boundaryNodeCategories[1].ReadProperty<string>(BoundaryRegion.Name.Key));
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeCategories[1].ReadProperty<string>(BoundaryRegion.Function.Key));
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.BlockFrom, boundaryNodeCategories[1].ReadProperty<string>(BoundaryRegion.Interpolation.Key));
            Assert.AreEqual(true, boundaryNodeCategories[1].ReadProperty<bool>(BoundaryRegion.Periodic.Key));
            Assert.AreEqual(2, boundaryNodeCategories[1].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeCategories[1].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + model.StartTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeCategories[1].Table[0].Unit.Value);
            Assert.AreEqual(4, boundaryNodeCategories[1].Table[0].Values.Count);
            Assert.AreEqual("0", boundaryNodeCategories[1].Table[0].Values[0]);
            Assert.AreEqual("60", boundaryNodeCategories[1].Table[0].Values[1]);
            Assert.AreEqual("120", boundaryNodeCategories[1].Table[0].Values[2]);
            Assert.AreEqual("180", boundaryNodeCategories[1].Table[0].Values[3]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR, boundaryNodeCategories[1].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeCategories[1].Table[1].Unit.Value);
            Assert.AreEqual(4, boundaryNodeCategories[1].Table[1].Values.Count);
            Assert.AreEqual("0", boundaryNodeCategories[1].Table[1].Values[0]);
            Assert.AreEqual("20", boundaryNodeCategories[1].Table[1].Values[1]);
            Assert.AreEqual("40", boundaryNodeCategories[1].Table[1].Values[2]);
            Assert.AreEqual("60", boundaryNodeCategories[1].Table[1].Values[3]);
        }

        [Test]
        public void Constructor_BcFileWriterNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RainfallRunoffBoundaryDataFileWriter(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("bcFileWriter"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void WriteFile_FilePathNullOrWhiteSpace_ThrowsArgumentException(string filePath)
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcFileWriter>();
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
            var bcFileWriter = Substitute.For<IBcFileWriter>();
            var rainfallRunoffBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(bcFileWriter);

            // Call
            void Call() => rainfallRunoffBoundaryDataFileWriter.WriteFile("boundaryConditions.bc", null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("rainfallRunoffModel"));
        }

        [Test]
        public void WriteFile_WithRunoffBoundary_AddsCorrectCategoriesToBcFileWriter()
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcFileWriter>();
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
            var expectedCategories = new List<IDelftIniCategory>
            {
                CreateExpectedGeneralCategory(),
                CreateExpectedConstantBcCategory("runoff_boundary_constant", 1),
                CreateExpectedTimeSeriesBcCategory("runoff_boundary_time_series", modelStartTime, 2, 3, 4)
            };

            bcFileWriter.Received(1).WriteBcFile(
                MatchingCategories(expectedCategories),
                filePath);
        }

        [Test]
        public void WriteFile_WithUnpavedData_AddsCorrectCategoriesToBcFileWriter_AndShouldSkipCatchmentsLinkedToARunoffBoundary()
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcFileWriter>();
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
            var expectedCategories = new List<IDelftIniCategory>
            {
                CreateExpectedGeneralCategory(),
                CreateExpectedConstantBcCategory("unpaved_catchment_constant_boundary", 1),
                CreateExpectedConstantBcCategory("lateral_source_constant", 2),
                CreateExpectedConstantBcCategory("unpaved_catchment_linked_to_wwtp_constant_boundary", 4),
                CreateExpectedTimeSeriesBcCategory("unpaved_catchment_time_series_boundary", modelStartTime, 1, 2, 3),
                CreateExpectedTimeSeriesBcCategory("lateral_source_time_series", modelStartTime, 4, 5, 6),
                CreateExpectedTimeSeriesBcCategory("unpaved_catchment_linked_to_wwtp_time_series_boundary", modelStartTime, 10, 11, 12)
            };

            bcFileWriter.Received(1).WriteBcFile(
                MatchingCategories(expectedCategories),
                filePath);
        }

        [Test]
        public void WriteFile_WithCatchmentModelData_AddsCorrectCategoriesToBcFileWriter_WithDefaultBoundaries_AndShouldSkipCatchmentsLinkedToARunoffBoundary()
        {
            // Setup
            var bcFileWriter = Substitute.For<IBcFileWriter>();
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
            var expectedCategories = new List<IDelftIniCategory>
            {
                CreateExpectedGeneralCategory(),
                CreateExpectedConstantBcCategory("catchment_boundary", 0),
                CreateExpectedConstantBcCategory("lateral_source", 0),
                CreateExpectedConstantBcCategory("catchment_linked_to_wwtp_boundary", 0)
            };

            bcFileWriter.Received(1).WriteBcFile(
                MatchingCategories(expectedCategories),
                filePath);
        }

        [Test]
        public void Two_unpaved_catchments_with_a_link_to_the_same_lateral_results_in_only_one_boundary_being_written()
        {
            // Setup
            var delftBcWriter = Substitute.For<IBcFileWriter>();
            var rrBoundaryDataFileWriter = new RainfallRunoffBoundaryDataFileWriter(delftBcWriter);
            
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
            var expectedCategories = new List<IDelftIniCategory>
            {
                CreateExpectedGeneralCategory(),
                CreateExpectedConstantBcCategory(lateralSourceName, 0)
            };
            
            delftBcWriter.Received(1).WriteBcFile(MatchingCategories(expectedCategories), filePath);
        }

        private static DelftIniCategory CreateExpectedGeneralCategory()
        {
            var generalCategory = new DelftIniCategory("General");
            generalCategory.AddProperty("fileVersion", "1.01", "File version. Do not edit this.");
            generalCategory.AddProperty("fileType", "boundConds", "File type. Do not edit this.");

            return generalCategory;
        }

        private static DelftBcCategory CreateExpectedConstantBcCategory(string name, double value)
        {
            var bcCategory = new DelftBcCategory("Boundary");
            bcCategory.AddProperty("name", name, "Name of the boundary location (node id)");
            bcCategory.AddProperty("function", "constant", "Possible values: TimeSeries, QHTable");
            bcCategory.AddProperty("timeInterpolation", (string)null, "Possible values: linear, block-from (value holds from specified time step), block-to (value holds until next specified time step)");

            bcCategory.Table.Add(CreateWaterLevelQuantityData(value));

            return bcCategory;
        }

        private static DelftBcCategory CreateExpectedTimeSeriesBcCategory(string name, DateTime startDate, params double[] values)
        {
            var bcCategory = new DelftBcCategory("Boundary");
            bcCategory.AddProperty("name", name, "Name of the boundary location (node id)");
            bcCategory.AddProperty("function", "timeseries", "Possible values: TimeSeries, QHTable");
            bcCategory.AddProperty("timeInterpolation", "linear", "Possible values: linear, block-from (value holds from specified time step), block-to (value holds until next specified time step)");

            double[] times = Enumerable.Range(0, values.Length).Select(i => (startDate.AddDays(i) - startDate).TotalMinutes).ToArray();
            bcCategory.Table.Add(CreateTimeQuantityData(startDate, times));
            bcCategory.Table.Add(CreateWaterLevelQuantityData(values));

            return bcCategory;
        }

        private static DelftBcQuantityData CreateWaterLevelQuantityData(params double[] values)
        {
            var waterLevelQuantityProperty = new DelftIniProperty("quantity", "water_level", "Possible values (netcdf-CF standard): time, water_level, water_discharge, sea_water_salinity");
            var waterLevelUnitProperty = new DelftIniProperty("unit", "m", "Possible values for 'time' column: yyyy-MM-dd hh:mm:ss, seconds since begintime format: yyyy-MM-dd hh:mm:ss +00:00 (+00:00: time zone), minutes since begintime, hours since begintime");
            return new DelftBcQuantityData(waterLevelQuantityProperty, waterLevelUnitProperty, values);
        }

        private static DelftBcQuantityData CreateTimeQuantityData(DateTime startDate, double[] values)
        {
            var timeQuantityProperty = new DelftIniProperty("quantity", "time", "Possible values (netcdf-CF standard): time, water_level, water_discharge, sea_water_salinity");
            var timeLevelUnitProperty = new DelftIniProperty("unit", $"minutes since {startDate:yyyy-MM-dd HH:mm:ss}", "Possible values for 'time' column: yyyy-MM-dd hh:mm:ss, seconds since begintime format: yyyy-MM-dd hh:mm:ss +00:00 (+00:00: time zone), minutes since begintime, hours since begintime");
            return new DelftBcQuantityData(timeQuantityProperty, timeLevelUnitProperty, values);
        }

        private static IEnumerable<IDelftIniCategory> MatchingCategories(IEnumerable<IDelftIniCategory> expectedCategories)
        {
            return Arg.Is<IEnumerable<IDelftIniCategory>>(actualCategories => CategoriesEqual(actualCategories.ToArray(), expectedCategories.ToArray()));
        }

        private static bool CategoriesEqual(IDelftIniCategory[] actualCategories, IDelftIniCategory[] expectedCategories)
        {
            if (actualCategories.Length != expectedCategories.Length)
            {
                return false;
            }

            var categoryComparer = new DelftIniCategoryEqualityComparer();
            var bcCategoryComparer = new DelftBcCategoryEqualityComparer();

            for (var i = 0; i < actualCategories.Length; i++)
            {
                if (actualCategories[i] is IDelftBcCategory actualBc && expectedCategories[i] is IDelftBcCategory expectedBc)
                {
                    if (!bcCategoryComparer.Equals(actualBc, expectedBc))
                    {
                        return false;
                    }
                }

                else if (!categoryComparer.Equals(actualCategories[i], expectedCategories[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}