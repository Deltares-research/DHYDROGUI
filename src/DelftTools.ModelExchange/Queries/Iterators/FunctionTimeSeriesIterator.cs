using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using GeoAPI.Extensions.Coverages;

namespace DelftTools.ModelExchange.Queries.Iterators
{
    internal abstract class FunctionTimeSeriesIterator
    {
        /// <summary>
        /// Transforms the IFunction data to a sequence of (DateTime, double) tuples
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>        
        internal static IEnumerable<Utils.Tuple<DateTime, double>> ToTimesValuesTuples(IFunction function)
        {
            if (function.Components[0].ValueType == typeof(bool))
            {
                DateTime[] timesArray = ((IEnumerable<DateTime>)function.Arguments[0].Values).ToArray();
                bool[] valuesArray = ((IEnumerable<bool>)function.Components[0].Values).ToArray();
                for (int i = 0; i < valuesArray.Length; i++)
                    yield return new Utils.Tuple<DateTime, double>(timesArray[i], Convert.ToDouble(valuesArray[i]));
            }
            else
            {
                DateTime[] timesArray = ((IEnumerable<DateTime>)function.Arguments[0].Values).ToArray();
                double[] valuesArray = ((IEnumerable<double>)function.Components[0].Values).ToArray();
                for (int i = 0; i < valuesArray.Length; i++)
                    yield return new Utils.Tuple<DateTime, double>(timesArray[i], valuesArray[i]);
            }
        }

        internal static void ThrowIfLocationIdIsEmpty(string locationId)
        {
            if (String.IsNullOrEmpty(locationId))
                throw new ArgumentException("locationId");
        }

        internal static void ThrowIfLocationIsNotFound(string locationId, INetworkLocation location)
        {
            if (location == null)
                throw new InvalidOperationException("There is no location found for location id: " + locationId);
        }

        internal abstract IEnumerable<Utils.Tuple<DateTime, double>> GetIterator();
    }
}