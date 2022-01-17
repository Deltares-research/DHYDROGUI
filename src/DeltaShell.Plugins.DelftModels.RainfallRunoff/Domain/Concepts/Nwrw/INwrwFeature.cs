using System;
using System.Collections.Concurrent;
using DelftTools.Utils;
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
        void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper);
        void InitializeNwrwCatchmentModelData(NwrwData nwrwData);
    }
}