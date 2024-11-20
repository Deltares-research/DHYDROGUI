using DelftTools.Functions;
using DelftTools.Hydro;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{ 
    /// <summary>
    /// Interface holding <see cref="TimeSeries"/> for the given <see cref="IStructure1D"/>
    /// </summary>
    public interface IStructureTimeSeries
    {
        /// <summary>
        /// Gets the structure.
        /// </summary>
        IStructure1D Structure { get; }
        
        /// <summary>
        /// Gets the time series.
        /// </summary>
        ITimeSeries TimeSeries { get; }
    }
}