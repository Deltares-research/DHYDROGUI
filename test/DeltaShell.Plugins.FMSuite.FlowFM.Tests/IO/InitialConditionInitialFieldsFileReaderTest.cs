using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
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
    public class InitialConditionInitialFieldsFileReaderTest
    {
        [Test]
        public void GivenInvalidPath_WhenCallingReadFile_ThenThrowsException()
        {
            string invalidPath = "invalidPath";
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                TestDelegate action = () =>
                    InitialConditionInitialFieldsFileReader.ReadFile(invalidPath, modelDefinition);

                Assert.Throws<FileReadingException>(action);
            }
        }

        [Test]
        public void GivenFileWithNoCategories_WhenCallingReadFile_ThenThrowsException()
        {
            var noCategoriesFile = TestHelper.GetTestFilePath(@"IO\noCategories.ini");
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                TestDelegate action = () =>
                    InitialConditionInitialFieldsFileReader.ReadFile(noCategoriesFile, modelDefinition);

                Assert.Throws<FileReadingException>(action);
            }
        }

        [Test]
        public void GivenInitialFieldsFile_WhenCallingReadFile_ThenReturnsExpectedTuple()
        {
            var multipleValidCategoriesFile = TestHelper.GetTestFilePath(@"IO\initialFieldsWaterLevel_expected.ini");
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                (InitialConditionQuantity, string) expectedReturnValue =
                    (InitialConditionQuantity.WaterLevel, "InitialWaterLevel.ini");

                (InitialConditionQuantity, string) actualReturnValue =
                    InitialConditionInitialFieldsFileReader.ReadFile(multipleValidCategoriesFile, modelDefinition);

                Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue)); 
            }
        }

        [Test]
        public void GivenFileWithMultipleValidCategories_WhenCallingReadFile_ThenReturnsDataFromFirstCategoryAndLogsWarning()
        {
            var multipleValidCategoriesFile = TestHelper.GetTestFilePath(@"IO\multipleValidCategories.ini");
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                (InitialConditionQuantity, string) expectedReturnValue =
                    (InitialConditionQuantity.WaterDepth, "InitialWaterDepth.ini");


                (InitialConditionQuantity, string) actualReturnValue =
                    InitialConditionInitialFieldsFileReader.ReadFile(multipleValidCategoriesFile, modelDefinition);

                Action action = () =>
                    InitialConditionInitialFieldsFileReader.ReadFile(multipleValidCategoriesFile, modelDefinition);
                TestHelper.AssertLogMessageIsGenerated(action, Properties.Resources.Initial_Condition_Warning_Only_one_quantity_type_is_currently_supported_reading_the_first_and_ignoring_all_others, 1);


                Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue)); 
            }
        }

        [Test]
        [TestCase(PointwiseOperationType.Add)]
        [TestCase(PointwiseOperationType.Overwrite)]
        [TestCase(PointwiseOperationType.OverwriteWhereMissing)]
        [TestCase(PointwiseOperationType.Multiply)]
        [TestCase(PointwiseOperationType.Maximum)]
        [TestCase(PointwiseOperationType.Minimum)]
        public void GivenRoughnessSpatialOperationPolFileQuantity_WhenReadingToFile_ThenIsSameAsGenerated(PointwiseOperationType pointwiseOperationType)
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
                        $"{(int) InitialConditionQuantity.WaterLevel}");

                    //Add a 'value' operation, another warning should be given.
                    var dataItem = fmModel.AllDataItems.FirstOrDefault(di =>
                        di.Name.Equals(WaterFlowFMModelDefinition.RoughnessDataItemName,
                            StringComparison.InvariantCultureIgnoreCase));

                    // retrieve / create value converter for roughness dataitem
                    var valueConverter =
                        SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem,
                            WaterFlowFMModelDefinition.RoughnessDataItemName);

                    valueConverter.SpatialOperationSet.Inputs[0].FeatureType = typeof(UnstructuredGridVertexCoverage);
                    valueConverter.SpatialOperationSet.Inputs[0].Provider = new CoverageFeatureProvider
                        {Coverage = fmModel.Roughness};

                    var maskFeatureColl = new FeatureCollection(
                        new[]
                        {
                            new Feature()
                            {
                                Geometry = new Polygon(
                                    new LinearRing(new[]
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
                    var initialSpatialOps = new List<string>() {WaterFlowFMModelDefinition.RoughnessDataItemName};
                    // update model definition (called during export)
                    fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions,
                        initialSpatialOps);
                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, ((int)InitialConditionQuantity.WaterLevel).ToString());
                    // call
                    InitialConditionInitialFieldsFileWriter.WriteFile(actualFile, fmModel.ModelDefinition, true);
                }
                
                using (var fmModel = new WaterFlowFMModel() { MduFilePath = mduFilePath })
                {
                    var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                    fmModel.Grid = grid;

                    
                    var initialSpatialOps = new List<string>() { WaterFlowFMModelDefinition.RoughnessDataItemName };
                    
                    // update model definition (called during export)
                    fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions,
                        initialSpatialOps);

                    InitialConditionInitialFieldsFileReader.ReadFile(actualFile, fmModel.ModelDefinition);
                    
                    // assert
                    Assert.That(File.Exists(actualFile), Is.True);
                    Assert.That(fmModel.ModelDefinition.SpatialOperations.Count, Is.EqualTo(1));
                    Assert.That(fmModel.ModelDefinition.SpatialOperations.ContainsKey(WaterFlowFMModelDefinition.RoughnessDataItemName), Is.True);
                    Assert.That(fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName].Count, Is.EqualTo(1));
                    Assert.That(fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0], Is.InstanceOf<SetValueOperation>());
                    Assert.That(((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).OperationType, Is.EqualTo(pointwiseOperationType));
                    Assert.That(((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).Value, Is.EqualTo(80.1).Within(0.001));
                    Assert.That(((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).Name, Does.Contain("Maud"));
                    Assert.That(((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).Mask.Provider, Is.InstanceOf<FeatureCollection>());
                    Assert.That(((FeatureCollection)((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).Mask.Provider).Features.Count, Is.EqualTo(1));
                    Assert.That(((Feature)((FeatureCollection)((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).Mask.Provider).Features[0]).Geometry, Is.Not.Null);
                    Assert.That(((Feature)((FeatureCollection)((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).Mask.Provider).Features[0]).Geometry.Coordinates, Contains.Item(new Coordinate(0, 0)));
                    Assert.That(((Feature)((FeatureCollection)((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).Mask.Provider).Features[0]).Geometry.Coordinates, Contains.Item(new Coordinate(10, 10)));
                    Assert.That(((Feature)((FeatureCollection)((SetValueOperation)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).Mask.Provider).Features[0]).Geometry.Coordinates, Contains.Item(new Coordinate(20, -20)));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(PointwiseOperationType.Add)]
        [TestCase(PointwiseOperationType.Overwrite)]
        [TestCase(PointwiseOperationType.OverwriteWhereMissing)]
        [TestCase(PointwiseOperationType.Multiply)]
        [TestCase(PointwiseOperationType.Maximum)]
        [TestCase(PointwiseOperationType.Minimum)]
        public void WriteFile_ReadFile_WithWaterLevelQuantity_CorrectOperand(PointwiseOperationType operand)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string filePath = Path.Combine(temp.Path, "initialFields.ini");

                var writeSpatialOperation = new ImportSamplesSpatialOperationExtension
                {
                    Operand = operand,
                    FilePath = filePath
                };

                var writeModelDefinition = new WaterFlowFMModelDefinition();
                writeModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.InitialWaterLevelDataItemName] = new List<ISpatialOperation> {writeSpatialOperation};

                // Call: write
                InitialConditionInitialFieldsFileWriter.WriteFile(filePath, writeModelDefinition, true);

                // Call: read
                var readModelDefinition = new WaterFlowFMModelDefinition();
                InitialConditionInitialFieldsFileReader.ReadFile(filePath, readModelDefinition);

                var readSpatialOperation = readModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.InitialWaterLevelDataItemName].Single() as ImportSamplesSpatialOperationExtension;

                // Assert
                Assert.That(readSpatialOperation, Is.Not.Null);
                Assert.That(readSpatialOperation.Operand, Is.EqualTo(writeSpatialOperation.Operand));
            }
        }

        [Test]
        public void GivenRoughnessSpatialOperationSampleQuantity_WhenReadingFromFile_ThenIsSameAsGenerated()
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
                        $"{(int) InitialConditionQuantity.WaterLevel}");

                    //Add a 'value' operation, another warning should be given.
                    var dataItem = fmModel.AllDataItems.FirstOrDefault(di =>
                        di.Name.Equals(WaterFlowFMModelDefinition.RoughnessDataItemName,
                            StringComparison.InvariantCultureIgnoreCase));

                    // retrieve / create value converter for roughness dataitem
                    var valueConverter =
                        SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem,
                            WaterFlowFMModelDefinition.RoughnessDataItemName);

                    valueConverter.SpatialOperationSet.Inputs[0].FeatureType = typeof(UnstructuredGridVertexCoverage);
                    valueConverter.SpatialOperationSet.Inputs[0].Provider = new CoverageFeatureProvider
                        {Coverage = fmModel.Roughness};

                    var maskFeatureColl = new FeatureCollection(
                        new[]
                        {
                            new Feature()
                            {
                                Geometry = new Polygon(
                                    new LinearRing(new[]
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
                    var initialSpatialOps = new List<string>() {WaterFlowFMModelDefinition.RoughnessDataItemName};
                    // update model definition (called during export)
                    fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions,
                        initialSpatialOps);
                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, ((int)InitialConditionQuantity.WaterLevel).ToString());

                    // call
                    InitialConditionInitialFieldsFileWriter.WriteFile(actualFile, fmModel.ModelDefinition, true);
                }
                using (var fmModel = new WaterFlowFMModel() { MduFilePath = mduFilePath })
                {
                    var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                    fmModel.Grid = grid;


                    var initialSpatialOps = new List<string>() { WaterFlowFMModelDefinition.RoughnessDataItemName };

                    // update model definition (called during export)
                    fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions,
                        initialSpatialOps);

                    InitialConditionInitialFieldsFileReader.ReadFile(actualFile, fmModel.ModelDefinition);

                    
                    // assert
                    Assert.That(File.Exists(actualFile), Is.True);
                    Assert.That(fmModel.ModelDefinition.SpatialOperations.Count, Is.EqualTo(1));
                    Assert.That(fmModel.ModelDefinition.SpatialOperations.ContainsKey(WaterFlowFMModelDefinition.RoughnessDataItemName), Is.True);
                    Assert.That(fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName].Count, Is.EqualTo(1));
                    Assert.That(fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0], Is.InstanceOf<ImportSamplesSpatialOperationExtension>());
                    Assert.That(((ImportSamplesSpatialOperationExtension)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).FilePath, Does.EndWith("xyz"));
                    Assert.That(((ImportSamplesSpatialOperationExtension)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).AveragingMethod, Is.EqualTo(GridCellAveragingMethod.ClosestPoint));
                    Assert.That(((ImportSamplesSpatialOperationExtension)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Averaging));
                    Assert.That(((ImportSamplesSpatialOperationExtension)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).RelativeSearchCellSize, Is.EqualTo(1).Within(0.0001));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        public void GivenRoughnessSpatialOperationAddSampleQuantity_WhenReadingFromFile_ThenIsSameAsGenerated()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var actualFile = Path.Combine(tempFolder, "initialFields.ini");

            try
            {
                // setup
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel() {MduFilePath = mduFilePath})
                {
                    var grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 2, 2);
                    fmModel.Grid = grid;

                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D,
                        $"{(int) InitialConditionQuantity.WaterLevel}");
                    //Add a 'value' operation, another warning should be given.
                    var dataItem = fmModel.AllDataItems.FirstOrDefault(di =>
                        di.Name.Equals(WaterFlowFMModelDefinition.RoughnessDataItemName,
                            StringComparison.InvariantCultureIgnoreCase));

                    // retrieve / create value converter for roughness dataitem
                    var valueConverter =
                        SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem,
                            WaterFlowFMModelDefinition.RoughnessDataItemName);
                    
                    //var coverageProvider = SpatialOperationTestHelper.CreateSquareCoverageFeatureProvider();
                    
                    // Generate samples to add
                    var samples = new AddSamplesOperation(false);
                    //samples.SetInputData(AddSamplesOperation.MainInputName, coverageProvider);
                    
                    samples.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
                    {
                        PointCloud = new PointCloud
                        {
                            PointValues = new List<IPointValue>
                    {
                        new PointValue { X = fmModel.Grid.Cells[0].CenterX, Y = fmModel.Grid.Cells[0].CenterY, Value = 45},
                        new PointValue { X = fmModel.Grid.Cells[1].CenterX, Y = fmModel.Grid.Cells[1].CenterY, Value = 67},
                        new PointValue { X = fmModel.Grid.Cells[2].CenterX, Y = fmModel.Grid.Cells[2].CenterY, Value = 78},
                        new PointValue { X = fmModel.Grid.Cells[3].CenterX, Y = fmModel.Grid.Cells[3].CenterY, Value = 58},
                        new PointValue { X = fmModel.Grid.Cells[4].CenterX, Y = fmModel.Grid.Cells[4].CenterY, Value = 39}
                    }
                        }
                    });

                    // add samples
                    Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(samples));

                    // create an interpolate operation using the samples added earlier
                    var interpolateOperation = new InterpolateOperation()
                    {
                        InterpolationMethod = SpatialInterpolationMethod.Averaging,
                        OperationType = PointwiseOperationType.Overwrite
                    };
                    //interpolateOperation.SetInputData(InterpolateOperation.InputSamplesName, samples.Output.Provider);
                    //interpolateOperation.Mask.Provider = new FeatureCollection(new List<Feature>(), typeof(Feature));
                    interpolateOperation.LinkInput(InterpolateOperation.InputSamplesName, samples.Output);
                    Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(interpolateOperation));

                    valueConverter.SpatialOperationSet.Execute();
                    var initialSpatialOps = new List<string>() { WaterFlowFMModelDefinition.RoughnessDataItemName };
                    // update model definition (called during export)
                    fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions,
                        initialSpatialOps);
                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, ((int)InitialConditionQuantity.WaterLevel).ToString());

                    // call
                    InitialConditionInitialFieldsFileWriter.WriteFile(actualFile, fmModel.ModelDefinition,true);
                }

                using (var fmModel = new WaterFlowFMModel() { MduFilePath = mduFilePath })
                {
                    var grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 2, 2);
                    fmModel.Grid = grid;
                    
                    var initialSpatialOps = new List<string>() { WaterFlowFMModelDefinition.RoughnessDataItemName };

                    // update model definition (called during export)
                    fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions,
                        initialSpatialOps);

                    InitialConditionInitialFieldsFileReader.ReadFile(actualFile, fmModel.ModelDefinition);

                    // assert
                    Assert.That(File.Exists(actualFile), Is.True);
                    Assert.That(fmModel.ModelDefinition.SpatialOperations.Count, Is.EqualTo(1));
                    Assert.That(fmModel.ModelDefinition.SpatialOperations.ContainsKey(WaterFlowFMModelDefinition.RoughnessDataItemName), Is.True);
                    Assert.That(fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName].Count, Is.EqualTo(1));
                    Assert.That(fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0], Is.InstanceOf<ImportSamplesSpatialOperationExtension>());
                    Assert.That(((ImportSamplesSpatialOperationExtension)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).FilePath, Does.EndWith("xyz"));
                    Assert.That(((ImportSamplesSpatialOperationExtension)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).AveragingMethod, Is.EqualTo(GridCellAveragingMethod.ClosestPoint));
                    Assert.That(((ImportSamplesSpatialOperationExtension)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Averaging));
                    Assert.That(((ImportSamplesSpatialOperationExtension)fmModel.ModelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName][0]).RelativeSearchCellSize, Is.EqualTo(1).Within(0.0001));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase("tif", "GeoTiff", "waterlevel", WaterFlowFMModelDefinition.InitialWaterLevelDataItemName)]
        [TestCase("asc", "arcinfo", "waterlevel", WaterFlowFMModelDefinition.InitialWaterLevelDataItemName)]
        [TestCase("tif", "GeoTiff", "bedlevel", WaterFlowFMModelDefinition.BathymetryDataItemName)]
        [TestCase("asc", "arcinfo", "bedlevel", WaterFlowFMModelDefinition.BathymetryDataItemName)]
        [TestCase("tif", "GeoTiff", "InfiltrationCapacity", WaterFlowFMModelDefinition.InfiltrationDataItemName)]
        [TestCase("asc", "arcinfo", "InfiltrationCapacity", WaterFlowFMModelDefinition.InfiltrationDataItemName)]
        public void ReadFile_CreatesTheCorrectSpatialOperation(string extension, string fileType, string quantity, string dataItemName)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string content =
                    "[General]                                       " + Environment.NewLine +
                    "    fileVersion           = 2.00                " + Environment.NewLine +
                    "    fileType              = iniField            " + Environment.NewLine +
                    "                                                " + Environment.NewLine +
                    "[Initial]                                       " + Environment.NewLine +
                    $"   quantity              = {quantity}          " + Environment.NewLine +
                    $"   dataFile              = quantity.{extension}" + Environment.NewLine +
                    $"   dataFileType          = {fileType}          " + Environment.NewLine +
                    "    interpolationMethod   = averaging           " + Environment.NewLine +
                    "    operand               = O                   " + Environment.NewLine +
                    "    locationType          = 2d                  " + Environment.NewLine +
                    "    averagingType         = nearestNb           " + Environment.NewLine +
                    "    averagingRelSize      = 1                   " + Environment.NewLine +
                    "    averagingNumMin       = 4                   ";


                string filePath = temp.CreateFile("initialFields.ini", content);
                var modelDefinition = new WaterFlowFMModelDefinition();
                
                // Call
                InitialConditionInitialFieldsFileReader.ReadFile(filePath, modelDefinition);
                
                // Assert
                var spatialOperation = modelDefinition.SpatialOperations[dataItemName].Single() as ImportRasterSamplesSpatialOperationExtension;
                
                Assert.That(spatialOperation, Is.Not.Null);
                Assert.That(spatialOperation.Name, Is.EqualTo("quantity"));
                Assert.That(spatialOperation.FilePath, Is.EqualTo(Path.Combine(temp.Path, $"quantity.{extension}")));
                Assert.That(spatialOperation.Operand, Is.EqualTo(PointwiseOperationType.Overwrite));
                Assert.That(spatialOperation.AveragingMethod, Is.EqualTo(GridCellAveragingMethod.ClosestPoint));
                Assert.That(spatialOperation.RelativeSearchCellSize, Is.EqualTo(1));
                Assert.That(spatialOperation.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Averaging));
                Assert.That(spatialOperation.MinSamplePoints, Is.EqualTo(4));
            }
        }
    }
}