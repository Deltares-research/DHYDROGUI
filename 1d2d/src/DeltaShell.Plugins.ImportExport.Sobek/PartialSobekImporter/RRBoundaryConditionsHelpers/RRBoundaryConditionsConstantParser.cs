using System;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Rainfall runoff parser for boundary conditions that are constant.
    /// </summary>
    public class RRBoundaryConditionsConstantParser : IRRBoundaryConditionsDataParser
    {
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Rainfall runoff boundary conditions parser for constant data.
        /// </summary>
        /// <param name="logHandler">Log handler to log information.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="logHandler"/> is <c>null</c>.</exception>
        public RRBoundaryConditionsConstantParser(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            this.logHandler = logHandler;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">When <paramref name="bcBlockData"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="bcBlockData"/> does not have a <c>FunctionType</c> of constant.
        /// </exception>
        public RainfallRunoffBoundaryData Parse(BcBlockData bcBlockData)
        {
            Ensure.NotNull(bcBlockData, nameof(bcBlockData));

            if (!bcBlockData.FunctionType.EqualsCaseInsensitive(BoundaryRegion.FunctionStrings.Constant))
            {
                throw new ArgumentException(Resources.RRBoundaryConditionsConstantParser_Parse_The_provided__BcBlockData__is_not_constant_);
            }

            return new RainfallRunoffBoundaryData
            {
                IsConstant = true,
                Value = GetConstantValue(bcBlockData)
            };
        }

        private double GetConstantValue(BcBlockData bcBlockData)
        {
            string valueFromDataBlock = bcBlockData.Quantities.FirstOrDefault()?.Values.FirstOrDefault();

            if (valueFromDataBlock == null)
            {
                logHandler.ReportWarning(string.Format(Resources.RRBoundaryConditionsParser_Parse_No_boundary_data_available_for_boundary___0__, bcBlockData.SupportPoint));
                return 0;
            }

            if (double.TryParse(valueFromDataBlock, out double value))
            {
                return value;
            }

            const string expectedValueType = "Double";
            logHandler.ReportError(string.Format(Resources.RRBoundaryConditionsConstantParser_GetValue_No_valid_data_available_for_boundary___0____data___1___is_not_of_format___2__,
                                                 bcBlockData.SupportPoint, valueFromDataBlock, expectedValueType));
            return value;
        }
    }
}