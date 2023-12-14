using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Features;

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
        public const string ConcentrationAtBound = "sedfracbnd";

        protected class ForcingTypeDefinition
        {
            public BoundaryConditionDataType ForcingType;
            public string[] ArgumentDefinitions;
            public string[] ComponentDefinitions;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof (BcFileFlowBoundaryDataBuilder));

        protected static readonly IDictionary<string, ForcingTypeDefinition> ForcingTypeDefinitions =
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

        protected virtual IDictionary<string[], FlowBoundaryQuantityType> FlowQuantityKeys
        {
            get { return flowQuantityKeys; }
        }

        private static readonly IDictionary<string[], FlowBoundaryQuantityType> flowQuantityKeys = new Dictionary<string[], FlowBoundaryQuantityType>
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
            {new[] {ExtForceQuantNames.TracerAtBound}, FlowBoundaryQuantityType.Tracer},
            {new[] {ConcentrationAtBound}, FlowBoundaryQuantityType.SedimentConcentration},
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

            // Get matching set for this data from ALL boundaryConditionSets (the other boundaryConditionSets are not used...)
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

            // If we are filtering on location and the location doesn't match up, return
            if (LocationFilter != null && !Equals(selectedSet.Feature, LocationFilter))
            {
                return false;
            }

            bool skippedAny = false;
            using (CultureUtils.SwitchToInvariantCulture())
            {
                // parse and validate forcingType
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

                // parse and validate verticalProfileDefinition
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

                // parse timeInterpolation
                InterpolationType timeInterpolationType;
                if (!TryParseTimeInterpolationType(data, out timeInterpolationType))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: time interpolation type {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.TimeInterpolationType);
                    return false;
                }


                // parse series index
                int seriesIndex;
                if (!TryParseSeriesIndex(data, out seriesIndex))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: series index {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.SeriesIndex);
                    return false;
                }
                seriesIndex--; //to C-style indexing.

                // parse offset and factor
                double offset;
                double factor;
                if (!TryParseOffsetFactor(data, out offset, out factor))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at {1}: offset {2} or factor {3} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.Offset, data.Factor);
                    return false;
                }

                // parse the Quantities in bc / bcm file
                var componentKeys = forcingTypeDefinition.ComponentDefinitions;
                var argVariables = new Dictionary<int, BcQuantityData>();
                var compVariables = new Dictionary<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData>();

                foreach (var quantity in data.Quantities)
                {
                    var quantityString = quantity.Quantity;

                    // extracting the name of the tracer (if it's a tracer)
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

                    // if it's an argument quantity, add it to the argVariables and continue
                    if (forcingTypeDefinition.ArgumentDefinitions.Contains(quantityString))
                    {
                        argVariables.Add(
                            forcingTypeDefinition.ArgumentDefinitions.ToList().IndexOf(quantityString),
                            quantity);

                        continue;
                    }

                    foreach (var componentKey in componentKeys)
                    {
                        if (String.IsNullOrEmpty(componentKey))
                        {
                            flowQuantity = quantityString.ToLower();
                            break;
                        }
                        if (Equals(quantityString, componentKey) &&
                            forcingTypeDefinition.ForcingType == BoundaryConditionDataType.Qh)
                        {
                            flowQuantity =
                                FlowQuantityKeys.First(kvp => kvp.Value == FlowBoundaryQuantityType.WaterLevel).Key[0];
                            break;
                        }
                        if (quantityString.EndsWith(componentKey))
                        {
                            flowQuantity =
                                quantityString.ToLower().Substring(0, quantityString.Length - componentKey.Length).TrimEnd();
                            break;
                        }
                    }

                    // if we haven't match the quantity, give a warning and continue
                    if (flowQuantity == null)
                    {
                        Log.WarnFormat(
                            "File {0}, block starting at line {1}: quantity {2} could not be parsed; omitting data column.",
                            data.FilePath, data.LineNumber, quantity.Quantity);
                        continue;
                    }
                    
                    // add component variable
                    if (FlowQuantityKeys.Keys.SelectMany(a => a).Any(k => flowQuantity.StartsWith(k)))
                    {
                        var flowQuantityComponentsPair = FlowQuantityKeys.FirstOrDefault(kvp => kvp.Key.Any(k => flowQuantity == k));
                        if (flowQuantityComponentsPair.Key == null)
                            flowQuantityComponentsPair = FlowQuantityKeys.FirstOrDefault(kvp => kvp.Key.Any(k => flowQuantity.StartsWith(k)));

                        int layerIndex;
                        if (flowQuantityComponentsPair.Key != null && TryParseVerticalPosition(quantity, out layerIndex))
                        {
                            var totalComponents = flowQuantityComponentsPair.Key.Length;

                            var existingQuantities = compVariables
                                .Where(v => v.Key.Item1 == flowQuantityComponentsPair.Value)
                                .Select(v => v.Value.Quantity)
                                .Distinct().ToList();
                            
                            var quantityIndex = existingQuantities.IndexOf(quantity.Quantity);
                            if (quantityIndex == -1) quantityIndex = existingQuantities.Count;
                            
                            var index = (totalComponents * layerIndex) + quantityIndex;
                            
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
                    else
                    {
                        // If the flowQuantity is not in our dictionary, return false and process this data block again.
                        // Note: this is a bloody awful 'lazy' implementation... we can be far more efficient here
                        return false;
                    }
                    
                }

                // we now have argVariables and compVariables
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
                    
                    // create actual boundary condition if not already exists
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

                                boundaryCondition = CreateNewBoundaryCondition(quantity.Value.Quantity, flowQuantityEnum, forcingTypeDefinition.ForcingType, selectedSet.Feature, timelag, quantityGroup);
                                
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

                    // adjust boundary condition for correction blocks
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
                        if (data.SupportPoint == selectedSet.Feature.Name)
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

                    // import and add the actual data
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
                        
                        boundaryCondition.VerticalInterpolationType = verticalInterpolationType;

                        var existingData = boundaryCondition.GetDataAtPoint(dataPoint);
                        try
                        {
                            existingData.BeginEdit("Importing data...");
                            if (forcingTypeDefinition.ForcingType == BoundaryConditionDataType.AstroCorrection ||
                                forcingTypeDefinition.ForcingType == BoundaryConditionDataType.HarmonicCorrection)
                            {
                                var existingArgument = existingData.Arguments[0].GetValues();

                                var type = forcingTypeDefinition.ForcingType ==
                                           BoundaryConditionDataType.AstroCorrection
                                    ? typeof (string)
                                    : typeof (double);

                                var parsedArgumentValues = ParseValues(argVariables[0], type, data.SupportPoint).ToList();

                                var indexMapping = parsedArgumentValues.Select(existingArgument.IndexOf).ToList();

                                foreach (var comp in quantityGroup)
                                {
                                    var k = comp.Key.Item2;
                                    var l = (k%2 == 0) ? 2*k + 2 : 2*k + 1;
                                    var variable = existingData.Components[l];
                                    var variableValues = variable.GetValues<double>().ToList();
                                    var values = ParseValues(comp.Value, variable.ValueType, data.SupportPoint).ToList();

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
                                    variable.SetValues(ParseValues(arg.Value, variable.ValueType, data.SupportPoint));
                                    if (variable is IVariable<DateTime>)
                                    {
                                        variable.InterpolationType = timeInterpolationType;
                                    }
                                    
                                    if (variable.ValueType == typeof(DateTime))
                                    {
                                        boundaryCondition.TimeZone = BcQuantityDataParsingHelper.ParseTimeZone(arg.Value.Unit, data.SupportPoint);
                                    }
                                }

                                foreach (var comp in quantityGroup)
                                {
                                    IVariable variable = existingData.Components.ElementAtOrDefault(comp.Key.Item2);
                                    if (variable == null)
                                    {
                                        throw new ArgumentOutOfRangeException(comp.Key.Item1.ToString());
                                    }
                                    variable.SetValues(ParseValues(comp.Value, variable.ValueType, data.SupportPoint));
                                }
                            }
                            existingData.EndEdit();
                        }
                        catch (Exception e)
                        {
                            var ef = string.Format("Skipped DataPoint {0} for Boundary Condition {1} could not be added as the following exception was risen during import: {2}", dataPoint, boundaryCondition.Name, e.Message);
                            Log.ErrorFormat(ef);
                            if (addedData)
                            {
                                boundaryCondition.DataPointIndices.Remove(dataPoint);
                            }
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

        protected virtual FlowBoundaryCondition CreateNewBoundaryCondition(string quantityName, FlowBoundaryQuantityType flowQuantityEnum, BoundaryConditionDataType forcingType, Feature2D feature, TimeSpan timelag, IGrouping<FlowBoundaryQuantityType, KeyValuePair<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData>> grouping)
        {
            var bc = new FlowBoundaryCondition(flowQuantityEnum, forcingType)
            {
                Feature = feature,
                ThatcherHarlemanTimeLag = timelag,
            };
            bc.SedimentFractionNames = GetFractionNames(grouping).ToList();
            if (flowQuantityEnum == FlowBoundaryQuantityType.SedimentConcentration)
            {
                var flowQuantityComponentsPair = FlowQuantityKeys.FirstOrDefault(kvp => kvp.Key.Any(k => quantityName.Equals(k)));
                if (flowQuantityComponentsPair.Key == null)
                {
                    flowQuantityComponentsPair = FlowQuantityKeys.FirstOrDefault(kvp => kvp.Key.Any(k => quantityName.StartsWith(k)));
                }
                if (flowQuantityComponentsPair.Key != null)
                {
                    var matchingQuantity = flowQuantityComponentsPair.Key.FirstOrDefault(k => quantityName.StartsWith(k));
                    if (matchingQuantity != null)
                    {
                        var fractionName = quantityName.Replace(matchingQuantity, string.Empty);
                        bc.SedimentFractionName = fractionName;
                    }
                }
            }
            return bc;
        }

        private IEnumerable<string> GetFractionNames(IGrouping<FlowBoundaryQuantityType, KeyValuePair<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData>> quantityGroup)
        {
            return quantityGroup.Key == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                ? quantityGroup.Select(qg => qg.Value).Select(q => q.Quantity.Replace(BcmFileFlowBoundaryDataBuilder.BedLoadAtBound, String.Empty))
                : Enumerable.Empty<string>();
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

        public IEnumerable<BcBlockData> CreateBlockData(FlowBoundaryCondition boundaryCondition,
            IEnumerable<string> supportPointNames, DateTime? refDate, int seriesIndex = 0, bool correctionFile = false)
        {

            foreach (var dataPointIndex in boundaryCondition.DataPointIndices)
            {
                var supportPoint = supportPointNames.ElementAt(dataPointIndex);
                var bcBlockData = CreateBlockData(boundaryCondition, supportPoint);

                if (PopulateBcBlockData(boundaryCondition, dataPointIndex, supportPoint,
                    refDate, seriesIndex, correctionFile, bcBlockData))
                {
                    yield return bcBlockData;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private static bool TryParseForcingType(BcBlockData blockData, out ForcingTypeDefinition forcingType)
        {
            return ForcingTypeDefinitions.TryGetValue(blockData.FunctionType.ToLower(), out forcingType);
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


        protected static bool TryParseTimeInterpolationType(BcBlockData data, out InterpolationType interpolationType)
        {
            if (String.IsNullOrEmpty(data.TimeInterpolationType) || data.TimeInterpolationType.ToLower() == "linear")
            {
                interpolationType = InterpolationType.Linear;
                return true;
            }
            if (data.TimeInterpolationType.ToLower().Contains("block"))
            {
                interpolationType = InterpolationType.Constant;
                return true;
            }
            interpolationType = InterpolationType.None;
            return false;
        }

        protected static string TimeInterpolationString(InterpolationType interpolationType)
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

        protected virtual IEnumerable<object> ParseValues(BcQuantityData data, Type type, string supportPointName)
        {
            IEnumerable<string> stringValues = data.Values;
            string format = data.Unit;
            if (type == typeof (DateTime))
            {
                return BcQuantityDataParsingHelper.ParseDateTimes(supportPointName, data).Cast<object>();
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

        protected virtual BcBlockData CreateBlockData(FlowBoundaryCondition boundaryCondition, string supportPoint)
        {
            return new BcBlockData
            {
                SupportPoint = boundaryCondition.IsHorizontallyUniform ? boundaryCondition.FeatureName : supportPoint
            };
        }

        private bool PopulateBcBlockData(FlowBoundaryCondition boundaryCondition, int index, string supportPoint,
            DateTime? referenceTime, int seriesIndex, bool correctionFile, BcBlockData bcBlockData)
        {
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
                return false;
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
                bcBlockData.Quantities.Add(CreateBcQuantityDataForArgument(quantity, argument, referenceTime, boundaryCondition.TimeZone));
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
                if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration
                    && boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries)
                {
                    quantityString = quantityString + boundaryCondition.SedimentFractionName;
                }
                if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                    && boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries)
                {
                    quantityString = quantityString + component.Name;
                }
                bcBlockData.Quantities.Add(new BcQuantityData
                {
                    Quantity = quantityString,
                    Unit = component.Unit.Symbol,
                    VerticalPosition = verticalProfile == null ? null : layerIndex.ToString(),
                    Values = PrintValues(component, null, null).ToList()
                });
            }

            return true;
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

        protected virtual BcQuantityData CreateBcQuantityDataForArgument(string quantity, IVariable argument, DateTime? referenceTime, TimeSpan timeZone)
        {
            var unit = argument.Unit?.Symbol;
            Func<double, double> converter = null;
            if (argument.ValueType == typeof(DateTime) && referenceTime != null)
            {
                unit = BcQuantityDataParsingHelper.GetDateTimeUnit(referenceTime.Value, timeZone);
            }
            if (argument.ValueType == typeof (double) && unit?.ToLower() == "deg/h") //convert frequencies to periods...
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

        protected virtual IEnumerable<string> PrintValues(IVariable variable, DateTime? referenceTime,
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

        public void InsertEmptyBoundaryData(List<BoundaryConditionSet> bcSets, FlowBoundaryQuantityType flowBoundaryQuantityType)
        {
            // Get matching set for this data from ALL boundaryConditionSets (the other boundaryConditionSets are not used...)
            var selectedSet = bcSets.FirstOrDefault();
            if (selectedSet == null)
            {
                Log.WarnFormat("Support points are not found in boundaries; omitting block.");
                return;
            }

            // If we are filtering on location and the location doesn't match up, return
            if (LocationFilter != null && !Equals(selectedSet.Feature, LocationFilter))
            {
                return;
            }
            
                // parse and validate forcingType
                if (ExcludedDataTypes != null && ExcludedDataTypes.Contains(BoundaryConditionDataType.Empty))
                {
                    Log.InfoFormat("Skipping boundary data of function type {0}.", BoundaryConditionDataType.Empty);
                    return ;
                }
                FlowBoundaryCondition boundaryCondition =
                    new FlowBoundaryCondition(flowBoundaryQuantityType,
                        BoundaryConditionDataType.Empty)
                    {
                        Feature = selectedSet.Feature,
                    };

                if (!selectedSet.BoundaryConditions.Contains(boundaryCondition))
                {
                    selectedSet.BoundaryConditions.Add(boundaryCondition);
                }

        }
    }
}
