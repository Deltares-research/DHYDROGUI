using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization
{
    /// <summary>
    /// Parser for a initial field file.
    /// </summary>
    public sealed class InitialFieldFileParser
    {
        private readonly InitialFieldValidator initialFieldValidator;
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldFileParser"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with.</param>
        /// <param name="initialFieldValidator"> The initial field validator. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        public InitialFieldFileParser(ILogHandler logHandler, InitialFieldValidator initialFieldValidator)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(initialFieldValidator, nameof(initialFieldValidator));

            this.logHandler = logHandler;
            this.initialFieldValidator = initialFieldValidator;
        }

        /// <summary>
        /// Parse the INI data from the initial field file to a data access object.
        /// INI sections with the header "Initial" or "Parameter" can be parsed.
        /// Other sections cannot be parsed and for these a warning message is reported to the user.
        /// </summary>
        /// <param name="iniData"> The INI data from the initial field file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="iniData"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// A <see cref="InitialFieldFileData"/> data access object that contains the parsed data of the initial field file.
        /// </returns>
        public InitialFieldFileData Parse(IniData iniData)
        {
            Ensure.NotNull(iniData, nameof(iniData));

            var initialFieldFileData = new InitialFieldFileData();

            foreach (IniSection section in iniData.Sections)
            {
                ParseSection(section, initialFieldFileData);
            }

            return initialFieldFileData;
        }

        private void ParseSection(IniSection section, InitialFieldFileData initialFieldFileData)
        {
            if (IsGeneralSection(section))
            {
                return;
            }

            if (IsInitialCondition(section))
            {
                ParseInitialSection(section, initialFieldFileData);
                return;
            }

            if (IsParameter(section))
            {
                ParseParameterSection(section, initialFieldFileData);
                return;
            }

            logHandler.ReportWarning(string.Format(Resources.Section_0_has_an_unknown_header_and_cannot_be_parsed_Line_1_, section.Name, section.LineNumber));
        }

        private void ParseParameterSection(IniSection section, InitialFieldFileData initialFieldFileData)
        {
            InitialField initialField = InitialFieldParser.Parse(section);
            if (initialFieldValidator.Validate(initialField, section.LineNumber))
            {
                initialFieldFileData.AddParameter(initialField);
            }
        }

        private void ParseInitialSection(IniSection section, InitialFieldFileData initialFieldFileData)
        {
            InitialField initialField = InitialFieldParser.Parse(section);
            if (initialFieldValidator.Validate(initialField, section.LineNumber))
            {
                initialFieldFileData.AddInitialCondition(initialField);
            }
        }

        private static bool IsGeneralSection(IniSection section)
        {
            return section.IsNameEqualTo(InitialFieldFileConstants.Headers.General);
        }

        private static bool IsInitialCondition(IniSection section)
        {
            return section.IsNameEqualTo(InitialFieldFileConstants.Headers.Initial);
        }

        private static bool IsParameter(IniSection section)
        {
            return section.IsNameEqualTo(InitialFieldFileConstants.Headers.Parameter);
        }
    }
}