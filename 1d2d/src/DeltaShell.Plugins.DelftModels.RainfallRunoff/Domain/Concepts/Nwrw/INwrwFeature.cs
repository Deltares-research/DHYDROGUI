using System;
using System.Collections.Concurrent;
using DelftTools.Utils;
using Deltares.Infrastructure.API.Logging;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwImporterHelper
    {
        public ConcurrentDictionary<string, NwrwData> CurrentNwrwCatchmentModelDataByNodeOrBranchId { get; set; } = new ConcurrentDictionary<string, NwrwData>(StringComparer.InvariantCultureIgnoreCase);

    }

    public interface INwrwFeature : INameable
    {
        IGeometry Geometry { get; set; }

        /// <summary>
        /// Adds the NWRW catchments data to the provided <paramref name="rrModel"/>.
        /// </summary>
        /// <param name="rrModel"> The rainfall runoff model. </param>
        /// <param name="helper"> The NWRW importer helper that contains the NWRW data. </param>
        /// <param name="logHandler">Log handler for logs made in this method.</param>
        void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper, ILogHandler logHandler);
        void InitializeNwrwCatchmentModelData(NwrwData nwrwData);
    }
}