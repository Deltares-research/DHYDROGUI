using System;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Provider of <see cref="IRRBoundaryConditionsDataParser"/> parser based on <see cref="BcBlockData"/>.
    /// </summary>
    public class RRBoundaryConditionsDataParserProvider
    {
        private const string timeSeries = BoundaryRegion.FunctionStrings.TimeSeries;
        private const string constant = BoundaryRegion.FunctionStrings.Constant;
        private readonly ILogHandler logHandler;
        private readonly IBcSectionParser bcSectionParser;

        /// <summary>
        /// Constructor of provider of data parsers for parsing <see cref="BcBlockData"/>.
        /// </summary>
        /// <param name="logHandler">Log handler to log information.</param>
        /// <param name="bcSectionParser">Category parser for bc data.</param>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="logHandler"/> or <paramref name="bcSectionParser"/> is <c>null</c>.
        /// </exception>
        public RRBoundaryConditionsDataParserProvider(ILogHandler logHandler, IBcSectionParser bcSectionParser)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(bcSectionParser, nameof(bcSectionParser));
            this.logHandler = logHandler;
            this.bcSectionParser = bcSectionParser;
        }

        /// <summary>
        /// Retrieve the appropriate parser based on the function type of the given <see cref="BcBlockData"/>.
        /// </summary>
        /// <param name="bcBlockData">Data for which a parser is retrieved.</param>
        /// <returns><see cref="IRRBoundaryConditionsDataParser"/> Parser appropriate for the data in <see cref="BcBlockData"/></returns>
        public IRRBoundaryConditionsDataParser GetParser(BcBlockData bcBlockData)
        {
            Ensure.NotNull(bcBlockData,nameof(bcBlockData));
            
            if (DataIsConstant(bcBlockData))
            {
                return new RRBoundaryConditionsConstantParser(logHandler);
            }

            if (DataIsTimeSeries(bcBlockData))
            {
                return new RRBoundaryConditionsTimeSeriesParser(logHandler, bcSectionParser);
            }

            logHandler.ReportError(string.Format(Resources.RRBoundaryConditionsDataParserProvider_GetParser_Invalid_function_type_for_boundary_condition___0__, bcBlockData.SupportPoint));
            return new RRBoundaryConditionsInvalidFunctionParser();
        }

        private static bool DataIsConstant(BcBlockData bcBlockData)
        {
            return bcBlockData.FunctionType.EqualsCaseInsensitive(constant);
        }

        private static bool DataIsTimeSeries(BcBlockData bcBlockData)
        {
            return bcBlockData.FunctionType.EqualsCaseInsensitive(timeSeries);
        }
    }
}