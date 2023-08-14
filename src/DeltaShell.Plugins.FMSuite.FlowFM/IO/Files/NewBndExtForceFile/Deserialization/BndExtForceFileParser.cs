using System.Collections.Generic;
using System.Net;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;
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
        private readonly LateralValidator lateralValidator;
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
            lateralValidator = new LateralValidator(logHandler);
        }

        /// <summary>
        /// Parse the Delft INI categories from the boundary external forcing file to a data access object.
        /// Delft INI categories with the header "boundary" can be parsed.
        /// Other categories cannot be parsed and for these a warning message is reported to the user.
        /// </summary>
        /// <param name="delftIniCategories"> The Delft INI categories from the boundary external forcing file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="delftIniCategories"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// A <see cref="BndExtForceFileDTO"/> data access object that contains the parsed data of the boundary external forcing
        /// file.
        /// </returns>
        public BndExtForceFileDTO Parse(IEnumerable<DelftIniCategory> delftIniCategories)
        {
            Ensure.NotNull(delftIniCategories, nameof(delftIniCategories));

            var bndExtForceFileDTO = new BndExtForceFileDTO();

            foreach (DelftIniCategory delftIniCategory in delftIniCategories)
            {
                ParseCategory(delftIniCategory, bndExtForceFileDTO);
            }

            return bndExtForceFileDTO;
        }

        private void ParseCategory(DelftIniCategory delftIniCategory, BndExtForceFileDTO bndExtForceFileDTO)
        {
            string header = delftIniCategory.Name;
            if (IsBoundaryCategory(header))
            {
                BoundaryDTO boundaryDTO = boundaryParser.Parse(delftIniCategory);
                bndExtForceFileDTO.AddBoundary(boundaryDTO);
            }
            else if (IsLateralCategory(header))
            {
                LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);
                if (lateralValidator.Validate(lateralDTO, delftIniCategory.LineNumber))
                {
                    bndExtForceFileDTO.AddLateral(lateralDTO);
                }
            }
            else
            {
                if (IsGeneralCategory(header))
                {
                    return;
                }
                
                logHandler.ReportWarningFormat(Resources.Category_0_has_an_unknown_header_and_cannot_be_parsed_Line_1_, header, delftIniCategory.LineNumber);
            }
        }

        private static bool IsBoundaryCategory(string header) =>
            IsCategory(BndExtForceFileConstants.BoundaryBlockKey, header);

        private static bool IsLateralCategory(string header) =>
            IsCategory(BndExtForceFileConstants.LateralBlockKey , header);

        private static bool IsGeneralCategory(string header) =>
            IsCategory(BndExtForceFileConstants.GeneralBlockKey, header);

        private static bool IsCategory(string currentCategory, string category) => 
            currentCategory.EqualsCaseInsensitive(category);
    }
}