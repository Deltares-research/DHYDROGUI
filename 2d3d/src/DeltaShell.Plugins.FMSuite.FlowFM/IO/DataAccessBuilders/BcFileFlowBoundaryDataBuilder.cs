﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Extensions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    public class BcFileFlowBoundaryDataBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BcFileFlowBoundaryDataBuilder));
        private const StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase;
        private static readonly IDictionary<string, ForcingTypeDefinition> ForcingTypeDefinitions =
            new Dictionary<string, ForcingTypeDefinition>( StringComparer.CurrentCultureIgnoreCase )
            {
                {
                    "timeseries", new ForcingTypeDefinition
                    {
                        ForcingType = BoundaryConditionDataType.TimeSeries,
                        ArgumentDefinitions = new[]
                        {
                            "time"
                        },
                        ComponentDefinitions = new[]
                        {
                            ""
                        }
                    }
                },
                {
                    "t3d", new ForcingTypeDefinition
                    {
                        ForcingType = BoundaryConditionDataType.TimeSeries,
                        ArgumentDefinitions = new[]
                        {
                            "time"
                        },
                        ComponentDefinitions = new[]
                        {
                            ""
                        }
                    }
                },
                {
                    "astronomic", new ForcingTypeDefinition
                    {
                        ForcingType = BoundaryConditionDataType.AstroComponents,
                        ArgumentDefinitions = new[]
                        {
                            "astronomic component"
                        },
                        ComponentDefinitions = new[]
                        {
                            "amplitude",
                            "phase"
                        }
                    }
                },
                {
                    "astronomic-correction", new ForcingTypeDefinition
                    {
                        ForcingType = BoundaryConditionDataType.AstroCorrection,
                        ArgumentDefinitions = new[]
                        {
                            "astronomic component"
                        },
                        ComponentDefinitions = new[]
                        {
                            "amplitude",
                            "phase"
                        }
                    }
                },
                {
                    "harmonic", new ForcingTypeDefinition
                    {
                        ForcingType = BoundaryConditionDataType.Harmonics,
                        ArgumentDefinitions = new[]
                        {
                            "harmonic component"
                        },
                        ComponentDefinitions = new[]
                        {
                            "amplitude",
                            "phase"
                        }
                    }
                },
                {
                    "harmonic-correction", new ForcingTypeDefinition
                    {
                        ForcingType = BoundaryConditionDataType.HarmonicCorrection,
                        ArgumentDefinitions = new[]
                        {
                            "harmonic component"
                        },
                        ComponentDefinitions = new[]
                        {
                            "amplitude",
                            "phase"
                        }
                    }
                },
                {
                    "qhtable", new ForcingTypeDefinition
                    {
                        ForcingType = BoundaryConditionDataType.Qh,
                        ArgumentDefinitions = new[]
                        {
                            "qhbnd discharge"
                        },
                        ComponentDefinitions = new[]
                        {
                            "qhbnd waterlevel"
                        }
                    }
                }
            };

        private static readonly IDictionary<BoundaryConditionDataType, BoundaryConditionDataType> CorrectionTypes =
            new Dictionary<BoundaryConditionDataType, BoundaryConditionDataType>
            {
                {BoundaryConditionDataType.HarmonicCorrection, BoundaryConditionDataType.Harmonics},
                {BoundaryConditionDataType.AstroCorrection, BoundaryConditionDataType.AstroComponents}
            };

        private static readonly IDictionary<string, FlowBoundaryQuantityType> quantityNameToTypeDictionary =
            new Dictionary<string, FlowBoundaryQuantityType>
            {
                {ExtForceQuantNames.WaterLevelAtBound, FlowBoundaryQuantityType.WaterLevel},
                {ExtForceQuantNames.DischargeAtBound, FlowBoundaryQuantityType.Discharge},
                {ExtForceQuantNames.QhAtBound, FlowBoundaryQuantityType.Discharge},
                {ExtForceQuantNames.VelocityAtBound, FlowBoundaryQuantityType.Velocity},
                {ExtForceQuantNames.NeumannConditionAtBound, FlowBoundaryQuantityType.Neumann},
                {ExtForceQuantNames.RiemannConditionAtBound, FlowBoundaryQuantityType.Riemann},
                {ExtForceQuantNames.RiemannVelocityAtBound, FlowBoundaryQuantityType.RiemannVelocity},
                {ExtForceQuantNames.NormalVelocityAtBound, FlowBoundaryQuantityType.NormalVelocity},
                {ExtForceQuantNames.TangentialVelocityAtBound, FlowBoundaryQuantityType.TangentVelocity},
                {"x-velocity", FlowBoundaryQuantityType.VelocityVector},
                {"y-velocity", FlowBoundaryQuantityType.VelocityVector},
                {ExtForceQuantNames.SalinityAtBound, FlowBoundaryQuantityType.Salinity},
                {ExtForceQuantNames.TemperatureAtBound, FlowBoundaryQuantityType.Temperature},
                {ExtForceQuantNames.TracerAtBound, FlowBoundaryQuantityType.Tracer},
                {ExtForceQuantNames.ConcentrationAtBound, FlowBoundaryQuantityType.SedimentConcentration}
            };

        private static readonly IDictionary<string, VerticalProfileType> VerticalDefinitionKeys =
            new Dictionary<string, VerticalProfileType>(StringComparer.CurrentCultureIgnoreCase)
            {
                {"percBed",VerticalProfileType.PercentageFromBed},
                {"zDatum", VerticalProfileType.ZFromDatum},
                {"zBed",VerticalProfileType.ZFromBed},
                {"zSurf", VerticalProfileType.ZFromSurface},
                // obsolete values retained to keep our tests from failing
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
                {"z from datum", VerticalProfileType.ZFromDatum},
                {"z above datum", VerticalProfileType.ZFromDatum},
                {"percentage from bed", VerticalProfileType.PercentageFromBed},
                {"percentage above bed", VerticalProfileType.PercentageFromBed},
                {"percentage from bottom", VerticalProfileType.PercentageFromBed},
                {"percentage above bottom", VerticalProfileType.PercentageFromBed},
                {"percentage from surface", VerticalProfileType.PercentageFromSurface},
                {"percentage from top", VerticalProfileType.PercentageFromSurface}
            };

        public BcFileFlowBoundaryDataBuilder()
        {
            ExcludedDataTypes = new List<BoundaryConditionDataType>();
            ExcludedQuantities = new List<FlowBoundaryQuantityType>();
            OverwriteExistingData = true;
            CanCreateNewBoundaryCondition = true;
        }

        public static IEnumerable<string> CorrectionFunctionTypes
        {
            get
            {
                return ForcingTypeDefinitions
                       .Where(kvp =>
                                  kvp.Value.ForcingType == BoundaryConditionDataType.AstroCorrection
                                  || kvp.Value.ForcingType == BoundaryConditionDataType.HarmonicCorrection)
                       .Select(kvp => kvp.Key);
            }
        }

        public IList<BoundaryConditionDataType> ExcludedDataTypes { private get; set; }

        public IList<FlowBoundaryQuantityType> ExcludedQuantities { private get; set; }

        public bool OverwriteExistingData { private get; set; }

        public bool CanCreateNewBoundaryCondition { private get; set; }

        public IFeature LocationFilter { private get; set; }

        public void InsertBoundaryData(IEnumerable<BoundaryConditionSet> boundaryConditionSets,
                                       IEnumerable<BcBlockData> dataBlock, double? thatcherHarlemanTimeLag = null)
        {
            List<BoundaryConditionSet> bcSets = boundaryConditionSets.ToList();
            foreach (BcBlockData bcBlockData in dataBlock)
            {
                InsertBoundaryData(bcSets, bcBlockData, thatcherHarlemanTimeLag);
            }
        }

        public bool InsertBoundaryData(IEnumerable<BoundaryConditionSet> boundaryConditionSets, BcBlockData dataBlock,
                                       double? thatcherHarlemanTimeLag = null)
        {
            if (dataBlock == null)
            {
                return false;
            }

            // Get matching set for this dataBlock from ALL boundaryConditionSets (the other boundaryConditionSets are not used...)
            BoundaryConditionSet matchingBoundaryConditionSet =
                GetMatchingBoundaryConditionSet(boundaryConditionSets, dataBlock.SupportPoint);
            if (matchingBoundaryConditionSet == null)
            {
                if (dataBlock.Quantities.All(q => !q.QuantityName.Equals(BcFileConstants.LateralDischargeQuantityName, stringComparison)))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: support point {2} was not found in boundaries; omitting dataBlock block.",
                        dataBlock.FilePath, dataBlock.LineNumber, dataBlock.SupportPoint);
                }
                
                return false;
            }

            // If we are filtering on location and the location doesn't match up, return
            if (LocationFilter != null && !Equals(matchingBoundaryConditionSet.Feature, LocationFilter))
            {
                return false;
            }

            var skippedAny = false;

            using (CultureUtils.SwitchToInvariantCulture())
            {
                DaoDataBlock daoDataBlock = GetDaoDataBlock(dataBlock);
                if (daoDataBlock == null)
                    return false;


                // parse the Quantities in bc / bcm file
                string[] componentKeys = daoDataBlock.ForcingTypeDefinition.ComponentDefinitions;
                var argVariables = new Dictionary<int, BcQuantityData>();
                var compVariables = new Dictionary<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData>();

                foreach (BcQuantityData quantityData in dataBlock.Quantities)
                {
                    string quantityName = GetQuantityName(quantityData);

                    // if it's an argument quantity, add it to the argVariables and continue
                    if (daoDataBlock.ForcingTypeDefinition.ArgumentDefinitions.Contains(quantityName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        argVariables.Add(daoDataBlock.ForcingTypeDefinition.ArgumentDefinitions.ToList().FindIndex(q => q.Equals(quantityName, stringComparison)),
                                         quantityData);

                        continue;
                    }

                    quantityName = ParseQuantityName(componentKeys, quantityName, daoDataBlock.ForcingTypeDefinition);

                    // if we haven't match the quantity, give a warning and continue
                    if (quantityName == null)
                    {
                        LogWarningParsePropertyFailed(dataBlock, "quantity", quantityData.QuantityName);
                        continue;
                    }

                    // add component variable
                    if (QuantityNameToTypeDictionary.Keys.Any(key => quantityName.StartsWith(key, stringComparison)))
                    {
                        KeyValuePair<string, FlowBoundaryQuantityType> quantityKeyValuePair =
                            QuantityNameToTypeDictionary.FirstOrDefault(kvp => kvp.Key.Equals(quantityName, stringComparison));
                        if (quantityKeyValuePair.Key == null)
                        {
                            quantityKeyValuePair =
                                QuantityNameToTypeDictionary.FirstOrDefault(kvp => quantityName.StartsWith(kvp.Key, stringComparison));
                        }

                        if (quantityKeyValuePair.Key != null &&
                            ParseVerticalPosition(quantityData, out int layerIndex))
                        {
                            int totalComponents =
                                QuantityNameToTypeDictionary.Count(kvp =>
                                                                       kvp.Value == quantityKeyValuePair.Value);

                            List<string> existingQuantities = compVariables
                                                              .Where(v => v.Key.Item1 == quantityKeyValuePair.Value)
                                                              .Select(v => v.Value.QuantityName)
                                                              .Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();

                            int quantityIndex = existingQuantities.FindIndex(q => q.Equals(quantityData.QuantityName, stringComparison));
                            if (quantityIndex == -1)
                            {
                                quantityIndex = existingQuantities.Count;
                            }

                            int index = (totalComponents * layerIndex) + quantityIndex;

                            compVariables.Add(
                                new System.Tuple<FlowBoundaryQuantityType, int>(quantityKeyValuePair.Value,
                                                                                index),
                                quantityData);
                        }
                        else
                        {
                            LogWarningParsePropertyFailed(dataBlock, "vertical position",
                                                          quantityData.VerticalPosition);
                        }
                    }
                    else
                    {
                        // If the flowQuantity is not in our dictionary, return false and process this dataBlock block again.
                        // Note: this is a bloody awful 'lazy' implementation... we can be far more efficient here
                        return false;
                    }
                }

                // we now have argVariables and compVariables
                IEnumerable<IGrouping<FlowBoundaryQuantityType,
                    KeyValuePair<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData>>> quantityGroups =
                    compVariables.GroupBy(kvp => kvp.Key.Item1);

                foreach (IGrouping<FlowBoundaryQuantityType,
                             KeyValuePair<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData>> quantityGroup in
                    quantityGroups)
                {
                    FlowBoundaryQuantityType flowQuantityEnum = quantityGroup.Key;

                    if (ExcludedQuantities.Contains(flowQuantityEnum))
                    {
                        skippedAny = true;
                        continue;
                    }

                    FlowBoundaryCondition boundaryCondition = null;

                    // create actual boundary condition if not already exists
                    foreach (KeyValuePair<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData> quantity in
                        quantityGroup)
                    {
                        List<FlowBoundaryCondition> existingConditions = matchingBoundaryConditionSet
                                                                         .BoundaryConditions
                                                                         .OfType<FlowBoundaryCondition>()
                                                                         .Where(bc =>
                                                                                    MatchBoundaryCondition(
                                                                                        bc, flowQuantityEnum,
                                                                                        quantity.Value,
                                                                                        daoDataBlock.ForcingTypeDefinition))
                                                                         .ToList();

                        boundaryCondition = existingConditions.ElementAtOrDefault(daoDataBlock.SeriesIndex);

                        if (boundaryCondition != null)
                        {
                            continue;
                        }

                        bool isCorrection = IsCorrectionDataType(daoDataBlock.ForcingTypeDefinition);

                        if (CanCreateNewBoundaryCondition && !isCorrection)
                        {
                            TimeSpan timelag = TimeSpan.Zero;
                            if (thatcherHarlemanTimeLag != null)
                            {
                                timelag = TimeSpan.FromSeconds(thatcherHarlemanTimeLag.Value);
                            }

                            boundaryCondition = CreateNewBoundaryCondition(quantity.Value.QuantityName, flowQuantityEnum,
                                                                           daoDataBlock.ForcingTypeDefinition.ForcingType,
                                                                           matchingBoundaryConditionSet.Feature,
                                                                           timelag, quantityGroup);

                            if (flowQuantityEnum == FlowBoundaryQuantityType.Tracer)
                            {
                                boundaryCondition.TracerName = quantity.Value.TracerName;
                            }
                        }
                        else
                        {
                            Log.Warn(
                                $"File {dataBlock.FilePath}, block starting at line {dataBlock.LineNumber}: quantity {quantity.Value} and forcing type {daoDataBlock.ForcingTypeDefinition.ForcingType} do not match given boundary condition.");
                        }
                    }

                    if (boundaryCondition == null)
                    {
                        continue;
                    }

                    switch (daoDataBlock.ForcingTypeDefinition.ForcingType)
                    {
                        // adjust boundary condition for correction blocks
                        case BoundaryConditionDataType.AstroCorrection
                            when boundaryCondition.DataType == BoundaryConditionDataType.AstroComponents:
                            boundaryCondition.DataType = BoundaryConditionDataType.AstroCorrection;
                            break;
                        case BoundaryConditionDataType.HarmonicCorrection
                            when boundaryCondition.DataType == BoundaryConditionDataType.Harmonics:
                            boundaryCondition.DataType = BoundaryConditionDataType.HarmonicCorrection;
                            break;
                    }

                    boundaryCondition.Offset = daoDataBlock.Offset;
                    boundaryCondition.Factor = daoDataBlock.Factor;

                    int dataIndex = matchingBoundaryConditionSet
                                    .SupportPointNames.ToList().IndexOf(dataBlock.SupportPoint);

                    if (boundaryCondition.IsHorizontallyUniform)
                    {
                        if (dataBlock.SupportPoint == matchingBoundaryConditionSet.Feature.Name)
                        {
                            dataIndex = 0;
                        }
                        else if (dataIndex != 0)
                        {
                            Log.InfoFormat(
                                "File {0}, block starting at line {1}: {2} uniform boundary condition cannot be specified at point {3}; omitting dataBlock columns.",
                                dataBlock.FilePath, dataBlock.LineNumber, flowQuantityEnum, dataBlock.SupportPoint);
                            continue;
                        }
                    }

                    List<int> dataPoints = dataIndex == -1
                                               ? Enumerable
                                                 .Range(0, boundaryCondition.Feature.Geometry.Coordinates.Count())
                                                 .ToList()
                                               : new List<int> { dataIndex };

                    // import and add the actual dataBlock
                    foreach (int dataPoint in dataPoints)
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
                                    "File {0}, block starting at line {1}: {2} boundary condition already contains dataBlock at point {3}; omitting dataBlock columns.",
                                    dataBlock.FilePath, dataBlock.LineNumber, flowQuantityEnum, dataBlock.SupportPoint);
                                continue;
                            }
                        }

                        int verticalProfileIndex = boundaryCondition.DataPointIndices.IndexOf(dataPoint);
                        if (!boundaryCondition.IsVerticallyUniform)
                        {
                            boundaryCondition.PointDepthLayerDefinitions[verticalProfileIndex] =
                                daoDataBlock.VerticalProfileDefinition;
                        }

                        boundaryCondition.VerticalInterpolationType = daoDataBlock.VerticalInterpolationType;

                        IFunction existingData = boundaryCondition.GetDataAtPoint(dataPoint);
                        try
                        {
                            existingData.BeginEdit("Importing dataBlock...");
                            if (daoDataBlock.ForcingTypeDefinition.ForcingType == BoundaryConditionDataType.AstroCorrection ||
                                daoDataBlock.ForcingTypeDefinition.ForcingType == BoundaryConditionDataType.HarmonicCorrection)
                            {
                                IMultiDimensionalArray existingArgument = existingData.Arguments[0].GetValues();

                                Type type = daoDataBlock.ForcingTypeDefinition.ForcingType ==
                                            BoundaryConditionDataType.AstroCorrection
                                                ? typeof(string)
                                                : typeof(double);

                                List<object> parsedArgumentValues = ParseValues(argVariables[0], type, dataBlock.SupportPoint).ToList();

                                List<int> indexMapping = parsedArgumentValues.Select(existingArgument.IndexOf).ToList();

                                foreach (KeyValuePair<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData> comp
                                    in quantityGroup)
                                {
                                    int k = comp.Key.Item2;
                                    int l = k % 2 == 0 ? (2 * k) + 2 : (2 * k) + 1;
                                    IVariable variable = existingData.Components[l];
                                    List<double> variableValues = variable.GetValues<double>().ToList();
                                    List<object> values = ParseValues(comp.Value, variable.ValueType, dataBlock.SupportPoint).ToList();

                                    for (var i = 0; i < parsedArgumentValues.Count; ++i)
                                    {
                                        int index = indexMapping[i];

                                        if (index == -1)
                                        {
                                            continue;
                                        }

                                        if (i == values.Count)
                                        {
                                            break;
                                        }

                                        variableValues[index] = (double)values[i];
                                    }

                                    variable.Values.Clear();
                                    FunctionHelper.SetValuesRaw<double>(variable, variableValues);
                                }
                            }
                            else
                            {
                                foreach (KeyValuePair<int, BcQuantityData> arg in argVariables)
                                {
                                    IVariable variable = existingData.Arguments.ElementAtOrDefault(arg.Key);
                                    if (variable == null)
                                    {
                                        throw new ArgumentOutOfRangeException(arg.Value.ToString());
                                    }
                                    variable.Values.Clear();
                                    variable.SetValues(ParseValues(arg.Value, variable.ValueType, dataBlock.SupportPoint));
                                    if (variable is IVariable<DateTime>)
                                    {
                                        variable.InterpolationType = daoDataBlock.InterpolationType;
                                    }

                                    if (variable.ValueType == typeof(DateTime))
                                    {
                                        boundaryCondition.TimeZone = BcQuantityDataParsingHelper.ParseTimeZone(arg.Value.Unit, dataBlock.SupportPoint);
                                    }
                                }

                                foreach (KeyValuePair<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData> comp
                                    in quantityGroup)
                                {
                                    IVariable variable = existingData.Components.ElementAtOrDefault(comp.Key.Item2);
                                    if (variable == null)
                                    {
                                        throw new ArgumentOutOfRangeException(comp.Key.Item1.ToString());
                                    }
                                    variable.SetValues(ParseValues(comp.Value, variable.ValueType, dataBlock.SupportPoint));
                                }
                            }

                            existingData.EndEdit();
                        }
                        catch (Exception e)
                        {
                            var ef = string.Format(Resources.BcFileFlowBoundaryDataBuilder_InsertBoundaryData_Skipped_DataPoint__0__for_Boundary_Condition__1__could_not_be_added_as_the_following_exception_was_risen_during_import___2_, dataPoint, boundaryCondition.Name, e.Message);
                            Log.ErrorFormat(ef);
                            if (addedData)
                            {
                                boundaryCondition.DataPointIndices.Remove(dataPoint);
                            }
                        }
                    }

                    if (!matchingBoundaryConditionSet.BoundaryConditions.Contains(boundaryCondition))
                    {
                        matchingBoundaryConditionSet.BoundaryConditions.Add(boundaryCondition);
                    }
                }
            }

            // If the dataBlock block is not completely used, the logic should go through this dataBlock block again.
            // So if any blocks are skipped, return false so it is not removed from the list of blocks to go through.
            return !skippedAny;
        }

        public IEnumerable<BcBlockData> CreateBlockData(FlowBoundaryCondition boundaryCondition,
                                                        IEnumerable<string> supportPointNames, DateTime? refDate,
                                                        int seriesIndex = 0, bool correctionFile = false)
        {
            supportPointNames = supportPointNames.ToList();
            foreach (int dataPointIndex in boundaryCondition.DataPointIndices)
            {
                string supportPoint = supportPointNames.ElementAt(dataPointIndex);
                BcBlockData dataBlock = CreateBlockData(boundaryCondition, supportPoint);

                if (PopulateBcBlockData(boundaryCondition, dataPointIndex, supportPoint,
                                        refDate, seriesIndex, correctionFile, dataBlock))
                {
                    yield return dataBlock;
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="BcBlockData"/> from the lateral time series data.
        /// </summary>
        /// <param name="lateral"> The lateral. </param>
        /// <param name="referenceDate"> The reference date. </param>
        /// <returns>
        /// The <see cref="BcBlockData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lateral"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided lateral discharge type is not time series.
        /// </exception>
        public BcBlockData CreateBcBlockForLateral(Lateral lateral, DateTime? referenceDate)
        {
            Ensure.NotNull(lateral, nameof(lateral));
            
            if (lateral.Data.Discharge.Type != LateralDischargeType.TimeSeries)
            {
                throw new ArgumentException($"Lateral discharge type should be {LateralDischargeType.TimeSeries}.");
            }

            LateralDischargeFunction dischargeFunction = lateral.Data.Discharge.TimeSeries;
            
            var block = new BcBlockData();
            block.SupportPoint = lateral.Name;
            block.FunctionType = BcFileConstants.TimeSeriesFunctionName;
            block.TimeInterpolationType = TimeInterpolationString(dischargeFunction.Time.InterpolationType);
            BcQuantityData timeQuantity = CreateLateralDischargeTimesQuantityData(lateral, referenceDate);
            BcQuantityData dischargeQuantity = CreateLateralDischargeValuesQuantityData(lateral);

            block.Quantities.Add(timeQuantity);
            block.Quantities.Add(dischargeQuantity);

            return block;
        }

        private BcQuantityData CreateLateralDischargeTimesQuantityData(Lateral lateral, DateTime? referenceDate)
        {
            const string quantity = "time";
            LateralDischargeFunction timeSeries = lateral.Data.Discharge.TimeSeries;
            return CreateBcQuantityDataForArgument(quantity, timeSeries.Time, referenceDate, timeSeries.TimeZone);

        }

        private static BcQuantityData CreateLateralDischargeValuesQuantityData(Lateral lateral)
        {
            IVariable dischargeComponent = lateral.Data.Discharge.TimeSeries.DischargeComponent;
            string unit = dischargeComponent.Unit.Symbol;
            List<string> values = dischargeComponent.GetValues<double>().Select(DoubleToString).ToList();

            return new BcQuantityData
            {
                QuantityName = BcFileConstants.LateralDischargeQuantityName,
                Unit = unit,
                Values = values
            };
        }

        public void InsertEmptyBoundaryData(List<BoundaryConditionSet> bcSets,
                                            FlowBoundaryQuantityType flowBoundaryQuantityType)
        {
            // Get matching set for this dataBlock from ALL boundaryConditionSets (the other boundaryConditionSets are not used...)
            BoundaryConditionSet selectedSet = bcSets.FirstOrDefault();
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
            if (ExcludedDataTypes?.Contains(BoundaryConditionDataType.Empty) == true)
            {
                Log.InfoFormat("Skipping boundary dataBlock of function type {0}.", BoundaryConditionDataType.Empty);
                return;
            }

            var boundaryCondition =
                new FlowBoundaryCondition(flowBoundaryQuantityType,
                                          BoundaryConditionDataType.Empty)
                { Feature = selectedSet.Feature };

            if (!selectedSet.BoundaryConditions.Contains(boundaryCondition))
            {
                selectedSet.BoundaryConditions.Add(boundaryCondition);
            }
        }

        protected virtual IDictionary<string, FlowBoundaryQuantityType> QuantityNameToTypeDictionary =>
            quantityNameToTypeDictionary;

        protected virtual FlowBoundaryCondition CreateNewBoundaryCondition(string quantityName,
                                                                           FlowBoundaryQuantityType flowQuantityEnum,
                                                                           BoundaryConditionDataType forcingType,
                                                                           Feature2D feature,
                                                                           TimeSpan timelag,
                                                                           IGrouping<FlowBoundaryQuantityType,
                                                                                   KeyValuePair<System.Tuple<
                                                                                       FlowBoundaryQuantityType
                                                                                       , int>, BcQuantityData>>
                                                                               grouping)
        {
            var boundaryCondition = new FlowBoundaryCondition(flowQuantityEnum, forcingType)
            {
                Feature = feature,
                ThatcherHarlemanTimeLag = timelag,
                SedimentFractionNames = GetFractionNames(grouping).ToList()
            };

            if (flowQuantityEnum != FlowBoundaryQuantityType.SedimentConcentration)
            {
                return boundaryCondition;
            }

            KeyValuePair<string, FlowBoundaryQuantityType> flowQuantityComponentsPair =
                QuantityNameToTypeDictionary.FirstOrDefault(kvp => kvp.Key.Equals(quantityName, stringComparison));
            if (flowQuantityComponentsPair.Key == null)
            {
                flowQuantityComponentsPair =
                    QuantityNameToTypeDictionary.FirstOrDefault(kvp => quantityName.StartsWith(kvp.Key, stringComparison));
            }

            if (flowQuantityComponentsPair.Key == null)
            {
                return boundaryCondition;
            }

            string fractionName = GetFractionNameFromQuantityName(quantityName);
            boundaryCondition.SedimentFractionName = fractionName;

            return boundaryCondition;
        }

        protected virtual IEnumerable<object> ParseValues(BcQuantityData quantityData, Type type, string supportPointName)
        {
            IEnumerable<string> stringValues = quantityData.Values;
            string format = quantityData.Unit;
            if (type == typeof(DateTime))
            {
                return BcQuantityDataParsingHelper.ParseDateTimes(supportPointName, quantityData).Cast<object>();
            }

            if (type == typeof(string))
            {
                return stringValues;
            }

            if (type != typeof(double))
            {
                throw new ArgumentException($"Value type {type} with unit {format} not supported by bc file parser.");
            }

            if (format?.ToLower().Equals("minutes") == true)
            {
                return
                    stringValues.Select(double.Parse)
                                .Select(FlowBoundaryCondition.GetFrequencyInDegPerHour)
                                .Cast<object>();
            }

            return stringValues.Select(double.Parse).Cast<object>();
        }

        protected virtual BcBlockData CreateBlockData(FlowBoundaryCondition boundaryCondition, string supportPoint)
        {
            return new BcBlockData
            {
                SupportPoint = boundaryCondition.IsHorizontallyUniform
                                   ? boundaryCondition.FeatureName
                                   : supportPoint
            };
        }

        protected virtual BcQuantityData CreateBcQuantityDataForArgument(string quantity, IVariable argument,
                                                                         DateTime? referenceTime, TimeSpan timeZone)
        {
            string unit = argument.Unit?.Symbol;
            Func<double, double> converter = null;
            if (argument.ValueType == typeof(DateTime) && referenceTime != null)
            {
                unit = BcQuantityDataParsingHelper.GetDateTimeUnit(referenceTime.Value, timeZone);
            }

            if (argument.ValueType != typeof(double) || unit?.ToLower() != "deg/h")
            {
                return CreateBcQuantityData(quantity, argument, referenceTime, unit, converter);
            }

            unit = "minutes";
            converter = FlowBoundaryCondition.GetPeriodInMinutes;

            return CreateBcQuantityData(quantity, argument, referenceTime, unit, converter);
        }

        protected virtual IEnumerable<string> PrintValues(IVariable variable, DateTime? referenceTime,
                                                          Func<double, double> converter)
        {
            if (variable.ValueType == typeof(string))
            {
                return variable.GetValues<string>();
            }

            if (variable.ValueType == typeof(double))
            {
                return converter == null
                           ? variable.GetValues<double>().Select(DoubleToString)
                           : variable.GetValues<double>()
                                     .Select(d => DoubleToString(converter(d)));
            }

            if (variable.ValueType != typeof(DateTime))
            {
                return Enumerable.Empty<string>();
            }

            if (referenceTime == null)
            {
                return variable.GetValues<DateTime>().Select(d => d.ToString("yyyyMMddHHmmss"));
            }

            return
                variable.GetValues<DateTime>()
                        .Select(d => (d - referenceTime.Value).TotalSeconds)
                        .Select(DoubleToString);
        }

        private static string DoubleToString(double value) => value.ToString(CultureInfo.InvariantCulture);

        private string ParseQuantityName(string[] componentKeys, string quantityName,
                                         ForcingTypeDefinition forcingTypeDefinition)
        {
            foreach (string componentKey in componentKeys)
            {
                if (string.IsNullOrEmpty(componentKey))
                {
                    break;
                }

                if (quantityName.Equals(componentKey, stringComparison) &&
                    forcingTypeDefinition.ForcingType == BoundaryConditionDataType.Qh)
                {
                    quantityName = QuantityNameToTypeDictionary
                                   .First(kvp => kvp.Value == FlowBoundaryQuantityType.WaterLevel)
                                   .Key;
                    break;
                }

                if (quantityName.EndsWith(componentKey, stringComparison))
                {
                    quantityName = quantityName.Substring(0, quantityName.Length - componentKey.Length).TrimEnd();
                    break;
                }
            }

            return quantityName;
        }

        private static string GetQuantityName(BcQuantityData quantityData)
        {
            string quantityName = quantityData.QuantityName;
            if (quantityName.StartsWith(ExtForceQuantNames.TracerAtBound + "_", stringComparison))
            {
                quantityData.TracerName = quantityName.Substring(10);
                quantityName = ExtForceQuantNames.TracerAtBound;
            }
            else if (quantityName.StartsWith(ExtForceQuantNames.TracerAtBound, stringComparison))
            {
                quantityData.TracerName = quantityName.Substring(9);
                quantityName = ExtForceQuantNames.TracerAtBound;
            }

            return quantityName.ToLower();
        }

        private static bool IsCorrectionDataType(ForcingTypeDefinition forcingTypeDefinition)
        {
            return forcingTypeDefinition.ForcingType ==
                   BoundaryConditionDataType.AstroCorrection ||
                   forcingTypeDefinition.ForcingType ==
                   BoundaryConditionDataType.HarmonicCorrection;
        }

        private static BoundaryConditionSet GetMatchingBoundaryConditionSet(
            IEnumerable<BoundaryConditionSet> boundaryConditionSets, string supportPointName)
        {
            return boundaryConditionSets.FirstOrDefault(
                bcs => bcs.SupportPointNames.Contains(supportPointName)
                       || bcs.Feature.Name == supportPointName);
        }

        private static string GetFractionNameFromQuantityName(string quantityName)
        {
            return quantityName.ReplaceCaseInsensitive(ExtForceQuantNames.ConcentrationAtBound, string.Empty);
        }

        private IEnumerable<string> GetFractionNames(
            IGrouping<FlowBoundaryQuantityType,
                KeyValuePair<System.Tuple<FlowBoundaryQuantityType, int>, BcQuantityData>> quantityGroup)
        {
            return quantityGroup.Key == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                       ? quantityGroup.Select(qg => qg.Value).Select(q =>
                                                                         q.QuantityName.ReplaceCaseInsensitive(
                                                                             BcmFileFlowBoundaryDataBuilder
                                                                                 .BedLoadAtBound, string.Empty))
                       : Enumerable.Empty<string>();
        }

        private static bool MatchBoundaryCondition(FlowBoundaryCondition bc, FlowBoundaryQuantityType quantity,
                                                   BcQuantityData quantityData,
                                                   ForcingTypeDefinition forcingTypeDefinition)
        {
            if (bc.FlowQuantity != quantity)
            {
                return false;
            }

            switch (bc.FlowQuantity)
            {
                case FlowBoundaryQuantityType.Tracer when quantityData.TracerName != bc.TracerName:
                case FlowBoundaryQuantityType.SedimentConcentration
                    when GetFractionNameFromQuantityName(quantityData.QuantityName) != bc.SedimentFractionName:
                    return false;
            }

            switch (forcingTypeDefinition.ForcingType)
            {
                case BoundaryConditionDataType.AstroCorrection:
                    return bc.DataType == BoundaryConditionDataType.AstroComponents ||
                           bc.DataType == BoundaryConditionDataType.AstroCorrection;
                case BoundaryConditionDataType.HarmonicCorrection:
                    return bc.DataType == BoundaryConditionDataType.Harmonics ||
                           bc.DataType == BoundaryConditionDataType.HarmonicCorrection;
                default:
                    return bc.DataType == forcingTypeDefinition.ForcingType;
            }
        }

        private static string VerticalProfileTypeString(VerticalProfileType type)
        {
            if (VerticalDefinitionKeys.Values.Contains(type))
            {
                return VerticalDefinitionKeys.First(kvp => kvp.Value == type).Key;
            }

            string supportedValues = string.Join(", ", VerticalDefinitionKeys.Keys);
            throw new NotSupportedException($"Vertical profile definition {type} not supported. " +
                                            $"Supported values are: {supportedValues}");
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
                return string.Join(" ", verticalProfile.SortedPointDepths.Select(d => d.ToString()));
            }
        }

        #region Parse Datablocks

        private static ForcingTypeDefinition ParseForcingType(BcBlockData dataBlock)
        {
            try
            {
                return ForcingTypeDefinitions[dataBlock.FunctionType];
            }
            catch
            {
                LogWarningParsePropertyFailed(dataBlock, "function type", dataBlock.FunctionType);
                throw;
            }
        }

        private static int ParseSeriesIndex(BcBlockData dataBlock)
        {
            try
            {
                // C-style indexing.
                return dataBlock.SeriesIndex != null ? int.Parse(dataBlock.SeriesIndex) - 1 : 0;
            }
            catch
            {
                LogWarningParsePropertyFailed(dataBlock, dataBlock.SeriesIndex, "series index");
                throw;
            }
        }

        private static double ParseOffset(BcBlockData dataBlock)
        {
            try
            {
                if (dataBlock.Offset == null)
                {
                    return 0;
                }

                return double.Parse(dataBlock.Offset);
            }
            catch
            {
                LogWarningParsePropertyFailed(dataBlock, "offset", dataBlock.Offset);
                throw;
            }
        }

        private static double ParseFactor(BcBlockData dataBlock)
        {
            try
            {
                return dataBlock.Factor == null ? 1 : double.Parse(dataBlock.Factor);
            }
            catch
            {
                LogWarningParsePropertyFailed(dataBlock, "factor", dataBlock.Factor);
                throw;
            }
        }

        private static VerticalProfileDefinition ParseDepthLayerDefinition(BcBlockData dataBlock)
        {
            if (dataBlock.VerticalPositionType == null)
            {
                return new VerticalProfileDefinition();
            }

            if (!VerticalDefinitionKeys.ContainsKey(dataBlock.VerticalPositionType))
            {
                LogWarningParsePropertyFailed(dataBlock, "vertical position type", dataBlock.VerticalPositionType);
                string supportedValues = string.Join(", ", VerticalDefinitionKeys.Keys);
                throw new NotSupportedException($"Vertical profile definition {dataBlock.VerticalPositionType} not supported. " +
                                                $"Supported values are: {supportedValues}");
            }

            try
            {
                var depths = new List<double>();
                VerticalProfileType type = VerticalDefinitionKeys[dataBlock.VerticalPositionType];

                if (type == VerticalProfileType.Uniform || type == VerticalProfileType.TopBottom)
                {
                    return VerticalProfileDefinition.Create(type, depths);
                }

                depths = dataBlock.VerticalPositionDefinition.Split(null as char[], StringSplitOptions.RemoveEmptyEntries)
                                  .Select(double.Parse)
                                  .ToList();

                return VerticalProfileDefinition.Create(type, depths);
            }
            catch
            {
                LogWarningParsePropertyFailed(dataBlock, "vertical profile definition",
                                              dataBlock.VerticalPositionDefinition);
                throw;
            }
        }

        private static bool ParseVerticalPosition(BcQuantityData quantityData, out int layerIndex)
        {
            if (quantityData.VerticalPosition == null)
            {
                layerIndex = 0;
                return true;
            }

            bool result = int.TryParse(quantityData.VerticalPosition, out layerIndex);
            if (result)
            {
                layerIndex -= 1; //to C-style indexing
            }

            return result;
        }

        private static VerticalInterpolationType ParseVerticalInterpolationType(BcBlockData dataBlock)
        {
            string interpolationType = dataBlock.VerticalInterpolationType;
            if (string.IsNullOrEmpty(interpolationType) || interpolationType.ToLower() == "linear")
            {
                return VerticalInterpolationType.Linear;
            }

            switch (interpolationType.ToLower())
            {
                case "block":
                    return VerticalInterpolationType.Step;
                case "log":
                    return VerticalInterpolationType.Logarithmic;
                default:
                    LogWarningParsePropertyFailed(dataBlock, "vertical interpolation type",
                                                  dataBlock.VerticalInterpolationType);
                    return VerticalInterpolationType.Uniform;
            }
        }

        private static void LogWarningParsePropertyFailed(BcBlockData dataBlock, string propertyName,
                                                          string propertyValue)
        {
            Log.Warn(
                $"File {dataBlock.FilePath}, block starting at line {dataBlock.LineNumber}: {propertyName} {propertyValue} could not be parsed; omitting dataBlock block.");
        }

        #endregion

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
                        $"Vertical interpolation type {verticalInterpolationType} not supported by bc file writer.");
            }
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

        private bool PopulateBcBlockData(FlowBoundaryCondition boundaryCondition, int index, string supportPoint,
                                         DateTime? referenceTime, int seriesIndex, bool correctionFile,
                                         BcBlockData dataBlock)
        {
            // Inconsistency in kernel: for discharges it expects a single support point name...
            if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Discharge)
            {
                dataBlock.SupportPoint = supportPoint;
            }

            ForcingTypeDefinition forcingTypeDefinition =
                CreateForcingTypeDefinition(boundaryCondition, index, correctionFile);

            if (forcingTypeDefinition == null)
            {
                Log.WarnFormat(
                    "Boundary condition function type {0} not supported by bc-file writer; skipping condition.",
                    boundaryCondition.DataType);
                return false;
            }

            BoundaryConditionDataType forcingType = forcingTypeDefinition.ForcingType;

            string functionType =
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

            dataBlock.FunctionType = functionType;

            if (seriesIndex > 0)
            {
                dataBlock.SeriesIndex = (seriesIndex + 1).ToString(); //to one-based index.
            }

            IFunction function = boundaryCondition.GetDataAtPoint(index);

            dataBlock.TimeInterpolationType = !(function.Arguments.FirstOrDefault() is IVariable<DateTime> timeArgument)
                                                  ? null
                                                  : TimeInterpolationString(timeArgument.InterpolationType);

            VerticalProfileDefinition verticalProfile = boundaryCondition.IsVerticallyUniform
                                                            ? null
                                                            : boundaryCondition.GetDepthLayerDefinitionAtPoint(index);

            string verticalProfileTypeString = verticalProfile == null
                                                   ? null
                                                   : VerticalProfileTypeString(verticalProfile.Type);

            if (verticalProfileTypeString != null)
            {
                dataBlock.VerticalPositionType = verticalProfileTypeString;
                dataBlock.VerticalPositionDefinition = VerticalProfileDefinitionString(verticalProfile);
                dataBlock.VerticalInterpolationType =
                    VerticalInterpolationString(boundaryCondition.VerticalInterpolationType);
            }

            if (boundaryCondition.Offset != 0)
            {
                dataBlock.Offset = string.Format("{0:0.0000000e+00}", boundaryCondition.Offset);
            }

            if (boundaryCondition.Factor != 1)
            {
                dataBlock.Factor = string.Format("{0:0.0000000e+00}", boundaryCondition.Factor);
            }

            var i = 0;
            foreach (IVariable argument in function.Arguments)
            {
                string quantity = forcingTypeDefinition.ArgumentDefinitions[i++];
                dataBlock.Quantities.Add(CreateBcQuantityDataForArgument(quantity, argument, referenceTime, boundaryCondition.TimeZone));
            }

            bool skipCorrection = BcFile.IsCorrectionType(((IBoundaryCondition)boundaryCondition).DataType) &&
                                  !correctionFile;
            bool skipSignal = BcFile.IsCorrectionType(((IBoundaryCondition)boundaryCondition).DataType) &&
                              correctionFile;

            int componentCount = forcingTypeDefinition.ComponentDefinitions.Count();
            if (skipCorrection || skipSignal)
            {
                componentCount += 2;
            }

            if (!QuantityNameToTypeDictionary.Values.Contains(boundaryCondition.FlowQuantity))
            {
                Log.WarnFormat("Flow quantity {0} not supported by bc file writer", boundaryCondition.FlowQuantity);
            }

            List<string> flowVariables = QuantityNameToTypeDictionary
                                         .Where(kvp => kvp.Value == boundaryCondition.FlowQuantity)
                                         .Select(kvp => kvp.Key)
                                         .ToList();
            int variableCount = flowVariables.Count;
            int componentsPerLayer = componentCount * variableCount;

            var j = 0;

            foreach (IVariable component in function.Components)
            {
                int componentIndex = j % componentCount;

                if (skipSignal && componentIndex < 2 || skipCorrection && componentIndex > 1)
                {
                    j++;
                    continue;
                }

                int variableIndex = (j % componentsPerLayer) / componentCount;
                int layerIndex = (j / componentsPerLayer) + 1; //one-based indexing
                j++;

                string componentString =
                    forcingTypeDefinition.ComponentDefinitions[skipSignal ? componentIndex - 2 : componentIndex];

                string quantityString = string.IsNullOrEmpty(componentString)
                                            ? flowVariables[variableIndex]
                                            : flowVariables[variableIndex] + " " + componentString;
                if (boundaryCondition.DataType == BoundaryConditionDataType.Qh)
                {
                    quantityString = componentString;
                }

                switch (boundaryCondition.FlowQuantity)
                {
                    case FlowBoundaryQuantityType.Tracer:
                        quantityString = quantityString + boundaryCondition.TracerName;
                        break;
                    case FlowBoundaryQuantityType.SedimentConcentration
                        when boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries:
                        quantityString = quantityString + boundaryCondition.SedimentFractionName;
                        break;
                    case FlowBoundaryQuantityType.MorphologyBedLoadTransport
                        when boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries:
                        quantityString = quantityString + component.Name;
                        break;
                }

                dataBlock.Quantities.Add(new BcQuantityData
                {
                    QuantityName = quantityString,
                    Unit = component.Unit.Symbol,
                    VerticalPosition = verticalProfile == null ? null : layerIndex.ToString(),
                    Values = PrintValues(component, null, null).ToList()
                });
            }

            return true;
        }

        private static ForcingTypeDefinition CreateForcingTypeDefinition(FlowBoundaryCondition boundaryCondition,
                                                                         int index, bool correctionFile)
        {
            BoundaryConditionDataType dataType = boundaryCondition.DataType;

            if (dataType == BoundaryConditionDataType.AstroCorrection && !correctionFile)
            {
                dataType = BoundaryConditionDataType.AstroComponents;
            }

            if (dataType == BoundaryConditionDataType.HarmonicCorrection && correctionFile)
            {
                dataType = BoundaryConditionDataType.Harmonics;
            }

            List<ForcingTypeDefinition> forcingTypeDefinitions =
                ForcingTypeDefinitions.Values.Where(ftd => ftd.ForcingType == dataType).ToList();

            ForcingTypeDefinition forcingTypeDefinition;

            // The 2d version comes first, the 3d version afterwards
            if (forcingTypeDefinitions.Count > 1)
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

        private BcQuantityData CreateBcQuantityData(string quantity, IVariable argument, DateTime? referenceTime,
                                                    string unit,
                                                    Func<double, double> converter)
        {
            return new BcQuantityData
            {
                QuantityName = quantity,
                Unit = unit,
                Values = PrintValues(argument, referenceTime, converter).ToList()
            };
        }

        private class ForcingTypeDefinition
        {
            public BoundaryConditionDataType ForcingType;
            public string[] ArgumentDefinitions;
            public string[] ComponentDefinitions;
        }

        private DaoDataBlock GetDaoDataBlock(BcBlockData dataBlock)
        {
            try
            {
                var daoDataBlock = new DaoDataBlock
                {
                    ForcingTypeDefinition = ParseForcingType(dataBlock),
                    VerticalProfileDefinition = ParseDepthLayerDefinition(dataBlock),
                    VerticalInterpolationType = ParseVerticalInterpolationType(dataBlock),
                    InterpolationType = BcQuantityDataParsingHelper.ParseTimeInterpolationType(dataBlock),
                    SeriesIndex = ParseSeriesIndex(dataBlock),
                    Offset = ParseOffset(dataBlock),
                    Factor = ParseFactor(dataBlock)
                };

                if (ExcludedDataTypes?.Contains(daoDataBlock.ForcingTypeDefinition.ForcingType) != true)
                {
                    return daoDataBlock;
                }
                Log.InfoFormat(Resources.BcFileFlowBoundaryDataBuilder_GetDaoDataBlock_File__0___block_starting_at_line__1___skipping_boundary_dataBlock_of_function_type__2__, dataBlock.FilePath, dataBlock.LineNumber, daoDataBlock.ForcingTypeDefinition.ForcingType);
            }
            catch (Exception)
            {
                Log.ErrorFormat(Resources.BcFileFlowBoundaryDataBuilder_GetDaoDataBlock_File__0___block_starting_at_line__1___not_possible_to_parse_boundary_data_block_, dataBlock.FilePath, dataBlock.LineNumber);
            }

            return null;
        }

        private class DaoDataBlock
        {
            public ForcingTypeDefinition ForcingTypeDefinition { get; set; }
            public VerticalProfileDefinition VerticalProfileDefinition { get; set; }
            public VerticalInterpolationType VerticalInterpolationType { get; set; }
            public InterpolationType InterpolationType { get; set; }
            public int SeriesIndex { get; set; }
            public double Offset { get; set; }
            public double Factor { get; set; }
        }
    }
}