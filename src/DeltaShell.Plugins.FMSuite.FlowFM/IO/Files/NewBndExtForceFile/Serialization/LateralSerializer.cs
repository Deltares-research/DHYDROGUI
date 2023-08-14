using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization
{
    /// <summary>
    /// Class for serializing a <see cref="Lateral"/> into a <see cref="DelftIniCategory"/>.
    /// </summary>
    public sealed class LateralSerializer
    {
        private const string doubleFormat = "e7";
        private readonly LateralToDTOConverter lateralToDTOConverter;

        /// <summary>
        /// Initialize a new instance of the <see cref="LateralSerializer"/> class.
        /// </summary>
        public LateralSerializer()
        {
            lateralToDTOConverter = new LateralToDTOConverter();
        }

        /// <summary>
        /// Serialize the provided lateral to a <see cref="DelftIniCategory"/>.
        /// </summary>
        /// <param name="lateral"> The lateral to serialize. </param>
        /// <returns>
        /// A new <see cref="DelftIniCategory"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="lateral"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the discharge type of the <paramref name="lateral"/> is not a defined <see cref="LateralDischargeType"/>.
        /// </exception>
        public DelftIniCategory Serialize(Lateral lateral)
        {
            Ensure.NotNull(lateral, nameof(lateral));

            LateralDTO lateralDTO = lateralToDTOConverter.Convert(lateral);
            DelftIniCategory lateralCategory = Serialize(lateralDTO);
            return lateralCategory;
        }

        private static DelftIniCategory Serialize(LateralDTO lateralDTO)
        {
            Ensure.NotNull(lateralDTO, nameof(lateralDTO));

            var category = new DelftIniCategory(BndExtForceFileConstants.LateralBlockKey);

            SerializeStringProperty(category, BndExtForceFileConstants.IdKey, lateralDTO.Id);
            SerializeStringProperty(category, BndExtForceFileConstants.NameKey, lateralDTO.Name);
            SerializeLateralForcingType(category, BndExtForceFileConstants.TypeKey, lateralDTO.Type);
            SerializeLateralLocationType(category, BndExtForceFileConstants.LocationTypeKey, lateralDTO.LocationType);
            SerializeIntProperty(category, BndExtForceFileConstants.NumCoordinatesKey, lateralDTO.NumCoordinates);
            SerializeDoublesProperty(category, BndExtForceFileConstants.XCoordinatesKey, lateralDTO.XCoordinates);
            SerializeDoublesProperty(category, BndExtForceFileConstants.YCoordinatesKey, lateralDTO.YCoordinates);
            SerializeSteerableProperty(category, BndExtForceFileConstants.DischargeKey, lateralDTO.Discharge);

            return category;
        }

        private static void SerializeStringProperty(DelftIniCategory category, string key, string value)
        {
            if (value != null)
            {
                category.AddProperty(key, value);
            }
        }

        private static void SerializeLateralForcingType(DelftIniCategory category, string key, LateralForcingType value)
        {
            if (value != LateralForcingType.None)
            {
                SerializeEnumProperty<LateralForcingType>(category, key, value);
            }
        }

        private static void SerializeLateralLocationType(DelftIniCategory category, string key, LateralLocationType value)
        {
            if (value != LateralLocationType.None)
            {
                SerializeEnumProperty<LateralLocationType>(category, key, value);
            }
        }

        private static void SerializeEnumProperty<T>(DelftIniCategory category, string key, T? value) where T : struct, Enum
        {
            category.AddProperty(key, value.GetDescription());
        }

        private static void SerializeIntProperty(DelftIniCategory category, string key, int? value)
        {
            if (value != null)
            {
                category.AddProperty(key, value.Value);
            }
        }

        private static void SerializeDoublesProperty(DelftIniCategory category, string key, IEnumerable<double> value)
        {
            if (value != null)
            {
                category.AddProperty(key, string.Join(" ", value.Select(SerializeDouble)));
            }
        }

        private static void SerializeSteerableProperty(DelftIniCategory category, string key, Steerable value)
        {
            if (value != null)
            {
                category.AddProperty(key, SerializeSteerable(value));
            }
        }

        private static string SerializeSteerable(Steerable value)
        {
            switch (value.Mode)
            {
                case SteerableMode.ConstantValue:
                    return SerializeDouble(value.ConstantValue);
                case SteerableMode.TimeSeries:
                    return value.TimeSeriesFilename;
                case SteerableMode.External:
                    return BndExtForceFileConstants.RealTimeValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), $@"Mode of {nameof(value)} is out of range.");
            }
        }

        private static string SerializeDouble(double value) => value.ToString(doubleFormat, CultureInfo.InvariantCulture);
    }
}