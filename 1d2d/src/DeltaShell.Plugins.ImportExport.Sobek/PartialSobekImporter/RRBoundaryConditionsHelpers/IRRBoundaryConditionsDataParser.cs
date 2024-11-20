using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Interface for rainfall runoff boundary conditions parsers.
    /// </summary>
    public interface IRRBoundaryConditionsDataParser
    {
        /// <summary>
        /// Parse <see cref="bcBlockData"/> to <see cref="RainfallRunoffBoundaryData"/>.
        /// </summary>
        /// <param name="bcBlockData">Data block to parse from</param>
        /// <returns><see cref="RainfallRunoffBoundaryData"/> which contains the parsed data from <paramref name="bcBlockData"/>.</returns>
        RainfallRunoffBoundaryData Parse(BcBlockData bcBlockData);
    }
}