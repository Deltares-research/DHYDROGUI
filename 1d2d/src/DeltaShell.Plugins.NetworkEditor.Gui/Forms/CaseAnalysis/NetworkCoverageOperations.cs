using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CaseAnalysis
{
    public static class NetworkCoverageOperations
    {
        
        public interface INetworkCoverageOperation
        {
            bool RequiresScalarArgument { get; }
            bool RequiresSecondCoverage { get; }
            bool RequiresPrimaryTimeDependent { get; }
            bool AllowSecondaryNonTimeDependentIfFirstIs { get; }
            INetworkCoverage Perform(INetworkCoverage a, object b = null);
        }

        public abstract class NetworkCoverageOperation : INetworkCoverageOperation
        {
            public virtual bool RequiresScalarArgument { get { return false; } }
            public virtual bool RequiresSecondCoverage {  get { return false; } }
            public virtual bool RequiresPrimaryTimeDependent { get { return true; } }
            public virtual bool AllowSecondaryNonTimeDependentIfFirstIs { get { return false; } }

            /// <summary>
            /// Performs an operation on <paramref name="a"/>, using <paramref name="b"/> for
            /// additional data required by the operation.
            /// </summary>
            /// <param name="a">Source of the operation (left side)</param>
            /// <param name="b">Additional data of the operation (right side)</param>
            public abstract INetworkCoverage Perform(INetworkCoverage a, object b = null);
        }

        public abstract class NetworkCoveragePairOperation : NetworkCoverageOperation
        {
            public override bool RequiresSecondCoverage { get { return true; } }
            public override bool RequiresPrimaryTimeDependent { get { return false; } }
            public override bool AllowSecondaryNonTimeDependentIfFirstIs { get { return true; } }

            public override INetworkCoverage Perform(INetworkCoverage a, object b)
            {
                return Perform(a, b as INetworkCoverage);
            }

            public abstract INetworkCoverage Perform(INetworkCoverage a, INetworkCoverage b = null);
        }

        public abstract class NetworkCoverageScalarOperation : NetworkCoverageOperation
        {
            public override bool RequiresSecondCoverage { get { return false; } }
            public override bool RequiresPrimaryTimeDependent { get { return false; } }
            public override bool RequiresScalarArgument { get { return true; } }

            public override INetworkCoverage Perform(INetworkCoverage a, object b)
            {
                return Perform(a, (double)b);
            }

            public abstract INetworkCoverage Perform(INetworkCoverage a, double referenceValue);
        }

        public class CoverageMinOperation : NetworkCoverageOperation
        {
            public override INetworkCoverage Perform(INetworkCoverage a, object b = null)
            {
                //in the future we may want to add a second component with timestamp (using DoAggregationComplex)
                return DoAggregation(a, ds => ds.Min());
            }
            
            public override string ToString()
            {
                return "Min";
            }
        }

        public class CoverageMeanOperation : NetworkCoverageOperation
        {
            public override INetworkCoverage Perform(INetworkCoverage a, object b = null)
            {
                return DoAggregation(a, ds => ds.Average());
            }

            public override string ToString()
            {
                return "Mean";
            }
        }

        public class CoverageMaxOperation : NetworkCoverageOperation
        {
            public override INetworkCoverage Perform(INetworkCoverage a, object b = null)
            {
                return DoAggregation(a, ds => ds.Max());
            }

            public override string ToString()
            {
                return "Max";
            }
        }

        public class CoverageGreaterThanAsDoubleOperation : NetworkCoverageScalarOperation
        {
            public override INetworkCoverage Perform(INetworkCoverage networkCoverage, double referenceValue)
            {
                return DoOperation(networkCoverage, referenceValue, (coverageValue, refValue) =>
                    {
                        if (IsNoDataValue(networkCoverage, coverageValue))
                        {
                            return coverageValue;
                        }
                        return coverageValue > refValue ? 1.0 : 0.0;
                    });
            }

            public override string ToString()
            {
                return "Greater than";
            }
        }



        public class CoverageLessThanAsDoubleOperation : NetworkCoverageScalarOperation
        {
            public override INetworkCoverage Perform(INetworkCoverage networkCoverage, double referenceValue)
            {
                return DoOperation(networkCoverage, referenceValue, (coverageValue, refValue) =>
                    {
                        if (IsNoDataValue(networkCoverage, coverageValue))
                        {
                            return coverageValue;
                        }
                        return coverageValue < refValue ? 1.0 : 0.0;
                    });
            }

            public override string ToString()
            {
                return "Less than";
            }
        }

        public class CoverageGreaterThanDurationAsDoubleOperation : NetworkCoverageScalarOperation
        {
            public CoverageGreaterThanDurationAsDoubleOperation()
            {
                TimeInterpolationType = InterpolationType.None;
            }

            public InterpolationType TimeInterpolationType { get; set; }

            public override INetworkCoverage Perform(INetworkCoverage networkCoverage, double referenceValue)
            {
                return networkCoverage.MeasureDurationWhereTrue(referenceValue, (coverageValue, refValue) => coverageValue > refValue, TimeInterpolationType, true);
            }

            public override string ToString()
            {
                return "Greater than duration [days]";
            }
        }

        public class CoverageLessThanDurationAsDoubleOperation : NetworkCoverageScalarOperation
        {
            public CoverageLessThanDurationAsDoubleOperation()
            {
                TimeInterpolationType = InterpolationType.None;
            }

            public InterpolationType TimeInterpolationType { get; set; }

            public override INetworkCoverage Perform(INetworkCoverage networkCoverage, double referenceValue)
            {
                return networkCoverage.MeasureDurationWhereTrue(referenceValue, (coverageValue, refValue) => coverageValue < refValue, TimeInterpolationType, true);
            }

            public override string ToString()
            {
                return "Less than duration [days]";
            }
        }

        /// <summary>
        /// Adds the value defined in one <see cref="INetworkCoverage"/>, if defined and a data value,
        /// to a source <see cref="INetworkCoverage"/>.
        /// </summary>
        public class CoverageAddOperation : NetworkCoveragePairOperation
        {
            public override INetworkCoverage Perform(INetworkCoverage a, INetworkCoverage b)
            {
                return DoOperation(a, b, NetworkCoverageMathExtensions.Add);
            }
            
            public override string ToString()
            {
                return "Add";
            }
        }

        /// <summary>
        /// Subtracts the value defined in one <see cref="INetworkCoverage"/>, if defined and a data value,
        /// to a source <see cref="INetworkCoverage"/>.
        /// </summary>
        public class CoverageSubtractOperation : NetworkCoveragePairOperation
        {
            public override INetworkCoverage Perform(INetworkCoverage a, INetworkCoverage b)
            {
                return DoOperation(a, b, NetworkCoverageMathExtensions.Substract);
            }

            public override string ToString()
            {
                return "Subtract";
            }
        }

        public class CoverageAbsDiffOperation : NetworkCoveragePairOperation
        {
            public override INetworkCoverage Perform(INetworkCoverage a, INetworkCoverage b)
            {
                return DoOperation(a, b, (aa, bb) =>
                    {
                        if (IsNoDataValue(a, aa))
                        {
                            return aa;
                        }
                        if (IsNoDataValue(b, bb))
                        {
                            return GetNoDataValue(a, b, bb);
                        }

                        return Math.Abs(aa - bb);
                    });
            }

            public override string ToString()
            {
                return "Abs Difference";
            }
        }

        private static INetworkCoverage CopyNetworkCoverage(INetworkCoverage originalCoverage)
        {
            //clone works strange for NetCdf, for now just create a manual copy

            var result = new NetworkCoverage("", originalCoverage.IsTimeDependent) { Network = originalCoverage.Network };

            if (originalCoverage.IsTimeDependent)
            {
                result.Time.SetValues(originalCoverage.Time.Values);
            }

            result.Locations.SetValues(originalCoverage.Locations.Values.Select(loc => (INetworkLocation)loc.Clone()));
            result.Components[0].SetValues(originalCoverage.Components[0].Values);
            result.Components[0].NoDataValues = originalCoverage.Components[0].NoDataValues;

            if (result.Components.Count > 1)
            {
                throw new NotImplementedException(
                    "Support for spatial data with multiple components is not implemented in case analysis");
            }

            return result;
        }

        private static INetworkCoverage DoOperation(INetworkCoverage a, INetworkCoverage b, Action<INetworkCoverage, INetworkCoverage> operation)
        {
            var c = CopyNetworkCoverage(a);

            if (a.IsTimeDependent && !b.IsTimeDependent) //use b as 'scalar'
            {
                foreach (var time in a.Time.Values)
                {
                    var filtC = c.AddTimeFilter(time);
                    operation(filtC, b);
                }
            }
            else
            {
                operation(c, b);
            }

            return c;
        }

        private static INetworkCoverage DoOperation(INetworkCoverage a, INetworkCoverage b, Func<double, double, double> operation)
        {
            return DoOperation(a, b, (cc, bb) => cc.Operate(bb, operation));
        }

        private static INetworkCoverage DoOperation<T>(INetworkCoverage a, double b, Func<double, double, T> operation)
        {
            return a.OperateToNewCoverage(b, operation);
        }

        private static INetworkCoverage DoAggregation(INetworkCoverage a, Func<IEnumerable<double>, double> operation)
        {
            var networkCoverage = new NetworkCoverage("", false) { Network = a.Network };
            networkCoverage.Components[0].NoDataValues = a.Components[0].NoDataValues;

            foreach (var loc in a.Locations.Values)
            {
                var values = a.GetTimeSeries(loc).Components[0].GetValues<double>().
                    Where(v => !a.Components[0].NoDataValues.Contains(v)).ToList();
                if (values.Any())
                {
                    networkCoverage[loc.Clone()] = operation(values);
                }
                else
                {
                    networkCoverage[loc.Clone()] = a.Components[0].NoDataValues[0];
                }
            }
            return networkCoverage;

        }

        private static bool IsNoDataValue(INetworkCoverage coverage, double value)
        {
            return coverage.Components[0].NoDataValues != null && coverage.Components[0].NoDataValues.Contains(value);
        }

        private static double GetNoDataValue(INetworkCoverage coverageA, INetworkCoverage coverageB, double value)
        {
            if (IsNoDataValue(coverageA, value))
            {
                return value;
            }

            if (coverageA.Components[0].NoDataValues == null || coverageA.Components[0].NoDataValues.Count == 0)
            {
                coverageA.Components[0].NoDataValues = coverageB.Components[0].NoDataValues;
                return value; // Is no data value of B (and due to assignment also now of A)
            }

            return (double)coverageA.Components[0].NoDataValues[0];
        }
    }
}