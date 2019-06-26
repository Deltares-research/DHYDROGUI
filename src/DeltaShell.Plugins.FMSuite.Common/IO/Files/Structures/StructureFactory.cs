using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    public static class StructureFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StructureFactory));

        private static readonly Dictionary<StructureType, Func<Structure2D, string, DateTime, IStructure1D>>
            CreateStructureType = new Dictionary<StructureType, Func<Structure2D, string, DateTime, IStructure1D>>
            {
                {StructureType.Pump, CreatePumpCore},
                {StructureType.Gate, CreateGateCore},
                {StructureType.Weir, CreateSimpleWeirCore},
                {StructureType.GeneralStructure, CreateGeneralStructureCore}
            };

        /// <summary>
        /// Constructs a <see cref="IStructure1D" /> for the following supported types:
        /// Pump, Weir, Gate
        /// </summary>
        /// <param name="structure2D"> The description of the structure. </param>
        /// <param name="structuresSubFilesReferenceFilePath"> Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu. </param>
        /// <param name="refDate"> Reference date for the model, required for time dependent data. </param>
        /// <returns> Returns the constructed structure. </returns>
        public static IStructure1D CreateStructure(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, StructureFactoryValidator.SupportedTypes);

            IStructure1D structure = null;
            structure = CreateStructureType[structure2D.StructureType](structure2D, structuresSubFilesReferenceFilePath, refDate);
            SetCommonStructureData(structure, structure2D, structuresSubFilesReferenceFilePath);

            return structure;
        }

        /// <summary>
        /// Update general structure properties.
        /// </summary>
        /// <param name="structure"> Structure to be updated. </param>
        /// <param name="structure2D"> Source of data to update <paramref name="structure" />. </param>
        /// <param name="structuresSubFilesReferenceFilePath"> Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu. </param>
        private static void SetCommonStructureData(IStructure1D structure, Structure2D structure2D, string structuresSubFilesReferenceFilePath)
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

            structure.Chainage = double.NaN;
        }

        private static void SetTimeSeriesProperty(Structure2D structure2D, string propertyName, string structuresSubFilesReferenceFilePath,
                                                  DateTime refDate, IStructure1D structure,
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
                    throw new NotImplementedException();
            }
        }

        private static void SetTimeSeriesPropertyInsideWeirFormula(Structure2D structure2D, string propertyName,
                                                                   string structuresSubFilesReferenceFilePath, DateTime refDate,
                                                                   IWeirFormula structure, string useTimeSeriesProperty,
                                                                   string constantValueProperty,
                                                                   string timeSeriesProperties)
        {
            ModelProperty property = structure2D.GetProperty(propertyName);
            if (property != null)
            {
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
                        throw new NotImplementedException();
                }
            }
        }

        #region Pump

        /// <summary>
        /// Create a pump.
        /// </summary>
        /// <param name="structure"> Source of data to create structure with. </param>
        /// <param name="path"> Filepath of the <see cref="StructuresFile" />. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile" />s. </param>
        /// <returns> The created pump. </returns>
        public static IPump CreatePump(Structure2D structure, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure, new[]
            {
                StructureType.Pump
            });

            Pump2D pump = CreatePumpCore(structure, path, refDate);
            SetCommonStructureData(pump, structure, path);

            return pump;
        }

        /// <summary>
        /// Create and set pump related data.
        /// </summary>
        /// <param name="structure"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath"> Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile" />s. </param>
        /// <returns> The created pump. </returns>
        private static Pump2D CreatePumpCore(Structure2D structure, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var pump = new Pump2D(true);

            ModelProperty property = structure.GetProperty(KnownStructureProperties.Capacity);
            if (property != null)
            {
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
                        throw new NotImplementedException();
                }
            }

            return pump;
        }
        #endregion

        #region Weir

        /// <summary>
        /// Create a weir.
        /// </summary>
        /// <param name="structure2D"> Source of data to create structure with. </param>
        /// <param name="path"> Filepath of the <see cref="StructuresFile" />. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile" />s. </param>
        /// <returns> The created weir. </returns>
        public static IWeir CreateWeir(Structure2D structure2D, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, new[]
            {
                StructureType.Weir
            });

            IWeir structure = null;
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
        /// <param name="structuresSubFilesReferenceFilePath"> Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile" />s. </param>
        /// <returns> The created simple weir. </returns>
        private static IWeir CreateSimpleWeirCore(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            Weir2D weir = CreateWeirCore(structure2D, structuresSubFilesReferenceFilePath, refDate);
            weir.WeirFormula = CreateSimpleWeirFormula(structure2D);

            return weir;
        }

        /// <summary>
        /// Create a simple weir formula
        /// </summary>
        /// <param name="structure2D"> Source of data to create the formula with. </param>
        /// <returns> The created formula </returns>
        private static IWeirFormula CreateSimpleWeirFormula(Structure2D structure2D)
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
        /// <param name="structuresSubFilesReferenceFilePath"> Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile" />s. </param>
        /// <returns> The created simple weir. </returns>
        private static IWeir CreateGeneralStructureCore(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Weir2D(true);
            weir.WeirFormula = CreateGeneralStructureWeirFormula(structure2D, structuresSubFilesReferenceFilePath, refDate);

            ModelProperty crestWidthProperty =
                structure2D.GetProperty(KnownGeneralStructureProperties.CrestWidth.GetDescription());
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);

            SetTimeSeriesProperty(structure2D, KnownGeneralStructureProperties.CrestLevel.GetDescription(), structuresSubFilesReferenceFilePath,
                                  refDate, weir,
                                  TypeUtils.GetMemberName(() => weir.UseCrestLevelTimeSeries),
                                  TypeUtils.GetMemberName(() => weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        private static IWeirFormula CreateGeneralStructureWeirFormula(Structure2D structure2D, string structuresSubFilesReferenceFilePath,
                                                                      DateTime refDate)
        {
            var gsWeirFormula = new GeneralStructureWeirFormula()
            {
                // Set default values for Structure2D general structures.
                WidthStructureLeftSide = double.NaN,
                WidthStructureRightSide = double.NaN,
                WidthLeftSideOfStructure = double.NaN,
                WidthRightSideOfStructure = double.NaN
            };

            foreach (KnownGeneralStructureProperties property in Enum
                                                                 .GetValues(typeof(KnownGeneralStructureProperties))
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

                ModelProperty structureproperty = structure2D.GetProperty(property);
                if (structureproperty == null)
                {
                    Log.WarnFormat("Property [{0}] is not supported and is skipped.", property);
                }
                else
                {
                    gsWeirFormula.SetPropertyValue(
                        property, FMParser.FromString<double>(structureproperty.GetValueAsString()));
                }
            }

            ModelProperty horizontalDoorOpeningDirectionProperty =
                structure2D.GetProperty(KnownGeneralStructureProperties.GateOpeningHorizontalDirection);
            if (horizontalDoorOpeningDirectionProperty == null)
            {
                Log.WarnFormat("Property [{0}] is not supported and is skipped.",
                               KnownGeneralStructureProperties.GateOpeningHorizontalDirection);
            }
            else
            {
                gsWeirFormula.HorizontalDoorOpeningDirection =
                    (GateOpeningDirection) horizontalDoorOpeningDirectionProperty.Value;
            }

            SetTimeSeriesPropertyInsideWeirFormula(structure2D,
                                                   KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                                                   structuresSubFilesReferenceFilePath, refDate, gsWeirFormula,
                                                   TypeUtils.GetMemberName(
                                                       () => gsWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries),
                                                   TypeUtils.GetMemberName(
                                                       () => gsWeirFormula.HorizontalDoorOpeningWidth),
                                                   TypeUtils.GetMemberName(
                                                       () => gsWeirFormula.HorizontalDoorOpeningWidthTimeSeries));

            SetTimeSeriesPropertyInsideWeirFormula(structure2D,
                                                   KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(),
                                                   structuresSubFilesReferenceFilePath, refDate, gsWeirFormula,
                                                   TypeUtils.GetMemberName(
                                                       () => gsWeirFormula.UseLowerEdgeLevelTimeSeries),
                                                   TypeUtils.GetMemberName(() => gsWeirFormula.LowerEdgeLevel),
                                                   TypeUtils.GetMemberName(
                                                       () => gsWeirFormula.LowerEdgeLevelTimeSeries));

            return gsWeirFormula;
        }

        /// <summary>
        /// Create a weir.
        /// </summary>
        /// <param name="structure2D"> Source of data to create structure with. </param>
        /// <param name="structuresSubFilesReferenceFilePath"> Filepath of the reference file. This is structures file or Mdu
        /// dependent on the PathsRelativeToParent option in the Mdu. </param>
        /// <param name="refDate"> Reference data used for <see cref="TimFile" />s. </param>
        /// <returns> The created weir. </returns>
        private static Weir2D CreateWeirCore(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Weir2D(true);
            ModelProperty crestWidthProperty = structure2D.GetProperty(KnownStructureProperties.CrestWidth);
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);
            SetTimeSeriesProperty(structure2D, KnownStructureProperties.CrestLevel, structuresSubFilesReferenceFilePath, refDate, weir,
                                  TypeUtils.GetMemberName(() => weir.UseCrestLevelTimeSeries),
                                  TypeUtils.GetMemberName(() => weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        #endregion

        #region Gate

        public static IWeir CreateGate(Structure2D structure2D, string path, DateTime refDate)
        {
            StructureFactoryValidator.ThrowIfInvalidType(structure2D, new[]
            {
                StructureType.Gate
            });

            IWeir structure = null;
            if (structure2D.StructureType == StructureType.Gate)
            {
                structure = CreateGateCore(structure2D, path, refDate);
            }

            SetCommonStructureData(structure, structure2D, path);

            return structure;
        }

        private static IWeir CreateGateCore(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var weir = new Weir2D(true);
            weir.WeirFormula = CreateGateWeirFormula(structure2D, structuresSubFilesReferenceFilePath, refDate);

            ModelProperty crestWidthProperty = structure2D.GetProperty(KnownStructureProperties.CrestWidth);
            string crestWidthString = crestWidthProperty?.GetValueAsString();
            weir.CrestWidth = string.IsNullOrEmpty(crestWidthString)
                                  ? double.NaN
                                  : FMParser.FromString<double>(crestWidthString);

            SetTimeSeriesProperty(structure2D, KnownStructureProperties.CrestLevel, structuresSubFilesReferenceFilePath, refDate, weir,
                                  TypeUtils.GetMemberName(() => weir.UseCrestLevelTimeSeries),
                                  TypeUtils.GetMemberName(() => weir.CrestLevel), weir.CrestLevelTimeSeries);

            return weir;
        }

        private static IWeirFormula CreateGateWeirFormula(Structure2D structure2D, string structuresSubFilesReferenceFilePath, DateTime refDate)
        {
            var gateWeirFormula = new GatedWeirFormula(true);

            gateWeirFormula.DoorHeight =
                FMParser.FromString<double>(
                    structure2D.GetProperty(KnownStructureProperties.GateHeight).GetValueAsString());

            ModelProperty openingDirectionProperty =
                structure2D.GetProperty(KnownStructureProperties.GateOpeningHorizontalDirection);
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

            SetTimeSeriesPropertyInsideWeirFormula(structure2D,
                                                   KnownStructureProperties.GateLowerEdgeLevel,
                                                   structuresSubFilesReferenceFilePath, refDate,
                                                   gateWeirFormula,
                                                   TypeUtils.GetMemberName(
                                                       () => gateWeirFormula.UseLowerEdgeLevelTimeSeries),
                                                   TypeUtils.GetMemberName(() => gateWeirFormula.LowerEdgeLevel),
                                                   TypeUtils.GetMemberName(
                                                       () => gateWeirFormula.LowerEdgeLevelTimeSeries));

            SetTimeSeriesPropertyInsideWeirFormula(structure2D,
                                                   KnownStructureProperties.GateOpeningWidth,
                                                   structuresSubFilesReferenceFilePath, refDate,
                                                   gateWeirFormula,
                                                   TypeUtils.GetMemberName(
                                                       () => gateWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries),
                                                   TypeUtils.GetMemberName(
                                                       () => gateWeirFormula.HorizontalDoorOpeningWidth),
                                                   TypeUtils.GetMemberName(
                                                       () => gateWeirFormula.HorizontalDoorOpeningWidthTimeSeries));

            return gateWeirFormula;
        }

        #endregion
    }
}