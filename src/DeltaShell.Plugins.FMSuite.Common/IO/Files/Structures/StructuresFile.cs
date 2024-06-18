using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    /// <summary>
    /// A file containing structure definitions following the 3Di INI format.
    /// </summary>
    public class StructuresFile : NGHSFileBase, IFeature2DFileBase<IStructureObject>
    {
        private const string structureSectionName = "structure";
        private static readonly ILog log = LogManager.GetLogger(typeof(StructuresFile));

        private readonly Dictionary<string, string> backwardsCompatibilityMapping = new Dictionary<string, string>
        {
            {"levelcenter", KnownStructureProperties.CrestLevel},
            {"widthcenter", KnownStructureProperties.CrestWidth},
            {"widthleftWsdl", KnownGeneralStructureProperties.Upstream2Width.GetDescription()},
            {"levelleftZbsl", KnownGeneralStructureProperties.Upstream2Level.GetDescription()},
            {"widthleftW1", KnownGeneralStructureProperties.Upstream1Width.GetDescription()},
            {"levelleftZb1", KnownGeneralStructureProperties.Upstream1Level.GetDescription()},
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

        private readonly Dictionary<IStructureObject, StructureDAO> originalDataObjects = new Dictionary<IStructureObject, StructureDAO>();

        public StructureSchema<ModelPropertyDefinition> StructureSchema { private get; set; }

        public List<string> PropertyTypesFromIni { get; } = new List<string>();

        /// <summary>
        /// Reference date for the model, required for time dependent data.
        /// </summary>
        public DateTime ReferenceDate { private get; set; }
        
        /// <summary>
        /// Filepath of the reference file. This is structures file or Mdu dependent on the PathsRelativeToParent option in the Mdu.
        /// </summary>
        public string ReferencePath { get; set; }
        
        /// <summary>
        /// Method reads the structures file, creates temporary data access objects ("structures") and
        /// finally from these "structures" weirs with different weirformulas and pumps will be created (ConvertStructure step)
        /// </summary>
        /// <param name="filePath"> File path of the structures file</param>
        /// <returns>List with weirs with different weirformulas and pumps</returns>
        public IList<IStructureObject> Read(string filePath)
        {
            var logHandler = new LogHandler($"reading the structures file ({filePath}),", log);

            try
            {
                List<IStructureObject> structures =
                    ReadStructuresFromFile(filePath, logHandler)
                        .Select(ConvertStructure)
                        .Where(s => s != null)
                        .ToList();

                logHandler.LogReport();

                return structures;
            }
            catch (Exception)
            {
                log.ErrorFormat(Resources.StructuresFile_ReadStructuresFileRelativeToReferenceFile_Error_while_reading_and_converting_2D_Structures_from__0_, filePath);
                throw;
            }
            finally
            {
                ReferencePath = null;
            }
        }

        /// <summary>
        /// Method reads ini file and creates temporary data access objects ("structures")
        /// </summary>
        /// <param name="filePath">File path of the structures file</param>
        /// <param name="logHandler">
        /// Log messages collector and reporter. Not required if you are not interested in the log
        /// messages.
        /// </param>
        /// <returns>List with structure data access objects. </returns>
        public IEnumerable<StructureDAO> ReadStructuresFromFile(string filePath, ILogHandler logHandler = null)
        {
            if (string.IsNullOrEmpty(ReferencePath))
            {
                ReferencePath = filePath;
            }

            originalDataObjects.Clear();
            
            IniData iniData;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                iniData = new IniReader().ReadIniFile(fileStream, filePath);
            }

            iniData.Sections.ToList()
                   .ForEach(RenameBackwardsCompatibleProperties);

            var structureValidator = new StructureFileValidator(filePath, ReferencePath);
            
            foreach (IniSection section in iniData.Sections)
            {
                // Filter out unexpected .ini sections:
                if (section.Name != structureSectionName)
                {
                    logHandler?.ReportWarningFormat(Resources.StructureFile_Section__0__not_supported_for_structures_and_is_skipped_Line__1__,
                                                    section.Name,
                                                    section.LineNumber);
                    continue;
                }

                // Read required 'type' property:
                IniProperty structureTypeProperty =
                    section.Properties.FirstOrDefault(p => p.Key == KnownStructureProperties.Type);
                if (structureTypeProperty == null)
                {
                    logHandler?.ReportWarningFormat(Resources.StructureFile_Obligated_property__0__expected_but_is_missing_Structure_is_skipped_Line__1__,
                                                    KnownStructureProperties.Type,
                                                    section.LineNumber);
                    continue;
                }

                StructureDAO structureDataAccessObject =
                    CreateStructureDao(StructureSchema, structureTypeProperty.Value, section, filePath, logHandler);
                string errorMessage = structureValidator.Validate(structureDataAccessObject);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    logHandler?.ReportErrorFormat(Resources.StructureFile_Failed_to_convert_ini_structure_definition_to_actual_structure_Line__0____1__,
                                                  section.LineNumber,
                                                  errorMessage);
                    continue;
                }

                PropertyTypesFromIni.Add(structureTypeProperty.Value);

                yield return structureDataAccessObject;
            }
        }

        public static void WriteStructuresDataAccessObjects(string path, IEnumerable<StructureDAO> structuresDataAccessObjects)
        {
            var iniData = new IniData();
            iniData.AddMultipleSections(structuresDataAccessObjects.Select(CreateSection));
            
            new IniWriter().WriteIniFile(iniData, path);
        }

        /// <summary>
        /// Writes the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="features">The collection of structures to write.</param>
        public void Write(string filePath, IEnumerable<IStructureObject> features)
        {
            if (string.IsNullOrEmpty(ReferencePath))
            {
                ReferencePath = filePath;
            }
            
            var iniWriter = new IniWriter();
            var iniData = new IniData();

            var sections = GetSupportedStructures(features)
                .Select(CreateSection);

            iniData.AddMultipleSections(sections);
            iniWriter.WriteIniFile(iniData, filePath);

            ReferencePath = null;
        }

        private void RenameBackwardsCompatibleProperties(IniSection section)
        {
            foreach (string propertyKey in section.Properties.Select(x => x.Key).ToList())
            {
                if (backwardsCompatibilityMapping.TryGetValue(propertyKey, out string newKey))
                {
                    section.RenameProperties(propertyKey, newKey);
                }
            }
        }

        private IStructureObject ConvertStructure(StructureDAO structureDataAccessObject)
        {
            try
            {
                IStructureObject structure = StructureFactory.CreateStructure(structureDataAccessObject, ReferencePath, ReferenceDate);

                originalDataObjects[structure] = structureDataAccessObject;

                return structure;
            }
            catch (Exception e)
            {
                log.ErrorFormat(Resources.StructuresFile_ConvertStructure_Failed_to_convert__ini_structure_definition___0___to_actual_structure_, structureDataAccessObject.Name);

                if (e is ArgumentNullException || e is ArgumentException || e is FileNotFoundException ||
                    e is DirectoryNotFoundException || e is IOException || e is OutOfMemoryException ||
                    e is FormatException)
                {
                    // Let the parent caller go ahead with further conversions.
                    return null;
                }

                throw;
            }
        }

        private static IEnumerable<IStructureObject> GetSupportedStructures(IEnumerable<IStructureObject> structures)
        {
            var list = new List<IStructureObject>();
            foreach (IStructureObject structure in structures)
            {
                if (structure is IPump || structure is IStructure)
                {
                    list.Add(structure);
                }
                else
                {
                    log.ErrorFormat("Structure '{0}' is of unsupported type and therefore skipped.", structure.Name);
                }
            }

            return list;
        }

        private static IniSection CreateSection(StructureDAO structureDataAccessObject)
        {
            var section = new IniSection(structureSectionName);

            foreach (ModelProperty property in structureDataAccessObject.Properties)
            {
                if (property.PropertyDefinition.FilePropertyKey == KnownStructureProperties.CrestWidth &&
                    FMParser.FromString<double>(property.GetValueAsString()) <= 0.0)
                {
                    continue;
                }

                if (property.PropertyDefinition.FilePropertyKey == KnownStructureProperties.CrestLevel &&
                    FMParser.FromString<double>(property.GetValueAsString()) <= 0.0)
                {
                    continue;
                }

                var iniProperty = new IniProperty(
                    property.PropertyDefinition.FilePropertyKey, 
                    property.GetValueAsString(),
                    property.PropertyDefinition.Description);

                section.AddProperty(iniProperty);
            }

            return section;
        }

        private IniSection CreateSection(IStructureObject structure)
        {
            var section = new IniSection(structureSectionName);
            
            IEnumerable<IniProperty> properties = CreateProperties(structure);
            section.AddMultipleProperties(properties);

            return section;
        }

        private StructureDAO CreateStructureDao(StructureSchema<ModelPropertyDefinition> schema,
                                              string structureType,
                                              IniSection section,
                                              string filePath,
                                              ILogHandler logHandler)
        {
            var newStructureDataAccessObject = new StructureDAO(structureType);

            foreach (IniProperty property in section.Properties)
            {
                ModelPropertyDefinition modelPropertyDefinition = schema.GetDefinition(structureType, property.Key);
                if (modelPropertyDefinition == null)
                {
                    logHandler?.ReportWarningFormat(
                        Resources.StructureFile_Property__0__not_supported_for_structures_of_type__1__and_is_skipped_Line__2__,
                        property.Key, structureType, property.LineNumber);
                    continue;
                }

                try
                {
                    var structureProperty = new StructureProperty(modelPropertyDefinition, property.Value) { LineNumber = property.LineNumber };
                    newStructureDataAccessObject.Properties.Add(structureProperty);
                }
                catch (FormatException e)
                {
                    throw new FormatException(string.Format(
                                                  Resources.StructureFile_An_invalid_value__0__was_encountered_expected__1__for_property__2__on_line__3__,
                                                  e.InnerException is OverflowException ? " (too large/small)" : "",
                                                  GetValueTypeDescription(
                                                      modelPropertyDefinition.DataType, modelPropertyDefinition),
                                                  property.Key, property.LineNumber, filePath), e);
                }
            }

            return newStructureDataAccessObject;
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

            throw new NotSupportedException();
        }

        #region Sobek Structure to INI related methods:

        private IEnumerable<IniProperty> CreateProperties(IStructureObject structure)
        {
            var properties = new List<IniProperty>();

            string structureType = DetermineType(structure);

            properties.Add(ConstructProperty(KnownStructureProperties.Type, structureType, structureType));
            properties.Add(ConstructProperty(KnownStructureProperties.Name, structure.Name, structureType));
            properties.AddRange(ConstructGeometryProperties(structure, structureType));
            properties.AddRange(ConstructStructureProperties(structure, structureType));

            return properties;
        }

        private IEnumerable<IniProperty> ConstructGeometryProperties(IStructureObject structure, string structureType)
        {
            if (structure.Geometry is IPoint point)
            {
                yield return ConstructProperty(KnownStructureProperties.X, point.X, structureType);
                yield return ConstructProperty(KnownStructureProperties.Y, point.Y, structureType);
            }

            if (structure.Geometry is ILineString)
            {
                string pliFileName = GetPliFilePath(structure);
                
                yield return ConstructProperty(KnownStructureProperties.PolylineFile, pliFileName, structureType);
                WritePliFile(GetOtherFilePathInSameDirectory(ReferencePath, GetPliFilePath(structure)), structure);
            }

            if (structure.Geometry != null && !(structure.Geometry is ILineString || structure.Geometry is IPoint))
            {
                log.ErrorFormat("Geometry type '{0}' for structure '{1}' not supported and therefore not written.",
                                structure.Geometry.GetType(), structure.Name);
            }
        }

        private IEnumerable<IniProperty> ConstructStructureProperties(IStructureObject structure, string structureType)
        {
            switch (structure)
            {
                case IPump pump:
                    return ConstructPumpProperties(pump, structureType);
                case IStructure weir:
                    return ConstructWeirProperties(weir, structureType);
                default:
                    throw new NotSupportedException();
            }
        }

        private IEnumerable<IniProperty> ConstructWeirProperties(IStructure structure, string structureType)
        {
            IStructureFormula structureFormula = structure.Formula;
            switch (structureFormula)
            {
                case SimpleWeirFormula _:
                    return ConstructSimpleWeirProperties(structure, structureType);
                case GeneralStructureFormula _:
                    return ConstructGeneralStructureProperties(structure, structureType);
                case SimpleGateFormula _:
                    return ConstructGatedWeirProperties(structure, structureType);
                default:
                    throw new NotSupportedException();
            }
        }

        private IEnumerable<IniProperty> ConstructGeneralStructureProperties(IStructure structure, string structureType)
        {
            var properties = new List<IniProperty>();
            var generalStructureFormula = (GeneralStructureFormula)structure.Formula;

            // Width
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.Upstream1Width.Key,
                                                  generalStructureFormula.Upstream1Width,
                                                  structureType);
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.Upstream2Width.Key,
                                                  generalStructureFormula.Upstream2Width,
                                                  structureType);
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.CrestWidth.Key,
                                                  structure.CrestWidth,
                                                  structureType);
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.Downstream1Width.Key,
                                                  generalStructureFormula.Downstream1Width,
                                                  structureType);
            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  StructureRegion.Downstream2Width.Key,
                                                  generalStructureFormula.Downstream2Width,
                                                  structureType);

            // Level
            properties.Add(ConstructProperty(StructureRegion.Upstream1Level.Key,
                                             generalStructureFormula.Upstream1Level, structureType));
            properties.Add(ConstructProperty(StructureRegion.Upstream2Level.Key,
                                             generalStructureFormula.Upstream2Level, structureType));

            if (structure.UseCrestLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(properties,
                                            structure,
                                            StructureRegion.CrestLevel.Key,
                                            structureType,
                                            structure.CrestLevelTimeSeries);
            }
            else
            {
                properties.Add(ConstructProperty(StructureRegion.CrestLevel.Key,
                                                 generalStructureFormula.CrestLevel,
                                                 structureType));
            }

            properties.Add(ConstructProperty(StructureRegion.Downstream1Level.Key,
                                             generalStructureFormula.Downstream1Level, structureType));
            properties.Add(ConstructProperty(StructureRegion.Downstream2Level.Key,
                                             generalStructureFormula.Downstream2Level, structureType));

            // GateLowerEdgeLevel
            if (generalStructureFormula.UseGateLowerEdgeLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(properties,
                                            structure,
                                            StructureRegion.GateLowerEdgeLevel.Key,
                                            structureType,
                                            generalStructureFormula.GateLowerEdgeLevelTimeSeries);
            }
            else
            {
                properties.Add(ConstructProperty(StructureRegion.GateLowerEdgeLevel.Key,
                                                 generalStructureFormula.GateLowerEdgeLevel,
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

            properties.Add(ConstructProperty(StructureRegion.GateHeight.Key, generalStructureFormula.GateHeight,
                                             structureType));

            // Horizontal gate opening
            if (generalStructureFormula.UseHorizontalGateOpeningWidthTimeSeries)
            {
                ConstructTimeSeriesProperty(properties,
                                            structure,
                                            KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                                            structureType,
                                            generalStructureFormula.HorizontalGateOpeningWidthTimeSeries);
            }
            else
            {
                properties.Add(ConstructProperty(KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                                                 generalStructureFormula.HorizontalGateOpeningWidth,
                                                 structureType));
            }

            string gateOpeningHorizontalDirection =
                GateOpeningDirectionToString(generalStructureFormula.GateOpeningHorizontalDirection);

            properties.Add(ConstructProperty(
                               KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription(),
                               gateOpeningHorizontalDirection,
                               structureType));

            return properties;
        }

        private IEnumerable<IniProperty> ConstructPumpProperties(IPump pump, string structureType)
        {
            var properties = new List<IniProperty>();

            if (pump.UseCapacityTimeSeries)
            {
                ConstructTimeSeriesProperty(properties,
                                            pump,
                                            KnownStructureProperties.Capacity,
                                            structureType,
                                            pump.CapacityTimeSeries);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.Capacity, pump.Capacity, structureType));
            }

            return properties;
        }

        private IEnumerable<IniProperty> ConstructSimpleWeirProperties(IStructure structure, string structureType)
        {
            var properties = new List<IniProperty>();

            if (structure.UseCrestLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(properties,
                                            structure,
                                            KnownStructureProperties.CrestLevel,
                                            structureType,
                                            structure.CrestLevelTimeSeries);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel,
                                                 structure.CrestLevel,
                                                 structureType));
            }

            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  KnownStructureProperties.CrestWidth,
                                                  structure.CrestWidth,
                                                  structureType);

            var formula = (SimpleWeirFormula)structure.Formula;
            properties.Add(ConstructProperty(KnownStructureProperties.LateralContractionCoefficient,
                                             formula.LateralContraction, structureType));
            return properties;
        }

        /// <summary>
        /// Add the specified <paramref name="value"/> property with key
        /// <paramref name="propertyKey"/> to <paramref name="properties"/> if
        /// <paramref name="value"/> is NaN or greater than zero.
        /// </summary>
        /// <param name="properties"> The properties. </param>
        /// <param name="propertyKey"> The name of the new property. </param>
        /// <param name="value"> The value to be added as property. </param>
        /// <param name="structureType"> Type of the structure. </param>
        /// <remarks>
        /// If <paramref name="value"/> is NaN then an empty value field will be written.
        /// Properties is not null
        /// </remarks>
        private void AddDoubleOrEmptyPropertyConditionally(ICollection<IniProperty> properties,
                                                           string propertyKey,
                                                           double value,
                                                           string structureType)
        {
            if (double.IsNaN(value))
            // we do not want to add an empty string as it will be filtered out in the write step.
            {
                properties.Add(ConstructProperty(propertyKey, " ", structureType));
            }
            else if (value > 0)
            {
                properties.Add(ConstructProperty(propertyKey, value, structureType));
            }
        }

        /// <summary>
        /// Construct the set of gated weir properties to be written to the
        /// structures.ini file.
        /// </summary>
        /// <param name="structure"> The Weir </param>
        /// <param name="structureType"> </param>
        /// <returns>
        /// The set of properties which should be written to structures.ini
        /// </returns>
        private IEnumerable<IniProperty> ConstructGatedWeirProperties(IStructure structure, string structureType)
        {
            var gatedWeirFormula = (IGatedStructureFormula)structure.Formula;

            var properties = new List<IniProperty>();

            if (structure.UseCrestLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(properties,
                                            structure,
                                            KnownStructureProperties.CrestLevel,
                                            structureType,
                                            structure.CrestLevelTimeSeries);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel,
                                                 structure.CrestLevel,
                                                 structureType));
            }

            if (gatedWeirFormula.UseGateLowerEdgeLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(properties,
                                            structure,
                                            KnownStructureProperties.GateLowerEdgeLevel,
                                            structureType,
                                            gatedWeirFormula.GateLowerEdgeLevelTimeSeries);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateLowerEdgeLevel,
                                                 gatedWeirFormula.GateLowerEdgeLevel,
                                                 structureType));
            }

            if (gatedWeirFormula.UseHorizontalGateOpeningWidthTimeSeries)
            {
                ConstructTimeSeriesProperty(properties,
                                            structure,
                                            KnownStructureProperties.GateOpeningWidth,
                                            structureType,
                                            gatedWeirFormula.HorizontalGateOpeningWidthTimeSeries);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningWidth,
                                                 gatedWeirFormula.HorizontalGateOpeningWidth,
                                                 structureType));
            }

            properties.Add(ConstructProperty(KnownStructureProperties.GateHeight,
                                             gatedWeirFormula.GateHeight,
                                             structureType));

            string gateOpeningHorizontalDirection =
                GateOpeningDirectionToString(gatedWeirFormula.GateOpeningHorizontalDirection);

            properties.Add(ConstructProperty(KnownStructureProperties.GateOpeningHorizontalDirection,
                                             gateOpeningHorizontalDirection,
                                             structureType));

            AddDoubleOrEmptyPropertyConditionally(properties,
                                                  KnownStructureProperties.CrestWidth,
                                                  structure.CrestWidth,
                                                  structureType);
            return properties;
        }

        private static string GateOpeningDirectionToString(GateOpeningDirection direction)
        {
            switch (direction)
            {
                case GateOpeningDirection.Symmetric:
                    return "symmetric";
                case GateOpeningDirection.FromLeft:
                    return "from_left";
                case GateOpeningDirection.FromRight:
                    return "from_right";
                default:
                    throw new ArgumentException("We can't write " + direction);
            }
        }

        private void ConstructTimeSeriesProperty(ICollection<IniProperty> properties,
                                                 IStructureObject structure,
                                                 string propertyKey,
                                                 string structureType,
                                                 TimeSeries timeSeries)
        {
            string timeFilePath = GetTimeFilePath(structure, propertyKey);

            properties.Add(ConstructProperty(propertyKey,
                                             timeFilePath,
                                             structureType));

            WriteTimeFile(GetOtherFilePathInSameDirectory(ReferencePath, timeFilePath), timeSeries);
        }

        private IniProperty ConstructProperty(string propertyKey, object value, string structureType)
        {
            ModelPropertyDefinition definition = StructureSchema.GetDefinition(structureType, propertyKey);
            var propertyValue = FMParser.ToString(value, value is ICollection ? typeof(IList<double>) : value.GetType());
            
            var property = new IniProperty(
                definition.FilePropertyKey, 
                propertyValue, 
                definition.Description);
            
            return property;
        }

        private static string DetermineType(IStructureObject structure)
        {
            switch (structure)
            {
                case IPump _:
                    return StructureRegion.StructureTypeName.Pump;
                case IStructure weir:
                    switch (weir.Formula)
                    {
                        case SimpleWeirFormula _:
                            return StructureRegion.StructureTypeName.Weir;
                        // A GatedWeir is a Gate for the Kernel, hence we specify it as a "gate".
                        case SimpleGateFormula _:
                            return StructureRegion.StructureTypeName.Gate;
                        case GeneralStructureFormula _:
                            return StructureRegion.StructureTypeName.GeneralStructure;
                    }

                    break;
            }

            throw new NotSupportedException();
        }

        #endregion

        #region FileWriting

        private string GetPliFilePath(IStructureObject structure)
        {
            if (originalDataObjects.TryGetValue(structure, out StructureDAO dao))
            {
                return dao.GetProperty(KnownStructureProperties.PolylineFile).GetValueAsString();
            }
            
            return $"{structure.Name}.pli";
        }
        
        private static void WritePliFile(string pliFilePath, IStructureObject structure)
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

        private string GetTimeFilePath(IStructureObject structure, string propertyKey)
        {
            if (originalDataObjects.TryGetValue(structure, out StructureDAO dao))
            {
                return dao.GetProperty(propertyKey).GetValueAsString();
            }
            
            return $"{structure.Name}_{propertyKey}.tim";
        }

        private void WriteTimeFile(string filePath, IFunction capacityTimeSeries)
        {
            var timeFile = new TimFile();
            string directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            timeFile.Write(filePath, capacityTimeSeries, ReferenceDate);
        }

        #endregion
    }
}