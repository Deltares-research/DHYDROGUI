using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    /// <summary>
    /// <see cref="StructureFactory"/> facilitates the construction of <see cref="IStructureObject"/> out
    /// of <see cref="Structure2D"/> objects.
    /// </summary>
    public static class StructureFactory
    {
        private static readonly Dictionary<StructureType, Func<Structure2D, string, DateTime,  IStructureObject>>
            createStructureType = new Dictionary<StructureType, Func<Structure2D, string, DateTime, IStructureObject>>
            {
                {StructureType.Pump, CreatePumpCore},
                {StructureType.Gate, CreateGateCore},
                {StructureType.Weir, CreateSimpleWeirCore},
                {StructureType.GeneralStructure, CreateGeneralStructureCore}
            };

        /// <summary>
        /// Constructs a <see cref="IStructure"/> for the following supported types:
        /// Pump, Weir, Gate
        /// </summary>
        /// <param name="structure2D"> The description of the structure. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference date for the model, required for time dependent data. </param>
        /// <returns> Returns the constructed structure. </returns>
        public static IStructureObject CreateStructure(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, StructureFactoryValidator.SupportedTypes);

            IStructureObject structure = null;
            structure = createStructureType[structure2D.StructureType](structure2D, structuresSubFilesReferenceFilePath, refDate);
            SetCommonStructureData(structure, structure2D, structuresSubFilesReferenceFilePath);

            return structure;
        }

        /// <summary>
        /// Update general structure properties.
        /// </summary>
        /// <param name="structure"> Structure to be updated. </param>
        /// <param name="structure2D"> Source of data to update <paramref name="structure"/>. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        private static void SetCommonStructureData(IStructureObject structure, Structure2D structure2D, string structuresSubFilesReferenceFilePath)
        {
            ModelProperty property = structure2D.GetProperty(KnownStructureProperties.Name);
            if (property != null)
            {
                structure.Name = property.GetValueAsString();
            }

            property = structure2D.GetProperty(KnownStructureProperties.X);
            if (property != null)
            {
                var xCoordinate = FMParser.FromString<double>(property.GetValueAsString());
                var yCoordinate =
                    FMParser.FromString<double>(structure2D.GetProperty(KnownStructureProperties.Y).GetValueAsString());
                structure.Geometry = new Point(xCoordinate, yCoordinate);
            }

            property = structure2D.GetProperty(KnownStructureProperties.PolylineFile);
            if (property != null)
            {
                string polylineFileName = property.GetValueAsString();
                string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresSubFilesReferenceFilePath, polylineFileName);
                var pliFile = new PliFile<Feature2D>();
                structure.Geometry = pliFile.Read(filePath).First().Geometry;
            }
        }

        private static void SetTimeSeriesProperty(Structure2D structure2D, string propertyName, string structuresSubFilesReferenceFilePath,
                                                  DateTime refDate, IStructure structure,
                                                  string useTimeSeriesProperty, string constantValueProperty,
                                                  TimeSeries timeSeries)
        {
            ModelProperty property = structure2D.GetProperty(propertyName);
            if (property == null)
            {
                return;
            }

            var steerable = (Steerable) property.Value;
            switch (steerable.Mode)
            {
                case SteerableMode.ConstantValue:
                    TypeUtils.SetPropertyValue(structure, useTimeSeriesProperty, false);
                    TypeUtils.SetPropertyValue(structure, constantValueProperty, steerable.ConstantValue);
                    break;
                case SteerableMode.TimeSeries:
                    TypeUtils.SetPropertyValue(structure, useTimeSeriesProperty, true);
                    string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresSubFilesReferenceFilePath, steerable.TimeSeriesFilename);
                    var reader = new TimFile();
                    reader.Read(filePath, timeSeries, refDate);
                    break;
                default:
                    throw new NotImplementedException(GetNotSupportedTimeSeriesMessage(structure2D.Name, property, steerable));
            }
        }

        private static void SetTimeSeriesPropertyInsideWeirFormula(Structure2D structure2D, string propertyName,
                                                                   string structuresSubFilesReferenceFilePath, DateTime refDate,
                                                                   IStructureFormula structure, string useTimeSeriesProperty,
                                                                   string constantValueProperty,
                                                                   string timeSeriesProperties)
        {
            ModelProperty property = structure2D.GetProperty(propertyName);
            if (property == null)
            {
                return;
            }

            var steerable = (Steerable) property.Value;
            switch (steerable.Mode)
            {
                case SteerableMode.ConstantValue:
                    TypeUtils.SetPropertyValue(structure, useTimeSeriesProperty, false);
                    TypeUtils.SetPropertyValue(structure, constantValueProperty, steerable.ConstantValue);
                    break;
                case SteerableMode.TimeSeries:
                    TypeUtils.SetPropertyValue(structure, useTimeSeriesProperty, true);
                    var timeSeries =
                        (TimeSeries) TypeUtils.GetPropertyValue(structure, timeSeriesProperties, false);

                    string filePath =
                        NGHSFileBase.GetOtherFilePathInSameDirectory(structuresSubFilesReferenceFilePath, steerable.TimeSeriesFilename);
                    var reader = new TimFile();
                    reader.Read(filePath, timeSeries, refDate);
                    break;
                default:

                    throw new NotImplementedException(GetNotSupportedTimeSeriesMessage(structure2D.Name, property, steerable));
            }
        }

        private static string GetNotSupportedTimeSeriesMessage(string structureName, ModelProperty modelProperty, Steerable steerableProperty)
        {
           return string.Format(Resources.StructureFactory_GetNotSupportedTimeSeriesMessage_Trying_to_generate_Time_series_for_2D_Structure___0___property___1__mapped_as__2___type___3__which_is_not_yet_supported_, structureName, modelProperty, modelProperty.PropertyDefinition.FilePropertyName, steerableProperty.Mode);
        }

        #region Pump

        /// <summary>
        /// Create a pump.
        /// </summary>
        /// <param name="structure"> Source of data to create structure with. </param>
        /// <param name="path"> Filepath of the <see cref="StructuresFile"/>. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created pump. </returns>
        public static IPump CreatePump(Structure2D structure, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure, new[]
            {
                StructureType.Pump
            });

            Pump pump = CreatePumpCore(structure, path, refDate);
            SetCommonStructureData(pump, structure, path);

            return pump;
        }

        /// <summary>
        /// Create and set pump related data.
        /// </summary>
        /// <param name="structure"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created pump. </returns>
        private static Pump CreatePumpCore(Structure2D structure, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var pump = new Pump();

            ModelProperty property = structure.GetProperty(KnownStructureProperties.Capacity);
            
            if (property == null)
            {
                return pump;
            }

            var steerable = (Steerable) property.Value;
            switch (steerable.Mode)
            {
                case SteerableMode.ConstantValue:
                    pump.UseCapacityTimeSeries = false;
                    pump.Capacity = steerable.ConstantValue;
                    break;
                case SteerableMode.TimeSeries:
                    pump.UseCapacityTimeSeries = true;
                    string filePath =
                        NGHSFileBase.GetOtherFilePathInSameDirectory(structuresSubFilesReferenceFilePath, steerable.TimeSeriesFilename);
                    var timFile = new TimFile();
                    timFile.Read(filePath, pump.CapacityTimeSeries, refDate);
                    break;
                default:
                    throw new NotImplementedException(GetNotSupportedTimeSeriesMessage(structure.Name, property, steerable));
            }

            return pump;
        }

        #endregion

        #region Weir

        /// <summary>
        /// Create a weir.
        /// </summary>
        /// <param name="structure2D"> Source of data to create structure with. </param>
        /// <param name="path"> Filepath of the <see cref="StructuresFile"/>. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created weir. </returns>
        public static IStructure CreateWeir(Structure2D structure2D, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, new[]
            {
                StructureType.Weir
            });

            IStructure structure = null;
            if (structure2D.StructureType == StructureType.Weir)
            {
                structure = CreateSimpleWeirCore(structure2D, path, refDate);
            }

            SetCommonStructureData(structure, structure2D, path);

            return structure;
        }

        /// <summary>
        /// Create a simple weir.
        /// </summary>
        /// <param name="structure2D"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created simple weir. </returns>
        private static IStructure CreateSimpleWeirCore(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            IStructure weir = CreateWeirCore(structure2D, structuresSubFilesReferenceFilePath, refDate);
            weir.Formula = CreateSimpleWeirFormula(structure2D);

            return weir;
        }

        /// <summary>
        /// Create a simple weir formula
        /// </summary>
        /// <param name="structure2D"> Source of data to create the formula with. </param>
        /// <returns> The created formula </returns>
        private static IStructureFormula CreateSimpleWeirFormula(Structure2D structure2D)
        {
            var simpleWeirFormula = new SimpleWeirFormula();

            ModelProperty property = structure2D.GetProperty(KnownStructureProperties.LateralContractionCoefficient);
            if (property != null)
            {
                simpleWeirFormula.LateralContraction = FMParser.FromString<double>(property.GetValueAsString());
            }

            return simpleWeirFormula;
        }

        /// <summary>
        /// Create a weir that has a General Structure formula.
        /// </summary>
        /// <param name="structure2D"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created simple weir. </returns>
        private static IStructure CreateGeneralStructureCore(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Structure
            {
                Formula = CreateGeneralStructureWeirFormula(structure2D, structuresSubFilesReferenceFilePath, refDate)
            };

            ModelProperty crestWidthProperty =
                structure2D.GetProperty(KnownGeneralStructureProperties.CrestWidth.GetDescription());
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);

            SetTimeSeriesProperty(structure2D, KnownGeneralStructureProperties.CrestLevel.GetDescription(), structuresSubFilesReferenceFilePath,
                                  refDate, weir,
                                  nameof(weir.UseCrestLevelTimeSeries),
                                  nameof(weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        private static IStructureFormula CreateGeneralStructureWeirFormula(Structure2D structure2D, string structuresSubFilesReferenceFilePath,
                                                                      DateTime refDate)
        {
            var gsWeirFormula = new GeneralStructureFormula()
            {
                // Set default values for Structure2D general structures.
                WidthStructureLeftSide = double.NaN,
                WidthStructureRightSide = double.NaN,
                WidthLeftSideOfStructure = double.NaN,
                WidthRightSideOfStructure = double.NaN
            };

            foreach (KnownGeneralStructureProperties property in 
                Enum.GetValues(typeof(KnownGeneralStructureProperties))
                    .Cast<KnownGeneralStructureProperties>())
            {
                //Ignored, because they are already set in the general weir or they could be timeseries or the variable is not a double, but an enum.
                if (property == KnownGeneralStructureProperties.GateLowerEdgeLevel ||
                    property == KnownGeneralStructureProperties.GateOpeningWidth ||
                    property == KnownGeneralStructureProperties.GateOpeningHorizontalDirection ||
                    property == KnownGeneralStructureProperties.CrestLevel ||
                    property == KnownGeneralStructureProperties.CrestWidth)
                {
                    continue;
                }

                ModelProperty structureProperty = structure2D.GetProperty(property);
                if (structureProperty != null)
                {
                    gsWeirFormula.SetPropertyValue(
                        property, FMParser.FromString<double>(structureProperty.GetValueAsString()));
                }
            }

            ModelProperty horizontalDoorOpeningDirectionProperty =
                structure2D.GetProperty(KnownGeneralStructureProperties.GateOpeningHorizontalDirection);
            if (horizontalDoorOpeningDirectionProperty != null)
            {
                gsWeirFormula.HorizontalDoorOpeningDirection =
                    (GateOpeningDirection) horizontalDoorOpeningDirectionProperty.Value;
            }

            SetTimeSeriesPropertyInsideWeirFormula(structure2D,
                                                   KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                                                   structuresSubFilesReferenceFilePath, refDate, gsWeirFormula,
                                                   nameof(gsWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries),
                                                   nameof(gsWeirFormula.HorizontalDoorOpeningWidth),
                                                   nameof(gsWeirFormula.HorizontalDoorOpeningWidthTimeSeries));

            SetTimeSeriesPropertyInsideWeirFormula(structure2D,
                                                   KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(),
                                                   structuresSubFilesReferenceFilePath, refDate, gsWeirFormula,
                                                   nameof(gsWeirFormula.UseLowerEdgeLevelTimeSeries),
                                                   nameof(gsWeirFormula.LowerEdgeLevel),
                                                   nameof(gsWeirFormula.LowerEdgeLevelTimeSeries));

            return gsWeirFormula;
        }

        /// <summary>
        /// Create a weir.
        /// </summary>
        /// <param name="structure2D"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created weir. </returns>
        private static IStructure CreateWeirCore(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Structure();
            ModelProperty crestWidthProperty = structure2D.GetProperty(KnownStructureProperties.CrestWidth);
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);
            SetTimeSeriesProperty(structure2D, KnownStructureProperties.CrestLevel, structuresSubFilesReferenceFilePath, refDate, weir,
                                  nameof(weir.UseCrestLevelTimeSeries),
                                  nameof(weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        #endregion

        #region Gate

        public static IStructure CreateGate(Structure2D structure2D, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, new[]
            {
                StructureType.Gate
            });

            IStructure structure = null;
            if (structure2D.StructureType == StructureType.Gate)
            {
                structure = CreateGateCore(structure2D, path, refDate);
            }

            SetCommonStructureData(structure, structure2D, path);

            return structure;
        }

        private static IStructure CreateGateCore(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Structure();
            weir.Formula = CreateGateWeirFormula(structure2D, structuresSubFilesReferenceFilePath, refDate);

            ModelProperty crestWidthProperty = structure2D.GetProperty(KnownStructureProperties.CrestWidth);
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);

            SetTimeSeriesProperty(structure2D, KnownStructureProperties.CrestLevel, structuresSubFilesReferenceFilePath, refDate, weir,
                                  nameof(weir.UseCrestLevelTimeSeries),
                                  nameof(weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        private static IStructureFormula CreateGateWeirFormula(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var gateWeirFormula = new SimpleGateFormula(true);

            ModelProperty gateHeightProperty = structure2D.GetProperty(KnownStructureProperties.GateHeight);
            if (gateHeightProperty != null)
            {
                gateWeirFormula.DoorHeight = FMParser.FromString<double>(gateHeightProperty.GetValueAsString());
            }

            ModelProperty openingDirectionProperty = structure2D.GetProperty(KnownStructureProperties.GateOpeningHorizontalDirection);
            if (openingDirectionProperty != null)
            {
                var openingDirectionValue =
                    (Enum)
                    FMParser.FromString(openingDirectionProperty.GetValueAsString(),
                                        openingDirectionProperty.PropertyDefinition.DataType);
                string displayName = openingDirectionValue.GetDisplayName();
                switch (displayName)
                {
                    case "symmetric":
                        gateWeirFormula.HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric;
                        break;
                    case "from_left":
                        gateWeirFormula.HorizontalDoorOpeningDirection = GateOpeningDirection.FromLeft;
                        break;
                    case "from_right":
                        gateWeirFormula.HorizontalDoorOpeningDirection = GateOpeningDirection.FromRight;
                        break;
                    default:
                        throw new ArgumentException("Could not parse horizontal_opening_direction of type: " + displayName);
                }
            }

            SetTimeSeriesPropertyInsideWeirFormula(structure2D,
                                                   KnownStructureProperties.GateLowerEdgeLevel,
                                                   structuresSubFilesReferenceFilePath, refDate,
                                                   gateWeirFormula,
                                                   nameof(gateWeirFormula.UseLowerEdgeLevelTimeSeries),
                                                   nameof(gateWeirFormula.LowerEdgeLevel),
                                                   nameof(gateWeirFormula.LowerEdgeLevelTimeSeries));

            SetTimeSeriesPropertyInsideWeirFormula(structure2D,
                                                   KnownStructureProperties.GateOpeningWidth,
                                                   structuresSubFilesReferenceFilePath, refDate,
                                                   gateWeirFormula,
                                                   nameof(gateWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries),
                                                   nameof(gateWeirFormula.HorizontalDoorOpeningWidth),
                                                   nameof(gateWeirFormula.HorizontalDoorOpeningWidthTimeSeries));

            return gateWeirFormula;
        }

        #endregion
    }
}