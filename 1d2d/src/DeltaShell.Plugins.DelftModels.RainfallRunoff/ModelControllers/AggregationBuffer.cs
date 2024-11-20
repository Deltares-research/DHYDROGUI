using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public class AggregationBuffer
    {
        #region AggregationType enum

        public enum AggregationType
        {
            Last,
            Minimum,
            Maximum,
            Average,
            Sum,
        }

        #endregion

        private readonly IDictionary<object, AggregationBufferItem> buffers =
            new Dictionary<object, AggregationBufferItem>();

        public void AddToBuffer(object key, AggregationType aggregationType, double[] values)
        {
            lock (buffers)
            {
                if (!buffers.ContainsKey(key))
                {
                    buffers.Add(key, new AggregationBufferItem(values));
                }
                else
                {
                    AggregationBufferItem buffer = buffers[key];
                    buffer.Add(values, aggregationType);
                }
            }
        }

        public double[] GetOutputAndClearBuffer(object key)
        {
            lock (buffers)
            {
                if (!buffers.ContainsKey(key))
                {
                    throw new ArgumentException("Unknown key");
                }

                double[] values = buffers[key].Values;
                buffers.Remove(key);
                return values;
            }
        }
    }

    internal class AggregationBufferItem
    {
        public readonly double[] Values;
        private int Count;

        public AggregationBufferItem(double[] values)
        {
            Values = values;
            Count = 1;
        }

        public void Add(double[] newValues, AggregationBuffer.AggregationType aggregationType)
        {
            if (newValues.Length != Values.Length)
                throw new ArgumentException("Values array not of same length");

            switch (aggregationType)
            {
                case AggregationBuffer.AggregationType.Last:
                    AdjustValues(newValues, (oldValue, newValue) => newValue);
                    break;
                case AggregationBuffer.AggregationType.Minimum:
                    AdjustValues(newValues, Math.Min);
                    break;
                case AggregationBuffer.AggregationType.Maximum:
                    AdjustValues(newValues, Math.Max);
                    break;
                case AggregationBuffer.AggregationType.Average:
                    AdjustValues(newValues, (oldValue, newValue) => ((oldValue*Count) + newValue)/(Count + 1));
                    break;
                case AggregationBuffer.AggregationType.Sum:
                    AdjustValues(newValues, (oldValue, newValue) => oldValue + newValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("aggregationType");
            }

            Count++;
        }

        private void AdjustValues(double[] newValues, Func<double, double, double> operation)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = operation(Values[i], newValues[i]);
            }
        }
    }
}