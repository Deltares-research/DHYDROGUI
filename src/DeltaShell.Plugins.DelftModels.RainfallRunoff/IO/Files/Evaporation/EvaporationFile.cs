using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation
{
    /// <summary>
    /// Base class for evaporation files.
    /// </summary>
    public abstract class EvaporationFile : IEvaporationFile
    {
        protected readonly IDictionary<EvaporationDate, double[]> SortedEvaporations = new SortedDictionary<EvaporationDate, double[]>(new EvaporationDateComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedEvaporationFile"/> class.
        /// </summary>
        protected EvaporationFile()
        {
            Evaporation = new ReadOnlyDictionary<EvaporationDate, double[]>(SortedEvaporations);
        }

        /// <inheritdoc/>
        public abstract IReadOnlyCollection<string> Header { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<EvaporationDate, double[]> Evaporation { get; }

        /// <summary>
        /// Adds a new evaporation data entry to the file.
        /// </summary>
        /// <param name="date"> The date. </param>
        /// <param name="evaporationValues"> The evaporation values. </param>
        /// <remarks> The year of the <paramref name="date"/> will be ignored and 0 will be used a the year. </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="evaporationValues"/> is <c>null</c>.
        /// </exception>
        public void Add(DateTime date, double[] evaporationValues)
        {
            Ensure.NotNull(evaporationValues, nameof(evaporationValues));
            SetEvaporationValues(date, evaporationValues);
        }

        protected abstract void SetEvaporationValues(DateTime date, double[] evaporationValues);
    }
}