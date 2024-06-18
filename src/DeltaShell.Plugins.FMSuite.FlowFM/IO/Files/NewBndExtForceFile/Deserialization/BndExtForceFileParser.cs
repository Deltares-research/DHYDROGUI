using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization
{
    /// <summary>
    /// Parser for a boundary external forcing file (*_bnd.ext).
    /// </summary>
    public sealed class BndExtForceFileParser
    {
        private readonly BoundaryParser boundaryParser;
        private readonly LateralParser lateralParser;
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initialize a new instance of the <see cref="BndExtForceFileParser"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        public BndExtForceFileParser(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            this.logHandler = logHandler;
            boundaryParser = new BoundaryParser();
            lateralParser = new LateralParser(logHandler);
        }

        /// <summary>
        /// Parse the INI data from the boundary external forcing file to a data access object.
        /// INI sections with the header "boundary" and "lateral" can be parsed.
        /// Other sections cannot be parsed and for these a warning message is reported to the user.
        /// </summary>
        /// <param name="iniData"> The INI data from the boundary external forcing file.</param>
        /// <param name="lateralValidator"> The validator for lateral data. </param>
        /// <param name="boundaryValidator"> The validator for boundary data. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="iniData"/>, <paramref name="lateralValidator"/> or
        /// <paramref name="boundaryValidator"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// A <see cref="BndExtForceFileDTO"/> data access object that contains the parsed data of the boundary external forcing
        /// file.
        /// </returns>
        public BndExtForceFileDTO Parse(IniData iniData)
        {
            Ensure.NotNull(iniData, nameof(iniData));

            var bndExtForceFileDTO = new BndExtForceFileDTO();

            foreach (IniSection section in iniData.Sections)
            {
                ParseSection(section, bndExtForceFileDTO);
            }

            return bndExtForceFileDTO;
        }

        private void ParseSection(IniSection section,
                                  BndExtForceFileDTO bndExtForceFileDTO)
        {
            string header = section.Name;
            if (IsBoundarySection(header))
            {
                BoundaryDTO boundaryDTO = boundaryParser.Parse(section);
                bndExtForceFileDTO.AddBoundary(boundaryDTO);
            }
            else if (IsLateralSection(header))
            {
                LateralDTO lateralDTO = lateralParser.Parse(section);
                bndExtForceFileDTO.AddLateral(lateralDTO);
            }
            else
            {
                if (IsGeneralSection(header))
                {
                    return;
                }

                logHandler.ReportWarningFormat(Resources.Section_0_has_an_unknown_header_and_cannot_be_parsed_Line_1_, header, section.LineNumber);
            }
        }

        private static bool IsBoundarySection(string header) =>
            IsSection(BndExtForceFileConstants.BoundaryBlockKey, header);

        private static bool IsLateralSection(string header) =>
            IsSection(BndExtForceFileConstants.LateralBlockKey , header);

        private static bool IsGeneralSection(string header) =>
            IsSection(BndExtForceFileConstants.GeneralBlockKey, header);

        private static bool IsSection(string currentCategory, string section) => 
            currentCategory.EqualsCaseInsensitive(section);
    }
}