using System.Collections.Generic;

namespace DelftTools.ModelExchange.Queries
{
    public abstract class DataAggregationStrategy
    {
        // TODO: these are not DataItems (IEnumerable<IDataItem>) these are just objects, use Project here as a property and rename this class to e.g. ProjectSearchHandler
        public IEnumerable<object> DataItems { get; set; }

        public bool ModelHasBeenInitialized { get; set; }

        /// <summary>
        /// Checks if the string is null empty or has only whitespaces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool IsNullOrEmptyOrWhitespace(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Trim() == "")
                return true;
            return false;
        }

        public abstract IEnumerable<AggregationResult> GetAll();
    }
}