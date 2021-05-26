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

        [TestCase("tif", "GeoTIFF", WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, "waterlevel" )]
        [TestCase("asc", "arcinfo", WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, "waterlevel")]
        [TestCase("xyz", "sample", WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, "waterlevel")]
        [TestCase("tif", "GeoTIFF", WaterFlowFMModelDefinition.BathymetryDataItemName, "bedlevel")]
        [TestCase("asc", "arcinfo", WaterFlowFMModelDefinition.BathymetryDataItemName, "bedlevel")]
        [TestCase("xyz", "sample", WaterFlowFMModelDefinition.BathymetryDataItemName, "bedlevel")]
        [TestCase("tif", "GeoTIFF", WaterFlowFMModelDefinition.InfiltrationDataItemName, "InfiltrationCapacity")]
        [TestCase("asc", "arcinfo", WaterFlowFMModelDefinition.InfiltrationDataItemName, "InfiltrationCapacity")]
        [TestCase("xyz", "sample", WaterFlowFMModelDefinition.InfiltrationDataItemName, "InfiltrationCapacity")]
        public void WriteFile_WithImportSamplesOperation_WritesCorrectFile(string fileExtension, string expFileType, string dataItemName, string expQuantity)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string filePath = Path.Combine(temp.Path, "initialFields.ini");

                var operation = new ImportSamplesSpatialOperationExtension
                {
                    FilePath = $"quantity.{fileExtension}",
                    RelativeSearchCellSize = 1234.5678,
                    MinSamplePoints = 9,
                    AveragingMethod = GridCellAveragingMethod.SimpleAveraging,
                    InterpolationMethod = SpatialInterpolationMethod.Averaging,
                    Operand = PointwiseOperationType.Add
                };

                var writeModelDefinition = new WaterFlowFMModelDefinition();
                writeModelDefinition.SpatialOperations[dataItemName] = new List<ISpatialOperation> {operation};

                // Call
                InitialConditionInitialFieldsFileWriter.WriteFile(filePath, writeModelDefinition, true);

                // Assert
                Assert.That(filePath, Does.Exist);

                IDictionary<string, string> properties = GetProperties(filePath);
                
                Assert.That(properties["quantity"], Is.EqualTo(expQuantity));
                Assert.That(properties["dataFile"], Is.EqualTo($"quantity.{fileExtension}"));
                Assert.That(properties["dataFileType"], Is.EqualTo(expFileType));
                Assert.That(properties["interpolationMethod"], Is.EqualTo("averaging"));
                Assert.That(properties["operand"], Is.EqualTo("+"));
                Assert.That(properties["locationType"], Is.EqualTo("2d"));
                Assert.That(properties["averagingType"], Is.EqualTo("mean"));
                Assert.That(properties["averagingRelSize"], Is.EqualTo("1234.5678"));
                Assert.That(properties["averagingNumMin"], Is.EqualTo("9"));
            }
        }

        private static IDictionary<string, string> GetProperties(string filePath)
        {
            var dict = new Dictionary<string, string>();
            
            foreach (string line in File.ReadAllLines(filePath))
            {
                string[] kvp = line.Split('=');
                if (kvp.Length != 2)
                {
                    continue;
                }
                dict[kvp[0].Trim()] = kvp[1].Trim();
            }

            return dict;
        }
    }
}