using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Class for changing RainFallRunoff related data into usable properties for <see cref="RRBoundaryConditionsSetter"/>.
    /// </summary>
    public class RRModelBoundarySetterData
    {
        /// <summary>
        /// Property which keeps boundary data by boundary name.
        /// </summary>
        public readonly IReadOnlyDictionary<string, RainfallRunoffBoundaryData> BoundaryDataByBoundaryName;
        
        /// <summary>
        /// Property which keeps the Unpaved data by name.
        /// </summary>
        /// <remarks>
        /// The <c>Value</c> (<see cref="UnpavedData"/>)  from the <see cref="IReadOnlyDictionary{TKey,TValue}"/> is not Readonly, this will be retrieved and have the <c>BoundaryData</c> set in <see cref="RRBoundaryConditionsSetter"/>.
        /// </remarks>
        public readonly IReadOnlyDictionary<string, UnpavedData> UnpavedDataByName;

        /// <summary>
        /// Property which keeps the connection between the laterals and catchments.
        /// </summary>
        public readonly IReadOnlyDictionary<string, SobekRRLink[]> LateralToCatchmentLookup;

        /// <summary>
        /// Constructor for <see cref="RRModelBoundarySetterData"/>, changes RainFallRunoff related data into usable properties for <see cref="RRBoundaryConditionsSetter"/>.
        /// </summary>
        /// <param name="rainfallRunoffModel">Rainfall runoff model to retrieve data from.</param>
        /// <param name="boundaryDataByBoundaryName">boundaryDataByBoundaryName to retrieve data from.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rainfallRunoffModel"/> is <c>Null</c> or <paramref name="boundaryDataByBoundaryName"/> is <c>Null</c>.</exception>
        public RRModelBoundarySetterData(RainfallRunoffModel rainfallRunoffModel, IReadOnlyDictionary<string, RainfallRunoffBoundaryData> boundaryDataByBoundaryName)
        {
            Ensure.NotNull(rainfallRunoffModel, nameof(rainfallRunoffModel));
            Ensure.NotNull(boundaryDataByBoundaryName, nameof(boundaryDataByBoundaryName));

            BoundaryDataByBoundaryName = boundaryDataByBoundaryName;
            LateralToCatchmentLookup = rainfallRunoffModel.LateralToCatchmentLookup;
            UnpavedDataByName = rainfallRunoffModel.ModelData.OfType<UnpavedData>()
                                                   .ToDictionary(unpavedData => unpavedData.Name, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}