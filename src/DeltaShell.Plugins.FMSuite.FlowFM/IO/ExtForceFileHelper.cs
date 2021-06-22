using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Features;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    // Hacky, but it does the job...
    public class ImportSamplesSpatialOperationExtension : ImportSamplesOperation
    {
        public virtual double RelativeSearchCellSize { get; set; }
        public virtual GridCellAveragingMethod AveragingMethod { get; set; }
        public virtual int MinSamplePoints { get; set; }
        public virtual SpatialInterpolationMethod InterpolationMethod { get; set; }
        public virtual PointwiseOperationType Operand { get; set; }
        
        public ImportSamplesSpatialOperationExtension() : base(false)
        {
            RelativeSearchCellSize = 1;
            MinSamplePoints = 1;
            AveragingMethod = GridCellAveragingMethod.ClosestPoint;
            InterpolationMethod = SpatialInterpolationMethod.Averaging;
        }

        public virtual DelftTools.Utils.Tuple<ImportSamplesOperation, InterpolateOperation> CreateOperations()
        {
            var importSamplesOperation = CreateImportSamplesOperation();

            var interpolateOperation = new InterpolateOperation
            {
                Name = "Interpolate",
                CoordinateSystem = CoordinateSystem,
                Dirty = true,
                Enabled = Enabled,
                RelativeSearchCellSize = RelativeSearchCellSize,
                MinNumSamples = MinSamplePoints,
                GridCellAveragingMethod = AveragingMethod,
                InterpolationMethod = InterpolationMethod,
                OperationType = Operand
            };
            interpolateOperation.Mask.Provider = new FeatureCollection(new List<Feature>(), typeof(Feature));
            interpolateOperation.LinkInput(InterpolateOperation.InputSamplesName, importSamplesOperation.Output);
            return new DelftTools.Utils.Tuple<ImportSamplesOperation, InterpolateOperation>(importSamplesOperation, interpolateOperation);
        }

        protected virtual ImportSamplesOperation CreateImportSamplesOperation()
        {
            return new ImportSamplesOperation(false)
            {
                Name = Name,
                CoordinateSystem = CoordinateSystem,
                Dirty = true,
                Enabled = Enabled,
                FilePath = FilePath,
            };
        }
    }

    // YAGNI (GvdO) merge into extforce file, or strip extforce file from all path/file logic, but now it is not clear who is doing what
    public static class ExtForceFileHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExtForceFileHelper));
        public static string GetPliFileName(IFeatureData featureData)
        {
            var featurePart =
                new string(
                    ((Feature2D) featureData.Feature).Name?.Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                                                           .ToArray());
            if (string.IsNullOrEmpty(featurePart)) return null;

            var quantityPart = ExtForceQuantNames.GetPliQuantitySuffix(featureData);
            var filename = featurePart + quantityPart;
            while (File.Exists(filename))
            {
                filename += "_corr";
            }
            return filename + "." + PliFile<Feature2D>.Extension;
        }

        // TODO: add support for 3D-layers and astro-/harmonic corrections.

        public static ExtForceFileItem WriteBoundaryData(string filePath, FlowBoundaryCondition boundaryCondition,
                                                         DateTime modelReferenceDate, int bcIndex,
                                                         ExtForceFileItem existingExtForceFileItem = null,
                                                         bool writeToDisk = true)
        {
            var quantityName =
                ExtForceQuantNames.GetQuantityString(boundaryCondition);

            var operand = bcIndex == 0 ? Operator.Overwrite : Operator.Add;

            var extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
                {
                    FileName = GetPliFileName(boundaryCondition),
                    FileType = ExtForceQuantNames.FileTypes.PolyTim,
                    Method = 3,
                    Operand = ExtForceQuantNames.OperatorToStringMapping[operand]
                };

            extForceFileItem.Quantity = quantityName;
            extForceFileItem.Offset = boundaryCondition.Offset == 0 ? double.NaN : boundaryCondition.Offset;
            extForceFileItem.Factor = boundaryCondition.Factor == 1 ? double.NaN : boundaryCondition.Factor;

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            if (writeToDisk)
            {
                var directory = Path.GetDirectoryName(filePath);

                var pliFilePath = Path.Combine(directory, extForceFileItem.FileName);

                new PliFile<Feature2D>().Write(pliFilePath, new EventedList<Feature2D> {boundaryCondition.Feature});

                var count = boundaryCondition.Feature.Geometry.Coordinates.Count();

                var qhBoundary = boundaryCondition.DataType == BoundaryConditionDataType.Qh;
                if (qhBoundary)
                {
                    count = 1; //yet another inconsistency in the kernel
                }

                for (var i = 0; i < count; ++i)
                {
                    var dataFileExtension = ExtForceQuantNames.ForcingToFileExtensionMapping[boundaryCondition.DataType];

                    var dataFilePath = GetNumberedFilePath(pliFilePath, dataFileExtension, qhBoundary ? 0 : (i + 1));

                    var data = boundaryCondition.GetDataAtPoint(i);

                    if (data == null)
                    {
                        if (File.Exists(dataFilePath))
                        {
                            File.Delete(dataFilePath);
                        }
                    }
                    else
                    {
                        switch (boundaryCondition.DataType)
                        {
                            case BoundaryConditionDataType.HarmonicCorrection:
                            case BoundaryConditionDataType.Harmonics:
                            case BoundaryConditionDataType.AstroCorrection:
                            case BoundaryConditionDataType.AstroComponents:
                                new CmpFile().Write(dataFilePath, ToHarmonicComponents(data));
                                break;
                            case BoundaryConditionDataType.TimeSeries:
                                var depthLayerDefinition = boundaryCondition.GetDepthLayerDefinitionAtPoint(i);
                                if (depthLayerDefinition != null &&
                                    depthLayerDefinition.Type != VerticalProfileType.Uniform)
                                {
                                    new T3DFile().Write(
                                        dataFilePath.Replace(ExtForceQuantNames.TimFileExtension,ExtForceQuantNames.T3DFileExtension), data, depthLayerDefinition,
                                        modelReferenceDate);
                                }
                                else
                                {
                                    new TimFile().Write(dataFilePath, data, modelReferenceDate);                                    
                                }
                                break;
                            case BoundaryConditionDataType.Qh:
                                new QhFile().Write(dataFilePath, data);
                                break;
                            default:
                                throw new Exception("Writing boundary condition type " + boundaryCondition.DataType +
                                                    " not (yet) implemented");
                        }                        
                    }
                }
            }

            return extForceFileItem;
        }

        public static ExtForceFileItem WriteSourceAndSinkData(string filePath, SourceAndSink sourceAndSink,
                                                              DateTime referenceTime,
                                                              ExtForceFileItem existingExtForceFileItem,
                                                              bool writeToDisk, WaterFlowFMModelDefinition modelDefinition)
        {
            var extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(ExtForceQuantNames.SourceAndSink)
                {
                    FileName = GetPliFileName(sourceAndSink),
                    FileType = ExtForceQuantNames.FileTypes.PolyTim,
                    Method = 1,
                    Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite]
                };

            if (sourceAndSink.Area > 0)
            {
                extForceFileItem.ModelData[ExtForceFile.AreaKey] = sourceAndSink.Area;
            }

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            if (writeToDisk)
            {
                var directory = Path.GetDirectoryName(filePath);
                var pliFilePath = Path.Combine(directory, extForceFileItem.FileName);

                new PliFile<Feature2D>().Write(pliFilePath, new EventedList<Feature2D> {sourceAndSink.Feature});
                var dataFilePath = Path.ChangeExtension(pliFilePath, ExtForceQuantNames.TimFileExtension);

                var originalFunction = sourceAndSink.Function;
                if (originalFunction == null) return extForceFileItem;

                var function = (IFunction) originalFunction.Clone(true);

                RemoveDisabledComponentsFromSourceAndSink(sourceAndSink, modelDefinition, function);

                new TimFile().Write(dataFilePath, function, referenceTime);
            }

            return extForceFileItem;

        }

        private static void RemoveDisabledComponentsFromSourceAndSink(SourceAndSink sourceAndSink, WaterFlowFMModelDefinition modelDefinition,
            IFunction function)
        {
            if (!useProperty(modelDefinition, KnownProperties.UseSalinity))
                function.RemoveComponentByName(SourceAndSink.SalinityVariableName);

            if ((HeatFluxModelType)(modelDefinition.GetModelProperty(KnownProperties.Temperature).Value) == HeatFluxModelType.None)
                function.RemoveComponentByName(SourceAndSink.TemperatureVariableName);

            if (!useProperty(modelDefinition, GuiProperties.UseMorSed))
                sourceAndSink.SedimentFractionNames.ForEach(sfn => function.RemoveComponentByName(sfn));

            if (!useProperty(modelDefinition, KnownProperties.SecondaryFlow))
                function.RemoveComponentByName(SourceAndSink.SecondaryFlowVariableName);
        }

        private static bool useProperty(WaterFlowFMModelDefinition modelDefinition, string useProperty)
        {
            var enable = modelDefinition.GetModelProperty(useProperty);
            return enable != null ? (bool)enable.Value : true; // default to True
        }

        public static IEnumerable<string[]> GetBoundaryDataFiles(FlowBoundaryCondition boundaryCondition,
                                                                 BoundaryConditionSet boundaryCoditionSet,
                                                                 ExtForceFileItem existingExtForceFileItem = null)
        {
            var quantityName =
                ExtForceQuantNames.GetQuantityString(boundaryCondition);

            var extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
                {
                    FileName = GetPliFileName(boundaryCondition),
                    FileType = ExtForceQuantNames.FileTypes.PolyTim,
                };

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            foreach (int i in boundaryCondition.DataPointIndices)
            {
                var quantity = string.Format("{0} {1} at {2}", boundaryCondition.VariableDescription.ToLower(),
                                             boundaryCondition.DataType.GetDescription().ToLower(),
                                             boundaryCoditionSet.SupportPointNames[i]);

                string filePath;

                if (boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries &&
                    !boundaryCondition.IsVerticallyUniform)
                {
                    filePath = GetNumberedFilePath(extForceFileItem.FileName, ExtForceQuantNames.T3DFileExtension, i + 1);

                }
                else
                {
                    filePath = GetNumberedFilePath(extForceFileItem.FileName,
                        ExtForceQuantNames.ForcingToFileExtensionMapping[boundaryCondition.DataType], i + 1);
                }

                if (filePath == null)
                {
                    yield break; //TODO: emit warning.
                }

                yield return new[] { quantity, filePath };
            }
        }

        public static IEnumerable<string[]> GetSourceAndSinkDataFiles(SourceAndSink sourceAndSink, ExtForceFileItem existingExtForceFileItem)
        {
            const string quantityName = ExtForceQuantNames.SourceAndSink;

            var extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = GetPliFileName(sourceAndSink),
                FileType = ExtForceQuantNames.FileTypes.PolyTim,
            };

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            var filePath = Path.ChangeExtension(extForceFileItem.FileName, ExtForceQuantNames.TimFileExtension);
           
            yield return new[] { "Source/Sink", filePath };
        }

        private static readonly List<string> PreviousPaths = new List<string>();
        private const string DuplicationSuffixPattern = "{0}__{1:00000}{2}";
        public static void StartWritingSubFiles()
        {
            PreviousPaths.Clear();
        }

        // works (only) in conjuction with StartWritingSubFiles
        public static void AddSuffixInCaseOfDuplicateFile(ExtForceFileItem item)
        {
            if (PreviousPaths.Contains(item.FileName))
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item.FileName);
                var extension = Path.GetExtension(item.FileName);

                // search for unique filename using recognizable suffix
                int i = 2;
                while (true)
                {
                    var newFileName = String.Format(DuplicationSuffixPattern, fileNameWithoutExtension, i, extension);
                    if (!PreviousPaths.Contains(newFileName))
                    {
                        item.FileName = newFileName;
                        break;
                    }
                    i++;
                }
            }

            PreviousPaths.Add(item.FileName);
        }

        // TODO: add support for 3D and harmonic/astro corrections.

        public static IEnumerable<HarmonicComponent> ToHarmonicComponents(IFunction function)
        {
            var list = new EventedList<HarmonicComponent>();

            var isAstro = function.Arguments[0].ValueType == typeof (string);
            
            foreach (var arg in function.Arguments[0].Values)
            {
                var amplitude = (double) function.Components[0][arg];

                var phaseIndex = function.Components.Count == 4 ? 2 : 1;

                var phase = (double) function.Components[phaseIndex][arg];

                list.Add(isAstro
                             ? new HarmonicComponent((string) arg, amplitude, phase)
                             : new HarmonicComponent((double) arg, amplitude, phase));
            }
            return list;
        }

        public static BoundaryCondition ReadBoundaryConditionData(string filePath, Feature2D feature2D,
                                                                      ExtForceFileItem extForceFileItem,
                                                                      DateTime modelReferenceDate)
        {
            FlowBoundaryQuantityType quantityType;
            if (ExtForceQuantNames.TryParseBoundaryQuantityType(extForceFileItem.Quantity, out quantityType))
            {
                IList<int> dataFileNumbers;
                BoundaryConditionDataType dataType;
                string fileExtension;

                if (TryDetermineForcingType(feature2D, filePath, out dataType,
                    out dataFileNumbers,
                    out fileExtension))
                {
                    // create a quantity name from the type and the tracer name if it is set to tracer.
                    string quantityName = quantityType.ToString();
                    if (quantityType == FlowBoundaryQuantityType.Tracer)
                    {
                        quantityName += "_" + extForceFileItem.Quantity.Substring(9); // remove tracerbnd
                    }

                    var boundaryCondition = (FlowBoundaryCondition)new FlowBoundaryConditionFactory().CreateBoundaryCondition(feature2D, quantityName, dataType);
                    if (boundaryCondition == null)
                    {
                        log.ErrorFormat("Could not create boundary condition of quantity type {0}", quantityName);
                        return null;
                    }

                    if (!double.IsNaN(extForceFileItem.Offset))
                    {
                        boundaryCondition.Offset = extForceFileItem.Offset;
                    }
                    if (!double.IsNaN(extForceFileItem.Factor))
                    {
                        boundaryCondition.Factor = extForceFileItem.Factor;
                    }

                    var splitExtension = fileExtension.Split('|');

                    foreach (var extension in splitExtension)
                    {
                        foreach (var dataFileNumber in dataFileNumbers)
                        {
                            var pointIndex = dataFileNumber == 0 ? 0 : dataFileNumber - 1;

                            boundaryCondition.AddPoint(pointIndex);

                            var dataFilePath = GetNumberedFilePath(filePath, extension, dataFileNumber);

                            if (!File.Exists(dataFilePath)) continue;

                            ReadBoundaryConditionValues(boundaryCondition, dataFilePath, pointIndex, modelReferenceDate);

                            if (boundaryCondition.IsHorizontallyUniform)
                            {
                                break;
                            }
                        }
                    }
                    return boundaryCondition;
                }
            }
            return null;
        }

        public static SourceAndSink ReadSourceAndSinkData(string filePath, Feature2D feature2D,
                                                              ExtForceFileItem extForceFileItem,
                                                              DateTime modelReferenceDate)
        {
            if (!Equals(extForceFileItem.Quantity, ExtForceQuantNames.SourceAndSink)) return null;

            var sourceAndSink = new SourceAndSink
            {
                Feature = feature2D,
            };
            object area;
            extForceFileItem.ModelData.TryGetValue(ExtForceFile.AreaKey, out area);
            if (area != null)
            {
                sourceAndSink.Area = Convert.ToDouble(area);
            }

            var dataFilePath = Path.ChangeExtension(filePath, ExtForceQuantNames.TimFileExtension);

            if (!File.Exists(dataFilePath))
            {
                return sourceAndSink;
            }

            ReadSourceAndSinkValues(sourceAndSink, dataFilePath, modelReferenceDate);

            return sourceAndSink;
        }

        private static bool TryDetermineForcingType(IFeature feature2D, string filePath,
                                                   out BoundaryConditionDataType conditionDataType, out IList<int> dataFileNumbers,
                                                   out string fileExtension)
        {
            var timFileNumbers = GetDataFileNumbers(filePath, feature2D.Geometry.NumPoints,
                                                 ExtForceQuantNames.TimFileExtension);

            var t3DFileNumbers = GetDataFileNumbers(filePath, feature2D.Geometry.NumPoints,
                                                 ExtForceQuantNames.T3DFileExtension);
            
            if (timFileNumbers.Any() && !t3DFileNumbers.Any())
            {
                dataFileNumbers = timFileNumbers;
                fileExtension = ExtForceQuantNames.TimFileExtension;
                conditionDataType = BoundaryConditionDataType.TimeSeries;
                return true;
            }

            if (t3DFileNumbers.Any() && !timFileNumbers.Any())
            {
                dataFileNumbers = t3DFileNumbers;
                fileExtension = ExtForceQuantNames.T3DFileExtension;
                conditionDataType = BoundaryConditionDataType.TimeSeries;
                return true;
            }

            if (timFileNumbers.Any() && t3DFileNumbers.Any())
            {
                dataFileNumbers = timFileNumbers.Concat(t3DFileNumbers).ToList();
                fileExtension = string.Join("|", ExtForceQuantNames.TimFileExtension, ExtForceQuantNames.T3DFileExtension);
                conditionDataType = BoundaryConditionDataType.TimeSeries;
                return true;
            }

            var cmpFileNumbers = GetDataFileNumbers(filePath, feature2D.Geometry.NumPoints,
                                                 ExtForceQuantNames.CmpFileExtension);
            if (cmpFileNumbers.Any())
            {
                dataFileNumbers = cmpFileNumbers;
                fileExtension = ExtForceQuantNames.CmpFileExtension;
                conditionDataType = new CmpFile().GetForcingType(GetNumberedFilePath(filePath, fileExtension, dataFileNumbers[0]));
                return true;
            }

            var qhFileNumbers = GetDataFileNumbers(filePath, 0, ExtForceQuantNames.QhFileExtension);

            if (qhFileNumbers.Any())
            {
                dataFileNumbers = qhFileNumbers;
                fileExtension = ExtForceQuantNames.QhFileExtension;
                conditionDataType = BoundaryConditionDataType.Qh;
                return true;
            }

            dataFileNumbers = new List<int>();
            conditionDataType = BoundaryConditionDataType.TimeSeries;
            fileExtension = ExtForceQuantNames.TimFileExtension;
            return true;
        }

        private static void ReadBoundaryConditionValues(IBoundaryCondition boundaryCondition, string filePath, int pointIndex, DateTime modelReferenceDate)
        {
            var data = boundaryCondition.GetDataAtPoint(pointIndex);
            if (data == null) return;

            IList<HarmonicComponent> harmonicComponents;
            switch (boundaryCondition.DataType)
            {
                case BoundaryConditionDataType.TimeSeries:
                    if (filePath.EndsWith(ExtForceQuantNames.T3DFileExtension))
                    {
                        VerticalProfileDefinition verticalProfileDefinition;
                        var series = new T3DFile().Read(filePath, out verticalProfileDefinition);
                        var index = boundaryCondition.DataPointIndices.IndexOf(pointIndex);
                        boundaryCondition.PointDepthLayerDefinitions[index] = verticalProfileDefinition;
                        data = boundaryCondition.GetDataAtPoint(pointIndex);
                        FunctionHelper.SetValuesRaw<DateTime>(data.Arguments[0], series.Arguments[0].Values);
                        for (int i = 0; i < data.Components.Count; ++i)
                        {
                            FunctionHelper.SetValuesRaw<double>(data.Components[i], series.Components[i].Values);
                        }
                    }
                    else
                    {
                        new TimFile().Read(filePath, data, modelReferenceDate);
                    }
                    break;
                case BoundaryConditionDataType.Qh:
                    var profile = new QhFile().Read(filePath);
                    FunctionHelper.SetValuesRaw<double>(data.Arguments[0], profile.Arguments[0].Values);
                    FunctionHelper.SetValuesRaw<double>(data.Components[0], profile.Components[0].Values);
                    break;
                case BoundaryConditionDataType.AstroComponents:
                    harmonicComponents = new CmpFile().Read(filePath, BoundaryConditionDataType.AstroComponents);
                    FunctionHelper.SetValuesRaw(data.Arguments[0], harmonicComponents.Select(h => h.Name));
                    FunctionHelper.SetValuesRaw(data.Components[0], harmonicComponents.Select(h => h.Amplitude));
                    FunctionHelper.SetValuesRaw(data.Components[1], harmonicComponents.Select(h => h.Phase));
                    break;
                case BoundaryConditionDataType.AstroCorrection:
                    harmonicComponents = new CmpFile().Read(filePath, BoundaryConditionDataType.AstroComponents);
                    FunctionHelper.SetValuesRaw(data.Arguments[0], harmonicComponents.Select(h => h.Name));
                    FunctionHelper.SetValuesRaw(data.Components[0], harmonicComponents.Select(h => h.Amplitude));
                    FunctionHelper.SetValuesRaw(data.Components[2], harmonicComponents.Select(h => h.Phase));
                    break;
                case BoundaryConditionDataType.Harmonics:
                    harmonicComponents = new CmpFile().Read(filePath, BoundaryConditionDataType.Harmonics).OrderBy(c => c.Frequency).ToList();
                    FunctionHelper.SetValuesRaw(data.Arguments[0], harmonicComponents.Select(h => h.Frequency));
                    FunctionHelper.SetValuesRaw(data.Components[0], harmonicComponents.Select(h => h.Amplitude));
                    FunctionHelper.SetValuesRaw(data.Components[1], harmonicComponents.Select(h => h.Phase));
                    break;
                case BoundaryConditionDataType.HarmonicCorrection:
                    harmonicComponents = new CmpFile().Read(filePath, BoundaryConditionDataType.Harmonics).OrderBy(c => c.Frequency).ToList();
                    FunctionHelper.SetValuesRaw(data.Arguments[0], harmonicComponents.Select(h => h.Frequency));
                    FunctionHelper.SetValuesRaw(data.Components[0], harmonicComponents.Select(h => h.Amplitude));
                    FunctionHelper.SetValuesRaw(data.Components[2], harmonicComponents.Select(h => h.Phase));
                    break;
            }
        }

        private static void ReadSourceAndSinkValues(SourceAndSink sourceAndSink, string filePath, DateTime modelReferenceDate)
        {
            var data = sourceAndSink.Data;
            if (data == null)
            {
                log.ErrorFormat(Resources.Read_SourceAndSink_values_failed__no_function_detected_for_SourceAndSink__0_, sourceAndSink.Name);
                return;
            }

            var readFunction = new TimFile().Read(filePath, modelReferenceDate);
            sourceAndSink.CopyValuesFromFileToSourceAndSinkAttributes(readFunction);
        }

        public static ExtForceFileItem WriteInitialConditionsPolygon(string extForceFilePath, string extForceFileQuantityName, SetValueOperation operation, ExtForceFileItem existingExtForceFileItem = null, bool writeToDisk = true, string prefix = null)
        {
            var quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            var extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = string.Format("{0}_{1}.pol", extForceFileQuantityName, operation.Name.Replace(" ", "_").Replace("\t", "_")),
                FileType = ExtForceQuantNames.FileTypes.InsidePolygon,
                Method = GetSpatialOperationMethod(operation),
            };

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            var op = ExtForceQuantNames.OperatorMapping[operation.OperationType];

            extForceFileItem.Value = operation.Value;
            extForceFileItem.Enabled = operation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[op];

            if (writeToDisk)
            {
                var directoryName = Path.GetDirectoryName(extForceFilePath);
                string polFilePath;
                if (directoryName != null)
                {
                    polFilePath = Path.Combine(directoryName, extForceFileItem.FileName);
                }
                else
                {
                    throw new ArgumentException("Could not get directory name from file path" + extForceFilePath);
                }
                if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                new PolFile<Feature2DPolygon>().Write(polFilePath, operation.Mask.Provider.Features.OfType<IFeature>());
            }

            return extForceFileItem;
        }

        public static ExtForceFileItem WriteInitialConditionsSamples(string extForceFilePath,
            string extForceFileQuantityName, ImportSamplesSpatialOperationExtension importSamplesOperation,
            ExtForceFileItem existingExtForceFileItem, bool writeToDisk, string prefix = null)
        {
            var targetDirectory = Path.GetDirectoryName(Path.GetFullPath(extForceFilePath));
            if (writeToDisk && Path.GetDirectoryName(importSamplesOperation.FilePath) != targetDirectory)
            {
                try
                {
                    importSamplesOperation.SwitchToDirectory(targetDirectory);
                }
                catch (Exception e)
                {
                    log.Warn("Unable to import samples "+ e.Message);
                }
                    
            }

            var quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            var extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = targetDirectory != null ? importSamplesOperation.FilePath.Replace(targetDirectory + "\\", "") : importSamplesOperation.FilePath,
                FileType = GetSpatialOperationFileType(importSamplesOperation),
                Method = GetSpatialOperationMethod(importSamplesOperation)
            };
            if (importSamplesOperation.InterpolationMethod == SpatialInterpolationMethod.Averaging)
            {
                extForceFileItem.ModelData[ExtForceFile.AveragingTypeKey] = (int) importSamplesOperation.AveragingMethod;
                extForceFileItem.ModelData[ExtForceFile.RelSearchCellSizeKey] = importSamplesOperation.RelativeSearchCellSize;
                extForceFileItem.ModelData[ExtForceFile.MinSamplePointsKey] = importSamplesOperation.MinSamplePoints;
            }

            extForceFileItem.Enabled = importSamplesOperation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[ExtForceQuantNames.OperatorMapping[importSamplesOperation.Operand]];

            return extForceFileItem;
        }

        public static ExtForceFileItem WriteInitialConditionsUnsupported(string filePath, string quantity,
            AddSamplesOperation operation, bool writeToDisk, string prefix = null)
        {
            var quantityName = prefix != null ? prefix + quantity : quantity;
            var forceFileItem = new ExtForceFileItem(quantityName)
            {
                FileName = ExtForceFile.MakeXyzFileName(quantity),
                FileType = ExtForceQuantNames.FileTypes.Triangulation,
                Method = GetSpatialOperationMethod(operation),
                Enabled = operation.Enabled,
                Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite],
            };
            forceFileItem.ModelData[ExtForceFile.AveragingTypeKey] = (int) GridCellAveragingMethod.ClosestPoint;
            forceFileItem.ModelData[ExtForceFile.RelSearchCellSizeKey] = 1.0;
            forceFileItem.ModelData[ExtForceFile.MinSamplePointsKey] = 1;

            if (writeToDisk)
            {
                var directoryName = Path.GetDirectoryName(filePath);
                if (directoryName != null)
                {
                    var xyzFilePath = Path.Combine(directoryName, forceFileItem.FileName);

                    XyzFile.Write(xyzFilePath, operation.GetPoints());
                }
                else
                {
                    throw new ArgumentException("Could not get directory name from file path" + filePath);
                }
            }

            return forceFileItem;
        }

        private static int GetSpatialOperationMethod(ISpatialOperation operation)
        {
            if (operation is SetValueOperation)
            {
                return 4;
            }
            if (operation is ImportSamplesSpatialOperationExtension extension)
            {
                switch (extension.InterpolationMethod)
                {
                    case SpatialInterpolationMethod.Triangulation:
                        return 5;
                    case SpatialInterpolationMethod.Averaging:
                        return 6;
                }
            }
            if (operation is AddSamplesOperation)
            {
                return 6;
            }
            return -1;
        }

        private static int GetSpatialOperationFileType(ISpatialOperation operation)
        {
            if (operation is ImportSamplesOperation importSamplesOperation)
            {
                var fileExtension = Path.GetExtension(importSamplesOperation.FilePath);
                if (fileExtension.Equals(".asc", StringComparison.OrdinalIgnoreCase))
                {
                    return ExtForceQuantNames.FileTypes.ArcInfo;
                }
                if (fileExtension.Equals(".tif", StringComparison.OrdinalIgnoreCase))
                {
                    return ExtForceQuantNames.FileTypes.GeoTiff;
                }
                return ExtForceQuantNames.FileTypes.Triangulation;
            }
            if (operation is SetValueOperation)
            {
                return ExtForceQuantNames.FileTypes.InsidePolygon;
            }
            return -1;
        }

        private static IList<int> GetDataFileNumbers(string fileNameOrPath, int numPointsOnPolyLine, string fileExtension)
        {
            IList<int> dataFileNumbers = new List<int>();
            if (numPointsOnPolyLine == 0 && File.Exists(GetNumberedFilePath(fileNameOrPath, fileExtension, 0)))
            {
                dataFileNumbers.Add(0);
            }
            else
            {
                for (var i = 1; i <= numPointsOnPolyLine; i++)
                {
                    var expectedFileName = GetNumberedFilePath(fileNameOrPath, fileExtension, i);
                    if (File.Exists(expectedFileName))
                    {
                        dataFileNumbers.Add(i);
                    }
                }                
            }
            return dataFileNumbers;
        }

        private static string GetNumberedFilePath(string pliFilePath, string fileExtension, int i)
        {
            var directoryName = Path.GetDirectoryName(pliFilePath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pliFilePath);
            if (fileNameWithoutExtension == null)
            {
                throw new FormatException("Invalid file path " + pliFilePath);
            }
            var filePathWithoutExtension = directoryName != null
                ? Path.Combine(directoryName, fileNameWithoutExtension)
                : fileNameWithoutExtension;
            return i == 0
                ? string.Join(".", filePathWithoutExtension, fileExtension)
                : string.Format("{0}_{1:0000}.{2}", filePathWithoutExtension, i, fileExtension);
        }

        public static ExtForceFileItem CreateWindFieldExtForceFileItem(IWindField windField, string filePath)
        {
            return new ExtForceFileItem(ExtForceQuantNames.WindQuantityNames[windField.Quantity])
                   {
                       FileName = filePath,
                       FileType = GetFileType(windField),
                       Method = GetMethod(windField),
                       Operand = "+"
                   };
        }

        private static int GetFileType(IWindField windField)
        {
            var uniformWindField = windField as UniformWindField;
            if (uniformWindField != null)
            {
                return uniformWindField.Components.Contains(WindComponent.Magnitude)
                    ? ExtForceQuantNames.FileTypes.UniMagDir
                    : ExtForceQuantNames.FileTypes.Uniform;
            }
            if (windField is GriddedWindField)
            {
                return windField.Quantity == WindQuantity.VelocityVectorAirPressure
                    ? ExtForceQuantNames.FileTypes.Curvi
                    : ExtForceQuantNames.FileTypes.ArcInfo;
            }
            if (windField is SpiderWebWindField)
            {
                return ExtForceQuantNames.FileTypes.SpiderWeb;
            }
            return -1;
        }

        private static int GetMethod(IWindField windField)
        {
            if (windField is UniformWindField)
            {
                return 1;
            }
            if (windField is GriddedWindField)
            {
                return windField.Quantity == WindQuantity.VelocityVectorAirPressure ? 3 : 2;
            }
            if (windField is SpiderWebWindField)
            {
                return 1;
            }
            return -1;
        }

        public static IWindField CreateWindField(ExtForceFileItem extForceFileItem, string extForceFilePath)
        {
            if (!ExtForceQuantNames.WindQuantityNames.Values.Contains(extForceFileItem.Quantity))
            {
                throw new NotSupportedException(string.Format("Wind quantity {0} is not supported",
                    extForceFileItem.Quantity));
            }

            var quantity = ExtForceQuantNames.WindQuantityNames.First(kvp => kvp.Value == extForceFileItem.Quantity).Key;

            var fileName = extForceFileItem.FileName == null
                ? null
                : Path.Combine(Path.GetDirectoryName(extForceFilePath), extForceFileItem.FileName);

            switch (extForceFileItem.FileType)
            {
                case ExtForceQuantNames.FileTypes.Uniform:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return UniformWindField.CreateWindXSeries();
                    }
                    if (quantity == WindQuantity.VelocityY)
                    {
                        return UniformWindField.CreateWindYSeries();
                    }
                    if (quantity == WindQuantity.VelocityVector)
                    {
                        return UniformWindField.CreateWindXYSeries();
                    }
                    if (quantity == WindQuantity.AirPressure)
                    {
                        return UniformWindField.CreatePressureSeries();
                    }
                    break;
                case ExtForceQuantNames.FileTypes.UniMagDir:
                    if (quantity == WindQuantity.VelocityVector)
                    {
                        return UniformWindField.CreateWindPolarSeries();
                    }
                    break;
                case ExtForceQuantNames.FileTypes.ArcInfo:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return GriddedWindField.CreateXField(fileName);
                    }
                    if (quantity == WindQuantity.VelocityY)
                    {
                        return GriddedWindField.CreateYField(fileName);
                    }
                    if (quantity == WindQuantity.AirPressure)
                    {
                        return GriddedWindField.CreatePressureField(fileName);
                    }
                    break;
                case ExtForceQuantNames.FileTypes.SpiderWeb:
                    if (quantity == WindQuantity.VelocityVectorAirPressure)
                    {
                        return SpiderWebWindField.Create(fileName);
                    }
                    break;
                case ExtForceQuantNames.FileTypes.Curvi:
                    if (quantity == WindQuantity.VelocityVectorAirPressure)
                    {
                        return GriddedWindField.CreateCurviField(fileName,
                            GriddedWindField.GetCorrespondingGridFilePath(fileName));
                    }
                    break;
                case ExtForceQuantNames.FileTypes.NCgrid:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return GriddedWindField.CreateXField(fileName);
                    }
                    if (quantity == WindQuantity.VelocityY)
                    {
                        return GriddedWindField.CreateYField(fileName);
                    }
                    if (quantity == WindQuantity.AirPressure)
                    {
                        return GriddedWindField.CreatePressureField(fileName);
                    }
                    break;
            }
            throw new NotSupportedException(
                string.Format("External forcing for wind quantity {0}, method {1} and file type {2} is not supported",
                    extForceFileItem.Quantity, extForceFileItem.Method, extForceFileItem.FileType));
        }
    }
}