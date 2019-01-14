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
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof (StructuresFile));

        public StructureSchema<ModelPropertyDefinition> StructureSchema { private get; set; }

        public DateTime ReferenceDate { private get; set; }

        private string TimFolder { get; set; }

        public IList<IStructure> Read(string filePath)
        {
            return
                ReadStructures2D(filePath)
                    .Select(s => ConvertStructure(s, filePath))
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

            foreach (var category in categories)
            {
                // Filter out unexpected .ini categories:
                if (category.Name != StructureCategoryName)
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

                var structure2D = CreateStructure2D(StructureSchema, structureTypeProperty.Value, category, filePath);
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

        public void Write(string filePath, IEnumerable<IStructure> structures)
        {
            new DelftIniWriter().WriteDelftIniFile(
                GetSupportedStructures(structures)
                    .Select(s => CreateDelftIniCategory(s, filePath, ReferenceDate)), filePath);
        }

        public static void WriteStructures2D(string path, IEnumerable<Structure2D> structures)
        {
            new DelftIniWriter().WriteDelftIniFile(structures.Select(CreateDelftIniCategory),path);
        }

        private IStructure ConvertStructure(Structure2D structure, string filePath, string oldFilePath = null)
        {
            try
            {
                if(oldFilePath != null && !filePath.Equals(oldFilePath))
                    CopyPolylineFile(structure.GetProperty(KnownStructureProperties.PolylineFile).GetValueAsString(), Path.GetDirectoryName(filePath), Path.GetDirectoryName(oldFilePath));
                return StructureFactory.CreateStructure(structure, filePath, ReferenceDate, oldFilePath);
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
            var delftIniCategory = new DelftIniCategory(StructureCategoryName);

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

        private DelftIniCategory CreateDelftIniCategory(IStructure structure, string filePath, DateTime refDate)
        {
            var delftIniCategory = new DelftIniCategory(StructureCategoryName);
            var delftIniProperties = CreateDelftIniProperties(structure, filePath, refDate);

            foreach (var property in delftIniProperties)
            {
                delftIniCategory.Properties.Add(property);
            }

            return delftIniCategory;
        }

        #region Sobek Structure to DelftIni related methods:

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
        
        private IEnumerable<DelftIniProperty> ConstructStructureProperties(IStructure structure, string structureType, string path, DateTime refDate)
        {
            var pump = structure as IPump;
            if(pump != null)
                return ConstructPumpProperties(pump, structureType, path, refDate);

            var weir = structure as IWeir;
            if(weir != null)
                return ConstructWeirProperties(weir, structureType, path, refDate);

            var gate = structure as IGate;
            if (gate != null)
                return ConstructGateProperties(gate, structureType, path, refDate);

            throw new NotImplementedException();
        }

        private IEnumerable<DelftIniProperty> ConstructWeirProperties(IStructure1D structure, string structureType, string path, DateTime refDate)
        {
            var weir = (IWeir)structure;
            var weirFormula = weir.WeirFormula;
            if (weirFormula is SimpleWeirFormula) return ConstructSimpleWeirProperties(weir, path, structureType, refDate);
            if (weirFormula is GeneralStructureWeirFormula) return ConstructGeneralStructureProperties(weir, path, structureType, refDate);
            if (weirFormula is GatedWeirFormula)
                return ConstructGatedWeirProperties(weir, structureType, path, refDate);
            throw new NotImplementedException();
        }

        private IEnumerable<DelftIniProperty> ConstructGeneralStructureProperties(IStructure1D structure, string path, string structureType, DateTime refDate)
        {
            var properties = new List<DelftIniProperty>();

            var weirStructure =  structure as IWeir;
            var generalStructureFormula = weirStructure.WeirFormula as GeneralStructureWeirFormula;

            // Width
            AddDoubleOrEmptyPropertyConditionally(properties
                                                , StructureRegion.WidthLeftW1.Key
                                                , generalStructureFormula
                                                      .WidthLeftSideOfStructure
                                                , structureType);
            AddDoubleOrEmptyPropertyConditionally(properties
                                                , StructureRegion.WidthLeftWsdl.Key
                                                , generalStructureFormula
                                                      .WidthStructureLeftSide
                                                , structureType);
            AddDoubleOrEmptyPropertyConditionally(properties
                                                , StructureRegion.WidthCenter.Key
                                                , weirStructure.CrestWidth
                                                , structureType);
            AddDoubleOrEmptyPropertyConditionally(properties
                                                , StructureRegion.WidthRightWsdr.Key
                                                , generalStructureFormula
                                                      .WidthStructureRightSide
                                                , structureType);
            AddDoubleOrEmptyPropertyConditionally(properties
                                                , StructureRegion.WidthRightW2.Key
                                                , generalStructureFormula
                                                      .WidthRightSideOfStructure
                                                , structureType);

            // Level
            properties.Add(ConstructProperty(StructureRegion.LevelLeftZb1.Key,   generalStructureFormula.BedLevelLeftSideOfStructure,  structureType));
            properties.Add(ConstructProperty(StructureRegion.LevelLeftZbsl.Key,  generalStructureFormula.BedLevelLeftSideStructure,    structureType));

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
                properties.Add(ConstructProperty(StructureRegion.LevelCenter.Key, generalStructureFormula.BedLevelStructureCentre, structureType));
            }

            properties.Add(ConstructProperty(StructureRegion.LevelRightZbsr.Key, generalStructureFormula.BedLevelRightSideStructure,   structureType));
            properties.Add(ConstructProperty(StructureRegion.LevelRightZb2.Key,  generalStructureFormula.BedLevelRightSideOfStructure, structureType));

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
            properties.Add(ConstructProperty(StructureRegion.PosFreeGateFlowCoeff.Key, generalStructureFormula.PositiveFreeGateFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.PosDrownGateFlowCoeff.Key, generalStructureFormula.PositiveDrownedGateFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.PosFreeWeirFlowCoeff.Key, generalStructureFormula.PositiveFreeWeirFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.PosDrownWeirFlowCoeff.Key, generalStructureFormula.PositiveDrownedWeirFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.PosContrCoefFreeGate.Key, generalStructureFormula.PositiveContractionCoefficient, structureType));

            properties.Add(ConstructProperty(StructureRegion.NegFreeGateFlowCoeff.Key, generalStructureFormula.NegativeFreeGateFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.NegDrownGateFlowCoeff.Key, generalStructureFormula.NegativeDrownedGateFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.NegFreeWeirFlowCoeff.Key, generalStructureFormula.NegativeFreeWeirFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.NegDrownWeirFlowCoeff.Key, generalStructureFormula.NegativeDrownedWeirFlow, structureType));
            properties.Add(ConstructProperty(StructureRegion.NegContrCoefFreeGate.Key, generalStructureFormula.NegativeContractionCoefficient, structureType));

            // Misc.
            var extraResistance = generalStructureFormula.UseExtraResistance ? generalStructureFormula.ExtraResistance : 0.0;
            properties.Add(ConstructProperty(StructureRegion.ExtraResistance.Key, extraResistance, structureType));

            properties.Add(ConstructProperty(StructureRegion.GateDoorHeight.Key, generalStructureFormula.DoorHeight, structureType));

            // Horizontal door opening
            if (generalStructureFormula.UseHorizontalDoorOpeningWidthTimeSeries)
            {
                ConstructTimeSeriesProperty(path, properties,
                                            weirStructure,
                                            EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.HorizontalDoorOpeningWidth),
                                            structureType,
                                            generalStructureFormula.HorizontalDoorOpeningWidthTimeSeries,
                                            refDate);
            }
            else
            {
                properties.Add(ConstructProperty(EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.HorizontalDoorOpeningWidth),
                    generalStructureFormula.HorizontalDoorOpeningWidth,
                    structureType));
            }

            string horizontalDoorOpeningDirection;
            switch (generalStructureFormula.HorizontalDoorOpeningDirection)
            {
                case GateOpeningDirection.Symmetric:
                    horizontalDoorOpeningDirection = "symmetric"; break;
                case GateOpeningDirection.FromLeft:
                    horizontalDoorOpeningDirection = "from_left"; break;
                case GateOpeningDirection.FromRight:
                    horizontalDoorOpeningDirection = "from_right"; break;
                default:
                    throw new ArgumentException("We can't write " +
                                                generalStructureFormula.HorizontalDoorOpeningDirection);
            }
            properties.Add(ConstructProperty(EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.HorizontalDoorOpeningDirection),
                                             horizontalDoorOpeningDirection,
                                             structureType));

            return properties;
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

        private IEnumerable<DelftIniProperty> ConstructSimpleWeirProperties(IStructure1D structure, string path, string structureType, DateTime refDate)
        {
            var weir = (IWeir) structure;
            var properties = new List<DelftIniProperty>();

            if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
            {
                var timeFilePath = ConstructTimeFilePath(weir, KnownStructureProperties.CrestLevel);
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), weir.CrestLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.CrestLevel, weir.CrestLevel, structureType));
            }

            AddDoubleOrEmptyPropertyConditionally(properties, KnownStructureProperties.CrestWidth, weir.CrestWidth, structureType);

            var formula = (SimpleWeirFormula)((IWeir)structure).WeirFormula;
            properties.Add(ConstructProperty(KnownStructureProperties.LateralContractionCoefficient, formula.LateralContraction, structureType));
            return properties;
        }

        /// <summary>
        /// Add the specified <paramref name="value"/> property with name
        /// <paramref name="propertyName"/> to <paramref name="properties"/> if
        /// <paramref name="value"/> is NaN or greater than zero.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="propertyName">The name of the new property.</param>
        /// <param name="value">The value to be added as property.</param>
        /// <param name="structureType">Type of the structure.</param>
        /// <remarks>
        /// If <paramref name="value"/> is NaN then an empty value field will be written.
        /// Properties is not null
        /// </remarks>
        private void AddDoubleOrEmptyPropertyConditionally(ICollection<DelftIniProperty> properties, 
                                                           string propertyName,
                                                           double value, 
                                                           string structureType)
        {
            if (double.IsNaN(value))
                // we do not want to add an empty string as it will be filtered out in the write step.
                properties.Add(ConstructProperty(propertyName, " ", structureType));
            else if (value > 0)
                properties.Add(ConstructProperty(propertyName, value, structureType));
        }

        private IEnumerable<DelftIniProperty> ConstructGateProperties(IStructure1D structure, string structureType, string path, DateTime refDate)
        {
            var gate = (IGate)structure;
            var properties = new List<DelftIniProperty>();

            if (gate.UseSillLevelTimeSeries)
            {
                var timeFilePath = ConstructTimeFilePath(gate, KnownStructureProperties.GateSillLevel);
                properties.Add(ConstructProperty(KnownStructureProperties.GateSillLevel, timeFilePath, structureType));
                WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath), gate.SillLevelTimeSeries, refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateSillLevel, gate.SillLevel, structureType));
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
            AddDoubleOrEmptyPropertyConditionally(properties, KnownStructureProperties.GateSillWidth, gate.SillWidth, structureType);

            return properties;
        }

        /// <summary>
        /// Construct the set of gated weir properties to be written to the
        /// structures.ini file.
        /// </summary>
        /// <param name="structure">The Weir </param>
        /// <param name="structureType"></param>
        /// <param name="path"></param>
        /// <param name="refDate"></param>
        /// <returns>
        /// The set of properties which should be written to structures.ini
        /// </returns>
        private IEnumerable<DelftIniProperty> ConstructGatedWeirProperties(IStructure1D structure,
                                                                           string structureType,
                                                                           string path,
                                                                           DateTime refDate)
        {
            var gatedWeir = (IWeir)structure;
            var gatedWeirFormula = (IGatedWeirFormula) gatedWeir.WeirFormula;

            var properties = new List<DelftIniProperty>();

            if (gatedWeir.UseCrestLevelTimeSeries)
            {
                ConstructTimeSeriesProperty(path, properties,
                                            gatedWeir, 
                                            KnownStructureProperties.GateSillLevel,
                                            structureType, 
                                            gatedWeir.CrestLevelTimeSeries, 
                                            refDate);
            }
            else
            {
                properties.Add(ConstructProperty(KnownStructureProperties.GateSillLevel, 
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

            properties.Add(ConstructProperty(KnownStructureProperties.GateDoorHeight, 
                                             gatedWeirFormula.DoorHeight, 
                                             structureType));

            string horizontalDoorOpeningDirection;
            switch (gatedWeirFormula.HorizontalDoorOpeningDirection)
            {
                case GateOpeningDirection.Symmetric:
                    horizontalDoorOpeningDirection = "symmetric"; break;
                case GateOpeningDirection.FromLeft:
                    horizontalDoorOpeningDirection = "from_left"; break;
                case GateOpeningDirection.FromRight:
                    horizontalDoorOpeningDirection = "from_right"; break;
                default:
                    throw new ArgumentException("We can't write " + 
                                                gatedWeirFormula.HorizontalDoorOpeningDirection);
            }
            properties.Add(ConstructProperty(KnownStructureProperties.GateHorizontalOpeningDirection, 
                                             horizontalDoorOpeningDirection, 
                                             structureType));

            AddDoubleOrEmptyPropertyConditionally(properties, KnownStructureProperties.GateSillWidth, gatedWeir.CrestWidth, structureType);
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
            var timeFilePath = ConstructTimeFilePath(structure, propertyName);
            properties.Add(
                ConstructProperty(propertyName,
                                 timeFilePath,
                                 structureType));
            WriteTimeFile(GetOtherFilePathInSameDirectory(path, timeFilePath),
                          timeSeries,
                          refDate);
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

        private DelftIniProperty ConstructProperty(string propertyName, object value, string structureType)
        {
            var definition = StructureSchema.GetDefinition(structureType, propertyName);
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
            if (structure is IPump) return StructureRegion.StructureTypeName.Pump;
            if (structure is IGate) return StructureRegion.StructureTypeName.Gate;

            var weir = structure as IWeir;
            if (weir != null)
            {
                if (weir.WeirFormula is SimpleWeirFormula) return StructureRegion.StructureTypeName.Weir;
                // A GatedWeir is a Gate for the Kernel, hence we specify it as a "gate".
                if (weir.WeirFormula is GatedWeirFormula) return StructureRegion.StructureTypeName.Gate;
                if (weir.WeirFormula is GeneralStructureWeirFormula) return StructureRegion.StructureTypeName.GeneralStructure;
            }

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

        private string ConstructTimeFilePath(IStructure1D structure, string propertyName)
        {
            var filePath = String.Format("{0}_{1}.tim", structure.Name, propertyName);
            if (TimFolder != null)
            {
                filePath = Path.Combine(TimFolder, filePath);
            }
            return filePath;
        }

        private static void WriteTimeFile(string filePath, IFunction capacityTimeSeries, DateTime refDate)
        {
            var timeFile = new TimFile();
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            timeFile.Write(filePath, capacityTimeSeries, refDate);
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
                                                            property.Name, property.LineNumber, filePath),e);
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