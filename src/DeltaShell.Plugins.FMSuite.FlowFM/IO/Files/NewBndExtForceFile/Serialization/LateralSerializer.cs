using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization
{
    /// <summary>
    /// Class for serializing a <see cref="Lateral"/> into a <see cref="IniSection"/>.
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
        /// Serialize the provided lateral to a <see cref="IniSection"/>.
        /// </summary>
        /// <param name="lateral"> The lateral to serialize. </param>
        /// <returns>
        /// A new <see cref="IniSection"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="lateral"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the discharge type of the <paramref name="lateral"/> is not a defined <see cref="LateralDischargeType"/>.
        /// </exception>
        public IniSection Serialize(Lateral lateral)
        {
            Ensure.NotNull(lateral, nameof(lateral));

            LateralDTO lateralDTO = lateralToDTOConverter.Convert(lateral);
            IniSection lateralSection = Serialize(lateralDTO);
            return lateralSection;
        }

        private static IniSection Serialize(LateralDTO lateralDTO)
        {
            Ensure.NotNull(lateralDTO, nameof(lateralDTO));

            var section = new IniSection(BndExtForceFileConstants.LateralBlockKey);

            SerializeStringProperty(section, BndExtForceFileConstants.IdKey, lateralDTO.Id);
            SerializeStringProperty(section, BndExtForceFileConstants.NameKey, lateralDTO.Name);
            SerializeLateralForcingType(section, BndExtForceFileConstants.TypeKey, lateralDTO.Type);
            SerializeLateralLocationType(section, BndExtForceFileConstants.LocationTypeKey, lateralDTO.LocationType);
            SerializeIntProperty(section, BndExtForceFileConstants.NumCoordinatesKey, lateralDTO.NumCoordinates);
            SerializeDoublesProperty(section, BndExtForceFileConstants.XCoordinatesKey, lateralDTO.XCoordinates);
            SerializeDoublesProperty(section, BndExtForceFileConstants.YCoordinatesKey, lateralDTO.YCoordinates);
            SerializeSteerableProperty(section, BndExtForceFileConstants.DischargeKey, lateralDTO.Discharge);

            return section;
        }

        private static void SerializeStringProperty(IniSection section, string key, string value)
        {
            if (value != null)
            {
                section.AddProperty(key, value);
            }
        }

        private static void SerializeLateralForcingType(IniSection section, string key, LateralForcingType value)
        {
            if (value != LateralForcingType.None)
            {
                SerializeEnumProperty<LateralForcingType>(section, key, value);
            }
        }

        private static void SerializeLateralLocationType(IniSection section, string key, LateralLocationType value)
        {
            if (value != LateralLocationType.None)
            {
                SerializeEnumProperty<LateralLocationType>(section, key, value);
            }
        }

        private static void SerializeEnumProperty<T>(IniSection section, string key, T? value) where T : struct, Enum
        {
            section.AddProperty(key, value.GetDescription());
        }

        private static void SerializeIntProperty(IniSection section, string key, int? value)
        {
            if (value != null)
            {
                section.AddProperty(key, value.Value);
            }
        }

        private static void SerializeDoublesProperty(IniSection section, string key, IEnumerable<double> value)
        {
            if (value != null)
            {
                section.AddProperty(key, string.Join(" ", value.Select(SerializeDouble)));
            }
        }

        private static void SerializeSteerableProperty(IniSection section, string key, Steerable value)
        {
            if (value != null)
            {
                section.AddProperty(key, SerializeSteerable(value));
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