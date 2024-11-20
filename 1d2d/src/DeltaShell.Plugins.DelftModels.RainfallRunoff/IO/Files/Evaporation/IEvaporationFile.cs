using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation
{
    /// <summary>
    /// Interface for an evaporation file.
    /// </summary>
    public interface IEvaporationFile
    {
        /// <summary>
        /// The header lines of the evaporation file.
        /// </summary>
        IReadOnlyCollection<string> Header { get; }

        /// <summary>
        /// The evaporation data per date.
        /// The dictionary is sorted by date in an ascending order.
        /// </summary>
        IReadOnlyDictionary<EvaporationDate, double[]> Evaporation { get; }

        /// <summary>
        /// Adds a new evaporation data entry to the file.
        /// </summary>
        /// <param name="date"> The date. </param>
        /// <param name="evaporationValues"> The evaporation values. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="evaporationValues"/> is <c>null</c>.
        /// </exception>
        void Add(DateTime date, double[] evaporationValues);
    }
}