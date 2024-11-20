using System;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public interface ITimeBasedFunction
    {
        int GetSliceSize();
        void SetValues(DateTime dateTime, int dateTimeIndex, double[] values);
        IFunction InnerFunction { get; }
    }
    
    #region ITimeBasedFunction Impl

    public class TimeSeriesFiller : ITimeBasedFunction
    {
        public TimeSeriesFiller(ITimeSeries innerFunction)
        {
            InnerFunction = innerFunction;
        }

        public int GetSliceSize()
        {
            return 1;
        }

        public void SetValues(DateTime dateTime, int dateTimeIndex, double[] values)
        {
            if (values.Length != 1)
                throw new ArgumentException();

            InnerFunction[dateTime] = values[0];
        }

        private ITimeSeries InnerFunction { get; set; }
        IFunction ITimeBasedFunction.InnerFunction
        {
            get { return InnerFunction; }
        }
    }

    public class FeatureCoverageFiller : ITimeBasedFunction
    {
        private int numFeatures = -1;

        public int GetSliceSize()
        {
            if (numFeatures == -1)
            {
                numFeatures = InnerFunction.Features.Count;
            }
            return numFeatures;
        }

        public void SetValues(DateTime dateTime, int dateTimeIndex, double[] values)
        {
            // performance hack!
            var store = InnerFunction.Store;
            var oldValue = store.FireEvents;
            store.FireEvents = false;

            store.SetVariableValues(InnerFunction.Components[0], values,
                                    new VariableIndexRangeFilter(InnerFunction.Time, dateTimeIndex));

            store.FireEvents = oldValue;
        }

        private IFeatureCoverage InnerFunction { get; set; }
        IFunction ITimeBasedFunction.InnerFunction
        {
            get { return InnerFunction; }
        }

        public FeatureCoverageFiller(IFeatureCoverage innerFunction)
        {
            InnerFunction = innerFunction;
        }
    }

    #endregion
}