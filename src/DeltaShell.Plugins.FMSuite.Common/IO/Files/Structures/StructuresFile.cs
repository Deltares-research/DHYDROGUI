using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    /// <summary>
    /// A file containing structure definitions following the 3Di 'delft .ini' format.
    /// </summary>
    public class StructuresFile : FMSuiteFileBase, IFeature2DFileBase<IStructure>
    {
        private const string StructureCategoryName = "structure";
        private static readonly ILog Log = LogManager.GetLogger(typeof(StructuresFile));

        public StructuresFile()
        {
            PropertyTypesFromIni = new List<string>();
        }

        private readonly Dictionary<string, string> backwardsCompatibilityMapping = new Dictionary<string, string>
        {
            {"levelcenter", KnownStructureProperties.CrestLevel},
            {"widthcenter", KnownStructureProperties.CrestWidth},
            {"widthleftWsdl", KnownGeneralStructureProperties.Upstream1Width.GetDescription()},
            {"levelleftZbsl", KnownGeneralStructureProperties.Upstream1Level.GetDescription()},
            {"widthleftW1", KnownGeneralStructureProperties.Upstream2Width.GetDescription()},
            {"levelleftZb1", KnownGeneralStructureProperties.Upstream2Level.GetDescription()},
            {"widthrightWsdr", KnownGeneralStructureProperties.Downstream1Width.GetDescription()},
            {"levelrightZbsr", KnownGeneralStructureProperties.Downstream1Level.GetDescription()},
            {"widthrightW2", KnownGeneralStructureProperties.Downstream2Width.GetDescription()},
            {"levelrightZb2", KnownGeneralStructureProperties.Downstream2Level.GetDescription()},
            {"gateheight", KnownStructureProperties.GateLowerEdgeLevel},
            {"gatedoorheight", KnownStructureProperties.GateHeight},
            {"door_opening_width", KnownStructureProperties.GateOpeningWidth},
            {"sill_level", KnownStructureProperties.CrestLevel},
            {"sill_width", KnownStructureProperties.CrestWidth},
            {"lower_edge_level", KnownStructureProperties.GateLowerEdgeLevel},
            {"door_height", KnownStructureProperties.GateHeight},
            {"opening_width", KnownStructureProperties.GateOpeningWidth},
            {"horizontal_opening_direction", KnownStructureProperties.GateOpeningHorizontalDirection},
            {"crest_level", KnownStructureProperties.CrestLevel},
            {"crest_width", KnownStructureProperties.CrestWidth}
        };

        public StructureSchema<ModelPropertyDefinition> StructureSchema { private get; set; }

        public DateTime ReferenceDate { private get; set; }

        public List<string> PropertyTypesFromIni { get; }

        /// <summary>
        /// Method reads the structures file, creates temporary data access objects ("structures") and
        /// finally from these "structures" weirs with different weirformulas and pumps will be created (ConvertStructure step)
        /// </summary>
        /// <param name="structuresFilePath"> File path of the structures file</param>
        /// <param name="structuresSubFilesReferenceFilePath"> Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu. </param>
        /// <returns>List with weirs with different weirformulas and pumps</returns>
        public IList<IStructure> ReadStructuresFileRelativeToReferenceFile(
            string structuresFilePath, string structuresSubFilesReferenceFilePath)
        {
            var logHandler = new LogHandler($"reading the structures file ({structuresFilePath}),", Log);

            List<IStructure> structures = ReadStructures2D(structuresFilePath, logHandler)
                                          .Select(s => ConvertStructure(s, structuresSubFilesReferenceFilePath))
                                          .Where(s => s != null)
                                          .ToList();

            logHandler.LogReport();

            return structures;
        }

        /// <summary>
        ///  Method reads ini file and creates temporary data access objects ("structures") 
        /// </summary>
        /// <param name="filePath">File path of the structures file</param>
        /// <param name="logHandler"> Log messages collector and reporter. Not required if you are not interested in the log messages.</param>
        /// <returns>List with structures</returns>
        public IEnumerable<Structure2D> ReadStructures2D(string filePath, ILogHandler logHandler = null)
        {
            IList<DelftIniCategory> categories;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                categories = new DelftIniReader().ReadDelftIniFile(fileStream, filePath);
            }

            foreach (DelftIniCategory category in categories)
            {
                RenameBackwardsCompatibleProperties(category);
                // Filter out unexpected .ini categories:
                if (category.Name != StructureCategoryName)
                {
                    logHandler?.ReportWarningFormat(Resources.StructureFile_Category__0__not_supported_for_structures_and_is_skipped_Line__1__, 
                                                    category.Name, 
                                                    category.LineNumber);
                    continue;
                }

                // TODO: Check for potentially other required properties:
                // Read required 'type' property:
                DelftIniProperty structureTypeProperty = 
                    category.Properties.FirstOrDefault(p => p.Name == KnownStructureProperties.Type);
                if (structureTypeProperty == null)
                {
                    logHandler?.ReportWarningFormat(Resources.StructureFile_Obligated_property__0__expected_but_is_missing_Structure_is_skipped_Line__1__,
                                                    KnownStructureProperties.Type, 
                                                    category.LineNumber);
                    continue;
                }

                Structure2D structure2D =
                    CreateStructure2D(StructureSchema, structureTypeProperty.Value, category, filePath, logHandler);
                string errorMessage = StructureFactoryValidator.Validate(structure2D);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    logHandler?.ReportErrorFormat(Resources.StructureFile_Failed_to_convert_ini_structure_definition_to_actual_structure_Line__0____1__,
                                                  category.LineNumber, 
                                                  errorMessage);
                    continue;
                }

                PropertyTypesFromIni.Add(structureTypeProperty.Value);

                yield return structure2D;
            }
        }

        public static void WriteStructures2D(string path, IEnumerable<Structure2D> structures)
        {
            new DelftIniWriter().WriteDelftIniFile(structures.Select(CreateDelftIniCategory), path);
        }

        public IList<IStructure> Read(string filePath)
        {
            var logHandler = new LogHandler($"reading the structures file ({filePath}),", Log);

            List<IStructure> structures = ReadStructures2D(filePath, logHandler)
                             .Select(s => ConvertStructure(s, filePath))
                             .Where(s => s != null)
                             .ToList();

            logHandler.LogReport();

            return structures;
        }

        public void Write(string filePath, IEnumerable<IStructure> structures)
        {
            new DelftIniWriter().WriteDelftIniFile(
                GetSupportedStructures(structures)
                    .Select(s => CreateDelftIniCategory(s, filePath, ReferenceDate)), filePath);
        }

        private string TimFolder { get; set; }

        private void RenameBackwardsCompatibleProperties(DelftIniCategory category)
        {
            foreach (DelftIniProperty property in category.Properties)
            {
                if (backwardsCompatibilityMapping.TryGetValue(property.Name, out string newName))
                {
                    property.Name = newName;
                }
            }
        }

        private IStructure ConvertStructure(Structure2D structure, string structuresSubFilesReferenceFilePath)
        {
            try
            {
                return StructureFactory.CreateStructure(structure, structuresSubFilesReferenceFilePath, ReferenceDate);
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
            foreach (IStructure structure in structures)
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
            var delftIniCategory = new DelftIniCategory(StructureCategoryName);

            foreach (ModelProperty property in structure.Properties)
            {
                if (property.PropertyDefinition.FilePropertyName == KnownStructureProperties.CrestWidth &&
                    FMParser.FromString<double>(property.GetValueAsString()) <= 0.0)
                {
                    continue;
                }

                if (property.PropertyDefinition.FilePropertyName == KnownStructureProperties.CrestLevel &&
                    FMParser.FromString<double>(property.GetValueAsString()) <= 0.0)
                {
                    continue;
                }

                var delftIniProperty = new DelftIniProperty(
                    property.PropertyDefinition.FilePropertyName, 
                    property.GetValueAsString(), 
                    property.PropertyDefinition.Description);

                delftIniCategory.AddProperty(delftIniProperty);
            }

            return delftIniCategory;
        }

        private DelftIniCategory CreateDelftIniCategory(IStructure structure, string filePath, DateTime refDate)
        {
            var delftIniCategory = new DelftIniCategory(StructureCategoryName);
            IEnumerable<DelftIniProperty> delftIniProperties = CreateDelftIniProperties(structure, filePath, refDate);

            foreach (DelftIniProperty property in delftIniProperties)
            {
                delftIniCategory.AddProperty(property);
            }

            return delftIniCategory;
        }

        private Structure2D CreateStructure2D(StructureSchema<ModelPropertyDefinition> schema, 
                                              string structureType,
                                              DelftIniCategory category, 
                                              string filePath, 
                                              ILogHandler logHandler)
        {
            var newStructure = new Structure2D(structureType);

            foreach (DelftIniProperty property in category.Properties)
            {
                ModelPropertyDefinition modelPropertyDefinition = schema.GetDefinition(structureType, property.Name);
                if (modelPropertyDefinition == null)
                {
                    logHandler?.ReportWarningFormat(
                        Resources.StructureFile_Property__0__not_supported_for_structures_of_type__1__and_is_skipped_Line__2__,
                        property.Name, structureType, property.LineNumber);
                    continue;
                }

                try
                {
                    var structureProperty = new StructureProperty(modelPropertyDefinition, property.Value);
                    Steerable propertyValue = structureProperty.Value as Steerable;
                    if (propertyValue != null && propertyValue.Mode == SteerableMode.TimeSeries)
                    {
                        SetOrUpdateTimFolder(propertyValue.TimeSeriesFilename, property.LineNumber, logHandler);
                    }

                    newStructure.Properties.Add(structureProperty);
                }
                catch (FormatException e)
                {
                    throw new FormatException(string.Format(
                                                  Resources.StructureFile_An_invalid_value__0__was_encountered_expected__1__for_property__2__on_line__3__,
                                                  e.InnerException is OverflowException ? " (too large/small)" : "",
                                                  GetValueTypeDescription(
                                                      modelPropertyDefinition.DataType, modelPropertyDefinition),
                                                  property.Name, property.LineNumber, filePath), e);
                }
            }

            return newStructure;
        }

        private void SetOrUpdateTimFolder(string timeSeriesFilename, int propertyLineNumber, ILogHandler logHandler)
        {
            string directory = Path.GetDirectoryName(timeSeriesFilename);
            if (TimFolder == directory)
            {
                return;
            }

            if (string.IsNullOrEmpty(directory) && TimFolder != null)
            {
                logHandler?.ReportWarningFormat(
                    Resources.StructureFile_Structure_time_series__0__will_be_written_to_the_time_series_folder__1__Line__2__,
                    timeSeriesFilename, TimFolder, propertyLineNumber);
            }
            else
            {
                if (TimFolder != null)
                {
                    logHandler?.ReportWarningFormat(
                        Resources.StructureFile_Replacing_structure_time_series_folder__0__with__1__all_structure_time_series_will_be_written_to_this_folder_Line__2__,
                        TimFolder, directory, propertyLineNumber);
                }

                if (!string.IsNullOrEmpty(directory))
                {
                    TimFolder = directory;
                }
            }
        }

        private static object GetValueTypeDescription(Type dataType, ModelPropertyDefinition modelPropertyDefinition)
        {
            if (dataType == typeof(int))
            {
                return "a whole number";
            }

            if (dataType == typeof(double))
            {
                return "a number";
            }

            if (dataType == typeof(IList<double>))
            {
                return "a series of space separated numbers";
            }

            if (dataType == typeof(TimeSpan))
            {
                return "a time span in seconds";
            }

            if (dataType == typeof(bool))
            {
                return "a '1' or '0'";
            }

            if (dataType == typeof(DateTime))
            {
                return "a date in yyyyMMdd or yyyyMMdHHmmss format";
            }

            if (dataType == typeof(Steerable))
            {
                return "a number or a filepath to a time series";
            }

            if (dataType.IsEnum)
            {
                return "Any of the following values: " +
                       string.Join(", ", Enum.GetValues(modelPropertyDefinition.DataType));
            }

            throw new NotImplementedException();
        }

        #region Sobek Structure to DelftIni related methods:

        private IEnumerable<DelftIniProperty> CreateDelftIniProperties(IStructure structure, string filePath,
                                                                       DateTime refDate)
        {
            var properties = new List<DelftIniProperty>();

            string structureType = DetermineType(structure);

            properties.Add(ConstructProperty(KnownStructureProperties.Type, structureType, structureType));
            properties.Add(ConstructProperty(KnownStructureProperties.Name, structure.Name, structureType));
            properties.AddRange(ConstructGeometryProperties(structure, structureType, filePath));
            properties.AddRange(ConstructStructureProperties(structure, structureType, filePath, refDate));

            return properties;
        }

        private IEnumerable<DelftIniProperty> ConstructGeometryProperties(
            IStructure structure, string structureType, string filePath)
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
                char[] charArray = structure.Name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray();
                if (charArray.Any())
                {
                    string pliFileName = string.Format("{0}.pli", new string(charArray));

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
                Log.ErrorFormat("Geometry type '{0}' for structure '{1}' not supported and therefore not written.",
                                structure.Geometry.GetType(), structure.Name);
            }
        }

        private IEnumerable<DelftIniProperty> ConstructStructureProperties(
            IStructure structure, string structureType, string path, DateTime refDate)
        {
            var pump = structure as IPump;
            if (pump != null)
            {
                return ConstructPumpProperties(pump, structureType, path, refDate);
            }

            var weir = structure as IWeir;
            if (weir != null)
            {
                return ConstructWeirProperties(weir, structureType, path, refDate);
            }

            var gate = structure as IGate;
            if (gate != null)
            {
                return ConstructGateProperties(gate, structureType, path, refDate);
            }

            throw new NotImplementedException();
        }

        private IEnumerable<DelftIniProperty> ConstructWeirProperties(IStructure1D structure, string structureType,
                                                                      string path, DateTime refDate)
        {
            var weir = (IWeir) structure;
            IWeirFormula weirFormula = weir.WeirFormula;
            if (weirFormula is SimpleWeirFormula)
            {
                return ConstructSimpleWeirProperties(weir, path, structureType, refDate);
            }

            if (weirFormula is GeneralStructureWeirFormula)
            {
                return ConstructGeneralStructureProperties(weir, path, structureType, refDate);
            }

            if (weirFormula is GatedWeirFormula)
            {
                return ConstructGatedWeirProperties(weir, structureType, path, refDate);
            }

            throw new NotImplementedException();
        }

        private IEnumerable<DelftIniProperty> ConstructGeneralStructureProperties(
            IStructure1D structure, string path, string structureType, DateTime refDate)
        {
            var properties = new List<DelftIniProperty>();

            var weirStructure = structure as IWeir;
            var generalStructureFormula = weirStructure.WeirFormula as GeneralStructureWeirFormula;

            // Width
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.WidthLeftW1.Key,
                                                  generalStructureFormula
                                                      .WidthLeftSideOfStructure,
                                                  structureType);
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.WidthLeftWsdl.Key,
                                                  generalStructureFormula
                                                      .WidthStructureLeftSide,
                                                  structureType);
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.WidthCenter.Key,
                                                  weirStructure.CrestWidth,
                                                  structureType);
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.WidthRightWsdr.Key,
                                                  generalStructureFormula
                                                      .WidthStructureRightSide,
                                                  structureType);
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.WidthRightW2.Key,
                                                  generalStructureFormula
                                                      .WidthRightSideOfStructure,
                                                  structureType);

            // Level
            properties.Add(ConstructProperty(StructureRegion.LevelLeftZb1.Key,
                                             generalStructureFormula.BedLevelLeftSideOfStructure, structureType));
            properties.Add(ConstructProperty(StructureRegion.LevelLeftZbsl.Key,
                                             generalStructureFormula.BedLevelLeftSideStructure, structureType));

            if (weirStructure.CanBeTimedependent && weirStructure.UseCrestLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(path, properties,
                                            weirStructure,
                                            StructureRegion.LevelCenter.Key,
                                            structureType,
                                            weirStructure.CrestLevelTimeSeries,
                                            refDate);
            }
            else
            {
                properties.Add(ConstructProperty(StructureRegion.LevelCenter.Key,
                                                 generalStructureFormula.BedLevelStructureCentre, structureType));
            }

            properties.Add(ConstructProperty(StructureRegion.LevelRightZbsr.Key,
                                             generalStructureFormula.BedLevelRightSideStructure, structureType));
            properties.Add(ConstructProperty(StructureRegion.LevelRightZb2.Key,
                                             generalStructureFormula.BedLevelRightSideOfStructure, structureType));

            // LowerEdgeLevel
            if (generalStructureFormula.UseLowerEdgeLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(path, properties,
                                            weirStructure,
                                            StructureRegion.GateHeight.Key,
                                            structureType,
                                            generalStructureFormula.LowerEdgeLevelTimeSeries,
                                            refDate);
            }
            else
            {
                properties.Add(ConstructProperty(StructureRegion.GateHeight.Key,
                                                 generalStructureFormula.LowerEdgeLevel,
                                                 structureType));
            }

            // Flow Coefficients
            properties.Add(ConstructProperty(StructureRegion.PosFreeGateFlowCoeff.Key,
                                             generalStructureFormula.PositiveFreeGateFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.PosDrownGateFlowCoeff.Key,
                                             generalStructureFormula.PositiveDrownedGateFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.PosFreeWeirFlowCoeff.Key,
                                             generalStructureFormula.PositiveFreeWeirFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.PosDrownWeirFlowCoeff.Key,
                                             generalStructureFormula.PositiveDrownedWeirFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.PosContrCoefFreeGate.Key,
                                             generalStructureFormula.PositiveContractionCoefficient, structureType));

            properties.Add(ConstructProperty(StructureRegion.NegFreeGateFlowCoeff.Key,
                                             generalStructureFormula.NegativeFreeGateFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.NegDrownGateFlowCoeff.Key,
                                             generalStructureFormula.NegativeDrownedGateFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.NegFreeWeirFlowCoeff.Key,
                                             generalStructureFormula.NegativeFreeWeirFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.NegDrownWeirFlowCoeff.Key,
                                             generalStructureFormula.NegativeDrownedWeirFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.NegContrCoefFreeGate.Key,
                                             generalStructureFormula.NegativeContractionCoefficient, structureType));

            // Misc.
            double extraResistance = generalStructureFormula.UseExtraResistance
                                         ? generalStructureFormula.ExtraResistance
                                         : 0.0;
            properties.Add(ConstructProperty(StructureRegion.ExtraResistance.Key, extraResistance, structureType));

            properties.Add(ConstructProperty(StructureRegion.GateDoorHeight.Key, generalStructureFormula.DoorHeight,
                                             structureType));

            // Horizontal door opening
            if (generalStructureFormula.UseHorizontalDoorOpeningWidthTimeSeries)
            {
                ConstructTimeSeriesProperty(path, properties,
                                            weirStructure,
                                            KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                                            structureType,
                                            generalStructureFormula.HorizontalDoorOpeningWidthTimeSeries,
                                            refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                                                 generalStructureFormula.HorizontalDoorOpeningWidth,
                                                 structureType));
            }

            string horizontalDoorOpeningDirection;
            switch (generalStructureFormula.HorizontalDoorOpeningDirection)
            {
                case GateOpeningDirection.Symmetric:
                    horizontalDoorOpeningDirection = "symmetric";
                    break;
                case GateOpeningDirection.FromLeft:
                    horizontalDoorOpeningDirection = "from_left";
                    break;
                case GateOpeningDirection.FromRight:
                    horizontalDoorOpeningDirection = "from_right";
                    break;
                default:
                    throw new ArgumentException("We can't write " +
                                                generalStructureFormula.HorizontalDoorOpeningDirection);
            }

            properties.Add(ConstructProperty(
                               KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription(),
                               horizontalDoorOpeningDirection,
                               structureType));

            return properties;
        }

        private IEnumerable<DelftIniProperty> ConstructPumpProperties(IStructure1D structure, string structureType,
                                                                      string path, DateTime refDate)
        {
            var pump = (IPump) structure;
            var properties = new List<DelftIniProperty>();

            if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
            {
                string timeFileName = ConstructTimeFilePath(pump, KnownStructureProperties.Capacity);
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

        private IEnumerable<DelftIniProperty> ConstructSimpleWeirProperties(
            IStructure1D structure, string path, string structureType, DateTime refDate)
        {
            var weir = (IWeir) structure;
            var properties = new List<DelftIniProperty>();

            if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
            {
                string timeFilePath = ConstructTimeFilePath(weir, KnownStructureProperties.CrestLevel);
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), weir.CrestLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel, weir.CrestLevel, structureType));
            }

            AddDoubleOrEmptyPropertyConditionally(properties, KnownStructureProperties.CrestWidth, weir.CrestWidth,
                                                  structureType);

            var formula = (SimpleWeirFormula) ((IWeir) structure).WeirFormula;
            properties.Add(ConstructProperty(KnownStructureProperties.LateralContractionCoefficient,
                                             formula.LateralContraction, structureType));
            return properties;
        }

        /// <summary>
        /// Add the specified <paramref name="value" /> property with name
        /// <paramref name="propertyName" /> to <paramref name="properties" /> if
        /// <paramref name="value" /> is NaN or greater than zero.
        /// </summary>
        /// <param name="properties"> The properties. </param>
        /// <param name="propertyName"> The name of the new property. </param>
        /// <param name="value"> The value to be added as property. </param>
        /// <param name="structureType"> Type of the structure. </param>
        /// <remarks>
        /// If <paramref name="value" /> is NaN then an empty value field will be written.
        /// Properties is not null
        /// </remarks>
        private void AddDoubleOrEmptyPropertyConditionally(ICollection<DelftIniProperty> properties,
                                                           string propertyName,
                                                           double value,
                                                           string structureType)
        {
            if (double.IsNaN(value))
                // we do not want to add an empty string as it will be filtered out in the write step.
            {
                properties.Add(ConstructProperty(propertyName, " ", structureType));
            }
            else if (value > 0)
            {
                properties.Add(ConstructProperty(propertyName, value, structureType));
            }
        }

        private IEnumerable<DelftIniProperty> ConstructGateProperties(IStructure1D structure, string structureType,
                                                                      string path, DateTime refDate)
        {
            var gate = (IGate) structure;
            var properties = new List<DelftIniProperty>();

            if (gate.UseSillLevelTimeSeries)
            {
                string timeFilePath = ConstructTimeFilePath(gate, KnownStructureProperties.CrestLevel);
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), gate.SillLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel, gate.SillLevel, structureType));
            }

            if (gate.UseLowerEdgeLevelTimeSeries)
            {
                string timeFilePath = ConstructTimeFilePath(gate, KnownStructureProperties.GateLowerEdgeLevel);
                properties.Add(ConstructProperty(KnownStructureProperties.GateLowerEdgeLevel, timeFilePath,
                                                 structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), gate.LowerEdgeLevelTimeSeries,
                              refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateLowerEdgeLevel, gate.LowerEdgeLevel,
                                                 structureType));
            }

            if (gate.UseOpeningWidthTimeSeries)
            {
                string timeFilePath = ConstructTimeFilePath(gate, KnownStructureProperties.GateOpeningWidth);
                properties.Add(
                    ConstructProperty(KnownStructureProperties.GateOpeningWidth, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), gate.OpeningWidthTimeSeries,
                              refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningWidth, gate.OpeningWidth,
                                                 structureType));
            }

            properties.Add(ConstructProperty(KnownStructureProperties.GateHeight, gate.DoorHeight, structureType));

            // switch the horizontal direction, because enums aren't used very nicely in the csv file (structure definition).
            string horizontalDirection;
            switch (gate.HorizontalOpeningDirection)
            {
                case GateOpeningDirection.Symmetric:
                    horizontalDirection = "symmetric";
                    break;
                case GateOpeningDirection.FromLeft:
                    horizontalDirection = "from_left";
                    break;
                case GateOpeningDirection.FromRight:
                    horizontalDirection = "from_right";
                    break;
                default:
                    throw new ArgumentException("We can't write " + gate.HorizontalOpeningDirection);
            }

            properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningHorizontalDirection,
                                             horizontalDirection, structureType));
            AddDoubleOrEmptyPropertyConditionally(properties, KnownStructureProperties.CrestWidth, gate.SillWidth,
                                                  structureType);

            return properties;
        }

        /// <summary>
        /// Construct the set of gated weir properties to be written to the
        /// structures.ini file.
        /// </summary>
        /// <param name="structure"> The Weir </param>
        /// <param name="structureType"> </param>
        /// <param name="path"> </param>
        /// <param name="refDate"> </param>
        /// <returns>
        /// The set of properties which should be written to structures.ini
        /// </returns>
        private IEnumerable<DelftIniProperty> ConstructGatedWeirProperties(IStructure1D structure,
                                                                           string structureType,
                                                                           string path,
                                                                           DateTime refDate)
        {
            var gatedWeir = (IWeir) structure;
            var gatedWeirFormula = (IGatedWeirFormula) gatedWeir.WeirFormula;

            var properties = new List<DelftIniProperty>();

            if (gatedWeir.UseCrestLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(path, properties,
                                            gatedWeir,
                                            KnownStructureProperties.CrestLevel,
                                            structureType,
                                            gatedWeir.CrestLevelTimeSeries,
                                            refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel,
                                                 gatedWeir.CrestLevel,
                                                 structureType));
            }

            if (gatedWeirFormula.UseLowerEdgeLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(path, properties,
                                            gatedWeir,
                                            KnownStructureProperties.GateLowerEdgeLevel,
                                            structureType,
                                            gatedWeirFormula.LowerEdgeLevelTimeSeries,
                                            refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateLowerEdgeLevel,
                                                 gatedWeirFormula.LowerEdgeLevel,
                                                 structureType));
            }

            if (gatedWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries)
            {
                ConstructTimeSeriesProperty(path, properties,
                                            gatedWeir,
                                            KnownStructureProperties.GateOpeningWidth,
                                            structureType,
                                            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries,
                                            refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningWidth,
                                                 gatedWeirFormula.HorizontalDoorOpeningWidth,
                                                 structureType));
            }

            properties.Add(ConstructProperty(KnownStructureProperties.GateHeight,
                                             gatedWeirFormula.DoorHeight,
                                             structureType));

            string horizontalDoorOpeningDirection;
            switch (gatedWeirFormula.HorizontalDoorOpeningDirection)
            {
                case GateOpeningDirection.Symmetric:
                    horizontalDoorOpeningDirection = "symmetric";
                    break;
                case GateOpeningDirection.FromLeft:
                    horizontalDoorOpeningDirection = "from_left";
                    break;
                case GateOpeningDirection.FromRight:
                    horizontalDoorOpeningDirection = "from_right";
                    break;
                default:
                    throw new ArgumentException("We can't write " +
                                                gatedWeirFormula.HorizontalDoorOpeningDirection);
            }

            properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningHorizontalDirection,
                                             horizontalDoorOpeningDirection,
                                             structureType));

            AddDoubleOrEmptyPropertyConditionally(properties, KnownStructureProperties.CrestWidth, gatedWeir.CrestWidth,
                                                  structureType);
            return properties;
        }

        private void ConstructTimeSeriesProperty(string path,
                                                 ICollection<DelftIniProperty> properties,
                                                 IStructure1D structure,
                                                 string propertyName,
                                                 string structureType,
                                                 TimeSeries timeSeries,
                                                 DateTime refDate)
        {
            string timeFilePath = ConstructTimeFilePath(structure, propertyName);
            properties.Add(
                ConstructProperty(propertyName,
                                  timeFilePath,
                                  structureType));
            WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath),
                          timeSeries,
                          refDate);
        }

        private void AddReductionTableRelatedProperties(IPump pump, List<DelftIniProperty> properties,
                                                        string structureType)
        {
            if (pump.ReductionTable.Arguments[0].Values.Count == 0)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, 0, structureType));
            }
            else if (pump.ReductionTable.Arguments[0].Values.Count == 1)
            {
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, 1, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.ReductionFactor,
                                                 (double) pump.ReductionTable.Components[0].DefaultValue,
                                                 structureType));
            }
            else
            {
                int count = pump.ReductionTable.Arguments[0].Values.Count;
                properties.Add(ConstructProperty(KnownStructureProperties.NrOfReductionFactors, count, structureType));
                double[] headValues = pump.ReductionTable.Arguments[0].Values.OfType<double>().ToArray();
                double[] reductionValues = pump.ReductionTable.Components[0].Values.OfType<double>().ToArray();

                properties.Add(ConstructProperty(KnownStructureProperties.Head, headValues, structureType));
                properties.Add(ConstructProperty(KnownStructureProperties.ReductionFactor, reductionValues,
                                                 structureType));
            }
        }

        private void AddControlDirectionRelatedProperties(IPump pump, ICollection<DelftIniProperty> properties,
                                                          string structureType)
        {
            switch (pump.ControlDirection)
            {
                case PumpControlDirection.DeliverySideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartDeliverySide, pump.StartDelivery,
                                                     structureType));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopDeliverySide, pump.StopDelivery,
                                                     structureType));
                    break;
                case PumpControlDirection.SuctionAndDeliverySideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartDeliverySide, pump.StartDelivery,
                                                     structureType));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopDeliverySide, pump.StopDelivery,
                                                     structureType));

                    properties.Add(ConstructProperty(KnownStructureProperties.StartSuctionSide, pump.StartSuction,
                                                     structureType));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopSuctionSide, pump.StopSuction,
                                                     structureType));
                    break;
                case PumpControlDirection.SuctionSideControl:
                    properties.Add(ConstructProperty(KnownStructureProperties.StartSuctionSide, pump.StartSuction,
                                                     structureType));
                    properties.Add(ConstructProperty(KnownStructureProperties.StopSuctionSide, pump.StopSuction,
                                                     structureType));
                    break;
            }
        }

        private DelftIniProperty ConstructProperty(string propertyName, object value, string structureType)
        {
            ModelPropertyDefinition definition = StructureSchema.GetDefinition(structureType, propertyName);
            string propertyValue = FMParser.ToString(value, value is ICollection ? typeof(IList<double>) : value.GetType());

            var delftIniProperty = new DelftIniProperty(
                definition.FilePropertyName, 
                propertyValue, 
                definition.Description
            );
            return delftIniProperty;
        }

        private static string DetermineType(IStructure structure)
        {
            if (structure is IPump)
            {
                return StructureRegion.StructureTypeName.Pump;
            }

            if (structure is IGate)
            {
                return StructureRegion.StructureTypeName.Gate;
            }

            var weir = structure as IWeir;
            if (weir != null)
            {
                if (weir.WeirFormula is SimpleWeirFormula)
                {
                    return StructureRegion.StructureTypeName.Weir;
                }

                // A GatedWeir is a Gate for the Kernel, hence we specify it as a "gate".
                if (weir.WeirFormula is GatedWeirFormula)
                {
                    return StructureRegion.StructureTypeName.Gate;
                }

                if (weir.WeirFormula is GeneralStructureWeirFormula)
                {
                    return StructureRegion.StructureTypeName.GeneralStructure;
                }
            }

            throw new NotImplementedException();
        }

        #endregion

        #region FileWriting

        private static void WritePliFile(string pliFilePath, IStructure structure)
        {
            var pliFile = new PliFile<Feature2D>();
            pliFile.Write(pliFilePath, new[]
            {
                new Feature2D
                {
                    Name = structure.Name,
                    Geometry = structure.Geometry
                }
            });
        }

        private string ConstructTimeFilePath(IStructure1D structure, string propertyName)
        {
            string filePath = string.Format("{0}_{1}.tim", structure.Name, propertyName);
            if (TimFolder != null)
            {
                filePath = Path.Combine(TimFolder, filePath);
            }

            return filePath;
        }

        private static void WriteTimeFile(string filePath, IFunction capacityTimeSeries, DateTime refDate)
        {
            var timeFile = new TimFile();
            string directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            timeFile.Write(filePath, capacityTimeSeries, refDate);
        }

        #endregion
    }
}