using System.IO;
using DHYDRO.Common.Guards;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using DHYDRO.Common.Properties;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Parses INI-formatted text to an initial field file data object.
    /// </summary>
    public sealed class InitialFieldFileParser
    {
        private readonly ILogHandler logHandler;
        private readonly IniParser iniParser;

        private InitialFieldFileData initialFieldFileData;

        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldFileParser"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="logHandler"/> is <c>null</c>.</exception>
        public InitialFieldFileParser(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            this.logHandler = logHandler;
            iniParser = new IniParser();
        }

        /// <summary>
        /// Parses INI-formatted text from the specified stream to an initial field file data object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> from which to read the INI-formatted text.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="stream"/> is <c>null</c>.</exception>
        /// <returns>An <see cref="InitialFieldFileData"/> object containing the parsed initial field file data.</returns>
        /// <remarks>
        /// INI sections with the header "Initial" or "Parameter" can be parsed.
        /// Other sections cannot be parsed and for these a warning message is reported to the user.
        /// </remarks>
        public InitialFieldFileData Parse(Stream stream)
        {
            Ensure.NotNull(stream, nameof(stream));

            initialFieldFileData = new InitialFieldFileData();

            IniData iniData = iniParser.Parse(stream);
            foreach (IniSection section in iniData.Sections)
            {
                ProcessSection(section);
            }

            return initialFieldFileData;
        }

        private void ProcessSection(IniSection section)
        {
            if (IsGeneralSection(section))
            {
                return;
            }

            if (IsInitialConditionSection(section))
            {
                AddInitialCondition(section);
                return;
            }

            if (IsParameterSection(section))
            {
                AddParameter(section);
                return;
            }

            logHandler.ReportWarning(string.Format(Resources.Section_0_has_an_unknown_header_and_cannot_be_parsed_Line_1_, section.Name, section.LineNumber));
        }

        private static bool IsGeneralSection(IniSection section)
        {
            return section.IsNameEqualTo(InitialFieldFileConstants.Headers.General);
        }

        private static bool IsInitialConditionSection(IniSection section)
        {
            return section.IsNameEqualTo(InitialFieldFileConstants.Headers.Initial);
        }

        private void AddInitialCondition(IniSection section)
        {
            InitialFieldData initialFieldData = CreateInitialField(section);
            initialFieldFileData.AddInitialCondition(initialFieldData);
        }

        private static bool IsParameterSection(IniSection section)
        {
            return section.IsNameEqualTo(InitialFieldFileConstants.Headers.Parameter);
        }

        private void AddParameter(IniSection section)
        {
            InitialFieldData initialFieldData = CreateInitialField(section);
            initialFieldFileData.AddParameter(initialFieldData);
        }

        private static InitialFieldData CreateInitialField(IniSection section)
        {
            return new InitialFieldData
            {
                LineNumber = section.LineNumber,
                Quantity = section.GetPropertyValue(InitialFieldFileConstants.Keys.Quantity, InitialFieldQuantity.None),
                DataFile = section.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile),
                DataFileType = section.GetPropertyValue(InitialFieldFileConstants.Keys.DataFileType, InitialFieldDataFileType.None),
                InterpolationMethod = section.GetPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod, InitialFieldInterpolationMethod.None),
                Operand = section.GetPropertyValue(InitialFieldFileConstants.Keys.Operand, InitialFieldOperand.Override),
                AveragingType = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingType, InitialFieldAveragingType.Mean),
                FrictionType = section.GetPropertyValue(InitialFieldFileConstants.Keys.FrictionType, InitialFieldFrictionType.Manning),
                AveragingRelSize = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingRelSize, 1.01),
                AveragingNumMin = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingNumMin, 1),
                AveragingPercentile = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingPercentile, 0.0),
                ExtrapolationMethod = section.GetPropertyValue(InitialFieldFileConstants.Keys.ExtrapolationMethod, false),
                LocationType = section.GetPropertyValue(InitialFieldFileConstants.Keys.LocationType, InitialFieldLocationType.All),
                Value = section.GetPropertyValue(InitialFieldFileConstants.Keys.Value, double.NaN)
            };
        }
    }
}