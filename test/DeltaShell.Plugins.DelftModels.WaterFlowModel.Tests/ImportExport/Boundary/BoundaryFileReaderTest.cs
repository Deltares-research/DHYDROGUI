using System;
using System.IO;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    class BoundaryFileReaderTest
    {
        private WaterFlowModel1D originalModel;

        [SetUp]
        public void SetUp()
        {
            var startTime = DateTime.Now;
            originalModel = BoundaryFileReaderTestHelper.GetSimpleModel();
            
            originalModel.BoundaryConditions[1].DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            originalModel.BoundaryConditions[1].Flow = 19.63;

            originalModel.BoundaryConditions[2].DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            originalModel.BoundaryConditions[2].WaterLevel = 13.57;

            originalModel.BoundaryConditions[3].DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            originalModel.BoundaryConditions[3].Data.Arguments.Clear();
            originalModel.BoundaryConditions[3].Data.Arguments.Add(
                new Variable<DateTime>()
                {
                    Values = { startTime, startTime.AddHours(1), startTime.AddHours(2), startTime.AddHours(3), startTime.AddHours(4) }
                });
            originalModel.BoundaryConditions[3].Data.Components.Clear();
            originalModel.BoundaryConditions[3].Data.Components.Add(
                new Variable<double>()
                {
                    Values = { 11.13, 13.17, 17.19, 19.23, 23.29 }
                });

            originalModel.BoundaryConditions[4].DataType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable;
            originalModel.BoundaryConditions[4].Data.Arguments.Clear();
            originalModel.BoundaryConditions[4].Data.Arguments.Add(
                new Variable<double>()
                {
                    Values = { 11.13, 13.17, 17.19, 19.23, 23.29 }
                });
            originalModel.BoundaryConditions[4].Data.Components.Clear();
            originalModel.BoundaryConditions[4].Data.Components.Add(
                new Variable<double>()
                {
                    Values = { 11.13, 13.17, 17.19, 19.23, 23.29 }
                });

            originalModel.BoundaryConditions[5].DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            originalModel.BoundaryConditions[5].Data.Arguments.Clear();
            originalModel.BoundaryConditions[5].Data.Arguments.Add(
                new Variable<DateTime>()
                {
                    Values = { startTime, startTime.AddHours(1), startTime.AddHours(2), startTime.AddHours(3), startTime.AddHours(4) }
                });
            originalModel.BoundaryConditions[5].Data.Components.Clear();
            originalModel.BoundaryConditions[5].Data.Components.Add(
                new Variable<double>()
                {
                    Values = { 11.13, 13.17, 17.19, 19.23, 23.29 }
                });


            originalModel.LateralSourceData[0].DataType = WaterFlowModel1DLateralDataType.FlowConstant;
            originalModel.LateralSourceData[0].Flow = 47.92;

            originalModel.LateralSourceData[1].DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries;
            originalModel.LateralSourceData[1].Data.Arguments.Clear();
            originalModel.LateralSourceData[1].Data.Arguments.Add(
                new Variable<DateTime>()
                {
                    Values = { startTime, startTime.AddHours(1), startTime.AddHours(2), startTime.AddHours(3), startTime.AddHours(4) }
                });
            originalModel.LateralSourceData[1].Data.Components.Clear();
            originalModel.LateralSourceData[1].Data.Components.Add(
                new Variable<double>()
                {
                    Values = { 11.13, 13.17, 17.19, 19.23, 23.29 }
                });

            originalModel.LateralSourceData[2].DataType = WaterFlowModel1DLateralDataType.FlowWaterLevelTable;
            originalModel.LateralSourceData[2].Data.Arguments.Clear();
            originalModel.LateralSourceData[2].Data.Arguments.Add(
                new Variable<double>()
                {
                    Values = { 11.13, 13.17, 17.19, 19.23, 23.29 }
                });
            originalModel.LateralSourceData[2].Data.Components.Clear();
            originalModel.LateralSourceData[2].Data.Components.Add(
                new Variable<double>()
                {
                    Values = { 11.13, 13.17, 17.19, 19.23, 23.29 }
                });

            
            originalModel.StartTime = startTime;
            var windArguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.WindTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.WindTimeSeriesArgument2) };
            var windSpeedComponents = new[] { BoundaryFileWriterTestHelper.WindVelocityTimeSeriesComponent1, BoundaryFileWriterTestHelper.WindVelocityTimeSeriesComponent2 };
            var windDirectionComponents = new[] { BoundaryFileWriterTestHelper.WindDirectionTimeSeriesComponent1, BoundaryFileWriterTestHelper.WindDirectionTimeSeriesComponent2 };

            originalModel.Wind.Clear();
            originalModel.Wind.Arguments[0].SetValues(windArguments);
            originalModel.Wind.Velocity.SetValues(windSpeedComponents);
            originalModel.Wind.Direction.SetValues(windDirectionComponents);
        }

        [TearDown]
        public void TearDown() { }

        [Test]
        public void TestBoundaryConditionFileReaderGivesExpectedResults()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var boundaryConditionsFile = new ModelFileNames(Path.Combine(tempDir, ModelFileNames.ModelDefinitionFilename)).BoundaryConditions;
                WaterFlowModel1DBoundaryFileWriter.WriteFile(boundaryConditionsFile, originalModel);

                var readModel = BoundaryFileReaderTestHelper.GetSimpleModel();

                (new BoundaryConditionFileReader()).Read(boundaryConditionsFile, 
                                                         readModel.MeteoData, 
                                                         readModel.Wind, 
                                                         readModel.BoundaryConditions, 
                                                         readModel.LateralSourceData);

                Assert.AreEqual(originalModel.BoundaryConditions.Count, readModel.BoundaryConditions.Count);
                for (var i = 0; i < originalModel.BoundaryConditions.Count; i++)
                {
                    Assert.IsTrue(BoundaryFileReaderTestHelper.CompareBoundaryNodeData(originalModel.BoundaryConditions[i],
                        readModel.BoundaryConditions[i]));
                }

                Assert.AreEqual(originalModel.LateralSourceData.Count, readModel.LateralSourceData.Count);
                for (var i = 0; i < originalModel.LateralSourceData.Count; i++)
                {
                    Assert.IsTrue(BoundaryFileReaderTestHelper.CompareLateralSourceData(originalModel.LateralSourceData[i],
                        readModel.LateralSourceData[i]));
                }

                Assert.IsTrue(BoundaryFileReaderTestHelper.CompareWindSourceData(originalModel.Wind, readModel.Wind));
            });
        }
    }
}
