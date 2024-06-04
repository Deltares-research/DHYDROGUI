using System.IO;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.Guards;
using DHYDRO.Common.IO.Ini;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Formats initial field file data to an INI-formatted string.
    /// </summary>
    public sealed class InitialFieldFileFormatter
    {
        private readonly IniFormatter iniFormatter;

        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldFileFormatter"/> class.
        /// </summary>
        public InitialFieldFileFormatter()
        {
            iniFormatter = new IniFormatter
            {
                Configuration =
                {
                    WriteComments = false,
                    PropertyIndentationLevel = 4,
                    WritePropertyWithoutValue = false
                }
            };
        }

        /// <summary>
        /// Formats the specified initial field file data to an INI-formatted string.
        /// </summary>
        /// <param name="initialFieldFileData">The <see cref="InitialFieldFileData"/> to format.</param>
        /// <returns>The formatted INI string.</returns>
        /// <exception cref="System.ArgumentNullException">When <paramref name="initialFieldFileData"/> is <c>null</c>.</exception>
        public string Format(InitialFieldFileData initialFieldFileData)
        {
            Ensure.NotNull(initialFieldFileData, nameof(initialFieldFileData));

            IniData iniData = CreateIniData(initialFieldFileData);

            return iniFormatter.Format(iniData);
        }

        /// <summary>
        /// Formats the specified initial field file data to an INI-formatted string and writes it to the specified stream.
        /// </summary>
        /// <param name="initialFieldFileData">The <see cref="InitialFieldFileData"/> to format.</param>
        /// <param name="stream">The <see cref="Stream"/> to write the formatted INI data to.</param>
        /// <exception cref="System.ArgumentNullException">
        /// When <paramref name="initialFieldFileData"/> or <paramref name="stream"/> is <c>null</c>.
        /// </exception>
        public void Format(InitialFieldFileData initialFieldFileData, Stream stream)
        {
            Ensure.NotNull(initialFieldFileData, nameof(initialFieldFileData));
            Ensure.NotNull(stream, nameof(stream));

            IniData iniData = CreateIniData(initialFieldFileData);

            iniFormatter.Format(iniData, stream);
        }

        private static IniData CreateIniData(InitialFieldFileData initialFieldFileData)
        {
            var iniData = new IniData();

            iniData.AddSection(CreateGeneralSection(initialFieldFileData));

            foreach (InitialFieldData initial in initialFieldFileData.InitialConditions)
            {
                iniData.AddSection(CreateInitialConditionSection(initial));
            }

            foreach (InitialFieldData parameter in initialFieldFileData.Parameters)
            {
                iniData.AddSection(CreateParameterSection(parameter));
            }

            return iniData;
        }

        private static IniSection CreateGeneralSection(InitialFieldFileData initialFieldFileData)
        {
            var generalSection = new IniSection(InitialFieldFileConstants.Headers.General);
            generalSection.AddProperty(InitialFieldFileConstants.Keys.FileVersion, initialFieldFileData.General.FileVersion);
            generalSection.AddProperty(InitialFieldFileConstants.Keys.FileType, initialFieldFileData.General.FileType);
            return generalSection;
        }

        private static IniSection CreateInitialConditionSection(InitialFieldData initialCondition)
        {
            Ensure.NotNull(initialCondition, nameof(initialCondition));

            var iniSection = new IniSection(InitialFieldFileConstants.Headers.Initial);
            AddPropertiesToSection(initialCondition, iniSection);

            return iniSection;
        }

        private static IniSection CreateParameterSection(InitialFieldData parameter)
        {
            Ensure.NotNull(parameter, nameof(parameter));

            var iniSection = new IniSection(InitialFieldFileConstants.Headers.Parameter);
            AddPropertiesToSection(parameter, iniSection);

            return iniSection;
        }

        private static void AddPropertiesToSection(InitialFieldData initialFieldData, IniSection iniSection)
        {
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Quantity, initialFieldData.Quantity.GetDescription());
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFile, initialFieldData.DataFile);
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFileType, initialFieldData.DataFileType.GetDescription());

            if (initialFieldData.DataFileType == InitialFieldDataFileType.OneDField)
            {
                return;
            }

            iniSection.AddProperty(InitialFieldFileConstants.Keys.InterpolationMethod, initialFieldData.InterpolationMethod.GetDescription());
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Operand, initialFieldData.Operand.GetDescription());

            if (initialFieldData.InterpolationMethod == InitialFieldInterpolationMethod.Averaging)
            {
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingType, initialFieldData.AveragingType.GetDescription());
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingRelSize, initialFieldData.AveragingRelSize);
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingNumMin, initialFieldData.AveragingNumMin);
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingPercentile, initialFieldData.AveragingPercentile);
            }

            iniSection.AddProperty(CreateBooleanProperty(InitialFieldFileConstants.Keys.ExtrapolationMethod, initialFieldData.ExtrapolationMethod));
            iniSection.AddProperty(InitialFieldFileConstants.Keys.LocationType, initialFieldData.LocationType.GetDescription());

            if (initialFieldData.DataFileType == InitialFieldDataFileType.Polygon)
            {
                iniSection.AddProperty(InitialFieldFileConstants.Keys.Value, initialFieldData.Value);
            }
        }

        private static IniProperty CreateBooleanProperty(string key, bool value)
        {
            return new IniProperty(key, value ? "yes" : "no");
        }
    }
}