using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    public class BcmFileFlowBoundaryDataBuilder : BcFileFlowBoundaryDataBuilder
    {
        public const string BedLevelAtBound = "bed level";
        public const string BedLevelChangeAtBound = "bed level change";
        public const string BedLoadAtBound = "transport incl pores ";

        private static readonly IDictionary<string, FlowBoundaryQuantityType> quantityNameToTypeDictionary =
            new Dictionary<string, FlowBoundaryQuantityType>
            {
                {BedLevelAtBound, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
                {BedLevelChangeAtBound, FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed},
                {BedLoadAtBound, FlowBoundaryQuantityType.MorphologyBedLoadTransport}
            };

        protected override IDictionary<string, FlowBoundaryQuantityType> QuantityNameToTypeDictionary =>
            quantityNameToTypeDictionary;

        protected override BcQuantityData CreateBcQuantityDataForArgument(
            string quantity, IVariable argument, DateTime? referenceTime, TimeSpan timeZone)
        {
            var refTime = "";
            Func<double, double> converter = null;
            if (referenceTime == null)
            {
                referenceTime = (DateTime) argument.Values[0];
            }

            converter = FlowBoundaryCondition.GetPeriodInMinutes; //In case needed for some other operations
            refTime = referenceTime.Value.ToString("yyyyMMdd");
            return new BcmQuantityData
            {
                QuantityName = quantity,
                Unit = "minutes",
                ReferenceTime = refTime,
                Values = PrintValues(argument, null, converter).ToList()
            };
        }

        protected override BcBlockData CreateBlockData(FlowBoundaryCondition boundaryCondition, string supportPoint)
        {
            return new BcmBlockData
            {
                SupportPoint =
                    boundaryCondition.IsHorizontallyUniform ? boundaryCondition.FeatureName : supportPoint,
                Location = boundaryCondition.FeatureName
            };
        }

        protected override IEnumerable<string> PrintValues(IVariable variable, DateTime? referenceTime,
                                                           Func<double, double> converter)
        {
            if (variable.ValueType == typeof(string))
            {
                return variable.GetValues<string>();
            }

            if (variable.ValueType == typeof(double))
            {
                if (converter == null)
                {
                    return variable.GetValues<double>().Select(d => d.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    return variable.GetValues<double>()
                                   .Select(d => converter(d).ToString(CultureInfo.InvariantCulture));
                }
            }

            if (variable.ValueType == typeof(DateTime))
            {
                if (referenceTime == null)
                {
                    return variable.GetValues<DateTime>().Select(d => d.ToString("yyyyMMddHHmmss"));
                }

                return
                    variable.GetValues<DateTime>()
                            .Select(d => (d - referenceTime.Value).TotalMinutes)
                            .Select(m => m.ToString(CultureInfo.InvariantCulture));
            }

            return Enumerable.Empty<string>();
        }

        protected override IEnumerable<object> ParseValues(BcQuantityData quantityData, Type type, string supportPointName)
        {
            IEnumerable<string> stringValues = quantityData.Values;
            string format = quantityData.Unit;

            var bcmData = quantityData as BcmQuantityData;
            string dateString = bcmData == null
                                    ? DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                                    : bcmData.ReferenceTime;

            if (type == typeof(DateTime))
            {
                DateTime startDate;
                bool succes = DateTime.TryParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture,
                                                     DateTimeStyles.AdjustToUniversal, out startDate);

                if (!succes)
                {
                    throw new FormatException("Time format " + dateString + " is not supported by bc file parser");
                }

                if (format.ToLower() == "seconds")
                {
                    return
                        stringValues.Select(s => startDate + new TimeSpan(0, 0, 0, Convert.ToInt32(double.Parse(s))))
                                    .Cast<object>();
                }

                if (format.ToLower() == "minutes")
                {
                    return
                        stringValues.Select(s => startDate + new TimeSpan(0, 0, Convert.ToInt32(double.Parse(s)), 0))
                                    .Cast<object>();
                }

                if (format.ToLower() == "hours")
                {
                    return
                        stringValues.Select(s => startDate + new TimeSpan(0, Convert.ToInt32(double.Parse(s)), 0, 0))
                                    .Cast<object>();
                }

                if (format.ToLower() == "days")
                {
                    return
                        stringValues.Select(s => startDate + new TimeSpan(Convert.ToInt32(double.Parse(s)), 0, 0, 0))
                                    .Cast<object>();
                }
            }

            if (type == typeof(string))
            {
                return stringValues;
            }

            if (type == typeof(double))
            {
                if (format != null && format.ToLower().Equals("minutes"))
                {
                    return
                        stringValues.Select(double.Parse)
                                    .Select(FlowBoundaryCondition.GetFrequencyInDegPerHour)
                                    .Cast<object>();
                }

                return stringValues.Select(double.Parse).Cast<object>();
            }

            throw new ArgumentException(string.Format("Value type {0} with unit {1} not supported by bcm file parser.",
                                                      type, format));
        }
    }
}