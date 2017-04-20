using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    /// <summary>
    /// A file containing structure definitions following the 3Di 'delft .ini' format.
    /// </summary>z
    public class StructuresFile : FMSuiteFileBase, IFeature2DFileBase<IStructure>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (StructuresFile));

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

        public IEnumerable<Structure2D> ReadStructures2D(string path)
        {
            var categories = new DelftIniReader().ReadDelftIniFile(path);

            foreach (var category in categories)
            {
                // Filter out unexpected .ini categories:
                if (category.Name != "structure")
                {
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

                var structure2D = CreateStructure2D(StructureSchema, structureTypeProperty.Value, category, path);
                var errorMessage = StructureFactoryValidator.Validate(structure2D);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Log.ErrorFormat("Failed to convert .ini structure definition to actual structure: {0}.",
                                    errorMessage);
                    continue;
                }

                yield return structure2D;
            }
        }

        public void Write(string path, IEnumerable<IStructure> structures)
        {
            new DelftIniWriter().WriteDelftIniFile(
                GetSupportedStructures(structures)
                    .Select(s => CreateDelftIniCategory(s, StructureSchema, path, ReferenceDate)), path);
        }

        public static void WriteStructures2D(string path, IEnumerable<Structure2D> structures)
        {
            new DelftIniWriter().WriteDelftIniFile(structures.Select(CreateDelftIniCategory),path);
        }

        private IStructure ConvertStructure(Structure2D structure, string path)
        {
            try
            {
                return StructureFactory.CreateStructure(structure, path, ReferenceDate);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is ArgumentException || e is FileNotFoundException ||
                    e is DirectoryNotFoundException || e is IOException || e is OutOfMemoryException ||
                    e is FormatException)
                {
                    Log.ErrorFormat("Failed to convert .ini structure definition '{0}' to actual structure: {1}.",
                                    structure.Name, e.Message);
                    return null;
                }

                // Unexpected Exception, don't handle:
                throw;
            }
        }

        private static IEnumerable<IStructure> GetSupportedStructures(IEnumerable<IStructure> structures)
        {
            var list = new List<IStructure>();
            foreach (var structure in structures)
            {
                if (structure is IPump || structure is IWeir || structure is IGate)
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
        
        private static DelftIniCategory CreateDelftIniCategory(Structure2D structure)
        {
            var delftIniCategory = new DelftIniCategory("structure");

            foreach (var property in structure.Properties)
            {
                if (property.PropertyDefinition.FilePropertyName == KnownStructureProperties.GateSillWidth &&
                    FMParser.FromString<double>(property.GetValueAsString()) <= 0.0)
                {
                    continue;
                }
                if (property.PropertyDefinition.FilePropertyName == KnownStructureProperties.CrestLevel &&
                    FMParser.FromString<double>(property.GetValueAsString()) <= 0.0)
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

        private DelftIniCategory CreateDelftIniCategory(IStructure structure,
                                                               StructureSchema<ModelPropertyDefinition> schema,
                                                               string path, DateTime refDate)
        {
            var delftIniCategory = new DelftIniCategory("structure");

            foreach (var property in CreateDelftIniProperties(structure, schema, path, refDate))
            {
                delftIniCategory.Properties.Add(property);
            }

            return delftIniCategory;
        }

        #region Sobek Structure to DelftIni related methods:

        private IEnumerable<DelftIniProperty> CreateDelftIniProperties(IStructure structure, StructureSchema<ModelPropertyDefinition> schema, string path, DateTime refDate)
        {
            var properties = new List<DelftIniProperty>();

            var type = DetermineType(structure);

            properties.Add(ConstructProperty(KnownStructureProperties.Type, type, schema, type));
            properties.Add(ConstructProperty(KnownStructureProperties.Name, structure.Name, schema, type));
            properties.AddRange(ConstructGeometryProperties(structure, schema, type, path));
            properties.AddRange(ConstructStructureProperties(structure, schema, type, path, refDate));

            return properties;
        }

        private static IEnumerable<DelftIniProperty> ConstructGeometryProperties(IStructure structure, StructureSchema<ModelPropertyDefinition> schema, string type, string path)
        {
            var point = structure.Geometry as IPoint;
            if (point != null)
            {
                yield return ConstructProperty(KnownStructureProperties.X, point.X, schema, type);
                yield return ConstructProperty(KnownStructureProperties.Y, point.Y, schema, type);
            }

            var lineString = structure.Geometry as ILineString;
            if (lineString != null)
            {
                var charArray = structure.Name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray();
                if (charArray.Any())
                {
                    var pliFileName = String.Format("{0}.pli", new string(charArray));

                    yield return ConstructProperty(KnownStructureProperties.PolylineFile, pliFileName, schema, type);
                    WritePliFile(GetOtherFilePathInSameDirectory(path, pliFileName), structure);
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

        private static void WritePliFile(string pliFilePath, IStructure structure)
        {
            var pliFile = new PliFile<Feature2D>();
            pliFile.Write(pliFilePath, new[]{new Feature2D
                {
                    Name = structure.Name,
                    Geometry = structure.Geometry
                }});
        }

        private IEnumerable<DelftIniProperty> ConstructStructureProperties(IStructure structure, StructureSchema<ModelPropertyDefinition> schema, string type, string path, DateTime refDate)
        {
            if (type == "pump")
            {
                return ConstructPumpProperties(structure, schema, type, path, refDate);
            }
            if (type == "weir")
            {
                return CreateWeirCoreProperties(structure, schema, type, path, refDate);
            }
            if (type == "gate")
            {
                return ConstructGateProperties(structure, schema, type, path, refDate);
            }
            throw new NotImplementedException();
        }

        private IEnumerable<DelftIniProperty> ConstructPumpProperties(IStructure structure, StructureSchema<ModelPropertyDefinition> schema, string type, string path, DateTime refDate)
        {
            var pump = (IPump)structure;
            var properties = new List<DelftIniProperty>();

            if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
            {
                var fileName = String.Format("{0}_{1}.tim", pump.Name, KnownStructureProperties.Capacity);
                if (TimFolder != null)
                {
                    fileName = Path.Combine(TimFolder, fileName);
                }
                properties.Add(ConstructProperty(KnownStructureProperties.Capacity, fileName, schema, type));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, fileName), pump.CapacityTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.Capacity, pump.Capacity, schema, type));
            }

            if (Pump.SupportSobekPumpPropertiesInFM)
            {
                AddControlDirectionRelatedProperties(schema, type, pump, properties);
                AddReductionTableRelatedProperties(schema, type, pump, properties);
            }

            return properties;
        }

        private static void WriteTimeFile(string filePath, IFunction capacityTimeSeries, DateTime refDate)
        {
            var timFile = new TimFile();
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            timFile.Write(filePath, capacityTimeSeries, refDate);
        }

        private IEnumerable<DelftIniProperty> CreateWeirCoreProperties(IStructure structure, StructureSchema<ModelPropertyDefinition> schema, string type, string path, DateTime refDate)
        {
            var weir = (IWeir) structure;
            var properties = new List<DelftIniProperty>();

            if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
            {
                var fileName = String.Format("{0}_{1}.tim", weir.Name, KnownStructureProperties.CrestLevel);
                if (TimFolder != null)
                {
                    fileName = Path.Combine(TimFolder, fileName);
                }
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel, fileName, schema, type));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, fileName), weir.CrestLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel, weir.CrestLevel, schema, type));
            }

            if (weir.CrestWidth > 0)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.CrestWidth, weir.CrestWidth, schema, type));
            }

            var formula = (SimpleWeirFormula)((IWeir)structure).WeirFormula;
            properties.Add(ConstructProperty(KnownStructureProperties.LateralContractionCoefficient, formula.LateralContraction, schema, type));
            return properties;
        }

        private IEnumerable<DelftIniProperty> ConstructGateProperties(IStructure structure, StructureSchema<ModelPropertyDefinition> schema, string type, string path, DateTime refDate)
        {
            var gate = (IGate)structure;
            var properties = new List<DelftIniProperty>();

            if (gate.UseSillLevelTimeSeries)
            {
                var fileName = String.Format("{0}_{1}.tim", gate.Name, KnownStructureProperties.GateSillLevel);
                if (TimFolder != null)
                {
                    fileName = Path.Combine(TimFolder, fileName);
                }
                properties.Add(ConstructProperty(KnownStructureProperties.GateSillLevel, fileName, schema, type));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, fileName), gate.SillLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateSillLevel, gate.SillLevel, schema, type));
            }

            if (gate.UseLowerEdgeLevelTimeSeries)
            {
                var fileName = String.Format("{0}_{1}.tim", gate.Name, KnownStructureProperties.GateLowerEdgeLevel);
                if (TimFolder != null)
                {
                    fileName = Path.Combine(TimFolder, fileName);
                }
                properties.Add(ConstructProperty(KnownStructureProperties.GateLowerEdgeLevel, fileName, schema, type));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, fileName), gate.LowerEdgeLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateLowerEdgeLevel, gate.LowerEdgeLevel, schema, type));
            }


            if (gate.UseOpeningWidthTimeSeries)
            {
                var fileName = String.Format("{0}_{1}.tim", gate.Name, KnownStructureProperties.GateOpeningWidth);
                if (TimFolder != null)
                {
                    fileName = Path.Combine(TimFolder, fileName);
                }
                properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningWidth, fileName, schema, type));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, fileName), gate.OpeningWidthTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningWidth, gate.OpeningWidth, schema, type));
            }

            properties.Add(ConstructProperty(KnownStructureProperties.GateDoorHeight, gate.DoorHeight, schema, type));

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
            properties.Add(ConstructProperty(KnownStructureProperties.GateHorizontalOpeningDirection, horizontalDirection, schema, type));
            if (gate.SillWidth > 0.0)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateSillWidth, gate.SillWidth, schema, type));
            }
            return properties;
        }

        private static void AddReductionTableRelatedProperties(StructureSchema<ModelPropertyDefinition> schema, string type, IPump pump, List<DelftIniProperty> properties)
        {
            if (pump.ReductionTable.Arguments[0].Values.Count == 0)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, 0, schema, type));
            }
            else if (pump.ReductionTable.Arguments[0].Values.Count == 1)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, 1, schema, type));
                properties.Add(ConstructProperty(KnownStructureProperties.ReductionFactor, (double)pump.ReductionTable.Components[0].DefaultValue, schema, type));
            }
            else
            {
                var count = pump.ReductionTable.Arguments[0].Values.Count;
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, count, schema, type));
                var headValues = pump.ReductionTable.Arguments[0].Values.OfType<double>().ToArray();
                var reductionValues = pump.ReductionTable.Components[0].Values.OfType<double>().ToArray();

                properties.Add(ConstructProperty(KnownStructureProperties.Head, headValues, schema, type));
                properties.Add(ConstructProperty(KnownStructureProperties.ReductionFactor, reductionValues, schema, type));
            }
        }

        private static void AddControlDirectionRelatedProperties(StructureSchema<ModelPropertyDefinition> schema, string type, IPump pump, ICollection<DelftIniProperty> properties)
        {
            switch (pump.ControlDirection)
            {
                case PumpControlDirection.DeliverySideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartDeliverySide, pump.StartDelivery, schema, type));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopDeliverySide, pump.StopDelivery, schema, type));
                    break;
                case PumpControlDirection.SuctionAndDeliverySideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartDeliverySide, pump.StartDelivery, schema, type));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopDeliverySide, pump.StopDelivery, schema, type));

                    properties.Add(ConstructProperty(KnownStructureProperties.StartSuctionSide, pump.StartSuction, schema, type));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopSuctionSide, pump.StopSuction, schema, type));
                    break;
                case PumpControlDirection.SuctionSideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartSuctionSide, pump.StartSuction, schema, type));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopSuctionSide, pump.StopSuction, schema, type));
                    break;
            }
        }

        private static DelftIniProperty ConstructProperty(string propertyName, object value, StructureSchema<ModelPropertyDefinition> schema, string type)
        {
            var definition = schema.GetDefinition(type, propertyName);
            var delftIniProperty = new DelftIniProperty
                {
                    Name = definition.FilePropertyName,
                    Value = FMParser.ToString(value, value is ICollection ? typeof(IList<double>) : value.GetType()),
                    Comment = definition.Description
                };
            return delftIniProperty;
        }

        private static string DetermineType(IStructure structure)
        {
            if (structure is IPump) return "pump";
            if (structure is IGate) return "gate";

            var weir = structure as IWeir;
            if (weir != null)
            {
                if (weir.WeirFormula is SimpleWeirFormula) return "weir";
            }

            throw new NotImplementedException();
        }

        #endregion

        private Structure2D CreateStructure2D(StructureSchema<ModelPropertyDefinition> schema, string structureType,
                                                     DelftIniCategory category, string path)
        {
            var newStructure = new Structure2D(structureType);

            foreach (var property in category.Properties)
            {
                var modelPropertyDefinition = schema.GetDefinition(structureType, property.Name);
                if (modelPropertyDefinition == null)
                {
                    Log.WarnFormat("Property '{0}' not supported for structures of type '{1}' and is skipped. (Line {2} of file {3})",
                                   property.Name, structureType, property.LineNumber, path);
                    continue;
                }
                try
                {
                    var structureProperty = new StructureProperty(modelPropertyDefinition, property.Value);
                    var propertyValue = structureProperty.Value as Steerable;
                    if (propertyValue!=null && propertyValue.Mode == SteerableMode.TimeSeries)
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
                                                            property.Name, property.LineNumber, path),e);
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
            if (dataType.IsEnum) return "Any of the following values: "+ String.Join(", ", Enum.GetValues(modelPropertyDefinition.DataType));

            throw new NotImplementedException();
        }

        public static void ParseStructure(FMSuiteFileBase fmSuiteFileBase)
        {
            throw new NotImplementedException();
        }
    }
}