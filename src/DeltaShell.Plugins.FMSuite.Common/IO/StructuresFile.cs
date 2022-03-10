using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    /// <summary>
    /// A file containing structure definitions following the 3Di 'delft .ini' format.
    /// </summary>
    public class StructuresFile : FMSuiteFileBase, IFeature2DFileBase<IStructure>
    {
        private const string StructureCategoryName = "structure";
        private static readonly ILog Log = LogManager.GetLogger(typeof(StructuresFile));

        public StructureSchema<ModelPropertyDefinition> StructureSchema { private get; set; }

        public DateTime ReferenceDate { private get; set; }

        private string TimFolder { get; set; }

        public IList<IStructure> Read(string path)
        {
            return
                ReadStructures2D(path)
                    .Select(s => ConvertStructure(s, path))
                    .Where(s => s != null)
                    .ToList();
        }

        public IList<IStructure> CopyFileAndRead(string filePath, string oldFilePath)
        {
            return
                ReadStructures2D(filePath)
                    .Select(s => ConvertStructure(s, filePath, oldFilePath))
                    .Where(s => s != null)
                    .ToList();
        }

        public IEnumerable<Structure2D> ReadStructures2D(string filePath)
        {
            var categories = new DelftIniReader().ReadDelftIniFile(filePath);
            if(!((IEnumerable<IDelftIniCategory>) categories).Any(c => c.ValidGeneralRegion(GeneralRegion.StructureDefinitionsMajorVersion,
                GeneralRegion.StructureDefinitionsMinorVersion, GeneralRegion.FileTypeName.StructureDefinition))) yield break;

            foreach (var category in categories.Where(c => c.ReadProperty<string>(StructureRegion.BranchId.Key, true)  == null)) // only write 2d features
            {
                // Filter out unexpected .ini categories:
                if (category.Name.ToLower() != StructureCategoryName.ToLower())
                {
                    if(!string.Equals(category.Name, GeneralRegion.IniHeader, StringComparison.CurrentCultureIgnoreCase))
                        Log.WarnFormat("Category [{0}] not supported for structures and is skipped.", category.Name);
                    continue;
                }

                // TODO: Check for potentially other required properties:
                // Read required 'type' property:
                var structureTypeProperty =
                    category.Properties.FirstOrDefault(p => p.Name == KnownStructureProperties.Type);
                if (structureTypeProperty == null)
                {
                    Log.WarnFormat("Obligated property '{0}' expected but is missing; Structure is skipped.",
                                   KnownStructureProperties.Type);
                    continue;
                }

                var structure2D = CreateStructure2D(StructureSchema, structureTypeProperty.Value, category, filePath);
                var errorMessage = StructureFactoryValidator.Validate(structure2D);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Log.WarnFormat("Failed to convert .ini structure definition to actual structure: {0}.",
                                    errorMessage);
                    continue;
                }

                yield return structure2D;
            }
        }

        public void Write(string path, IEnumerable<IStructure> features)
        {
            var supportedStructures = GetSupportedStructures(features);
            var delftIniCategories = supportedStructures.Select(s => CreateDelftIniCategory(s, path, ReferenceDate));

            new DelftIniWriter().WriteDelftIniFile(delftIniCategories, path);
        }

        private static IEnumerable<IStructure> GetSupportedStructures(IEnumerable<IStructure> structures)
        {
            var list = new List<IStructure>();
            foreach (var structure in structures)
            {
                if (structure is IPump || structure is IWeir || structure is IGate || structure is ILeveeBreach)
                {
                    list.Add(structure);
                }
                else
                {
                    Log.ErrorFormat("Structure '{0}' is of unsupported type and therefore skipped.", structure.Name);
                }
            }
            return list;
        }

        private DelftIniCategory CreateDelftIniCategory(IStructure structure, string filePath, DateTime refDate)
        {
            var delftIniCategory = new DelftIniCategory(StructureCategoryName);

            foreach (var property in CreateDelftIniProperties(structure, filePath, refDate))
            {
                delftIniCategory.Properties.Add(property);
            }

            return delftIniCategory;
        }

        private IEnumerable<DelftIniProperty> CreateDelftIniProperties(IStructure structure, string filePath, DateTime refDate)
        {
            var properties = new List<DelftIniProperty>();

            var structureType = DetermineType(structure);

            properties.Add(ConstructProperty(KnownStructureProperties.Type, structureType, structureType));
            properties.Add(ConstructProperty(KnownStructureProperties.Name, structure.Name, structureType));
            properties.AddRange(ConstructGeometryProperties(structure, structureType, filePath));
            properties.AddRange(ConstructStructureProperties(structure, structureType, filePath, refDate));

            return properties;
        }

        private IEnumerable<DelftIniProperty> ConstructGeometryProperties(IStructure structure, string structureType, string filePath)
        {
            var point = structure.Geometry as IPoint;
            if (point != null)
            {
                yield return ConstructProperty(KnownStructureProperties.X, point.X, structureType);
                yield return ConstructProperty(KnownStructureProperties.Y, point.Y, structureType);
            }

            var lineString = structure.Geometry as ILineString;
            if (lineString != null)
            {
                var charArray = structure.Name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray();
                if (charArray.Any())
                {
                    var pliFileName = String.Format("{0}.pli", new string(charArray));

                    yield return ConstructProperty(KnownStructureProperties.PolylineFile, pliFileName, structureType);
                    WritePliFile(GetOtherFilePathInSameDirectory(filePath, pliFileName), structure);
                }
                else
                {
                    Log.ErrorFormat("Structure with invalid name {0} could not be serialized to *.pli file.",
                        structure.Name);
                }
            }

            if (structure.Geometry != null && !(structure.Geometry is ILineString || structure.Geometry is IPoint))
            {
                Log.ErrorFormat("Geometry type '{0}' for structure '{1}' not supported and therefore not written.", structure.Geometry.GetType(), structure.Name);
            }
        }

        private DelftIniProperty ConstructProperty(string propertyName, object value, string structureType)
        {
            var definition = StructureSchema.GetDefinition(structureType, propertyName);
            var delftIniProperty = new DelftIniProperty
            {
                Name = definition.FilePropertyName,
                Value = DataTypeValueParser.ToString(value, value is ICollection ? typeof(IList<double>) : value.GetType()),
                Comment = definition.Description
            };
            return delftIniProperty;
        }

        private IEnumerable<DelftIniProperty> ConstructStructureProperties(IStructure structure, string structureType, string path, DateTime refDate)
        {
            var pump = structure as IPump;
            if (pump != null)
                return ConstructPumpProperties(pump, structureType, path, refDate);

            var weir = structure as IWeir;
            if (weir != null)
                return ConstructWeirProperties(weir, structureType, path, refDate);

            var gate = structure as IGate;
            if (gate != null)
                return ConstructGateProperties(gate, structureType, path, refDate);

            var leveeBreach = structure as ILeveeBreach;
            if (leveeBreach != null)
                return ConstructLeveeBreachProperties(leveeBreach, structureType, path, refDate);
            throw new NotImplementedException();
        }

        private IEnumerable<DelftIniProperty> ConstructPumpProperties(IStructure1D structure, string structureType, string path, DateTime refDate)
        {
            var pump = (IPump)structure;
            var properties = new List<DelftIniProperty>();

            if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
            {
                var timeFileName = ConstructTimeFilePath(pump, KnownStructureProperties.Capacity);
                properties.Add(ConstructProperty(KnownStructureProperties.Capacity, timeFileName, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFileName), pump.CapacityTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.Capacity, pump.Capacity, structureType));
            }

            if (false)
            {
                AddControlDirectionRelatedProperties(pump, properties, structureType);
                AddReductionTableRelatedProperties(pump, properties, structureType);
            }

            return properties;
        }

        private IEnumerable<DelftIniProperty> ConstructWeirProperties(IStructure1D structure, string structureType, string path, DateTime refDate)
        {
            var weir = (IWeir)structure;
            var weirFormula = weir.WeirFormula;
            if (weirFormula is SimpleWeirFormula) return ConstructSimpleWeirProperties(weir, path, structureType, refDate);
            if (weirFormula is GeneralStructureWeirFormula) return ConstructGeneralStructureProperties(weir);
            throw new NotImplementedException();
        }

        private IEnumerable<DelftIniProperty> ConstructGateProperties(IStructure1D structure, string structureType, string path, DateTime refDate)
        {
            var gate = (IGate)structure;
            var properties = new List<DelftIniProperty>();

            if (gate.UseSillLevelTimeSeries)
            {
                var timeFilePath = ConstructTimeFilePath(gate, StructureRegion.GateCrestLevel.Key);
                properties.Add(ConstructProperty(StructureRegion.GateCrestLevel.Key, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), gate.SillLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(StructureRegion.GateCrestLevel.Key, gate.SillLevel, structureType));
            }

            if (gate.UseLowerEdgeLevelTimeSeries)
            {
                var timeFilePath = ConstructTimeFilePath(gate, KnownStructureProperties.GateLowerEdgeLevel);
                properties.Add(ConstructProperty(KnownStructureProperties.GateLowerEdgeLevel, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), gate.LowerEdgeLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateLowerEdgeLevel, gate.LowerEdgeLevel, structureType));
            }


            if (gate.UseOpeningWidthTimeSeries)
            {
                var timeFilePath = ConstructTimeFilePath(gate, KnownStructureProperties.GateOpeningWidth);
                properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningWidth, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), gate.OpeningWidthTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningWidth, gate.OpeningWidth, structureType));
            }

            properties.Add(ConstructProperty(KnownStructureProperties.GateDoorHeight, gate.DoorHeight, structureType));

            // switch the horizontal direction, because enums aren't used very nicely in the csv file (structure definition).
            string horizontalDirection;
            switch (gate.HorizontalOpeningDirection)
            {
                case GateOpeningDirection.Symmetric:
                    horizontalDirection = "symmetric"; break;
                case GateOpeningDirection.FromLeft:
                    horizontalDirection = "from_left"; break;
                case GateOpeningDirection.FromRight:
                    horizontalDirection = "from_right"; break;
                default:
                    throw new ArgumentException("We can't write " + gate.HorizontalOpeningDirection);
            }
            properties.Add(ConstructProperty(KnownStructureProperties.GateHorizontalOpeningDirection, horizontalDirection, structureType));
            if (gate.SillWidth > 0.0)
            {
                properties.Add(ConstructProperty(StructureRegion.CrestWidth.Key, gate.SillWidth, structureType));
            }
            return properties;
        }

        private IEnumerable<DelftIniProperty> ConstructLeveeBreachProperties(IStructure structure, string structureType, string path, DateTime refDate)
        {
            var leveeBreach = structure as ILeveeBreach;
            var properties = new List<DelftIniProperty>();
            if (leveeBreach == null) return properties;

            var settings = leveeBreach.GetActiveLeveeBreachSettings();
            if (settings == null) return properties;

            // general properties
            properties.Add(ConstructProperty(KnownStructureProperties.BreachLocationX, leveeBreach.BreachLocationX, structureType));
            properties.Add(ConstructProperty(KnownStructureProperties.BreachLocationY, leveeBreach.BreachLocationY, structureType));
            var secondsSinceRefDate = (int)(settings.StartTimeBreachGrowth - refDate).TotalSeconds;
            properties.Add(ConstructProperty(KnownStructureProperties.StartTimeBreachGrowth, secondsSinceRefDate, structureType));
            properties.Add(ConstructProperty(KnownStructureProperties.BreachGrowthActivated, settings.BreachGrowthActive, structureType));

            properties.Add(ConstructProperty(StructureRegion.WaterLevelUpstreamLocationX.Key, leveeBreach.WaterLevelUpstreamLocationX, structureType));
            properties.Add(ConstructProperty(StructureRegion.WaterLevelUpstreamLocationY.Key, leveeBreach.WaterLevelUpstreamLocationY, structureType));
            properties.Add(ConstructProperty(StructureRegion.WaterLevelDownstreamLocationX.Key, leveeBreach.WaterLevelDownstreamLocationX, structureType));
            properties.Add(ConstructProperty(StructureRegion.WaterLevelDownstreamLocationY.Key, leveeBreach.WaterLevelDownstreamLocationY, structureType));
        
            
            if (!settings.BreachGrowthActive) return properties;
            
            // specific properties
            properties.Add(ConstructProperty(KnownStructureProperties.Algorithm, (int)leveeBreach.LeveeBreachFormula, structureType));
            var breachSettings = leveeBreach.GetActiveLeveeBreachSettings() as VerheijVdKnaap2002BreachSettings;
            var useVerheij = breachSettings != null;
            if (useVerheij)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.InitialCrestLevel, breachSettings.InitialCrestLevel, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.MinimumCrestLevel, breachSettings.MinimumCrestLevel, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.InitalBreachWidth, breachSettings.InitialBreachWidth, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.TimeToReachMinimumCrestLevel, breachSettings.PeriodToReachZmin, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.Factor1, breachSettings.Factor1Alfa, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.Factor2, breachSettings.Factor2Beta, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.CriticalFlowVelocity, breachSettings.CriticalFlowVelocity, structureType));
            }

            // Write tm file for table
            var userDefinedSettings = settings as UserDefinedBreachSettings;
            if (userDefinedSettings != null)
            {
                var timeSeries = userDefinedSettings.CreateTimeSeriesFromTable();
                var timeFilePath = ConstructTimeFilePath(leveeBreach, KnownStructureProperties.TimeFilePath);
                properties.Add(ConstructProperty(KnownStructureProperties.TimeFilePath, timeFilePath, structureType));
                var commentLines = new List<string> { "Time entries are defined in minutes, relative to the breach growth start" };
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), timeSeries, settings.StartTimeBreachGrowth, commentLines);
            }

            return properties;
        }

        private string ConstructTimeFilePath(IStructure structure, string propertyName)
        {
            var filePath = String.Format("{0}_{1}.tim", structure.Name, propertyName);
            if (TimFolder != null)
            {
                filePath = Path.Combine(TimFolder, filePath);
            }
            return filePath;
        }

        public static void WriteStructures2D(string path, IEnumerable<Structure2D> structures)
        {
            new DelftIniWriter().WriteDelftIniFile(structures.Select(CreateDelftIniCategory), path);
        }

        private IStructure ConvertStructure(Structure2D structure, string filePath, string oldFilePath = null)
        {
            try
            {
                if (oldFilePath != null && !filePath.Equals(oldFilePath))
                    CopyPolylineFile(structure.GetProperty(KnownStructureProperties.PolylineFile).GetValueAsString(), Path.GetDirectoryName(filePath), Path.GetDirectoryName(oldFilePath));
                return StructureFactory.CreateStructure(structure, filePath, ReferenceDate, oldFilePath);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is ArgumentException || e is FileNotFoundException ||
                    e is DirectoryNotFoundException || e is IOException || e is OutOfMemoryException ||
                    e is FormatException)
                {
                    Log.WarnFormat("Failed to convert .ini structure definition '{0}' to actual structure: {1}.",
                                    structure.Name, e.Message);
                    return null;
                }

                // Unexpected Exception, don't handle:
                throw;
            }
        }

        /// <summary>
        /// oldDirectory and newDirectory point to locations of a structures file (.ini) from which an IStructure object needs to be created.
        /// Whenever oldDirectory and newDirectory are pointing to different directories, a polyline file (.pli) will be copied from oldDirectory to newDirectory.
        /// </summary>
        /// <param name="polylineFileName">Name with extension of the referred polyline file.</param>
        /// <param name="newDirectory">The directory to which the polyline file needs to be copied.</param>
        /// <param name="oldDirectory">The directory from which the polyline file need to be copied.</param>
        private static void CopyPolylineFile(string polylineFileName, string newDirectory, string oldDirectory)
        {
            var polylineFilePath = Path.Combine(oldDirectory, polylineFileName);
            File.Copy(polylineFilePath, Path.Combine(newDirectory, polylineFileName));
        }

        private static DelftIniCategory CreateDelftIniCategory(Structure2D structure)
        {
            var delftIniCategory = new DelftIniCategory(StructureCategoryName);

            foreach (var property in structure.Properties)
            {
                if (property.PropertyDefinition.FilePropertyName == KnownStructureProperties.GateSillWidth &&
                    DataTypeValueParser.FromString<double>(property.GetValueAsString()) <= 0.0)
                {
                    continue;
                }
                if (property.PropertyDefinition.FilePropertyName == KnownStructureProperties.CrestLevel &&
                    DataTypeValueParser.FromString<double>(property.GetValueAsString()) <= 0.0)
                {
                    continue;
                }
                delftIniCategory.Properties.Add(new DelftIniProperty
                {
                    Name = property.PropertyDefinition.FilePropertyName,
                    Value = property.GetValueAsString(),
                    Comment = property.PropertyDefinition.Description
                });
            }

            return delftIniCategory;
        }

        #region Sobek Structure to DelftIni related methods:

        private IEnumerable<DelftIniProperty> ConstructGeneralStructureProperties(IStructure1D structure)
        {
            var structureGenerator = new DefinitionGeneratorStructureGeneralStructure();
            var generalStructureCategory = structureGenerator.CreateStructureRegion(structure);
            return generalStructureCategory.Properties;
        }

        private IEnumerable<DelftIniProperty> ConstructSimpleWeirProperties(IStructure1D structure, string path, string structureType, DateTime refDate)
        {
            var weir = (IWeir)structure;
            var properties = new List<DelftIniProperty>();

            if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
            {
                var timeFilePath = ConstructTimeFilePath(weir, StructureRegion.CrestLevel.Key);
                properties.Add(ConstructProperty(StructureRegion.CrestLevel.Key, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), weir.CrestLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, structureType));
            }

            if (weir.CrestWidth > 0)
            {
                properties.Add(ConstructProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, structureType));
            }

            var formula = (SimpleWeirFormula)((IWeir)structure).WeirFormula;
            properties.Add(ConstructProperty(StructureRegion.CorrectionCoeff.Key, formula.CorrectionCoefficient, structureType));
            return properties;
        }

        private void AddReductionTableRelatedProperties(IPump pump, List<DelftIniProperty> properties, string structureType)
        {
            if (pump.ReductionTable.Arguments[0].Values.Count == 0)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, 0, structureType));
            }
            else if (pump.ReductionTable.Arguments[0].Values.Count == 1)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, 1, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.ReductionFactor, (double)pump.ReductionTable.Components[0].DefaultValue, structureType));
            }
            else
            {
                var count = pump.ReductionTable.Arguments[0].Values.Count;
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, count, structureType));
                var headValues = pump.ReductionTable.Arguments[0].Values.OfType<double>().ToArray();
                var reductionValues = pump.ReductionTable.Components[0].Values.OfType<double>().ToArray();

                properties.Add(ConstructProperty(KnownStructureProperties.Head, headValues, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.ReductionFactor, reductionValues, structureType));
            }
        }

        private void AddControlDirectionRelatedProperties(IPump pump, ICollection<DelftIniProperty> properties, string structureType)
        {
            switch (pump.ControlDirection)
            {
                case PumpControlDirection.DeliverySideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartDeliverySide, pump.StartDelivery, structureType));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopDeliverySide, pump.StopDelivery, structureType));
                    break;
                case PumpControlDirection.SuctionAndDeliverySideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartDeliverySide, pump.StartDelivery, structureType));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopDeliverySide, pump.StopDelivery, structureType));

                    properties.Add(ConstructProperty(KnownStructureProperties.StartSuctionSide, pump.StartSuction, structureType));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopSuctionSide, pump.StopSuction, structureType));
                    break;
                case PumpControlDirection.SuctionSideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartSuctionSide, pump.StartSuction, structureType));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopSuctionSide, pump.StopSuction, structureType));
                    break;
            }
        }

        private static string DetermineType(IStructure structure)
        {
            if (structure is IPump) return StructureRegion.StructureTypeName.Pump;
            if (structure is IGate) return StructureRegion.StructureTypeName.Gate;

            var weir = structure as IWeir;
            if (weir != null)
            {
                if (weir.WeirFormula is SimpleWeirFormula) return StructureRegion.StructureTypeName.Weir;
                if (weir.WeirFormula is GeneralStructureWeirFormula) return StructureRegion.StructureTypeName.GeneralStructure;
            }

            var leveeBreach = structure as ILeveeBreach;
            if (leveeBreach != null) return StructureRegion.StructureTypeName.LeveeBreach;

            throw new NotImplementedException();
        }

        #endregion

        #region FileWriting

        private static void WritePliFile(string pliFilePath, IStructure structure)
        {
            var pliFile = new PliFile<Feature2D>();
            pliFile.Write(pliFilePath, new[]{new Feature2D
            {
                Name = structure.Name,
                Geometry = structure.Geometry
            }});
        }

        private static void WriteTimeFile(string filePath, IFunction capacityTimeSeries, DateTime refDate, IEnumerable<string> commentLines = null)
        {
            var timeFile = new TimFile();
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            timeFile.Write(filePath, capacityTimeSeries, refDate, commentLines);
        }
        #endregion

        private Structure2D CreateStructure2D(StructureSchema<ModelPropertyDefinition> schema, string structureType,
                                                     DelftIniCategory category, string filePath)
        {
            var newStructure = new Structure2D(structureType);

            foreach (var property in category.Properties)
            {
                var modelPropertyDefinition = schema.GetDefinition(structureType, property.Name);
                if (modelPropertyDefinition == null)
                {
                    Log.WarnFormat("Property '{0}' not supported for structures of type '{1}' and is skipped. (Line {2} of file {3})",
                                   property.Name, structureType, property.LineNumber, filePath);
                    continue;
                }
                try
                {
                    var structureProperty = new StructureProperty(modelPropertyDefinition, property.Value);
                    var propertyValue = structureProperty.Value as Steerable;
                    if (propertyValue != null && propertyValue.Mode == SteerableMode.TimeSeries)
                    {
                        var directory = Path.GetDirectoryName(propertyValue.TimeSeriesFilename);
                        if (directory != TimFolder)
                        {
                            if (string.IsNullOrEmpty(directory) && TimFolder != null)
                            {
                                Log.WarnFormat(
                                    "Structure time series {0} will be written to the time series folder {1}",
                                    propertyValue.TimeSeriesFilename, TimFolder);
                            }
                            else
                            {
                                if (TimFolder != null)
                                {
                                    Log.WarnFormat(
                                        "Replacing structure time series folder {0} with {1}... all structure time series will be written to this folder",
                                        TimFolder, directory);
                                }
                                if (!string.IsNullOrEmpty(directory))
                                {
                                    TimFolder = directory;
                                }
                            }
                        }
                    }
                    newStructure.Properties.Add(structureProperty);
                }
                catch (FormatException e)
                {
                    throw new FormatException(String.Format("An invalid value{0} was encountered (expected {1}) for property '{2}' on line {3} of file {4}",
                                                            e.InnerException is OverflowException ? " (too large/small)" : "",
                                                            GetValueTypeDescription(modelPropertyDefinition.DataType, modelPropertyDefinition),
                                                            property.Name, property.LineNumber, filePath), e);
                }
            }

            return newStructure;
        }

        private static object GetValueTypeDescription(Type dataType, ModelPropertyDefinition modelPropertyDefinition)
        {
            if (dataType == typeof(int)) return "a whole number";
            if (dataType == typeof(double)) return "a number";
            if (dataType == typeof(IList<double>)) return "a series of space separated numbers";
            if (dataType == typeof(TimeSpan)) return "a time span in seconds";
            if (dataType == typeof(bool)) return "a '1' or '0'";
            if (dataType == typeof(DateTime)) return "a date in yyyyMMdd or yyyyMMdHHmmss format";
            if (dataType == typeof(Steerable)) return "a number or a filepath to a time series";
            if (dataType.IsEnum) return "Any of the following values: " + String.Join(", ", Enum.GetValues(modelPropertyDefinition.DataType));

            throw new NotImplementedException();
        }

        public static void ParseStructure(FMSuiteFileBase fmSuiteFileBase)
        {
            throw new NotImplementedException();
        }
    }
}