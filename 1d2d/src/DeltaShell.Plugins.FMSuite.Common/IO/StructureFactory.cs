
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class StructureFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StructureFactory));
        
        private static readonly Dictionary<Structure2DType, Func<Structure2D, string, DateTime, IStructure>> CreateStructureType = new Dictionary<Structure2DType, Func<Structure2D, string, DateTime, IStructure>>
        {
            {Structure2DType.Pump, CreatePumpCore},
            {Structure2DType.Gate, CreateGateCore},
            {Structure2DType.Weir, CreateSimpleWeirCore},
            {Structure2DType.GeneralStructure, CreateGeneralStructureCore},
            {Structure2DType.LeveeBreach, CreateLeveeBreach}
        };

        /// <summary>
        /// Constructs a <see cref="IStructure1D"/> for the following supported types:
        /// Pump, Weir, Gate
        /// </summary>
        /// <param name="structure2D">The description of the structure.</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        /// <param name="refDate">Reference date for the model, required for time dependent data.</param>
        /// <returns>Returns the constructed structure.</returns>
        public static IStructure CreateStructure(Structure2D structure2D, string path, DateTime refDate, string oldPath = null)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, StructureFactoryValidator.SupportedTypes);

            IStructure structure = null;
            structure = CreateStructureType[structure2D.Structure2DType](structure2D, path, refDate);
            SetCommonStructureData(structure, structure2D, path);

            return structure;
        }

        /// <summary>
        /// Update general structure properties.
        /// </summary>
        /// <param name="structure">Structure to be updated.</param>
        /// <param name="structure2D">Source of data to update <paramref name="structure"/>.</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        private static void SetCommonStructureData(IStructure structure, Structure2D structure2D, string path)
        {
            var property = structure2D.GetProperty(KnownStructureProperties.Name);
            if (property != null)
            {
                structure.Name = property.GetValueAsString();
            }

            property = structure2D.GetProperty(StructureRegion.XCoordinates.Key);
            if (property != null)
            {
                var xCoordinates = DataTypeValueParser.FromString<IList<Double>>(property.GetValueAsString()).ToArray();
                var yCoordinates = DataTypeValueParser.FromString<IList<Double>>(structure2D.GetProperty(StructureRegion.YCoordinates.Key).GetValueAsString());
                var coordinates = new Coordinate[xCoordinates.Length];
                for (var i = 0; i < coordinates.Length; i++)
                    coordinates[i] = new Coordinate(xCoordinates[i], yCoordinates[i]);
                structure.Geometry = coordinates.Length == 1 ? (IGeometry) new Point(coordinates[0]) : (IGeometry)new LineString(coordinates);
            }

            property = structure2D.GetProperty(KnownStructureProperties.PolylineFile);
            if (property != null)
            {
                var polylineFileName = property.GetValueAsString();
                var filePath = FMSuiteFileBase.GetOtherFilePathInSameDirectory(path, polylineFileName);
                var pliFile = new PliFile<Feature2D>();
                structure.Geometry = pliFile.Read(filePath).First().Geometry;
            }

            var structure1D = structure as IStructure1D;
            if (structure1D != null)
            {
                structure1D.Chainage = double.NaN;
            }
        }

        private static void SetTimeSeriesProperty(Structure2D structure2D, 
                                                  DateTime refDate, 
                                                  IStructure1D structure, 
                                                  ITimeSeries timeSeries, 
                                                  TimeSeriesStructurePropertyStringData propertyStringData)
        {
            ModelProperty property = structure2D.GetProperty(propertyStringData.PropertyName);
            if (property == null)
            {
                return;
            }

            var steerable = (Steerable)property.Value;
            switch (steerable.Mode)
            {
                case SteerableMode.ConstantValue:
                    TypeUtils.SetPropertyValue(structure, propertyStringData.UseTimeSeriesProperty, false);
                    TypeUtils.SetPropertyValue(structure, propertyStringData.ConstantValueProperty, steerable.ConstantValue);
                    break;
                case SteerableMode.TimeSeries:
                    TypeUtils.SetPropertyValue(structure, propertyStringData.UseTimeSeriesProperty, true);
                    var filePath = FMSuiteFileBase.GetOtherFilePathInSameDirectory(propertyStringData.Path, steerable.TimeSeriesFilename);
                    var reader = new TimFile();
                    reader.Read(filePath, timeSeries, refDate);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #region Pump

        /// <summary>
        /// Create a pump.
        /// </summary>
        /// <param name="structure">Source of data to create structure with.</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        /// <param name="refDate">Reference data used for <see cref="TimFile"/>s.</param>
        /// <returns>The created pump.</returns>
        public static IPump CreatePump(Structure2D structure, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure, new[] { Structure2DType.Pump });

            var pump = CreatePumpCore(structure, path, refDate);
            SetCommonStructureData(pump, structure, path);

            return pump;
        }

        /// <summary>
        /// Create and set pump related data.
        /// </summary>
        /// <param name="structure">Source of data to create structure with.</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        /// <param name="refDate">Reference data used for <see cref="TimFile"/>s.</param>
        /// <returns>The created pump.</returns>
        private static Pump2D CreatePumpCore(Structure2D structure, string path, DateTime refDate)
        {
            var pump = new Pump2D(true);

            var property = structure.GetProperty(KnownStructureProperties.Capacity);
            if (property != null)
            {
                var steerable = (Steerable)property.Value;
                switch (steerable.Mode)
                {
                    case SteerableMode.ConstantValue:
                        pump.UseCapacityTimeSeries = false;
                        pump.Capacity = steerable.ConstantValue;
                        break;
                    case SteerableMode.TimeSeries:
                        pump.UseCapacityTimeSeries = true;
                        var filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(path, steerable.TimeSeriesFilename);
                        var timFile = new TimFile();
                        timFile.Read(filePath, pump.CapacityTimeSeries, refDate);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (false)
            {
                AddPumpSobekProperties(structure, pump);
            }

            return pump;
        }

        private static void AddPumpSobekProperties(Structure2D structure, Pump2D pump)
        {
            var hasSuctionSideLevels = false;
            var property = structure.GetProperty(KnownStructureProperties.StartSuctionSide);
            if (property != null)
            {
                pump.StartSuction = DataTypeValueParser.FromString<double>(property.GetValueAsString());
                hasSuctionSideLevels = true;
            }
            property = structure.GetProperty(KnownStructureProperties.StopSuctionSide);
            if (property != null)
            {
                pump.StopSuction = DataTypeValueParser.FromString<double>(property.GetValueAsString());
                hasSuctionSideLevels = true;
            }

            var hasDeliverySideLevels = false;
            property = structure.GetProperty(KnownStructureProperties.StartDeliverySide);
            if (property != null)
            {
                pump.StartDelivery = DataTypeValueParser.FromString<double>(property.GetValueAsString());
                hasDeliverySideLevels = true;
            }
            property = structure.GetProperty(KnownStructureProperties.StopDeliverySide);
            if (property != null)
            {
                pump.StopDelivery = DataTypeValueParser.FromString<double>(property.GetValueAsString());
                hasDeliverySideLevels = true;
            }

            if (hasSuctionSideLevels && !hasDeliverySideLevels)
            {
                pump.ControlDirection = PumpControlDirection.SuctionSideControl;
            }
            else if (!hasSuctionSideLevels && hasDeliverySideLevels)
            {
                pump.ControlDirection = PumpControlDirection.DeliverySideControl;
            }
            else
            {
                pump.ControlDirection = PumpControlDirection.SuctionAndDeliverySideControl;
            }

            property = structure.GetProperty(KnownStructureProperties.NrOfReductionFactors);
            if (property != null)
            {
                var reductionLevels = DataTypeValueParser.FromString<int>(property.GetValueAsString());
                if (reductionLevels == 0)
                {
                    // Do nothing, defaults are okey
                }
                else if (reductionLevels == 1)
                {
                    var reductionFactorProperty = structure.GetProperty(KnownStructureProperties.ReductionFactor);
                    pump.ReductionTable[0.0] = DataTypeValueParser.FromString<double>(reductionFactorProperty.GetValueAsString());
                }
                else
                {
                    var headProperty = structure.GetProperty(KnownStructureProperties.Head);
                    var reductionFactorProperty = structure.GetProperty(KnownStructureProperties.ReductionFactor);

                    // Assumes the strings have already been validated:
                    var headValues = DataTypeValueParser.FromString<IList<double>>(headProperty.GetValueAsString());
                    var reductionValues = DataTypeValueParser.FromString<IList<double>>(reductionFactorProperty.GetValueAsString());

                    for (var i = 0; i < headValues.Count; i++)
                    {
                        pump.ReductionTable[headValues[i]] = reductionValues[i];
                    }
                }
            }
        }

        #endregion

        #region Weir

        /// <summary>
        /// Create a weir.
        /// </summary>
        /// <param name="structure2D">Source of data to create structure with.</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        /// <param name="refDate">Reference data used for <see cref="TimFile"/>s.</param>
        /// <returns>The created weir.</returns>
        public static IWeir CreateWeir(Structure2D structure2D, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, new[] { Structure2DType.Weir });

            IWeir structure = null;
            if (structure2D.Structure2DType == Structure2DType.Weir)
            {
                structure = CreateSimpleWeirCore(structure2D, path, refDate);
            }
            SetCommonStructureData(structure, structure2D, path);

            return structure;
        }


        /// <summary>
        /// Create a simple weir.
        /// </summary>
        /// <param name="structure2D">Source of data to create structure with.</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        /// <param name="refDate">Reference data used for <see cref="TimFile"/>s.</param>
        /// <returns>The created simple weir.</returns>
        private static IWeir CreateSimpleWeirCore(Structure2D structure2D, string path, DateTime refDate)
        {
            var weir = CreateWeirCore(structure2D, path, refDate);
            weir.WeirFormula = CreateSimpleWeirFormula(structure2D);

            return weir;
        }

        /// <summary>
        /// Create a simple weir formula
        /// </summary>
        /// <param name="structure2D">Source of data to create the formula with.</param>
        /// <returns>The created formula</returns>
        private static IWeirFormula CreateSimpleWeirFormula(Structure2D structure2D)
        {
            var simpleWeirFormula = new SimpleWeirFormula();

            var property = structure2D.GetProperty(StructureRegion.CorrectionCoeff.Key);
            if (property != null)
            {
                simpleWeirFormula.CorrectionCoefficient = DataTypeValueParser.FromString<double>(property.GetValueAsString());
            }
            return simpleWeirFormula;
        }

        /// <summary>
        /// Create a weir that has a General Structure formula.
        /// </summary>
        /// <param name="structure2D">Source of data to create structure with.</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        /// <param name="refDate">Reference data used for <see cref="TimFile"/>s.</param>
        /// <returns>The created simple weir.</returns>
        private static IWeir CreateGeneralStructureCore(Structure2D structure2D, string path, DateTime refDate)
        {
            var weir = new Weir2D();
            weir.WeirFormula = CreateGeneralStructureWeirFormula(structure2D);

            return weir;
        }

        private static IWeirFormula CreateGeneralStructureWeirFormula(Structure2D structure2D)
        {
            var gsWeirFormula = new GeneralStructureWeirFormula();

            foreach (var property in Enum.GetValues(typeof(KnownGeneralStructureProperties)).Cast<KnownGeneralStructureProperties>())
            {
                var structureproperty = structure2D.GetProperty(property);
                if (structureproperty == null)
                    Log.WarnFormat("Property [{0}] is not supported and is skipped.", property);
                else
                {
                    if (property == KnownGeneralStructureProperties.GateOpeningHorizontalDirection)
                        gsWeirFormula.GateOpeningHorizontalDirection = (GateOpeningDirection)DataTypeValueParser.FromString(structureproperty.GetValueAsString(), typeof(GateOpeningDirection));
                    else
                        gsWeirFormula.SetPropertyValue(property,
                            DataTypeValueParser.FromString<double>(structureproperty.GetValueAsString()));
                }
            }

            return gsWeirFormula;
        }

        /// <summary>
        /// Create a weir.
        /// </summary>
        /// <param name="structure2D">Source of data to create structure with.</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        /// <param name="refDate">Reference data used for <see cref="TimFile"/>s.</param>
        /// <returns>The created weir.</returns>
        private static Weir2D CreateWeirCore(Structure2D structure2D, string path, DateTime refDate)
        {
            var weir = new Weir2D(true);
            var crestWidthProperty = structure2D.GetProperty(StructureRegion.CrestWidth.Key);
            var crestWidthString = crestWidthProperty == null ? null : crestWidthProperty.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                ? 0.0
                : DataTypeValueParser.FromString<double>(crestWidthString);
            
            var Weir2DStringData = new TimeSeriesStructurePropertyStringData(StructureRegion.CrestLevel.Key,
                                                                             path,
                                                                             nameof(weir.UseCrestLevelTimeSeries),
                                                                             nameof(weir.CrestLevel),
                                                                             structure2D.Name);
            
            SetTimeSeriesProperty(structure2D, refDate, weir, weir.CrestLevelTimeSeries, Weir2DStringData);
            var useVelocityHeightProperty = structure2D.GetProperty(StructureRegion.UseVelocityHeight.Key);
            if (useVelocityHeightProperty != null)
            {
                weir.UseVelocityHeight = (bool)useVelocityHeightProperty.Value;
            }
            
            return weir;
        }

        #endregion

        #region Levee breach

        /// <summary>
        /// Create a levee breach
        /// </summary>
        /// <param name="structure2D">Source of data to create structure</param>
        /// <param name="path">Filepath of the <see cref="StructuresFile"/>.</param>
        /// <param name="refDate">Reference data used for <see cref="TimFile"/>.</param>
        /// <returns></returns>
        private static IStructure CreateLeveeBreach(Structure2D structure2D, string path, DateTime refDate)
        {
            var leveeBreach = SetLeveeBreachProperties(structure2D);

            SetLeveeBreachSettings(leveeBreach, structure2D, path, refDate);
            return leveeBreach;
        }

        private static ILeveeBreach SetLeveeBreachProperties(Structure2D structure2D)
        {
            var breachLocationX = GetPropertyValue(structure2D, KnownStructureProperties.BreachLocationX, 0.0);
            var breachLocationY = GetPropertyValue(structure2D, KnownStructureProperties.BreachLocationY, 0.0);
            
            var isWaterLevelStreamActive =  structure2D.Properties.Any(p => p.PropertyDefinition.FilePropertyKey.Equals(StructureRegion.WaterLevelUpstreamLocationX.Key,StringComparison.InvariantCultureIgnoreCase)) &&
                                            structure2D.Properties.Any(p => p.PropertyDefinition.FilePropertyKey.Equals(StructureRegion.WaterLevelUpstreamLocationY.Key, StringComparison.InvariantCultureIgnoreCase)) &&
                                            structure2D.Properties.Any(p => p.PropertyDefinition.FilePropertyKey.Equals(StructureRegion.WaterLevelDownstreamLocationX.Key, StringComparison.InvariantCultureIgnoreCase)) &&
                                            structure2D.Properties.Any(p => p.PropertyDefinition.FilePropertyKey.Equals(StructureRegion.WaterLevelDownstreamLocationY.Key, StringComparison.InvariantCultureIgnoreCase));
            
            var leveeBreach = new LeveeBreach
            {
                BreachLocationX = breachLocationX,
                BreachLocationY = breachLocationY,
            };

            if (isWaterLevelStreamActive)
            {
                var waterLevelUpstreamLocationX =
                    GetPropertyValue(structure2D, StructureRegion.WaterLevelUpstreamLocationX.Key, 0.0);
                var waterLevelUpstreamLocationY =
                    GetPropertyValue(structure2D, StructureRegion.WaterLevelUpstreamLocationY.Key, 0.0);
                var waterLevelDownstreamLocationX =
                    GetPropertyValue(structure2D, StructureRegion.WaterLevelDownstreamLocationX.Key, 0.0);
                var waterLevelDownstreamLocationY =
                    GetPropertyValue(structure2D, StructureRegion.WaterLevelDownstreamLocationY.Key, 0.0);

                leveeBreach.WaterLevelFlowLocationsActive = true;
                leveeBreach.WaterLevelUpstreamLocationX = waterLevelUpstreamLocationX;
                leveeBreach.WaterLevelUpstreamLocationY = waterLevelUpstreamLocationY;
                leveeBreach.WaterLevelDownstreamLocationX = waterLevelDownstreamLocationX;
                leveeBreach.WaterLevelDownstreamLocationY = waterLevelDownstreamLocationY;
            }

            return leveeBreach;
        }

        private static void SetLeveeBreachSettings(ILeveeBreach leveeBreach, Structure2D structure2D, string path, DateTime refDate)
        {
            // Base settings
            var startTime = GetBreachGrowthStartTime(structure2D, refDate);
            var breachGrowthActive = GetPropertyValue(structure2D, KnownStructureProperties.BreachGrowthActivated, true);
            leveeBreach.SetBaseLeveeBreachSettings(startTime, breachGrowthActive);

            // formula
            var algorithmEnumValue = GetPropertyValue(structure2D, KnownStructureProperties.Algorithm, 0);
            var growthFormulaIsDefined = Enum.IsDefined(typeof(LeveeBreachGrowthFormula), algorithmEnumValue);

            if (!growthFormulaIsDefined)
            {
                // return, no use to set the settings
                return;
            }

            // settings 
            leveeBreach.LeveeBreachFormula = (LeveeBreachGrowthFormula)algorithmEnumValue;

            if (leveeBreach.LeveeBreachFormula == LeveeBreachGrowthFormula.VerheijvdKnaap2002)
            {
                SetVerheijVdKnaapSettings(leveeBreach, structure2D);
            }

            if (leveeBreach.LeveeBreachFormula == LeveeBreachGrowthFormula.UserDefinedBreach)
            {
                SetUserDefinedSettings(leveeBreach, structure2D, path, startTime);
            }
        }

        private static DateTime GetBreachGrowthStartTime(Structure2D structure2D, DateTime refDate)
        {
            var startTimeBreachGrowthInSecondsFrom = GetPropertyValue(structure2D, KnownStructureProperties.StartTimeBreachGrowth, 0);
            var timeSpan = new TimeSpan(0, 0, startTimeBreachGrowthInSecondsFrom);
            var startTime = refDate + timeSpan;
            return startTime;
        }

        private static void SetVerheijVdKnaapSettings(ILeveeBreach leveeBreach, Structure2D structure2D)
        {
            var settings = leveeBreach.GetActiveLeveeBreachSettings() as VerheijVdKnaap2002BreachSettings;
            if (settings == null) return;

            settings.InitialCrestLevel = GetPropertyValue(structure2D, KnownStructureProperties.InitialCrestLevel, 0.0);
            settings.MinimumCrestLevel = GetPropertyValue(structure2D, KnownStructureProperties.MinimumCrestLevel, 0.0);
            settings.InitialBreachWidth = GetPropertyValue(structure2D, KnownStructureProperties.InitalBreachWidth, 0.0);
            settings.Factor1Alfa = GetPropertyValue(structure2D, KnownStructureProperties.Factor1, 0.0);
            settings.Factor2Beta = GetPropertyValue(structure2D, KnownStructureProperties.Factor2, 0.0);
            var secondsToReachMinimumCrestLevel = GetPropertyValue(structure2D, KnownStructureProperties.TimeToReachMinimumCrestLevel, 0.0);
            settings.PeriodToReachZmin = new TimeSpan(0, 0, (int)secondsToReachMinimumCrestLevel);
            settings.CriticalFlowVelocity = GetPropertyValue(structure2D, KnownStructureProperties.CriticalFlowVelocity, 0.0);
        }

        private static void SetUserDefinedSettings(ILeveeBreach leveeBreach, Structure2D structure2D, string path, DateTime refDate)
        {
            var settings = leveeBreach.GetActiveLeveeBreachSettings() as UserDefinedBreachSettings;
            if (settings == null) return;

            var timeSeriesFilePath = GetPropertyValue(structure2D, KnownStructureProperties.TimeFilePath, "");
            var filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(path, timeSeriesFilePath);

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Log.WarnFormat($"Unable to import levee breach growth settings from .tim file, {0}", string.IsNullOrWhiteSpace(filePath) ? "the filepath is undefined" : $"the file '{filePath}' does not exist");
                return;
            }
            var timeSeries = UserDefinedBreachConversionHelper.GetFormattedTimeSeries();
            new TimFile().Read(filePath, timeSeries, refDate);

            settings.CreateTableFromTimeSeries(timeSeries);
        }
        
        private static T GetPropertyValue<T>(Structure2D structure2D, string propertyName, T defaultValue)
        {
            var property = structure2D.GetProperty(propertyName);
            var valueString = property?.GetValueAsString();
            var value = string.IsNullOrEmpty(valueString)
                ? defaultValue
                : DataTypeValueParser.FromString<T>(valueString);
            return value;
        }

        #endregion

        #region Gate

        public static IGate CreateGate(Structure2D structure2D, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, new[] { Structure2DType.Gate });

            IGate structure = null;
            if (structure2D.Structure2DType == Structure2DType.Gate)
            {
                structure = CreateGateCore(structure2D, path, refDate);
            }
            SetCommonStructureData(structure, structure2D, path);

            return structure;
        }

        private static IGate CreateGateCore(Structure2D structure2D, string path, DateTime refDate)
        {
            var gate = new Gate2D();
            gate.DoorHeight = DataTypeValueParser.FromString<double>(structure2D.GetProperty(StructureRegion.GateHeight.Key).GetValueAsString());
            var sillWidthProperty = structure2D.GetProperty(StructureRegion.GateCrestWidth.Key);
            var sillWidthString = sillWidthProperty == null ? null : sillWidthProperty.GetValueAsString();
            gate.SillWidth = string.IsNullOrEmpty(sillWidthString) ? 0.0 : DataTypeValueParser.FromString<double>(sillWidthString);
            var openingDirectionProperty = structure2D.GetProperty(StructureRegion.GateHorizontalOpeningDirection.Key);
            var openingDirectionValue = (Enum) DataTypeValueParser.FromString(openingDirectionProperty.GetValueAsString(), openingDirectionProperty.PropertyDefinition.DataType);
            var displayName = openingDirectionValue.GetDisplayName();
            switch (displayName)
            {
                case "symmetric":
                    gate.HorizontalOpeningDirection = GateOpeningDirection.Symmetric;
                    break;
                case "fromLeft":
                    gate.HorizontalOpeningDirection = GateOpeningDirection.FromLeft;
                    break;
                case "fromRight":
                    gate.HorizontalOpeningDirection = GateOpeningDirection.FromRight;
                    break;
                default:
                    throw new ArgumentException($"Could not parse {StructureRegion.GateHorizontalOpeningDirection.Key} of type: " + displayName);
            }

            SetGateTimeSeriesProperties(structure2D, path, refDate, gate);

            return gate;
        }

        private static void SetGateTimeSeriesProperties(Structure2D structure2D, string path, DateTime refDate, Gate2D gate)
        {
            var gateSillLevelStringData = new TimeSeriesStructurePropertyStringData(StructureRegion.GateCrestLevel.Key,
                                                                                    path,
                                                                                    nameof(gate.UseSillLevelTimeSeries),
                                                                                    nameof(gate.SillLevel),
                                                                                    GetGateStructureName(structure2D.Name, StructureRegion.GateCrestLevel.Key));
            SetTimeSeriesProperty(structure2D, refDate, gate, gate.SillLevelTimeSeries, gateSillLevelStringData);

            var gateLowerEdgeLevelStringData = new TimeSeriesStructurePropertyStringData(StructureRegion.GateLowerEdgeLevel.Key,
                                                                                         path,
                                                                                         nameof(gate.UseLowerEdgeLevelTimeSeries),
                                                                                         nameof(gate.LowerEdgeLevel),
                                                                                         GetGateStructureName(structure2D.Name, StructureRegion.GateLowerEdgeLevel.Key));

            SetTimeSeriesProperty(structure2D, refDate, gate, gate.LowerEdgeLevelTimeSeries, gateLowerEdgeLevelStringData);

            var gateOpeningWidthStringData = new TimeSeriesStructurePropertyStringData(StructureRegion.GateOpeningWidth.Key,
                                                                                       path,
                                                                                       nameof(gate.UseOpeningWidthTimeSeries),
                                                                                       nameof(gate.OpeningWidth),
                                                                                       GetGateStructureName(structure2D.Name, StructureRegion.GateOpeningWidth.Key));

            SetTimeSeriesProperty(structure2D, refDate, gate, gate.OpeningWidthTimeSeries, gateOpeningWidthStringData);
        }

        private static string GetGateStructureName(string structure2DName, string key)
        {
            return $"{structure2DName}_{key}";
        }

        private sealed class TimeSeriesStructurePropertyStringData
        {
            public string PropertyName { get; }
            public string Path { get; }
            public string UseTimeSeriesProperty { get; }
            public string ConstantValueProperty { get; }
            public string StructureName { get; }
            
            public TimeSeriesStructurePropertyStringData(string propertyName, 
                                                         string path, 
                                                         string useTimeSeriesProperty, 
                                                         string constantValueProperty, 
                                                         string structureName)
            {
                PropertyName = propertyName;
                Path = path;
                UseTimeSeriesProperty = useTimeSeriesProperty;
                ConstantValueProperty = constantValueProperty;
                StructureName = structureName;
            }
        }

        #endregion
    }
}