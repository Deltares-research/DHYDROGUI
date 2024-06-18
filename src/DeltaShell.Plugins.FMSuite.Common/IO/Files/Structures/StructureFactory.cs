using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
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
    /// of <see cref="StructureDAO"/> objects.
    /// </summary>
    public static class StructureFactory
    {
        private static readonly Dictionary<StructureType, Func<StructureDAO, string, DateTime,  IStructureObject>>
            createStructureType = new Dictionary<StructureType, Func<StructureDAO, string, DateTime, IStructureObject>>
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
        /// <param name="structureDataAccessObject"> The description of the structure. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference date for the model, required for time dependent data. </param>
        /// <returns> Returns the constructed structure. </returns>
        public static IStructureObject CreateStructure(StructureDAO structureDataAccessObject, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            StructureFileValidator.ThrowIfInvalidType(structureDataAccessObject, StructureFileValidator.SupportedTypes);

            IStructureObject structure = null;
            structure = createStructureType[structureDataAccessObject.StructureType](structureDataAccessObject, structuresSubFilesReferenceFilePath, refDate);
            SetCommonStructureData(structure, structureDataAccessObject, structuresSubFilesReferenceFilePath);

            return structure;
        }

        /// <summary>
        /// Update general structure properties.
        /// </summary>
        /// <param name="structure"> Structure to be updated. </param>
        /// <param name="structureDataAccessObject"> Source of data to update <paramref name="structure"/>. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        private static void SetCommonStructureData(IStructureObject structure, StructureDAO structureDataAccessObject, string structuresSubFilesReferenceFilePath)
        {
            ModelProperty property = structureDataAccessObject.GetProperty(KnownStructureProperties.Name);
            if (property != null)
            {
                structure.Name = property.GetValueAsString();
            }

            property = structureDataAccessObject.GetProperty(KnownStructureProperties.X);
            if (property != null)
            {
                var xCoordinate = FMParser.FromString<double>(property.GetValueAsString());
                var yCoordinate =
                    FMParser.FromString<double>(structureDataAccessObject.GetProperty(KnownStructureProperties.Y).GetValueAsString());
                structure.Geometry = new Point(xCoordinate, yCoordinate);
            }

            property = structureDataAccessObject.GetProperty(KnownStructureProperties.PolylineFile);
            if (property != null)
            {
                string polylineFileName = property.GetValueAsString();
                string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresSubFilesReferenceFilePath, polylineFileName);
                var pliFile = new PliFile<Feature2D>();
                structure.Geometry = pliFile.Read(filePath).First().Geometry;
            }
        }

        private static void SetTimeSeriesProperty(StructureDAO structureDataAccessObject, string propertyName, string structuresSubFilesReferenceFilePath,
                                                  DateTime refDate, IStructure structure,
                                                  string useTimeSeriesProperty, string constantValueProperty,
                                                  TimeSeries timeSeries)
        {
            ModelProperty property = structureDataAccessObject.GetProperty(propertyName);
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
                    throw new NotImplementedException(GetNotSupportedTimeSeriesMessage(structureDataAccessObject.Name, property, steerable));
            }
        }

        private static void SetTimeSeriesPropertyInsideWeirFormula(StructureDAO structureDataAccessObject, string propertyName,
                                                                   string structuresSubFilesReferenceFilePath, DateTime refDate,
                                                                   IStructureFormula structure, string useTimeSeriesProperty,
                                                                   string constantValueProperty,
                                                                   string timeSeriesProperties)
        {
            ModelProperty property = structureDataAccessObject.GetProperty(propertyName);
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

                    throw new NotImplementedException(GetNotSupportedTimeSeriesMessage(structureDataAccessObject.Name, property, steerable));
            }
        }

        private static string GetNotSupportedTimeSeriesMessage(string structureName, ModelProperty modelProperty, Steerable steerableProperty)
        {
           return string.Format(Resources.StructureFactory_GetNotSupportedTimeSeriesMessage_Trying_to_generate_Time_series_for_2D_Structure___0___property___1__mapped_as__2___type___3__which_is_not_yet_supported_, structureName, modelProperty, modelProperty.PropertyDefinition.FilePropertyKey, steerableProperty.Mode);
        }

        #region Pump

        /// <summary>
        /// Create a pump.
        /// </summary>
        /// <param name="structure"> Source of data to create structure with. </param>
        /// <param name="path"> Filepath of the <see cref="StructuresFile"/>. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created pump. </returns>
        public static IPump CreatePump(StructureDAO structure, string path, DateTime refDate)
        {
            StructureFileValidator.ThrowIfInvalidType(structure, new[]
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
        private static Pump CreatePumpCore(StructureDAO structure, string structuresSubFilesReferenceFilePath, DateTime refDate)
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

        #region Structure

        /// <summary>
        /// Create a weir.
        /// </summary>
        /// <param name="structureDao"> Source of data to create structure with. </param>
        /// <param name="path"> Filepath of the <see cref="StructuresFile"/>. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created weir. </returns>
        public static IStructure CreateWeir(StructureDAO structureDao, string path, DateTime refDate)
        {
            StructureFileValidator.ThrowIfInvalidType(structureDao, new[]
            {
                StructureType.Weir
            });

            IStructure structure = null;
            if (structureDao.StructureType == StructureType.Weir)
            {
                structure = CreateSimpleWeirCore(structureDao, path, refDate);
            }

            SetCommonStructureData(structure, structureDao, path);

            return structure;
        }

        /// <summary>
        /// Create a simple weir.
        /// </summary>
        /// <param name="structureDao"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created simple weir. </returns>
        private static IStructure CreateSimpleWeirCore(StructureDAO structureDao, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            IStructure weir = CreateWeirCore(structureDao, structuresSubFilesReferenceFilePath, refDate);
            weir.Formula = CreateSimpleWeirFormula(structureDao);

            return weir;
        }

        /// <summary>
        /// Create a simple weir formula
        /// </summary>
        /// <param name="structureDao"> Source of data to create the formula with. </param>
        /// <returns> The created formula </returns>
        private static IStructureFormula CreateSimpleWeirFormula(StructureDAO structureDao)
        {
            var simpleWeirFormula = new SimpleWeirFormula();

            ModelProperty property = structureDao.GetProperty(KnownStructureProperties.LateralContractionCoefficient);
            if (property != null)
            {
                simpleWeirFormula.LateralContraction = FMParser.FromString<double>(property.GetValueAsString());
            }

            return simpleWeirFormula;
        }

        /// <summary>
        /// Create a weir that has a General Structure formula.
        /// </summary>
        /// <param name="structureDao"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created simple weir. </returns>
        private static IStructure CreateGeneralStructureCore(StructureDAO structureDao, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Structure
            {
                Formula = CreateGeneralStructureWeirFormula(structureDao, structuresSubFilesReferenceFilePath, refDate)
            };

            ModelProperty crestWidthProperty =
                structureDao.GetProperty(KnownGeneralStructureProperties.CrestWidth.GetDescription());
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);

            SetTimeSeriesProperty(structureDao, KnownGeneralStructureProperties.CrestLevel.GetDescription(), structuresSubFilesReferenceFilePath,
                                  refDate, weir,
                                  nameof(weir.UseCrestLevelTimeSeries),
                                  nameof(weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        private static IStructureFormula CreateGeneralStructureWeirFormula(StructureDAO structureDao, string structuresSubFilesReferenceFilePath,
                                                                      DateTime refDate)
        {
            var gsWeirFormula = new GeneralStructureFormula()
            {
                // Set default values for Structure2D general structures.
                Upstream2Width = double.NaN,
                Downstream1Width = double.NaN,
                Upstream1Width = double.NaN,
                Downstream2Width = double.NaN
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

                ModelProperty structureProperty = structureDao.GetProperty(property);
                if (structureProperty != null)
                {
                    gsWeirFormula.SetPropertyValue(
                        property, FMParser.FromString<double>(structureProperty.GetValueAsString()));
                }
            }

            ModelProperty gateOpeningHorizontalDirectionProperty =
                structureDao.GetProperty(KnownGeneralStructureProperties.GateOpeningHorizontalDirection);
            if (gateOpeningHorizontalDirectionProperty != null)
            {
                gsWeirFormula.GateOpeningHorizontalDirection =
                    (GateOpeningDirection) gateOpeningHorizontalDirectionProperty.Value;
            }

            SetTimeSeriesPropertyInsideWeirFormula(structureDao,
                                                   KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                                                   structuresSubFilesReferenceFilePath, refDate, gsWeirFormula,
                                                   nameof(gsWeirFormula.UseHorizontalGateOpeningWidthTimeSeries),
                                                   nameof(gsWeirFormula.HorizontalGateOpeningWidth),
                                                   nameof(gsWeirFormula.HorizontalGateOpeningWidthTimeSeries));

            SetTimeSeriesPropertyInsideWeirFormula(structureDao,
                                                   KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(),
                                                   structuresSubFilesReferenceFilePath, refDate, gsWeirFormula,
                                                   nameof(gsWeirFormula.UseGateLowerEdgeLevelTimeSeries),
                                                   nameof(gsWeirFormula.GateLowerEdgeLevel),
                                                   nameof(gsWeirFormula.GateLowerEdgeLevelTimeSeries));

            return gsWeirFormula;
        }

        /// <summary>
        /// Create a weir.
        /// </summary>
        /// <param name="structureDao"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath">
        /// Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu.
        /// </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile"/>s. </param>
        /// <returns> The created weir. </returns>
        private static IStructure CreateWeirCore(StructureDAO structureDao, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Structure();
            ModelProperty crestWidthProperty = structureDao.GetProperty(KnownStructureProperties.CrestWidth);
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);
            SetTimeSeriesProperty(structureDao, KnownStructureProperties.CrestLevel, structuresSubFilesReferenceFilePath, refDate, weir,
                                  nameof(weir.UseCrestLevelTimeSeries),
                                  nameof(weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        #endregion

        #region Gate

        public static IStructure CreateGate(StructureDAO structureDataAccessObject, string path, DateTime refDate)
        {
            StructureFileValidator.ThrowIfInvalidType(structureDataAccessObject, new[]
            {
                StructureType.Gate
            });

            IStructure structure = null;
            if (structureDataAccessObject.StructureType == StructureType.Gate)
            {
                structure = CreateGateCore(structureDataAccessObject, path, refDate);
            }

            SetCommonStructureData(structure, structureDataAccessObject, path);

            return structure;
        }

        private static IStructure CreateGateCore(StructureDAO structureDataAccessObject, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Structure();
            weir.Formula = CreateGateWeirFormula(structureDataAccessObject, structuresSubFilesReferenceFilePath, refDate);

            ModelProperty crestWidthProperty = structureDataAccessObject.GetProperty(KnownStructureProperties.CrestWidth);
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);

            SetTimeSeriesProperty(structureDataAccessObject, KnownStructureProperties.CrestLevel, structuresSubFilesReferenceFilePath, refDate, weir,
                                  nameof(weir.UseCrestLevelTimeSeries),
                                  nameof(weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        private static IStructureFormula CreateGateWeirFormula(StructureDAO structureDataAccessObject, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var gateWeirFormula = new SimpleGateFormula(true);

            ModelProperty gateHeightProperty = structureDataAccessObject.GetProperty(KnownStructureProperties.GateHeight);
            if (gateHeightProperty != null)
            {
                gateWeirFormula.GateHeight = FMParser.FromString<double>(gateHeightProperty.GetValueAsString());
            }

            ModelProperty openingDirectionProperty = structureDataAccessObject.GetProperty(KnownStructureProperties.GateOpeningHorizontalDirection);
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
                        gateWeirFormula.GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric;
                        break;
                    case "from_left":
                        gateWeirFormula.GateOpeningHorizontalDirection = GateOpeningDirection.FromLeft;
                        break;
                    case "from_right":
                        gateWeirFormula.GateOpeningHorizontalDirection = GateOpeningDirection.FromRight;
                        break;
                    default:
                        throw new ArgumentException("Could not parse horizontal gate opening direction of type: " + displayName);
                }
            }

            SetTimeSeriesPropertyInsideWeirFormula(structureDataAccessObject,
                                                   KnownStructureProperties.GateLowerEdgeLevel,
                                                   structuresSubFilesReferenceFilePath, refDate,
                                                   gateWeirFormula,
                                                   nameof(gateWeirFormula.UseGateLowerEdgeLevelTimeSeries),
                                                   nameof(gateWeirFormula.GateLowerEdgeLevel),
                                                   nameof(gateWeirFormula.GateLowerEdgeLevelTimeSeries));

            SetTimeSeriesPropertyInsideWeirFormula(structureDataAccessObject,
                                                   KnownStructureProperties.GateOpeningWidth,
                                                   structuresSubFilesReferenceFilePath, refDate,
                                                   gateWeirFormula,
                                                   nameof(gateWeirFormula.UseHorizontalGateOpeningWidthTimeSeries),
                                                   nameof(gateWeirFormula.HorizontalGateOpeningWidth),
                                                   nameof(gateWeirFormula.HorizontalGateOpeningWidthTimeSeries));

            return gateWeirFormula;
        }

        #endregion
    }
}