using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Rainfall runoff parser for boundary conditions that are time dependent.
    /// </summary>
    public class RRBoundaryConditionsTimeSeriesParser : IRRBoundaryConditionsDataParser
    {
        private const string periodic = "true";
        private readonly ILogHandler logHandler;
        private readonly IBcSectionParser bcSectionParser;

        /// <summary>
        /// Rainfall runoff boundary conditions parser for time series.
        /// </summary>
        /// <param name="logHandler">Log handler to log information.</param>
        /// <param name="bcSectionParser">Category parser for bc data.</param>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="logHandler"/> or <paramref name="bcSectionParser"/> is <c>null</c>.
        /// </exception>
        public RRBoundaryConditionsTimeSeriesParser(ILogHandler logHandler, IBcSectionParser bcSectionParser)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(bcSectionParser, nameof(bcSectionParser));
            this.logHandler = logHandler;
            this.bcSectionParser = bcSectionParser;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">When <paramref name="bcBlockData"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="bcBlockData"/> does not have a <c>FunctionType</c> of timeseries.
        /// </exception>
        public RainfallRunoffBoundaryData Parse(BcBlockData bcBlockData)
        {
            Ensure.NotNull(bcBlockData, nameof(bcBlockData));

            if (!bcBlockData.FunctionType.EqualsCaseInsensitive(BoundaryRegion.FunctionStrings.TimeSeries))
            {
                throw new ArgumentException(Resources.RRBoundaryConditionsTimeSeriesParser_Parse_The_provided__BcBlockData__is_not_timeseries_);
            }

            var runoffBoundaryData = new RainfallRunoffBoundaryData();
            runoffBoundaryData.IsTimeSeries = true;

            if (TimeSeriesDataIsEmpty(bcBlockData))
            {
                logHandler.ReportWarning(string.Format(Resources.RRBoundaryConditionsParser_Parse_No_boundary_data_available_for_boundary___0__, bcBlockData.SupportPoint));
                return runoffBoundaryData;
            }

            CreateTimeSeries(bcBlockData.Quantities, runoffBoundaryData.Data, bcBlockData.LineNumber, bcBlockData.SupportPoint);
            return runoffBoundaryData;
        }

        private static bool TimeSeriesDataIsEmpty(BcBlockData bcBlockData)
        {
            return bcBlockData.Quantities.Count != 2 || bcBlockData.Quantities[0].Values.Count == 0 || bcBlockData.Quantities[1].Values.Count == 0;
        }

        private void CreateTimeSeries(IList<BcQuantityData> table, ITimeSeries givenTimeSeries, int lineNumber, string supportPoint)
        {
            if (TimeSeriesAreParsed(table, lineNumber, out IEnumerable<DateTime> argumentValues, out IEnumerable<double> functionValues))
            {
                bcSectionParser.CompleteFunction(givenTimeSeries, argumentValues, functionValues, periodic);
            }
            else
            {
                logHandler.ReportError(string.Format(Resources.RRBoundaryConditionsParser_Parse_No_valid_data_available_for_boundary___0__, supportPoint));
            }
        }

        private bool TimeSeriesAreParsed(IList<BcQuantityData> table, int lineNumber, out IEnumerable<DateTime> argumentValues, out IEnumerable<double> functionValues)
        {
            functionValues = Enumerable.Empty<double>();
            return DateTimesAreParsed(table, lineNumber, out argumentValues) && ValuesAreParsed(table, lineNumber, out functionValues);
        }

        private bool ValuesAreParsed(IList<BcQuantityData> table, int lineNumber, out IEnumerable<double> functionValues)
        {
            return bcSectionParser.TryParseDoubles(table[1].Values, lineNumber, out functionValues);
        }

        private bool DateTimesAreParsed(IList<BcQuantityData> table, int lineNumber, out IEnumerable<DateTime> argumentValues)
        {
            return bcSectionParser.TryParseDateTimes(table[0].Values, table[0].Unit, lineNumber, out argumentValues);
        }
    }
}