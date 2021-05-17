using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class InitialConditionInitialFieldsFileWriterTest
    {
        [TestCase(InitialConditionQuantity.WaterLevel)]
        [TestCase(InitialConditionQuantity.WaterDepth)]
        public void GivenInitialConditionQuantity_WhenWritingToFile_ThenIsSameAsExpectedFile(
            InitialConditionQuantity globalQuantity)
        {
            var expectedFile = TestHelper.GetTestFilePath($"IO\\initialFields{globalQuantity}_expected.ini");
            var tempFolder = FileUtils.CreateTempDirectory();
            var actualFile = Path.Combine(tempFolder, "initialFields.ini");

            try
            {
                // setup
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel() {MduFilePath = mduFilePath})
                {
                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D,
                        $"{(int) globalQuantity}");

                    // call
                    InitialConditionInitialFieldsFileWriter.WriteFile(actualFile, fmModel.ModelDefinition, false);

                    // assert
                    Assert.That(File.Exists(actualFile), Is.True);
                    FileAssert.AreEqual(actualFile, expectedFile); 
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        public void GivenRoughnessSpatialOperationQuantity_WhenWritingToFile_ThenIsSameAsExpectedFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var actualFile = Path.Combine(tempFolder, "initialFields.ini");

            try
            {
                // setup
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel() {MduFilePath = mduFilePath})
                {
                    var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                    fmModel.Grid = grid;

                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D,
                        $"{(int)InitialConditionQuantity.WaterLevel}");

                    //Add a 'value' operation, another warning should be given.
                    var dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name.Equals(WaterFlowFMModelDefinition.RoughnessDataItemName, StringComparison.InvariantCultureIgnoreCase));

                    // retrieve / create value converter for roughness dataitem
                    var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, WaterFlowFMModelDefinition.RoughnessDataItemName);

                    valueConverter.SpatialOperationSet.Inputs[0].FeatureType = typeof(UnstructuredGridVertexCoverage);
                    valueConverter.SpatialOperationSet.Inputs[0].Provider = new CoverageFeatureProvider { Coverage = fmModel.Roughness };

                    var maskFeatureColl = new FeatureCollection(
                        new[]
                        {
                        new Feature()
                        {
                            Geometry = new Polygon(
                                new LinearRing(new []
                                {
                                    new Coordinate(0, 0), new Coordinate(10, 10),
                                    new Coordinate(20, -20), new Coordinate(0, 0)
                                }))
                        }
                        }, typeof(Feature));

                    var setValueOperation = new SetValueOperation
                    {
                        Name = "Maud",
                        Value = 80.1,
                        OperationType = PointwiseOperationType.Overwrite
                    };
                    setValueOperation.Mask.Provider = maskFeatureColl;
                    Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(setValueOperation));

                    var cropOperation = new CropOperation();
                    cropOperation.Mask.Provider = maskFeatureColl;
                    Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(cropOperation));

                    var smoothOperation = new SmoothingOperation
                    {
                        InverseDistanceWeightExponent = 2.0,
                        IterationCount = 3
                    };
                    smoothOperation.Mask.Provider = maskFeatureColl;
                    Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(smoothOperation));
                    
                    valueConverter.SpatialOperationSet.Execute();
                    var initialSpatialOps = new List<string>() { WaterFlowFMModelDefinition.RoughnessDataItemName };
                    // update model definition (called during export)
                    fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);
                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, ((int)InitialConditionQuantity.WaterLevel).ToString());
                    // call
                    InitialConditionInitialFieldsFileWriter.WriteFile(actualFile, fmModel.ModelDefinition, true);
                    var categories = new DelftIniReader().ReadDelftIniFile(actualFile);
                    var parameterFriction = categories.FirstOrDefault(c => c.Name.Equals(InitialConditionRegion.ParameterIniHeader));

                    // assert
                    Assert.That(File.Exists(actualFile), Is.True);
                    Assert.IsNotNull(parameterFriction);
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.Quantity.Key), Is.EqualTo(ExtForceQuantNames.FrictCoef));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.DataFileType.Key), Is.EqualTo("sample"));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.InterpolationMethod.Key), Is.EqualTo("averaging"));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.Operand.Key), Is.EqualTo("O"));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.LocationType.Key), Is.EqualTo("2d"));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.AveragingType.Key), Is.EqualTo("nearestNb"));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.AveragingRelSize.Key), Is.EqualTo("1"));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        [TestCase(PointwiseOperationType.Add)]
        [TestCase(PointwiseOperationType.Overwrite)]
        [TestCase(PointwiseOperationType.OverwriteWhereMissing)]
        [TestCase(PointwiseOperationType.Multiply)]
        public void GivenRoughnessSpatialOperationPolFileQuantity_WhenWritingToFile_ThenIsSameAsExpectedFile(PointwiseOperationType pointwiseOperationType)
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var actualFile = Path.Combine(tempFolder, "initialFields.ini");

            try
            {
                // setup
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel() {MduFilePath = mduFilePath})
                {
                    var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                    fmModel.Grid = grid;

                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D,
                        $"{(int)InitialConditionQuantity.WaterLevel}");

                    //Add a 'value' operation, another warning should be given.
                    var dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name.Equals(WaterFlowFMModelDefinition.RoughnessDataItemName, StringComparison.InvariantCultureIgnoreCase));

                    // retrieve / create value converter for roughness dataitem
                    var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, WaterFlowFMModelDefinition.RoughnessDataItemName);

                    valueConverter.SpatialOperationSet.Inputs[0].FeatureType = typeof(UnstructuredGridVertexCoverage);
                    valueConverter.SpatialOperationSet.Inputs[0].Provider = new CoverageFeatureProvider { Coverage = fmModel.Roughness };

                    var maskFeatureColl = new FeatureCollection(
                        new[]
                        {
                        new Feature()
                        {
                            Geometry = new Polygon(
                                new LinearRing(new []
                                {
                                    new Coordinate(0, 0), new Coordinate(10, 10),
                                    new Coordinate(20, -20), new Coordinate(0, 0)
                                }))
                        }
                        }, typeof(Feature));

                    var setValueOperation = new SetValueOperation
                    {
                        Name = "Maud",
                        Value = 80.1,
                        OperationType = pointwiseOperationType
                    };
                    setValueOperation.Mask.Provider = maskFeatureColl;
                    Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(setValueOperation));

                    valueConverter.SpatialOperationSet.Execute();
                    var initialSpatialOps = new List<string>() { WaterFlowFMModelDefinition.RoughnessDataItemName };
                    // update model definition (called during export)
                    fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);
                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, ((int) InitialConditionQuantity.WaterLevel).ToString());
                    // call
                    InitialConditionInitialFieldsFileWriter.WriteFile(actualFile, fmModel.ModelDefinition, true);
                    var categories = new DelftIniReader().ReadDelftIniFile(actualFile);
                    var parameterFriction = categories.FirstOrDefault(c => c.Name.Equals(InitialConditionRegion.ParameterIniHeader));

                    // assert
                    Assert.That(File.Exists(actualFile), Is.True);
                    Assert.IsNotNull(parameterFriction);
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.Quantity.Key), Is.EqualTo(ExtForceQuantNames.FrictCoef));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.DataFileType.Key), Is.EqualTo("polygon"));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.InterpolationMethod.Key), Is.EqualTo("constant"));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.Operand.Key), Is.EqualTo(ExtForceQuantNames.OperatorToStringMapping[ExtForceQuantNames.OperatorMapping[pointwiseOperationType]]));
                    Assert.That(parameterFriction.ReadProperty<string>(InitialConditionRegion.LocationType.Key), Is.EqualTo("2d"));
                    Assert.That(parameterFriction.ReadProperty<double>(InitialConditionRegion.Value.Key), Is.EqualTo(80.1).Within(0.001));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [TestCase("tif", "GeoTiff")]
        [TestCase("asc", "arcinfo")]
        [TestCase("xyz", "sample")]
        public void WriteFile_WithImportSamplesOperation_WritesCorrectFileType(string fileExtension, string expFileType)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string filePath = Path.Combine(temp.Path, "initialFields.ini");

                var writeSpatialOperation = new ImportSamplesSpatialOperationExtension
                {
                    FilePath = $"quantity.{fileExtension}"
                };

                var writeModelDefinition = new WaterFlowFMModelDefinition();
                writeModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.InitialWaterLevelDataItemName] = new List<ISpatialOperation> {writeSpatialOperation};

                // Call
                InitialConditionInitialFieldsFileWriter.WriteFile(filePath, writeModelDefinition, true);

                // Assert
                Assert.That(filePath, Does.Exist);

                string line = File.ReadAllLines(filePath).First(l => l.Contains("dataFileType"));
                string fileType = line.Split('=')[1].Trim();
                
                Assert.That(fileType, Is.EqualTo(expFileType));

            }
        }
    }
}