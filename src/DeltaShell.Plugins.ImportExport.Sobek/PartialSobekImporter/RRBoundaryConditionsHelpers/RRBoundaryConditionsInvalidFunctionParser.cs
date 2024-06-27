using System;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Rainfall runoff parser for boundary conditions that contain an invalid function.
    /// </summary>
    public class RRBoundaryConditionsInvalidFunctionParser : IRRBoundaryConditionsDataParser
    {
        /// <summary>
        /// Returns new <see cref="RainfallRunoffBoundaryData"/> for data containing an invalid function.
        /// </summary>
        /// <param name="bcBlockData">Data block containing an invalid function.</param>
        /// <returns> New <see cref="RainfallRunoffBoundaryData"/>.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="bcBlockData"/> is <c>null</c>.</exception>
        public RainfallRunoffBoundaryData Parse(BcBlockData bcBlockData)
        {
            Ensure.NotNull(bcBlockData, nameof(bcBlockData));
            return new RainfallRunoffBoundaryData();
        }
    }
}