using System.IO;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Provides a formatter for the initial field file (*.ini).
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

            IniData iniData = ConvertInitialFieldFileData(initialFieldFileData);
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

            IniData iniData = ConvertInitialFieldFileData(initialFieldFileData);
            iniFormatter.Format(iniData, stream);
        }

        private static IniData ConvertInitialFieldFileData(InitialFieldFileData initialFieldFileData)
        {
            var iniData = new IniData();

            iniData.AddSection(ConvertFileInfo(initialFieldFileData.General));

            foreach (InitialFieldData initial in initialFieldFileData.InitialConditions)
            {
                iniData.AddSection(ConvertInitialCondition(initial));
            }

            foreach (InitialFieldData parameter in initialFieldFileData.Parameters)
            {
                iniData.AddSection(ConvertInitialParameter(parameter));
            }

            return iniData;
        }

        private static IniSection ConvertFileInfo(InitialFieldFileInfo fileInfo)
        {
            var generalSection = new IniSection(InitialFieldFileConstants.Headers.General);
            generalSection.AddProperty(InitialFieldFileConstants.Keys.FileVersion, fileInfo.FileVersion);
            generalSection.AddProperty(InitialFieldFileConstants.Keys.FileType, fileInfo.FileType);
            return generalSection;
        }

        private static IniSection ConvertInitialCondition(InitialFieldData initialCondition)
        {
            Ensure.NotNull(initialCondition, nameof(initialCondition));

            var iniSection = new IniSection(InitialFieldFileConstants.Headers.Initial);
            AddPropertiesToSection(initialCondition, iniSection);

            return iniSection;
        }

        private static IniSection ConvertInitialParameter(InitialFieldData parameter)
        {
            Ensure.NotNull(parameter, nameof(parameter));

            var iniSection = new IniSection(InitialFieldFileConstants.Headers.Parameter);
            AddPropertiesToSection(parameter, iniSection);

            return iniSection;
        }

        private static void AddPropertiesToSection(InitialFieldData initialFieldData, IniSection iniSection)
        {
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Quantity, initialFieldData.Quantity);
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFile, initialFieldData.DataFile);
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFileType, initialFieldData.DataFileType);

            if (initialFieldData.DataFileType == InitialFieldDataFileType.OneDField)
            {
                return;
            }

            iniSection.AddProperty(InitialFieldFileConstants.Keys.InterpolationMethod, initialFieldData.InterpolationMethod);
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Operand, initialFieldData.Operand);

            if (initialFieldData.InterpolationMethod == InitialFieldInterpolationMethod.Averaging)
            {
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingType, initialFieldData.AveragingType);
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingRelSize, initialFieldData.AveragingRelSize);
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingNumMin, initialFieldData.AveragingNumMin);
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingPercentile, initialFieldData.AveragingPercentile);
            }

            iniSection.AddProperty(InitialFieldFileConstants.Keys.ExtrapolationMethod, initialFieldData.ExtrapolationMethod ? "yes" : "no");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.LocationType, initialFieldData.LocationType);

            if (initialFieldData.DataFileType == InitialFieldDataFileType.Polygon)
            {
                iniSection.AddPropertyIf(InitialFieldFileConstants.Keys.Value, initialFieldData.Value, value => !double.IsNaN(value));
            }

            if (initialFieldData.Quantity == InitialFieldQuantity.FrictionCoefficient)
            {
                iniSection.AddProperty(InitialFieldFileConstants.Keys.FrictionType, (int)initialFieldData.FrictionType);
            }
        }
    }
}