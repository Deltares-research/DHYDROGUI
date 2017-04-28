using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class BcBlockData
    {
        public BcBlockData()
        {
            Quantities = new List<BcQuantityData>();
        }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string SupportPoint { get; set; }
        public string FunctionType { get; set; }
        public string SeriesIndex { get; set; }
        public string TimeInterpolationType { get; set; }
        public string VerticalPositionType { get; set; }
        public string VerticalPositionDefinition { get; set; }
        public string VerticalInterpolationType { get; set; }
        public string Offset { get; set; }
        public string Factor { get; set; }
        public IList<BcQuantityData> Quantities { get; set; }
    }

    public class BcQuantityData
    {
        public BcQuantityData()
        {
            Values = new List<string>();
        }

        public string Quantity { get; set; }
        public string Unit { get; set; }
        public string VerticalPosition { get; set; }
        public string TracerName { get; set; }

        public IList<string> Values;
    }

    public class BcFileFlowBoundaryDataBuilder
    {
        private class ForcingTypeDefinition
        {
            public BoundaryConditionDataType ForcingType;
            public string[] ArgumentDefinitions;
            public string[] ComponentDefinitions;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof (BcFileFlowBoundaryDataBuilder));

        private static readonly IDictionary<string, ForcingTypeDefinition> ForcingTypeDefinitions =
            new Dictionary<string, ForcingTypeDefinition>
                {
                    {
                        "timeseries",
                        new ForcingTypeDefinition
                            {
                                ForcingType = BoundaryConditionDataType.TimeSeries,
                                ArgumentDefinitions = new[] {"time"},
                                ComponentDefinitions = new[] {""}
                            }
                    },
                    {
                        "t3d",
                        new ForcingTypeDefinition
                            {
                                ForcingType = BoundaryConditionDataType.TimeSeries,
                                ArgumentDefinitions = new[] {"time"},
                                ComponentDefinitions = new[] {""}
                            }
                    },
                    {
                        "astronomic",
                        new ForcingTypeDefinition
                            {
                                ForcingType = BoundaryConditionDataType.AstroComponents,
                                ArgumentDefinitions = new[] {"astronomic component"},
                                ComponentDefinitions = new[] {"amplitude", "phase"}
                            }
                    },
                    {
                        "astronomic-correction",
                        new ForcingTypeDefinition
                            {
                                ForcingType = BoundaryConditionDataType.AstroCorrection,
                                ArgumentDefinitions = new[] {"astronomic component"},
                                ComponentDefinitions =
                                    new[] {"amplitude", "phase"}
                            }
                    },
                    {
                        "harmonic",
                        new ForcingTypeDefinition
                            {
                                ForcingType = BoundaryConditionDataType.Harmonics,
                                ArgumentDefinitions = new[] {"harmonic component"},
                                ComponentDefinitions = new[] {"amplitude", "phase"}
                            }
                    },
                    {
                        "harmonic-correction",
                        new ForcingTypeDefinition
                            {
                                ForcingType = BoundaryConditionDataType.HarmonicCorrection,
                                ArgumentDefinitions = new[] {"harmonic component"},
                                ComponentDefinitions =
                                    new[] {"amplitude", "phase"}
                            }
                    },
                    {
                        "qhtable",
                        new ForcingTypeDefinition
                            {
                                ForcingType = BoundaryConditionDataType.Qh,
                                ArgumentDefinitions = new[] {"qhbnd discharge"},
                                ComponentDefinitions = new[] {"qhbnd waterlevel"}
                            }
                    }
                };

        private static readonly IDictionary<BoundaryConditionDataType, BoundaryConditionDataType> CorrectionTypes =
            new Dictionary<BoundaryConditionDataType, BoundaryConditionDataType>
            {
                {BoundaryConditionDataType.HarmonicCorrection, BoundaryConditionDataType.Harmonics},
                {BoundaryConditionDataType.AstroCorrection, BoundaryConditionDataType.AstroComponents}
            };

        private static readonly IDictionary<string[], FlowBoundaryQuantityType> FlowQuantityKeys = new Dictionary
            <string[], FlowBoundaryQuantityType>
        {
            {new[] {ExtForceQuantNames.WaterLevelAtBound}, FlowBoundaryQuantityType.WaterLevel},
            {new[] {ExtForceQuantNames.DischargeAtBound}, FlowBoundaryQuantityType.Discharge},
            {new[] {ExtForceQuantNames.QhAtBound}, FlowBoundaryQuantityType.Discharge},
            {new[] {ExtForceQuantNames.VelocityAtBound}, FlowBoundaryQuantityType.Velocity},
            {new[] {ExtForceQuantNames.NeumannConditionAtBound}, FlowBoundaryQuantityType.Neumann},
            {new[] {ExtForceQuantNames.RiemannConditionAtBound}, FlowBoundaryQuantityType.Riemann},
            {new[] {ExtForceQuantNames.RiemannVelocityAtBound}, FlowBoundaryQuantityType.RiemannVelocity},
            {new[] {ExtForceQuantNames.NormalVelocityAtBound}, FlowBoundaryQuantityType.NormalVelocity},
            {new[] {ExtForceQuantNames.TangentialVelocityAtBound}, FlowBoundaryQuantityType.TangentVelocity},
            {new[] {"x-velocity", "y-velocity"}, FlowBoundaryQuantityType.VelocityVector},
            {new[] {ExtForceQuantNames.SalinityAtBound}, FlowBoundaryQuantityType.Salinity},
            {new[] {ExtForceQuantNames.TemperatureAtBound}, FlowBoundaryQuantityType.Temperature},
            {new[] {ExtForceQuantNames.TracerAtBound}, FlowBoundaryQuantityType.Tracer}
        };
         
        private static readonly IDictionary<string, VerticalProfileType> VerticalDefinitionKeys =
            new Dictionary<string, VerticalProfileType>
                {
                    {"single", VerticalProfileType.Uniform},
                    {"uniform", VerticalProfileType.Uniform},
                    {"none", VerticalProfileType.Uniform},
                    {"bed-surface", VerticalProfileType.TopBottom},
                    {"surface-bed", VerticalProfileType.TopBottom},
                    {"top-bottom", VerticalProfileType.TopBottom},
                    {"bottom-top", VerticalProfileType.TopBottom},
                    {"z from bed", VerticalProfileType.ZFromBed},
                    {"z above bed", VerticalProfileType.ZFromBed},
                    {"z from bottom", VerticalProfileType.ZFromBed},
                    {"z above bottom", VerticalProfileType.ZFromBed},
                    {"z from surface", VerticalProfileType.ZFromSurface},
                    {"z above surface", VerticalProfileType.ZFromSurface},
                    {"z from top", VerticalProfileType.ZFromSurface},
                    {"z above top", VerticalProfileType.ZFromSurface},
                    {"z from datum",VerticalProfileType.ZFromDatum},
                    {"z above datum",VerticalProfileType.ZFromDatum},
                    {"percentage from bed",VerticalProfileType.PercentageFromBed},
                    {"percentage above bed",VerticalProfileType.PercentageFromBed},
                    {"percentage from bottom",VerticalProfileType.PercentageFromBed},
                    {"percentage above bottom",VerticalProfileType.PercentageFromBed},
                    {"percentage from surface",VerticalProfileType.PercentageFromSurface},
                    {"percentage from top",VerticalProfileType.PercentageFromSurface}
                };

        public static IEnumerable<string> CorrectionFunctionTypes
        {
            get
            {
                return
                    ForcingTypeDefinitions.Where(
                        kvp =>
                            kvp.Value.ForcingType == BoundaryConditionDataType.AstroCorrection ||
                            kvp.Value.ForcingType == BoundaryConditionDataType.HarmonicCorrection)
                        .Select(kvp => kvp.Key);
            }
        }

        public IList<BoundaryConditionDataType> ExcludedDataTypes { private get; set; }

        public IList<FlowBoundaryQuantityType> ExcludedQuantities { private get; set; }

        public bool OverwriteExistingData { private get; set; }

        public bool CanCreateNewBoundaryCondition { private get; set; }

        public IFeature LocationFilter { private get; set; }

        public BcFileFlowBoundaryDataBuilder()
        {
            ExcludedDataTypes = new List<BoundaryConditionDataType>();
            ExcludedQuantities = new List<FlowBoundaryQuantityType>();
            OverwriteExistingData = true;
            CanCreateNewBoundaryCondition = true;
        }

        public void InsertBoundaryData(IEnumerable<BoundaryConditionSet> boundaryConditionSets, IEnumerable<BcBlockData> data, string thatcherHarlemanTimeLag = null)
        {
            var bcSets = boundaryConditionSets.ToList();
            foreach (var bcBlockData in data)
            {
                InsertBoundaryData(bcSets, bcBlockData, thatcherHarlemanTimeLag);
            }
        }

        public bool InsertBoundaryData(IEnumerable<BoundaryConditionSet> boundaryConditionSets, BcBlockData data, string thatcherHarlemanTimeLag = null)
        {
            if (data == null)
            {
                return false;
            }

            var selectedSet =
                boundaryConditionSets.FirstOrDefault(
                    bcs => bcs.SupportPointNames.Contains(data.SupportPoint) || bcs.Feature.Name == data.SupportPoint);
            if (selectedSet == null)
            {
                Log.WarnFormat(
                    "File {0}, block starting at line {1}: support point {2} was not found in boundaries; omitting data block.",
                    data.FilePath, data.LineNumber, data.SupportPoint);
                return false;
            }
            if (LocationFilter != null && !Equals(selectedSet.Feature, LocationFilter))
            {
                return false;
            }
            var isGlobal = data.SupportPoint == selectedSet.Feature.Name;
            bool skippedAny = false;
            using (CultureUtils.SwitchToInvariantCulture())
            {
                ForcingTypeDefinition forcingTypeDefinition;
                if (!TryParseForcingType(data, out forcingTypeDefinition))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: function type {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.FunctionType);
                    return false;
                }
                if (ExcludedDataTypes != null && ExcludedDataTypes.Contains(forcingTypeDefinition.ForcingType))
                {
                    Log.InfoFormat(
                        "File {0}, block starting at line {1}: skipping boundary data of function type {2}.",
                        data.FilePath, data.LineNumber, forcingTypeDefinition.ForcingType);
                    return true;
                }

                VerticalProfileDefinition verticalProfileDefinition;
                if (!TryParseDepthLayerDefinition(data, out verticalProfileDefinition))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: vertical profile definition {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.VerticalPositionDefinition);
                    return false;
                }

                VerticalInterpolationType verticalInterpolationType;
                if (!TryParseVerticalInterpolationType(data,out verticalInterpolationType))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: vertical interpolation type {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.VerticalInterpolationType);
                    return false;
                }

                InterpolationType timeInterpolationType;
                if (!TryParseTimeInterpolationType(data, out timeInterpolationType))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: time interpolation type {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.TimeInterpolationType);
                    return false;
                }

                int seriesIndex;
                if (!TryParseSeriesIndex(data, out seriesIndex))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: series index {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.SeriesIndex);
                    return false;
                }
                seriesIndex--; //to C-style indexing.

                double offset;
                double factor;
                if (!TryParseOffsetFactor(data, out offset, out factor))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at {1}: offset {2} or factor {3} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.Offset, data.Factor);
                    return false;
                }

                var componentKeys = forcingTypeDefinition.ComponentDefinitions;
                var argVariables = new Dictionary<int, BcQuantityData>();
                var compVariables = new Dictionary<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData>();

                foreach (var quantity in data.Quantities)
                {
                    var quantityString = quantity.Quantity;

                    if (quantityString.StartsWith("tracerbnd_"))
                    {
                        quantity.TracerName = quantityString.Substring(10);
                        quantityString = "tracerbnd";
                    }
                    else if (quantityString.StartsWith("tracerbnd"))
                    {
                        quantity.TracerName = quantityString.Substring(9);
                        quantityString = "tracerbnd";
                    }

                    string flowQuantity = null;
                    var componentIndex = -1;
                    var componentCount = 0;

                    if (forcingTypeDefinition.ArgumentDefinitions.Contains(quantityString))
                    {
                        argVariables.Add(
                            forcingTypeDefinition.ArgumentDefinitions.ToList().IndexOf(quantityString),
                            quantity);
                        continue;
                    }

                    var i = 0;
                    foreach (var componentKey in componentKeys)
                    {
                        if (String.IsNullOrEmpty(componentKey))
                        {
                            flowQuantity = quantityString.ToLower();
                            componentIndex = i;
                            componentCount = componentKeys.Count();
                            break;
                        }
                        if (Equals(quantityString, componentKey) &&
                            forcingTypeDefinition.ForcingType == BoundaryConditionDataType.Qh)
                        {
                            flowQuantity =
                                FlowQuantityKeys.First(kvp => kvp.Value == FlowBoundaryQuantityType.WaterLevel).Key[0];
                            componentIndex = i;
                            componentCount = 1;
                            break;
                        }
                        if (quantityString.EndsWith(componentKey))
                        {
                            flowQuantity =
                                quantityString.ToLower().Substring(0, quantityString.Length - componentKey.Length).TrimEnd();
                            componentIndex = i;
                            componentCount = componentKeys.Count();
                            break;
                        }
                        ++i;
                    }

                    if (flowQuantity == null)
                    {
                        Log.WarnFormat(
                            "File {0}, block starting at line {1}: quantity {2} could not be parsed; omitting data column.",
                            data.FilePath, data.LineNumber, quantity.Quantity);
                        continue;
                    }

                    if (FlowQuantityKeys.Keys.SelectMany(a => a).Contains(flowQuantity))
                    {
                        var flowQuantityComponentsPair = FlowQuantityKeys.First(kvp => kvp.Key.Contains(flowQuantity));
                        
                        var quantityCount = flowQuantityComponentsPair.Key.Count();
                        int layerIndex;
                        if (TryParseVerticalPosition(quantity, out layerIndex))
                        {
                            var quantityIndex = flowQuantityComponentsPair.Key.ToList().IndexOf(flowQuantity);

                            var index = quantityCount*componentCount*layerIndex + componentCount*quantityIndex +
                                        componentIndex;

                            compVariables.Add(
                                new System.Tuple<FlowBoundaryQuantityType, int>(flowQuantityComponentsPair.Value, index),
                                quantity);
                        }
                        else
                        {
                            Log.WarnFormat(
                                "File {0}, block starting at line {1}: vertical position {2} could not be parsed; omitting data column.",
                                data.FilePath, data.LineNumber, quantity.VerticalPosition);
                        }
                    }
                }

                var quantityGroups = compVariables.GroupBy(kvp => kvp.Key.Item1);

                foreach (var quantityGroup in quantityGroups)
                {
                    FlowBoundaryQuantityType flowQuantityEnum = quantityGroup.Key;

                    if (ExcludedQuantities.Contains(flowQuantityEnum))
                    {
                        skippedAny = true;
                        continue;
                    }

                    FlowBoundaryCondition boundaryCondition = null;

                    foreach (var quantity in quantityGroup)
                    {
                        var existingConditions = selectedSet.BoundaryConditions.OfType<FlowBoundaryCondition>().
                            Where(bc => MatchBoundaryCondition(bc, flowQuantityEnum, quantity.Value, forcingTypeDefinition))
                            .ToList();

                        boundaryCondition = existingConditions.ElementAtOrDefault(seriesIndex);

                        if (boundaryCondition == null)
                        {
                            var isCorrection = forcingTypeDefinition.ForcingType ==
                                               BoundaryConditionDataType.AstroCorrection ||
                                               forcingTypeDefinition.ForcingType ==
                                               BoundaryConditionDataType.HarmonicCorrection;
                            
                            if (CanCreateNewBoundaryCondition && !isCorrection)
                            {
                                TimeSpan timelag = TimeSpan.Zero;
                                if (thatcherHarlemanTimeLag != null)
                                {
                                    double timelagdouble;
                                    if (double.TryParse(thatcherHarlemanTimeLag, out timelagdouble))
                                    {
                                        timelag = TimeSpan.FromSeconds(timelagdouble);
                                    }
                                }

                                boundaryCondition =
                                    new FlowBoundaryCondition(flowQuantityEnum,
                                        forcingTypeDefinition.ForcingType)
                                    {
                                        Feature = selectedSet.Feature,
                                        ThatcherHarlemanTimeLag = timelag,
                                    };

                                if (flowQuantityEnum == FlowBoundaryQuantityType.Tracer)
                                {
                                    boundaryCondition.TracerName = quantity.Value.TracerName;
                                }
                            }
                            else
                            {
                                Log.WarnFormat(
                                    "File {0}, block starting at line {1}: quantity {2} and forcing type {3} do not match given boundary condition.",
                                    data.FilePath, data.LineNumber, quantity.Value,
                                    forcingTypeDefinition.ForcingType);
                            }
                        }
                    }

                    if (boundaryCondition == null) continue;

                    if (forcingTypeDefinition.ForcingType == BoundaryConditionDataType.AstroCorrection &&
                        boundaryCondition.DataType == BoundaryConditionDataType.AstroComponents)
                    {
                        boundaryCondition.DataType = BoundaryConditionDataType.AstroCorrection;
                    }

                    if (forcingTypeDefinition.ForcingType == BoundaryConditionDataType.HarmonicCorrection &&
                        boundaryCondition.DataType == BoundaryConditionDataType.Harmonics)
                    {
                        boundaryCondition.DataType = BoundaryConditionDataType.HarmonicCorrection;
                    }

                    boundaryCondition.Offset = offset;
                    boundaryCondition.Factor = factor;

                    var dataIndex = selectedSet.SupportPointNames.ToList().IndexOf(data.SupportPoint);
                    
                    if (boundaryCondition.IsHorizontallyUniform)
                    {
                        if (isGlobal)
                        {
                            dataIndex = 0;
                        }
                        else if (dataIndex != 0)
                        {
                            Log.InfoFormat(
                                "File {0}, block starting at line {1}: {2} uniform boundary condition cannot be specified at point {3}; omitting data columns.",
                                data.FilePath, data.LineNumber, flowQuantityEnum, data.SupportPoint);
                            continue;
                        }
                    }

                    var dataPoints = dataIndex == -1
                        ? Enumerable.Range(0, boundaryCondition.Feature.Geometry.Coordinates.Count()).ToList()
                        : new List<int> {dataIndex};

                    foreach (var dataPoint in dataPoints)
                    {
                        var addedData = false;
                        if (!boundaryCondition.DataPointIndices.Contains(dataPoint))
                        {
                            boundaryCondition.AddPoint(dataPoint);
                            addedData = true;
                        }
                        else
                        {
                            if (!OverwriteExistingData)
                            {
                                Log.InfoFormat(
                                    "File {0}, block starting at line {1}: {2} boundary condition already contains data at point {3}; omitting data columns.",
                                    data.FilePath, data.LineNumber, flowQuantityEnum, data.SupportPoint);
                                continue;
                            }
                        }
                        var verticalProfileIndex = boundaryCondition.DataPointIndices.IndexOf(dataPoint);
                        if (!boundaryCondition.IsVerticallyUniform)
                        {
                            boundaryCondition.PointDepthLayerDefinitions[verticalProfileIndex] =
                                verticalProfileDefinition;
                        }

                        // TODO: move this code to vertical profile (see TOOLS-21777)
                        boundaryCondition.VerticalInterpolationType = verticalInterpolationType;

                        var existingData = boundaryCondition.GetDataAtPoint(dataPoint);
                        try
                        {
                            existingData.BeginEdit(new DefaultEditAction("Importing data..."));
                            if (forcingTypeDefinition.ForcingType == BoundaryConditionDataType.AstroCorrection ||
                                forcingTypeDefinition.ForcingType == BoundaryConditionDataType.HarmonicCorrection)
                            {
                                var existingArgument = existingData.Arguments[0].GetValues();

                                var type = forcingTypeDefinition.ForcingType ==
                                           BoundaryConditionDataType.AstroCorrection
                                    ? typeof (string)
                                    : typeof (double);

                                var parsedArgumentValues = ParseValues(argVariables[0].Values, type,
                                    argVariables[0].Unit).ToList();

                                var indexMapping = parsedArgumentValues.Select(existingArgument.IndexOf).ToList();

                                foreach (var comp in quantityGroup)
                                {
                                    var k = comp.Key.Item2;
                                    var l = (k%2 == 0) ? 2*k + 2 : 2*k + 1;
                                    var variable = existingData.Components[l];
                                    var variableValues = variable.GetValues<double>().ToList();
                                    var values =
                                        ParseValues(comp.Value.Values, variable.ValueType, comp.Value.Unit).ToList();

                                    for (int i = 0; i < parsedArgumentValues.Count; ++i)
                                    {
                                        var index = indexMapping[i];

                                        if (index == -1) continue;

                                        if (i == values.Count) break;

                                        variableValues[index] = (double) values[i];
                                    }
                                    variable.Values.Clear();
                                    FunctionHelper.SetValuesRaw<double>(variable, variableValues);
                                }
                            }
                            else
                            {
                                foreach (var arg in argVariables)
                                {
                                    var variable = existingData.Arguments[arg.Key];
                                    variable.Values.Clear();
                                    variable.SetValues(ParseValues(arg.Value.Values, variable.ValueType, arg.Value.Unit));
                                    if (variable is IVariable<DateTime>)
                                    {
                                        variable.InterpolationType = timeInterpolationType;
                                    }
                                }

                                foreach (var comp in quantityGroup)
                                {
                                    var variable = existingData.Components[comp.Key.Item2];
                                    variable.SetValues(ParseValues(comp.Value.Values, variable.ValueType,
                                        comp.Value.Unit));
                                }
                            }
                            existingData.EndEdit();
                        }
                        catch (Exception)
                        {
                            if (addedData)
                            {
                                boundaryCondition.DataPointIndices.Remove(dataPoint);
                            }
                            throw;
                        }
                    }
                    if (!selectedSet.BoundaryConditions.Contains(boundaryCondition))
                    {
                        selectedSet.BoundaryConditions.Add(boundaryCondition);
                    }
                }
            }
            // If the data block is not completely used, the logic should go through this data block again.
            // So if any blocks are skipped, return false so it is not removed from the list of blocks to go through.
            return !skippedAny; 
        }

        private static bool MatchBoundaryCondition(FlowBoundaryCondition bc, FlowBoundaryQuantityType quantity,
            BcQuantityData quantityData, ForcingTypeDefinition forcingTypeDefinition)
        {
            if (bc.FlowQuantity != quantity) return false;

            if (bc.FlowQuantity == FlowBoundaryQuantityType.Tracer && quantityData.TracerName != bc.TracerName)
                return false;

            if (forcingTypeDefinition.ForcingType == BoundaryConditionDataType.AstroCorrection)
            {
                return bc.DataType == BoundaryConditionDataType.AstroComponents ||
                       bc.DataType == BoundaryConditionDataType.AstroCorrection;
            }

            if (forcingTypeDefinition.ForcingType == BoundaryConditionDataType.HarmonicCorrection)
            {
                return bc.DataType == BoundaryConditionDataType.Harmonics ||
                       bc.DataType == BoundaryConditionDataType.HarmonicCorrection;
            }

            return bc.DataType == forcingTypeDefinition.ForcingType;
        }

        public static IEnumerable<BcBlockData> CreateBlockData(FlowBoundaryCondition boundaryCondition,
            IEnumerable<string> supportPointNames, DateTime? refDate, int seriesIndex = 0, bool correctionFile = false)
        {
            return
                boundaryCondition.DataPointIndices.Select(
                    dataPointIndex =>
                        CreateBlockData(boundaryCondition, dataPointIndex, supportPointNames.ElementAt(dataPointIndex),
                            refDate, seriesIndex, correctionFile));
        }

        private static bool TryParseForcingType(BcBlockData blockData, out ForcingTypeDefinition forcingType)
        {
            return ForcingTypeDefinitions.TryGetValue(blockData.FunctionType, out forcingType);
        }

        private static bool TryParseSeriesIndex(BcBlockData blockData, out int index)
        {
            if (blockData.SeriesIndex == null)
            {
                index = 1;
                return true;
            }
            return Int32.TryParse(blockData.SeriesIndex, out index);
        }

        private static bool TryParseOffsetFactor(BcBlockData data, out double offset, out double factor)
        {
            bool offsetParsed;
            if (data.Offset == null)
            {
                offset = 0;
                offsetParsed = true;
            }
            else
            {
                offsetParsed = double.TryParse(data.Offset, out offset);
            }
            bool factorParsed;
            if (data.Factor == null)
            {
                factor = 1;
                factorParsed = true;
            }
            else
            {
                factorParsed = double.TryParse(data.Factor, out factor);
            }
            return offsetParsed && factorParsed;
        }

        private static bool TryParseDepthLayerDefinition(BcBlockData data, out VerticalProfileDefinition depthLayerDefinition)
        {
            if (data.VerticalPositionType == null)
            {
                depthLayerDefinition = new VerticalProfileDefinition();
                return true;
            }
            var verticalPositionType = data.VerticalPositionType.ToLower();
            if (VerticalDefinitionKeys.ContainsKey(verticalPositionType))
            {
                var type = VerticalDefinitionKeys[verticalPositionType];
                var depths = Enumerable.Empty<double>();
                if (type != VerticalProfileType.Uniform && type != VerticalProfileType.TopBottom)
                {
                    depths = data.VerticalPositionDefinition.Split().Select(Double.Parse).ToList();
                    var sortedDepths = VerticalProfileDefinition.SortDepths(depths, type);
                    if (!depths.SequenceEqual(sortedDepths))
                    {
                        Log.WarnFormat(
                            "File {0}, block starting at line {1}: vertical profile depths not correctly ordered; omitting data block.",
                            data.FilePath, data.LineNumber);
                    }
                }
                
                depthLayerDefinition = VerticalProfileDefinition.Create(type, depths);
            }
            else
            {
                depthLayerDefinition = null;
            }
            return depthLayerDefinition != null;
        }

        private static string VerticalProfileTypeString(VerticalProfileType type)
        {
            if (VerticalDefinitionKeys.Values.Contains(type))
            {
                return VerticalDefinitionKeys.First(kvp => kvp.Value == type).Key;
            }
            throw new NotImplementedException("Vertical profile definition " + type +
                                              " not supported by bc file writer");
        }

        private static string VerticalProfileDefinitionString(VerticalProfileDefinition verticalProfile)
        {
            if (verticalProfile.Type == VerticalProfileType.Uniform ||
                verticalProfile.Type == VerticalProfileType.TopBottom)
            {
                return null;
            }
            using (CultureUtils.SwitchToInvariantCulture())
            {
                return String.Join(" ", verticalProfile.SortedPointDepths.Select(d => d.ToString()));
            }

        }

        private static bool TryParseVerticalPosition(BcQuantityData quantityData, out int layerIndex)
        {
            if (quantityData.VerticalPosition == null)
            {
                layerIndex = 0;
                return true;
            }
            var result=Int32.TryParse(quantityData.VerticalPosition, out layerIndex);
            if (result)
            {
                layerIndex -= 1; //to C-style indexing
            }
            return result;
        }


        private static bool TryParseVerticalInterpolationType(BcBlockData data, out VerticalInterpolationType verticalInterpolationType)
        {
            var interpolationType = data.VerticalInterpolationType;
            if (String.IsNullOrEmpty(interpolationType) || interpolationType.ToLower() == "linear")
            {
                verticalInterpolationType = VerticalInterpolationType.Linear;
                return true;
            }
            if (interpolationType.ToLower() == "block")
            {
                verticalInterpolationType = VerticalInterpolationType.Step;
                return true;
            }
            if (interpolationType.ToLower() == "log")
            {
                verticalInterpolationType = VerticalInterpolationType.Logarithmic;
                return true;
            }
            verticalInterpolationType = VerticalInterpolationType.Uniform;
            return false;
        }


        private static string VerticalInterpolationString(VerticalInterpolationType verticalInterpolationType)
        {
            switch (verticalInterpolationType)
            {
                case VerticalInterpolationType.Linear:
                    return "linear";
                case VerticalInterpolationType.Logarithmic:
                    return "log";
                case VerticalInterpolationType.Step:
                    return "block";
                case VerticalInterpolationType.Uniform:
                    return null;
                default:
                    throw new NotImplementedException(
                        String.Format("Vertical interpolation type {0} not supported by bc file writer.",
                                      verticalInterpolationType));
            }
        }


        private static bool TryParseTimeInterpolationType(BcBlockData data, out InterpolationType interpolationType)
        {
            if (String.IsNullOrEmpty(data.TimeInterpolationType) || data.TimeInterpolationType.ToLower() == "linear")
            {
                interpolationType = InterpolationType.Linear;
                return true;
            }
            if (data.TimeInterpolationType.ToLower().Contains("block"))
            {
                //TODO: implement this correctly
                interpolationType = InterpolationType.Constant;
                return true;
            }
            interpolationType = InterpolationType.None;
            return false;
        }

        private static string TimeInterpolationString(InterpolationType interpolationType)
        {
            switch (interpolationType)
            {
                case InterpolationType.Linear:
                    return "linear";
                case InterpolationType.Constant:
                    return "block";
                default:
                    return null;
            }
        }

        private static IEnumerable<object> ParseValues(IEnumerable<string> stringValues, Type type, string format)
        {
            if (type == typeof (DateTime))
            {
                if (String.IsNullOrEmpty(format) || format == "-")
                {
                    return
                        stringValues.Select(s => DateTime.ParseExact(s, "yyyyMMddHHmmss", CultureInfo.InvariantCulture))
                            .Cast<object>();
                }
                var splittedFormat = format.Split().ToList();
                if (splittedFormat[1] == "since")
                {
                    var dateString = string.Join(" ", splittedFormat.Skip(2));
                    DateTime startDate;

                    var succes = DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal, out startDate);

                    if (!succes)
                    {
                        succes = DateTime.TryParseExact(dateString, "yyyy-MM-dd hh:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AdjustToUniversal, out startDate);
                    }

                    if (!succes)
                    {
                        throw new FormatException("Time format " + dateString + " is not supported by bc file parser");
                    }
                    if (splittedFormat[0].ToLower() == "seconds")
                    {
                        return
                            stringValues.Select(s => startDate + new TimeSpan(0, 0, 0, Convert.ToInt32(double.Parse(s))))
                                .Cast<object>();
                    }
                    if (splittedFormat[0].ToLower() == "minutes")
                    {
                        return
                            stringValues.Select(s => startDate + new TimeSpan(0, 0, Convert.ToInt32(double.Parse(s)), 0))
                                .Cast<object>();
                    }
                    if (splittedFormat[0].ToLower() == "hours")
                    {
                        return
                            stringValues.Select(s => startDate + new TimeSpan(0, Convert.ToInt32(double.Parse(s)), 0, 0))
                                .Cast<object>();
                    }
                    if (splittedFormat[0].ToLower() == "days")
                    {
                        return
                            stringValues.Select(s => startDate + new TimeSpan(Convert.ToInt32(double.Parse(s)), 0, 0, 0))
                                .Cast<object>();
                    }
                }
            }
            if (type == typeof (string))
            {
                return stringValues;
            }
            if (type == typeof (double))
            {
                if (format != null && format.ToLower().Equals("minutes"))
                {
                    return
                        stringValues.Select(double.Parse)
                            .Select(FlowBoundaryCondition.GetFrequencyInDegPerHour)
                            .Cast<object>();
                }
                return stringValues.Select(double.Parse).Cast<object>();
            }
            throw new ArgumentException(String.Format("Value type {0} with unit {1} not supported by bc file parser.",
                type, format));
        }

        private static BcBlockData CreateBlockData(FlowBoundaryCondition boundaryCondition, int index,
                                                   string supportPoint, DateTime? referenceTime, int seriesIndex = 0, bool correctionFile = false)
        {
            var bcBlockData = new BcBlockData
            {
                SupportPoint = boundaryCondition.IsHorizontallyUniform ? boundaryCondition.FeatureName : supportPoint
            };
            // Inconsistency in kernel: for discharges it expects a single support point name...
            if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Discharge)
            {
                bcBlockData.SupportPoint = supportPoint;
            }

            var forcingTypeDefinition = CreateForcingTypeDefinition(boundaryCondition, index, correctionFile);

            if (forcingTypeDefinition == null)
            {
                Log.WarnFormat(
                    "Boundary condition function type {0} not supported by bc-file writer; skipping condition.",
                    boundaryCondition.DataType);
                return null;
            }

            var forcingType = forcingTypeDefinition.ForcingType;

            var functionType =
                    ForcingTypeDefinitions.First(kvp => kvp.Value == forcingTypeDefinition).Key;

            if (CorrectionTypes.ContainsKey(forcingType) && !correctionFile)
            {
                forcingType = CorrectionTypes[forcingType];
                functionType =
                    ForcingTypeDefinitions.First(kvp => kvp.Value.ForcingType == forcingType).Key;
            }
            if (CorrectionTypes.Values.Contains(forcingType) && correctionFile)
            {
                forcingType = CorrectionTypes.First(kvp => kvp.Value == forcingType).Key;
                functionType =
                    ForcingTypeDefinitions.First(kvp => kvp.Value.ForcingType == forcingType).Key;
            }

            bcBlockData.FunctionType = functionType;

            if (seriesIndex > 0)
            {
                bcBlockData.SeriesIndex = (seriesIndex + 1).ToString(); //to one-based index.
            }

            var data = boundaryCondition.GetDataAtPoint(index);

            var timeArgument = data.Arguments.FirstOrDefault() as IVariable<DateTime>;

            bcBlockData.TimeInterpolationType = timeArgument == null
                                                    ? null
                                                    : TimeInterpolationString(timeArgument.InterpolationType);

            var verticalProfile = boundaryCondition.IsVerticallyUniform
                                      ? null
                                      : boundaryCondition.GetDepthLayerDefinitionAtPoint(index);

            var verticalProfileTypeString = verticalProfile == null
                                                ? null
                                                : VerticalProfileTypeString(verticalProfile.Type);

            if (verticalProfileTypeString != null)
            {
                bcBlockData.VerticalPositionType = verticalProfileTypeString;
                bcBlockData.VerticalPositionDefinition = VerticalProfileDefinitionString(verticalProfile);
                bcBlockData.VerticalInterpolationType =
                    VerticalInterpolationString(boundaryCondition.VerticalInterpolationType);
            }

            if (boundaryCondition.Offset != 0)
            {
                bcBlockData.Offset = string.Format("{0:0.0000000e+00}", boundaryCondition.Offset);
            }
            if (boundaryCondition.Factor != 1)
            {
                bcBlockData.Factor = string.Format("{0:0.0000000e+00}", boundaryCondition.Factor);
            }

            var i = 0;
            foreach (var argument in data.Arguments)
            {
                var quantity = forcingTypeDefinition.ArgumentDefinitions[i++];
                bcBlockData.Quantities.Add(CreateBcQuantityDataForArgument(quantity, argument, referenceTime));
            }

            var skipCorrection = BcFile.IsCorrectionType(((IBoundaryCondition) boundaryCondition).DataType) && !correctionFile;
            var skipSignal = BcFile.IsCorrectionType(((IBoundaryCondition) boundaryCondition).DataType) && correctionFile;
            
            var componentCount = forcingTypeDefinition.ComponentDefinitions.Count();
            if (skipCorrection || skipSignal)
            {
                componentCount += 2;
            }
            if (!FlowQuantityKeys.Values.Contains(boundaryCondition.FlowQuantity))
            {
                Log.WarnFormat("Flow quantity {0} not supported by bc file writer", boundaryCondition.FlowQuantity);
            }
            var flowVariables = FlowQuantityKeys.First(kvp => kvp.Value == boundaryCondition.FlowQuantity).Key;
            var variableCount = flowVariables.Count();
            var componentsPerLayer = componentCount*variableCount;

            var j = 0;

            foreach (var component in data.Components)
            {
                var componentIndex = j%componentCount;

                if ((skipSignal && componentIndex < 2) || (skipCorrection && componentIndex > 1))
                {
                    j++;
                    continue;
                }

                var variableIndex = (j%componentsPerLayer)/componentCount;
                var layerIndex = j/componentsPerLayer + 1; //one-based indexing
                j++;

                var componentString =
                    forcingTypeDefinition.ComponentDefinitions[skipSignal ? (componentIndex - 2) : componentIndex];

                var quantityString = String.IsNullOrEmpty(componentString)
                                         ? flowVariables[variableIndex]
                                         : (flowVariables[variableIndex] + " " + componentString);
                if (boundaryCondition.DataType == BoundaryConditionDataType.Qh)
                {
                    quantityString = componentString;
                }
                if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    quantityString = quantityString + boundaryCondition.TracerName;
                }

                bcBlockData.Quantities.Add(new BcQuantityData
                {
                    Quantity = quantityString,
                    Unit = component.Unit.Symbol,
                    VerticalPosition = verticalProfile == null ? null : layerIndex.ToString(),
                    Values = PrintValues(component, null, null).ToList()
                });
            }

            return bcBlockData;
        }

        private static ForcingTypeDefinition CreateForcingTypeDefinition(FlowBoundaryCondition boundaryCondition, int index, bool correctionFile)
        {
            var dataType = boundaryCondition.DataType;

            if (dataType == BoundaryConditionDataType.AstroCorrection && !correctionFile)
            {
                dataType = BoundaryConditionDataType.AstroComponents;
            }
            if (dataType == BoundaryConditionDataType.HarmonicCorrection && correctionFile)
            {
                dataType = BoundaryConditionDataType.Harmonics;
            }

            var forcingTypeDefinitions =
                ForcingTypeDefinitions.Values.Where(ftd => ftd.ForcingType == dataType).ToList();

            ForcingTypeDefinition forcingTypeDefinition;

            // The 2d version comes first, the 3d version afterwards
            if (forcingTypeDefinitions.Count() > 1)
            {
                if (boundaryCondition.GetDepthLayerDefinitionAtPoint(index) == null ||
                    boundaryCondition.GetDepthLayerDefinitionAtPoint(index).Type == VerticalProfileType.Uniform)
                {
                    forcingTypeDefinition = forcingTypeDefinitions[0];
                }
                else
                {
                    forcingTypeDefinition = forcingTypeDefinitions[1];
                }
            }
            else
            {
                forcingTypeDefinition = forcingTypeDefinitions.FirstOrDefault();
            }
            return forcingTypeDefinition;
        }

        private static BcQuantityData CreateBcQuantityDataForArgument(string quantity, IVariable argument, DateTime? referenceTime)
        {
            var unit = argument.Unit == null ? null : argument.Unit.Symbol;
            Func<double, double> converter = null;
            if (argument.ValueType == typeof(DateTime) && referenceTime != null)
            {
                unit = "seconds since " + referenceTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (argument.ValueType == typeof (double) && unit.ToLower() == "deg/h") //convert frequencies to periods...
            {
                unit = "minutes";
                converter = FlowBoundaryCondition.GetPeriodInMinutes;
            }
            return new BcQuantityData
            {
                Quantity = quantity,
                Unit = unit,
                Values = PrintValues(argument, referenceTime, converter).ToList()
            };
        }

        private static IEnumerable<string> PrintValues(IVariable variable, DateTime? referenceTime,
            Func<double, double> converter)
        {
            if (variable.ValueType == typeof (string))
            {
                return variable.GetValues<string>();
            }
            if (variable.ValueType == typeof (double))
            {
                if (converter == null)
                {
                    return variable.GetValues<double>().Select(d => d.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    return variable.GetValues<double>().Select(d => converter(d).ToString(CultureInfo.InvariantCulture));
                }
            }
            if (variable.ValueType == typeof (DateTime))
            {
                if (referenceTime == null)
                {
                    return variable.GetValues<DateTime>().Select(d => d.ToString("yyyyMMddHHmmss"));
                }
                return
                    variable.GetValues<DateTime>()
                        .Select(d => (d - referenceTime.Value).TotalSeconds)
                        .Select(m => m.ToString(CultureInfo.InvariantCulture));
            }
            return Enumerable.Empty<string>();
        }
    }
}
