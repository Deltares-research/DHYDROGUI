using DelftTools.Functions;
using DelftTools.Hydro;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    /// <summary>
    /// Class holding <see cref="TimeSeries"/> for the given <see cref="IStructure1D"/>
    /// </summary>
    public class StructureTimeSeries : IStructureTimeSeries
    {
        /// <summary>
        /// Creates a new instance of <see cref="StructureTimeSeries"/>.
        /// </summary>
        /// <param name="structure">The structure the time series are provided for.</param>
        /// <param name="timeSeries">The time series for the given structure.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="structure"/> or <paramref name="timeSeries"/> is <c>null</c>.
        /// </exception>
        public StructureTimeSeries(IStructure1D structure, TimeSeries timeSeries)
        {
            Ensure.NotNull(structure, nameof(structure));
            Ensure.NotNull(timeSeries, nameof(timeSeries));

            Structure = structure;
            TimeSeries = timeSeries;
        }

        /// <summary>
        /// Gets the structure.
        /// </summary>
        public IStructure1D Structure { get; }

        /// <summary>
        /// Gets the time series.
        /// </summary>
        public ITimeSeries TimeSeries { get; }
    }
}