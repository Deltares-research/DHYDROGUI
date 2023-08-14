using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization
{
    /// <summary>
    /// Class to help with retrieving and setting the time series data on a <see cref="LateralDischargeFunction"/>.
    /// </summary>
    public sealed class LateralTimeSeriesSetter : ILateralTimeSeriesSetter
    {
        private readonly ILogHandler logHandler;
        private readonly IDictionary<string, BcBlockData> bcDataById;

        /// <summary>
        /// Initialize a new instance of the <see cref="LateralTimeSeriesSetter"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <param name="bcBlockData"> The data blocks from the boundary conditions file. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> or <paramref name="bcBlockData"/> is <c>null</c>.
        /// </exception>
        public LateralTimeSeriesSetter(ILogHandler logHandler, IEnumerable<BcBlockData> bcBlockData)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(bcBlockData, nameof(bcBlockData));

            this.logHandler = logHandler;
            bcDataById = GetBcMapping(bcBlockData);
        }

        /// <inheritdoc />
        public void SetDischargeFunction(string lateralId, LateralDischargeFunction dischargeFunction)
        {
            Ensure.NotNullOrWhiteSpace(lateralId, nameof(lateralId));
            Ensure.NotNull(dischargeFunction, nameof(dischargeFunction));

            if (!bcDataById.TryGetValue(lateralId, out BcBlockData bcBlock))
            {
                logHandler.ReportError(string.Format(Resources.No_BC_data_could_be_found_for_lateral_with_id_0_, lateralId));
                return;
            }

            if (!bcBlock.FunctionType.EqualsCaseInsensitive(BcFileConstants.TimeSeriesFunctionName))
            {
                logHandler.ReportError(string.Format(Resources.Function_type_0_is_not_supported_for_lateral_with_id_1_Line_2_, bcBlock.FunctionType, lateralId, bcBlock.LineNumber));
                return;
            }

            BcQuantityData timeQuantity = GetQuantity(bcBlock, BcFileConstants.TimeQuantityName);
            if (timeQuantity == null)
            {
                ReportErrorMissingQuantity(BcFileConstants.TimeQuantityName, lateralId, bcBlock);
                return;
            }

            BcQuantityData dischargeQuantity = GetQuantity(bcBlock, BcFileConstants.LateralDischargeQuantityName);
            if (dischargeQuantity == null)
            {
                ReportErrorMissingQuantity(BcFileConstants.LateralDischargeQuantityName, lateralId, bcBlock);
                return;
            }

            SetDischargeFunction(dischargeFunction, bcBlock, timeQuantity, dischargeQuantity);
        }

        private void SetDischargeFunction(LateralDischargeFunction dischargeFunction, BcBlockData bcBlock, BcQuantityData timeQuantity, BcQuantityData dischargeQuantity)
        {
            IEnumerable<DateTime> dateTimes = BcQuantityDataParsingHelper.ParseDateTimes(bcBlock.SupportPoint, timeQuantity);
            bool dischargesParsed = TryParseDischarges(dischargeQuantity, out IEnumerable<double> discharges, bcBlock.LineNumber);
            if (!dischargesParsed)
            {
                return;
            }

            InterpolationType timeInterpolation = BcQuantityDataParsingHelper.ParseTimeInterpolationType(bcBlock);

            CompleteFunction(dischargeFunction, dateTimes, discharges, timeInterpolation);
        }

        private bool TryParseDischarges(BcQuantityData dischargeQuantity, out IEnumerable<double> values, int lineNumber)
        {
            var doubleValues = new List<double>();

            foreach (string value in dischargeQuantity.Values)
            {
                if (!value.TryParseToDouble(out double doubleValue))
                {
                    values = doubleValues;
                    logHandler.ReportError(string.Format(Resources.Could_not_parse_0_to_a_floating_value_Line_1_, value, lineNumber));
                    return false;
                }

                doubleValues.Add(doubleValue);
            }

            values = doubleValues;
            return true;
        }

        private void ReportErrorMissingQuantity(string quantity, string lateralId, BcBlockData bcBlock)
        {
            logHandler.ReportError(string.Format(Resources.Quantity_0_could_not_be_found_for_lateral_with_id_1_Line_2_, quantity, lateralId, bcBlock.LineNumber));
        }

        private static IDictionary<string, BcBlockData> GetBcMapping(IEnumerable<BcBlockData> bcBlockData)
        {
            var mapping = new Dictionary<string, BcBlockData>();

            foreach (BcBlockData bcBlock in bcBlockData)
            {
                mapping[bcBlock.SupportPoint] = bcBlock;
            }

            return mapping;
        }

        private static BcQuantityData GetQuantity(BcBlockData bcBlock, string quantityName)
        {
            return bcBlock.Quantities.FirstOrDefault(q => q.QuantityName.EqualsCaseInsensitive(quantityName));
        }

        private static void CompleteFunction(IFunction function, IEnumerable<DateTime> dateTimes, IEnumerable<double> values,
                                             InterpolationType timeInterpolation)
        {
            IVariable argument = function.Arguments[0];

            bool isAutoSorted = argument.IsAutoSorted;
            argument.IsAutoSorted = false;
            argument.SetValues(dateTimes);
            argument.InterpolationType = timeInterpolation;
            function.SetValues(values);
            argument.IsAutoSorted = isAutoSorted;
        }
    }
}